using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pathfinder.Attribute;
using Pathfinder.Event;
using Pathfinder.Util;
using Pathfinder.Util.Attribute;
using static Pathfinder.Command.Handler;
using static Pathfinder.Event.EventManager;

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

        public static List<IMod> MarkedModsForUnload = new List<IMod>();
        public static List<IMod> MarkedModsForLoad = new List<IMod>();

        public static IMod CurrentMod { get; set; }
        public static Dictionary<string, IMod> OperationalMods =>
            (
                from pair in LoadedMods
                where !(pair.Value is Placeholder)
                select pair 
            ).ToDictionary(pair => pair.Key, pair => pair.Value);

        public static IEnumerable<Type> GetModTypes(this Assembly asm) =>
                asm.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IMod).IsAssignableFrom(t));

        public static List<string> LoadedModIds => OperationalMods.Keys.ToList();

        public static IMod GetFirstMod(this Assembly asm)
        {
            foreach (var pair in LoadedMods)
                if (pair.Value.GetType().Assembly == asm)
                    return pair.Value;
            return null;
        }

        public static IMod GetLoadedMod(string id)
        {
            if (!LoadedMods.TryGetValue(id, out IMod mod)) return null;
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

                if (ModAttributeHandler.ModToCommandMethods.TryGetValue(CurrentMod.GetType(), out List<MethodInfo> infos))
                    foreach (var i in infos)
                    {
                        var attrib = i.GetFirstAttribute<CommandAttribute>();
                        RegisterCommand(attrib.Key ?? i.Name.RemoveLast("Command"), i.CreateDelegate<CommandFunc>(), attrib.Description, attrib.Autocomplete);
                    }

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

        public static void MarkForUnload(IMod mod) => MarkedModsForUnload.Add(mod);
        public static void UnloadMarkedMods()
        {
            foreach (var mod in MarkedModsForUnload)
                UnloadMod(mod);
            MarkedModsForUnload.Clear();
        }

        public static void MarkForLoad(IMod mod) => MarkedModsForLoad.Add(mod);
        public static void LoadMarkedMods()
        {
            IMod newMod;
            foreach (var mod in MarkedModsForLoad)
            {
                newMod = LoadMod(mod.GetType());
                CurrentMod = newMod;
                newMod.LoadContent();
                CurrentMod = null;
            }
            MarkedModsForLoad.Clear();
        }

        public static void UnloadMod(IMod mod)
        {
            if (mod == null || mod is Placeholder) return;

            CurrentMod = mod;
            var name = Utility.ActiveModId;

            var attrib = mod.GetType().GetFirstAttribute<ModInfoAttribute>();
            if (attrib != null)
                foreach (var ident in attrib.AfterIds)
                {
                    var id = ident.GetCleanId();
                    if (LoadedMods.ContainsKey(id)
                        && LoadedMods[id].GetType().GetFirstAttribute<AllowOrderUnloadAttribute>()?.Allowed == true)
                        UnloadMod(LoadedMods[id]);
                }

            foreach (var e in
                     (from p in Extension.Handler.ModExtensions
                        where p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name
                        select p.Key)
                     .ToArray()
                    )
                    Extension.Handler.UnregisterExtension(e);

            foreach (var e in
                     (from p in Executable.Handler.ModExecutables
                        where p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name
                        select p.Key)
                     .ToArray()
                    )
                    Executable.Handler.UnregisterExecutable(e);

            foreach (var d in
                     (from p in Daemon.Handler.ModDaemons
                        where p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name
                        select p.Key)
                     .ToArray()
                    )
                    Daemon.Handler.UnregisterDaemon(d);

            Command.Handler.ModIdToCommandKeyList.TryGetValue(name, out List<string> clist);
            if (clist != null)
                foreach (var c in clist.ToArray())
                    Command.Handler.UnregisterCommand(c);

            foreach (var g in
                     (from p in Mission.Handler.ModGoals
                      where p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name
                      select p.Key)
                     .ToArray()
                    )
                Mission.Handler.UnregisterMissionGoal(g);

            foreach (var m in
                     (from p in Mission.Handler.ModMissions
                      where p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name
                      select p.Key)
                     .ToArray()
                    )
                Mission.Handler.UnregisterMission(m);

            foreach (var p in
                     (from p in Port.Handler.PortTypes
                      where p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.IndexOf('.')) == name
                      select p.Key)
                     .ToArray()
                    )
                Port.Handler.UnregisterPort(p);

            var events = new List<ListenerTuple>();
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
            var attrib = modType.GetFirstAttribute<ModInfoAttribute>();
            var name = mod.GetCleanId();
            if (attrib != null)
            {
                foreach (var pair in ModIdReliance.ToArray())
                {
                    var i = pair.Value.FindIndex(m => m.GetCleanId() == name);
                    if (i != -1)
                        ModIdReliance[pair.Key][i] = mod;
                }
                foreach (var id in attrib.BeforeIds)
                {
                    if (!ModIdReliance.ContainsKey(id.GetCleanId()))
                        ModIdReliance.Add(id, new List<IMod>());
                    if (!ModIdReliance[id].Contains(mod))
                        ModIdReliance[id].Add(mod);
                }
                foreach (var id in attrib.AfterIds)
                {
                    if (!ModIdReliance.ContainsKey(name))
                        ModIdReliance.Add(name, new List<IMod>());
                    if (ModIdReliance[name].FindIndex(m => m.GetCleanId() == id.GetCleanId()) != -1)
                        ModIdReliance[name].Add(new Placeholder(id.GetCleanId()));
                }
                if (attrib.BeforeIds.Count > 0)
                    return mod;
            }
            try
            {
                if (!Pathfinder.IsModIdentifierValid(name, true))
                    return null; // never reached due to throw
                Logger.Info("Loading mod '{0}'", name);
                CurrentMod = mod;
                if (ModAttributeHandler.ModToEventMethods.TryGetValue(CurrentMod.GetType(), out List<MethodInfo> infos))
                    foreach (var i in infos)
                    {
                        var eventAttrib = i.GetFirstAttribute<EventAttribute>();
                        var paramType = i.GetParameters()[0].ParameterType;
                        RegisterListener(paramType, i.CreateDelegate<Action<PathfinderEvent>>(typeof(Action<>).MakeGenericType(paramType)), eventAttrib.DebugName, eventAttrib.Priority);
                    }
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
            var asm = Assembly.LoadFile(path);
            var modTypes = asm.GetModTypes();
            var needsDefaultAttrib = modTypes.Count() > 1;
            foreach (Type t in modTypes)
            {
                ModAttributeHandler.HandleType(t, needsDefaultAttrib);
                var mod = CreateMod(t);
                if (modId != null && mod?.GetCleanId() != modId) continue;
                LoadMod(mod);
                UnloadedModIds.Remove(mod?.GetCleanId());
                result.Add(mod);
            }
            ModAttributeHandler.Reset();
            return result;
        }

        public static void TryLoadMods(string path)
        {
            try { LoadMods(path); }
            catch (Exception ex) { Logger.Error("Mod file '{0}' failed to load:\n\t{1}", Path.GetFileName(path), ex); }
        }
    }
}
