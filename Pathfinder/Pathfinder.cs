using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Util;

namespace Pathfinder
{
    public static class Pathfinder
    {
        /*private static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly Version Version = new Version(AssemblyVersion.Major,AssemblyVersion.Minor,1);
        private static Version latestVersion;*/

        class InvalidIdException : ArgumentException { public InvalidIdException(string msg) : base(msg) {} }

        private static Dictionary<string, IPathfinderMod> mods = new Dictionary<string, IPathfinderMod>();
        private static List<string> unloadedMods = new List<string>();

        public static readonly string ModFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                                          + Path.DirectorySeparatorChar + "Mods";

        public static readonly string DepFolderPath = ModFolderPath + Path.DirectorySeparatorChar + "deps";

        public static IPathfinderMod CurrentMod { get; private set; }

        internal static Dictionary<string, IPathfinderMod> OperationalMods => mods.Where(pair => !(pair.Value is ModPlaceholder))
                                                                                  .ToDictionary(pair => pair.Key, pair => pair.Value);

        static Pathfinder()
        {
            mods.Add("Hacknet", new ModPlaceholder("Hacknet"));
            mods.Add("Pathfinder", new ModPlaceholder("Pathfinder"));
        }

        internal static void Initialize()
        {
            Logger.Verbose("Registering Pathfinder listeners");

            /*var verStr = Updater.GetString("https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases/latest",
                                           "tag_name"); // Does not work, IDK
            if (Version.TryParse(verStr.Select(c => Char.IsDigit(c) || c == '.' ? c : (char)0).ToString(), out latestVersion)
               && latestVersion > Version)
                EventManager.RegisterListener<DrawMainMenuEvent>(DrawNewReleaseGraphic);*/

            EventManager.RegisterListener<CommandSentEvent>(Internal.ExecutionOverride.OverwriteProbe);
            EventManager.RegisterListener<ExecutablePortExecuteEvent>(Internal.ExecutionOverride.OverridePortHack);

            EventManager.RegisterListener<CommandSentEvent>(Internal.HandlerListener.CommandListener);

            EventManager.RegisterListener<DrawMainMenuEvent>(Internal.GUI.ModList.DrawModList);
            EventManager.RegisterListener<DrawMainMenuButtonsEvent>(Internal.GUI.ModList.DrawModListButton);

            EventManager.RegisterListener<DrawExtensionMenuEvent>(Internal.GUI.ModExtensionsUI.ExtensionMenuListener);
            EventManager.RegisterListener<DrawExtensionMenuListEvent>(Internal.GUI.ModExtensionsUI.ExtensionListMenuListener);
            EventManager.RegisterListener<OSPostLoadContenEvent>(Internal.GUI.ModExtensionsUI.PostLoadForModExtensionsListener);

            EventManager.RegisterListener<ExecutableExecuteEvent>(Internal.HandlerListener.ExecutableListener);
            EventManager.RegisterListener<CommandSentEvent>(Internal.HandlerListener.ExecutableListInsertListener);

            EventManager.RegisterListener<OSSaveWriteEvent>(ManageSaveXml);

            EventManager.RegisterListener<GameLoadContentEvent>(ExeInfoManager.LoadExecutableStruct);

            EventManager.RegisterListener<GameUnloadEvent>(UnloadMods);

            Logger.Verbose("Loading mods");
            LoadMods();
        }

        internal static void LoadMods()
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
                TryLoadMod(dll);
        }

        internal static IEnumerable<Type> GetModTypes(this Assembly asm) =>
            asm.GetExportedTypes().Where(t =>  t.IsClass && !t.IsAbstract && typeof(IPathfinderMod).IsAssignableFrom(t));

        /// <summary>
        /// Determines whether a mod is loaded
        /// </summary>
        /// <returns><c>true</c>, if mod is loaded, <c>false</c> otherwise.</returns>
        /// <param name="id">Mod Identifier.</param>
        public static bool IsModLoaded(string id) => mods.ContainsKey(id);

        /// <summary>
        /// Determines whether a mod identifier is valid
        /// </summary>
        /// <returns><c>true</c>, if mod identifier is valid, <c>false</c> otherwise.</returns>
        /// <param name="id">The Mod Identifier.</param>
        /// <param name="shouldThrowReason">If set to <c>true</c> then this method will throw.</param>
        public static bool IsModIdentifierValid(string id, bool shouldThrowReason = false)
        {
            if (String.IsNullOrEmpty(id))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' is null or empty");
                return false;
            }
            if (IsModLoaded(id))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' has already been loaded");
                return false;
            }
            if (id.Contains('.'))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' contains a period");
                return false;
            }
            if (Char.IsDigit(id[0]))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' starts with a digit");
                return false;
            }
            if (id.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || id.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' contains invalid path characters");
                return false;
            }
            return true;
        }

        internal static IPathfinderMod GetModByAssembly(Assembly asm)
        {
            foreach (var pair in mods)
                if (pair.Value.GetType().Assembly == asm)
                    return pair.Value;
            return null;
        }

        internal static IPathfinderMod GetMod(string id)
        {
            IPathfinderMod mod;
            mods.TryGetValue(id, out mod);
            return mod;
        }

        internal static void LoadModContent()
        {
            foreach (var mod in OperationalMods)
            {
                CurrentMod = mod.Value;
                Logger.Verbose("Loading mod '{0}'s content", mod.Key);
                mod.Value.LoadContent();
                CurrentMod = null;
            }
        }

        /// <summary>
        /// Gets the loaded mod identifiers.
        /// </summary>
        /// <value>The loaded mod identifiers.</value>
        public static List<string> LoadedModIdentifiers => OperationalMods.Keys.ToList();

        /// <summary>
        /// Gets the unloaded mod identifiers.
        /// </summary>
        /// <value>The unloaded mod identifiers.</value>
        public static List<string> UnloadedModIdentifiers => unloadedMods.ToList();

        internal static void ManageSaveXml(OSSaveWriteEvent e)
        {
            var i = e.SaveString.IndexOf("</HacknetSave>", StringComparison.Ordinal);
            string modListStr = "";
            foreach (var pair in OperationalMods)
                modListStr += "\t<Mod assembly='" + Path.GetFileName(pair.Value.GetType().Assembly.Location) + "'>"
                                                        + pair.Key + "</Mod>\n";
            e.SaveString = e.SaveString.Insert(i, "\n<PathfinderMods>\n" + modListStr + "</PathfinderMods>\n");
        }

        internal static void UnloadMods(GameUnloadEvent e)
        {
            foreach (var mod in OperationalMods)
            {
                CurrentMod = mod.Value;
                Logger.Verbose("Unloading mod '{0}'", mod.Key);
                mod.Value.Unload();
                CurrentMod = null;
            }
        }

        internal static void UnloadMod(IPathfinderMod mod)
        {
            if (mod == null || mod is ModPlaceholder) return;

            CurrentMod = mod;
            string id = "";

            foreach (var e in Extension.Handler.idToInfo.ToArray())
                if (e.Key.IndexOf('.') != -1 && e.Key.Remove(id.IndexOf('.')) == mod.Identifier)
                    Extension.Handler.UnregisterExtension(e.Key);

            foreach (var e in Executable.Handler.idToInterface.ToArray())
                if (e.Key.IndexOf('.') != -1 && id.Remove(id.IndexOf('.')) == mod.Identifier)
                    Executable.Handler.UnregisterExecutable(id);

            foreach (var d in Daemon.Handler.idToInterface.ToArray())
                if (d.Key.IndexOf('.') != -1 && d.Key.Remove(d.Key.IndexOf('.')) == mod.Identifier)
                    Daemon.Handler.UnregisterDaemon(d.Key);

            foreach (var c in Command.Handler.modToCommands[mod.Identifier].ToArray())
                Command.Handler.UnregisterCommand(c);

            foreach (var g in Mission.Handler.goals.ToArray())
                if (g.Key.IndexOf('.') != -1 && g.Key.Remove(g.Key.IndexOf('.')) == mod.Identifier)
                    Mission.Handler.UnregisterMissionGoal(g.Key);

            foreach (var m in Mission.Handler.missions.ToArray())
                if (m.Key.IndexOf('.') != -1 && m.Key.Remove(m.Key.IndexOf('.')) == mod.Identifier)
                    Mission.Handler.UnregisterMission(m.Key);

            var events = new List<Tuple<Action<PathfinderEvent>, string, string>>();
            foreach (var v in EventManager.eventListeners.Values)
                events.AddRange(v.FindAll(t => t.Item3 == mod.Identifier));
            foreach (var e in events)
                EventManager.UnregisterListener(e.Item1);

            mod.Unload();
            unloadedMods.Add(mod.Identifier);
            mods.Remove(mod.Identifier);
            CurrentMod = null;
        }

        internal static IPathfinderMod LoadMod(Type modType)
        {
            string name = null;
            IPathfinderMod modInstance = null;
            try
            {
                modInstance = (IPathfinderMod)Activator.CreateInstance(modType);
                CurrentMod = modInstance;
                var methodInfo = modType.GetProperty("Identifier").GetGetMethod();
                if (methodInfo == null)
                    throw new NotSupportedException("Get Property 'Identifier' doesn't exist, mod '"
                                                    + Path.GetFileName(modType.Assembly.Location) + "' is invalid");
                name = ((string)methodInfo.Invoke(modInstance, null)).Trim();
                if (!IsModIdentifierValid(name, true))
                    return null;
                Logger.Info("Loading mod '{0}'", name);

                mods.Add(name, modInstance);

                modInstance.Load();
            }
            catch (Exception ex)
            {
                Logger.Error("Mod '{0}' of file '{1}' failed to load:\n\t{2}", modType.FullName, Path.GetFileName(modType.Assembly.Location), ex);
                if (mods.ContainsKey(name))
                    mods.Remove(name);
            }
            CurrentMod = null;
            return modInstance;
        }

        internal static void LoadMod(string path)
        {
            foreach (Type t in Assembly.LoadFile(path).GetModTypes())
                unloadedMods.Remove(LoadMod(t)?.Identifier);
        }

        internal static void TryLoadMod(string path)
        {
            try { LoadMod(path); }
            catch (Exception ex) { Logger.Error("Mod file '{0}' failed to load:\n\t{1}", Path.GetFileName(path), ex); }
        }

        internal static void DrawNewReleaseGraphic(DrawMainMenuEvent e)
        {
            if(e.GameTime.ElapsedGameTime.Seconds % 3 != 0)
                TextItem.doFontLabel(new Vector2(300, 100), "New Release Up", GuiData.font, Color.White);
        }
    }
}
