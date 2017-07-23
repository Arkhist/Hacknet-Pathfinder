using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pathfinder.Util;
using Pathfinder.Event;
using Pathfinder.Util.Attribute;
using ALoadOrderAttribute = Pathfinder.Util.Attribute.LoadOrderAttribute;

namespace Pathfinder.ModManager
{
    static class Manager
    {
        public static readonly string ModFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                                          + Path.DirectorySeparatorChar + "Mods";
        public static readonly string DepFolderPath = ModFolderPath + Path.DirectorySeparatorChar + "deps";

        public static Dictionary<string, List<IMod>> ModIdReliance = new Dictionary<string, List<IMod>>();
        public static Dictionary<string, IMod> LoadedMods = new Dictionary<string, IMod>();
        public static List<string> UnloadedModIds = new List<string>();

        public static IMod CurrentMod { get; internal set; }
        public static Dictionary<string, IMod> OperationalMods => LoadedMods
            .Where(pair => !(pair.Value is Placeholder || pair.Value is ModPlaceholder))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        public static IEnumerable<Type> GetModTypes(this Assembly asm) =>
                asm.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IMod).IsAssignableFrom(t));

        public static IMod GetFirstMod(this Assembly asm)
        {
            foreach (var pair in LoadedMods)
                if (pair.Value.GetType().Assembly == asm)
                    return pair.Value;
            return null;
        }

        public static IMod GetLoadedMod(string id)
        {
            IMod mod;
            LoadedMods.TryGetValue(id, out mod);
            return mod;
        }

        public static void LoadMods()
        {
            var separator = Path.DirectorySeparatorChar;

            Logger.Verbose("Checking/creating Mod folder '{0}'", ModFolderPath);
            if (!Directory.Exists(ModFolderPath))
                Directory.CreateDirectory(ModFolderPath);

            if (Directory.Exists(DepFolderPath))
                foreach (var dll in Directory.GetFiles(DepFolderPath + separator, "*.dll"))
                    try { Assembly.LoadFrom(dll); }
                    catch (Exception e) { Logger.Error("Loading Dependency '{0}' failed: \n\t{1}", dll, e); }

            foreach (var dll in Directory.GetFiles(ModFolderPath + separator, "*.dll"))
                TryLoadMods(dll);
        }

        public static void LoadModContent()
        {
            foreach (var mod in OperationalMods)
            {
                if (mod.Value is Placeholder) continue;
                CurrentMod = mod.Value;
                Logger.Verbose("Loading mod '{0}'s content", mod.Key);
                mod.Value.LoadContent();
            }
            CurrentMod = null;
        }

        public static void UnloadMods(GameUnloadEvent e)
        {
            foreach (var mod in OperationalMods)
            {
                CurrentMod = mod.Value;
                Logger.Verbose("Unloading mod '{0}'", mod.Key);
                mod.Value.Unload();
                CurrentMod = null;
            }
        }

        public static void UnloadMod(IMod mod)
        {
            if (mod == null || mod is Placeholder) return;

            CurrentMod = mod;
            var name = Utility.ActiveModId;

            var attrib = mod.GetType().GetFirstAttribute<ALoadOrderAttribute>();
            if (attrib != null)
                foreach (var ident in attrib.afterIds)
                {
                    var id = ident.GetCleanId();
                    if (LoadedMods.ContainsKey(id)
                        && LoadedMods[id].GetType().GetFirstAttribute<AllowOrderUnloadAttribute>()?.Allowed == true)
                        UnloadMod(LoadedMods[id]);
                }

            foreach (var e in Extension.Handler.ModExtensions.ToArray())
                if (e.Key.IndexOf('.') != -1 && e.Key.Remove(e.Key.IndexOf('.')) == name)
                    Extension.Handler.UnregisterExtension(e.Key);

            foreach (var e in Executable.Handler.ModExecutables.ToArray())
                if (e.Key.IndexOf('.') != -1 && e.Key.Remove(e.Key.IndexOf('.')) == name)
                    Executable.Handler.UnregisterExecutable(e.Key);

            foreach (var d in Daemon.Handler.ModDaemons.ToArray())
                if (d.Key.IndexOf('.') != -1 && d.Key.Remove(d.Key.IndexOf('.')) == name)
                    Daemon.Handler.UnregisterDaemon(d.Key);

            foreach (var c in Command.Handler.ModIdToCommandKeyList[name].ToArray())
                Command.Handler.UnregisterCommand(c);

            foreach (var g in Mission.Handler.ModGoals.ToArray())
                if (g.Key.IndexOf('.') != -1 && g.Key.Remove(g.Key.IndexOf('.')) == name)
                    Mission.Handler.UnregisterMissionGoal(g.Key);

            foreach (var m in Mission.Handler.ModMissions.ToArray())
                if (m.Key.IndexOf('.') != -1 && m.Key.Remove(m.Key.IndexOf('.')) == name)
                    Mission.Handler.UnregisterMission(m.Key);

            foreach (var p in Port.Handler.PortTypes.ToArray())
                if (p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name)
                    Port.Handler.UnregisterPort(p.Key);

            var events = new List<Tuple<Action<PathfinderEvent>, string, string, int>>();
            foreach (var v in EventManager.eventListeners.Values)
                events.AddRange(v.FindAll(t => t.Item3 == name));
            foreach (var list in EventManager.eventListeners.ToArray())
                foreach (var e in events)
                    list.Value.Remove(e);

            GUI.ModOptions.Handler.ModOptions.Remove(name);

            mod.Unload();
            UnloadedModIds.Add(name);
            LoadedMods.Remove(name);
            CurrentMod = null;
        }

        public static IMod CreateMod(Type modType) => (IMod)Activator.CreateInstance(modType);

        public static IMod LoadMod(IMod mod)
        {
            if (mod == null) return null;
            var modType = mod.GetType();
            var attrib = modType.GetFirstAttribute<ALoadOrderAttribute>();
            string name = mod.GetCleanId();
            if (attrib != null)
            {
                foreach (var pair in ModIdReliance.ToArray())
                {
                    var i = pair.Value.FindIndex(m => m.GetCleanId() == name);
                    if (i != -1)
                        ModIdReliance[pair.Key][i] = mod;
                }
                foreach (var id in attrib.beforeIds)
                {
                    if (!ModIdReliance.ContainsKey(id.GetCleanId()))
                        ModIdReliance.Add(id, new List<IMod>());
                    if (!ModIdReliance[id].Contains(mod))
                        ModIdReliance[id].Add(mod);
                }
                foreach (var id in attrib.afterIds)
                {
                    if (!ModIdReliance.ContainsKey(name))
                        ModIdReliance.Add(name, new List<IMod>());
                    if (ModIdReliance[name].FindIndex(m => m.GetCleanId() == id.GetCleanId()) != -1)
                        ModIdReliance[name].Add(new Placeholder(id.GetCleanId()));
                }
                if (attrib.beforeIds.Count > 0)
                    return mod;
            }
            try
            {
                if (!Pathfinder.IsModIdentifierValid(name, true))
                    return null; // never reached due to throw
                Logger.Info("Loading mod '{0}'", name);
                CurrentMod = mod;
                mod.Load();
                UnloadedModIds.Remove(name);
                LoadedMods.Add(name, mod);
                GUI.ModOptions.Handler.LoadFor(mod);
                if (ModIdReliance.ContainsKey(name))
                    foreach (var internalMod in ModIdReliance[name])
                        LoadMod(internalMod);
            }
            catch (Exception ex)
            {
                Logger.Error("Mod '{0}' of file '{1}' failed to load:\n\t{2}", modType.FullName, Path.GetFileName(modType.Assembly.Location), ex);
                UnloadMod(mod);
                UnloadedModIds.Remove(name);
                CurrentMod = null;
                return null;
            }
            CurrentMod = null;
            return mod;
        }

        public static IMod LoadMod(Type modType) => LoadMod(CreateMod(modType));


        public static List<IMod> LoadMods(string path, string modId = null)
        {
            var result = new List<IMod>();
            foreach (Type t in Assembly.LoadFile(path).GetModTypes())
            {
                var mod = CreateMod(t);
                if (modId != null && mod?.GetCleanId() != modId) continue;
                LoadMod(mod);
                UnloadedModIds.Remove(mod?.GetCleanId());
                result.Add(mod);
            }
            return result;
        }

        public static void TryLoadMods(string path)
        {
            try { LoadMods(path); }
            catch (Exception ex) { Logger.Error("Mod file '{0}' failed to load:\n\t{1}", Path.GetFileName(path), ex); }
        }
    }
}
