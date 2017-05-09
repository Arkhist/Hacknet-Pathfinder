using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Computer;
using Pathfinder.Event;
using Pathfinder.GUI;
using Pathfinder.OS;
using Pathfinder.Util;

namespace Pathfinder
{
    public static class Pathfinder
    {
        /*private static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly Version Version = new Version(AssemblyVersion.Major,AssemblyVersion.Minor,1);
        private static Version latestVersion;*/

        class ExceptionInvalidId : ArgumentException
        {
            public ExceptionInvalidId(string msg) : base(msg) { }
        }

        private static Dictionary<string, IPathfinderMod> mods = new Dictionary<string, IPathfinderMod>();
        //private static bool currentSaveMissingMods = false;
        private static Dictionary<string, GUI.Button> unloadedMods = new Dictionary<string, GUI.Button>();

        public static readonly string ModFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                                          + Path.DirectorySeparatorChar + "Mods";

        public static readonly string DepFolderPath = ModFolderPath + Path.DirectorySeparatorChar + "deps";

        public static IPathfinderMod CurrentMod { get; private set; }

        public static void init()
        {
            Logger.Verbose("Registering Pathfinder listeners");
            /*var verStr = Updater.GetString("https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases/latest",
                                           "tag_name"); // Does not work, IDK
            if (Version.TryParse(verStr.Select(c => Char.IsDigit(c) || c == '.' ? c : (char)0).ToString(), out latestVersion)
               && latestVersion > Version)
                EventManager.RegisterListener<DrawMainMenuEvent>(DrawNewReleaseGraphic);*/

            EventManager.RegisterListener<CommandSentEvent>(OverwriteProbe);
            EventManager.RegisterListener<ExecutablePortExecuteEvent>(OverridePortHack);

            EventManager.RegisterListener<CommandSentEvent>(Command.Handler.CommandListener);

            EventManager.RegisterListener<DrawMainMenuEvent>(PathfinderMainMenu.DrawMainMenu);
            EventManager.RegisterListener<DrawMainMenuButtonsEvent>(PathfinderMainMenu.DrawPathfinderButtons);

            EventManager.RegisterListener<DrawExtensionMenuEvent>(Extension.Handler.ExtensionMenuListener);
            EventManager.RegisterListener<DrawExtensionMenuListEvent>(Extension.Handler.ExtensionListMenuListener);
            EventManager.RegisterListener<OSPostLoadContenEvent>(Extension.Handler.PostLoadForModExtensionsListener);

            EventManager.RegisterListener<ExecutableExecuteEvent>(Executable.Handler.ExecutableListener);
            EventManager.RegisterListener<CommandSentEvent>(Executable.Handler.ExecutableListInsertListener);

            EventManager.RegisterListener<OSSaveWriteEvent>(ManageSaveXml);

            EventManager.RegisterListener<GameLoadContentEvent>(ExeInfoManager.LoadExecutableStruct);

            EventManager.RegisterListener<GameUnloadEvent>(UnloadMods);

            Logger.Verbose("Loading mods");
            LoadMods();
        }

        public static void LoadMods()
        {
            var separator = Path.DirectorySeparatorChar;

            Logger.Verbose("Checking/creating Mod folder '{0}'", ModFolderPath);
            if (!Directory.Exists(ModFolderPath))
                Directory.CreateDirectory(ModFolderPath);

            if (Directory.Exists(DepFolderPath))
            {
                foreach (var dll in Directory.GetFiles(DepFolderPath + separator, "*.dll"))
                {
                    try
                    {
                        Assembly.LoadFrom(dll);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Loading Dependency '{0}' failed: \n\t{1}", dll, e);
                    }
                }
            }

            foreach (var dll in Directory.GetFiles(ModFolderPath + separator, "*.dll"))
            {
                LoadMod(dll);
            }
        }

        private static IEnumerable<Type> GetModTypes(this Assembly asm) =>
            asm.GetExportedTypes().Where(t =>  t.IsClass && !t.IsAbstract && typeof(IPathfinderMod).IsAssignableFrom(t));

        /// <summary>
        /// Determines whether a mod is loaded
        /// </summary>
        /// <returns><c>true</c>, if mod is loaded, <c>false</c> otherwise.</returns>
        /// <param name="id">Mod Identifier.</param>
        public static bool IsModLoaded(string id) => id == "Pathfinder" || id == "Hacknet" || mods.ContainsKey(id);

        internal static IPathfinderMod GetModByAssembly(Assembly asm)
        {
            foreach (var pair in mods)
            {
                if (pair.Value.GetType().Assembly == asm)
                    return pair.Value;
            }
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
            foreach (var mod in mods)
            {
                CurrentMod = mod.Value;
                Logger.Verbose("Loading mod '{0}'s content", mod.Key);
                mod.Value.LoadContent();
                CurrentMod = null;
            }
        }

        internal static List<string> LoadedModIdentifiers => mods.Keys.ToList();

        internal static void ManageSaveXml(OSSaveWriteEvent e)
        {
            var i = e.SaveString.IndexOf("</HacknetSave>", StringComparison.Ordinal);
            string modListStr = "";
            foreach (var pair in mods)
                modListStr += "\t<Mod assembly='" + Path.GetFileName(pair.Value.GetType().Assembly.Location) + "'>" + pair.Key + "</Mod>\n";
            e.SaveString = e.SaveString.Insert(i, "\n<PathfinderMods>\n" + modListStr + "</PathfinderMods>\n");
        }

        internal static void UnloadMods(GameUnloadEvent e)
        {
            foreach (var mod in mods)
            {
                CurrentMod = mod.Value;
                Logger.Verbose("Unloading mod '{0}'", mod.Key);
                mod.Value.Unload();
                CurrentMod = null;
            }
        }

        internal static void UnloadMod(IPathfinderMod mod)
        {
            if (mod == null) return;

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
                    throw new NotSupportedException("Method 'Identifier' doesn't exist, mod '"
                                                    + Path.GetFileName(modType.Assembly.Location) + "' is invalid");
                name = ((string)methodInfo.Invoke(modInstance, null)).Trim();
                if (IsModLoaded(name))
                    throw new ExceptionInvalidId("Mod identifier '" + name + "' is either already loaded or is reserved");
                if (name.Contains('.'))
                    throw new ExceptionInvalidId("Mod identifier '" + name + "' contains a period, mod identifiers may not contain a period (.)");
                if (Char.IsDigit(name[0]))
                    throw new ExceptionInvalidId("Mod identifier '" + name + "' starts with a digit, mod identifiers may not start with digits");
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

        internal static void LoadMod(string path, bool noCatch = false)
        {
            if (noCatch)
                foreach (Type t in Assembly.LoadFile(path).GetModTypes())
                   LoadMod(t);
            else
                try
                {
                    foreach (Type t in Assembly.LoadFile(path).GetModTypes())
                        LoadMod(t);
                }
                catch (Exception ex)
                {
                    Logger.Error("Mod file '{0}' failed to load:\n\t{1}", Path.GetFileName(path), ex);
                }
        }

        internal static void OverridePortHack(ExecutablePortExecuteEvent e)
        {
            if (e.Arguments[0].ToLower() == "porthack")
            {
                e.IsCancelled = true;
                var os = e.OS;
                var cComp = os.connectedComp;
                bool canRun = false;
                bool firewallActive = false;
                if (cComp != null)
                {
                    int num2 = 0;
                    for (int i = 0; i < cComp.portsOpen.Count; i++)
                        num2 += os.connectedComp.portsOpen[i];
                    foreach (var p in cComp.GetModdedPortList())
                        num2 += p.Unlocked ? 1 : 0;
                    canRun |= num2 > cComp.portsNeededForCrack;
                    if (cComp.firewall != null && !cComp.firewall.solved)
                    {
                        firewallActive |= canRun;
                        canRun = false;
                    }
                }
                if (canRun)
                    os.addExe(new PortHackExe(e.Destination, os));
                else if (firewallActive)
                    os.write(LocaleTerms.Loc("Target Machine Rejecting Syndicated UDP Traffic") + " -\n" + LocaleTerms.Loc("Bypass Firewall to allow unrestricted traffic"));
                else
                    os.write(LocaleTerms.Loc("Too Few Open Ports to Run") + " - \n" + LocaleTerms.Loc("Open Additional Ports on Target Machine") + "\n");
            }
        }

        internal static void OverwriteProbe(CommandSentEvent e)
        {
            if (e.Arguments[0].ToLower() == "probe" || e.Arguments[0].ToLower() == "nmap")
            {
                e.IsCancelled = true;
                e.StateChange = CommandDisplayStateChange.Probe;
                var os = e.OS;
                int i;
                var c = os.GetCurrentComputer();
                os.write(string.Concat("Probing ", c.ip, "...\n"));
                for (i = 0; i < 10; i++)
                {
                    Thread.Sleep(80);
                    os.writeSingle(".");
                }
                os.write("\nProbe Complete - Open ports:\n");
                os.write("---------------------------------");
                if (Port.Instance.compToInst.ContainsKey(c)) foreach (var ins in Port.Instance.compToInst[c])
                {
                    os.write("Port#: " + ins.Port.PortDisplay + " - " + ins.Port.PortName + (ins.Unlocked ? "OPEN" : ""));
                    Thread.Sleep(120);
                }
                for (i = 0; i < c.ports.Count; i++)
                {
                    os.write("Port#: " + c.GetDisplayPortNumberFromCodePort(c.ports[i]) + "  -  " + PortExploits.services[c.ports[i]]
                            + (c.portsOpen[i] > 0 ? " : OPEN" : ""));
                    Thread.Sleep(120);
                }
                os.write("---------------------------------");
                os.write("Open Ports Required for Crack : " + Math.Max(c.portsNeededForCrack + 1, 0));
                if (c.hasProxy)
                    os.write("Proxy Detected : " + (c.proxyActive ? "ACTIVE" : "INACTIVE"));
                if (c.firewall != null)
                    os.write("Firewall Detected : " + (c.firewall.solved ? "SOLVED" : "ACTIVE"));
            }
        }

        internal static void DrawNewReleaseGraphic(DrawMainMenuEvent e)
        {
            if(e.GameTime.ElapsedGameTime.Seconds % 3 != 0)
                TextItem.doFontLabel(new Vector2(300, 100), "New Release Up", GuiData.font, Color.White);
        }
    }
}
