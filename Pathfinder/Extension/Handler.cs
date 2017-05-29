using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hacknet;
using Hacknet.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Event;
using Pathfinder.Util;

namespace Pathfinder.Extension
{
    public static class Handler
    {
        public static Info ActiveInfo { get; internal set; }
        public static ExtensionInfo ActiveExtension
        {
            get { return ExtensionLoader.ActiveExtensionInfo; }
            set { ExtensionLoader.ActiveExtensionInfo = value; }
        }

        internal static bool CanRegister { get; set; }

        internal static Dictionary<string, Tuple<Info, Texture2D, GUI.Button>> ModExtensions =
                                                                         new Dictionary<string, Tuple<Info, Texture2D, GUI.Button>>();

        public static string RegisterExtension(string id, Info extensionInfo)
        {
            if (Pathfinder.CurrentMod == null)
                throw new InvalidOperationException("RegisterExtension can not be called outside of mod loading.");
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to register extension {1} with id {2}",
                           Utility.ActiveModId,
                           extensionInfo.GetType().FullName,
                           id);
            if (ModExtensions.ContainsKey(id))
                return null;

            extensionInfo.Id = id;
            Texture2D t = null;
            if (File.Exists(extensionInfo.LogoPath))
                using (var fs = File.OpenRead(extensionInfo.LogoPath))
                    t = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, fs);
            ModExtensions.Add(id, new Tuple<Info, Texture2D, GUI.Button>(extensionInfo, t,
                                                                         new GUI.Button(-1, -1, 450, 50,
                                                                                        Utility.ActiveModId + ": "
                                                                                        + extensionInfo.Name,
                                                                                        Color.White)));
            return id;
        }

        internal static bool UnregisterExtension(string id)
        {
            id = Utility.GetId(id);
            if (!ModExtensions.ContainsKey(id))
                return true;
            var info = ModExtensions[id];
            info.Item1.Id = null;
            return ModExtensions.Remove(id);
        }

        internal static void LoadExtension(string id)
        {
            id = Utility.GetId(id);
            if (!ModExtensions.ContainsKey(id))
                return;
            var tuple = ModExtensions[id];
            if (tuple.Item1.executables.Count < 1)
                return;
            var info = ActiveInfo;
            ActiveInfo = tuple.Item1;
            CanRegister = true;
            foreach (var e in tuple.Item1.executables)
                Executable.Handler.RegisterExecutable(e.Key, e.Value);
            foreach (var d in tuple.Item1.daemons)
                Daemon.Handler.RegisterDaemon(d.Key, d.Value);
            foreach (var c in tuple.Item1.commands)
                Command.Handler.RegisterCommand(c.Key, c.Value.Item1, c.Value.Item2, c.Value.Item3);
            foreach (var g in tuple.Item1.goals)
                Mission.Handler.RegisterMissionGoal(g.Key, g.Value);
            foreach (var m in tuple.Item1.missions)
                Mission.Handler.RegisterMission(m.Key, m.Value);
            foreach (var p in tuple.Item1.ports)
                Port.Handler.RegisterPort(p.Key, p.Value);
            foreach (var e in tuple.Item1.eventListeners)
                if (EventManager.eventListeners.ContainsKey(e.Key))
                    EventManager.eventListeners[e.Key].AddRange(e.Value);
                else
                    EventManager.eventListeners.Add(e.Key, e.Value);
            CanRegister = false;
            ActiveInfo = info;
        }

        internal static void UnloadExtension(string id)
        {
            id = Utility.GetId(id);
            if (!ModExtensions.ContainsKey(id))
                return;
            var tuple = ModExtensions[id];
            string modIds;
            foreach (var e in Executable.Handler.ModExecutables.ToArray())
                if (e.Key.IndexOf('.') != -1 && e.Key.Remove(e.Key.LastIndexOf('.')) == id)
                {
                    modIds = e.Key.RemoveExtensionId();
                    if (!tuple.Item1.executables.ContainsKey(modIds))
                        tuple.Item1.executables.Add(modIds, e.Value.Item1);
                    Executable.Handler.UnregisterExecutable(e.Key);
                }

            foreach (var d in Daemon.Handler.ModDaemons.ToArray())
                if (d.Key.IndexOf('.') != -1 && d.Key.Remove(d.Key.LastIndexOf('.')) == id)
                {
                    modIds = d.Key.RemoveExtensionId();
                    if (!tuple.Item1.daemons.ContainsKey(modIds))
                        tuple.Item1.daemons.Add(modIds, d.Value);
                    Daemon.Handler.UnregisterDaemon(d.Key);
                }

            foreach (var c in Command.Handler.ModIdToCommandKeyList[id].ToArray())
            {
                if(!tuple.Item1.commands.ContainsKey(c))
                        tuple.Item1.commands.Add(c,
                                                 new Tuple<Command.Handler.CommandFunc, string, bool>(
                                                     Command.Handler.ModCommands[c],
                                                     Command.Help.help.ContainsKey(c) ? Command.Help.help[c] : null,
                                                     ProgramList.programs.Contains(c)));
                Command.Handler.UnregisterCommand(c);
            }

            foreach (var g in Mission.Handler.ModGoals.ToArray())
                if (g.Key.IndexOf('.') != -1 && g.Key.Remove(g.Key.LastIndexOf('.')) == id)
                {
                    modIds = g.Key.RemoveExtensionId();
                    if (!tuple.Item1.goals.ContainsKey(modIds))
                        tuple.Item1.goals.Add(modIds, g.Value);
                    Mission.Handler.UnregisterMissionGoal(g.Key);
                }

            foreach (var m in Mission.Handler.ModMissions.ToArray())
                if (m.Key.IndexOf('.') != -1 && m.Key.Remove(m.Key.LastIndexOf('.')) == id)
                {
                    modIds = m.Key.RemoveExtensionId();
                    if (!tuple.Item1.missions.ContainsKey(modIds))
                        tuple.Item1.missions.Add(modIds, m.Value);
                    Mission.Handler.UnregisterMission(m.Key);
                }

            foreach (var p in Port.Handler.PortTypes.ToArray())
                if (p.Key.IndexOf('.') != -1 && p.Key.Remove(p.Key.LastIndexOf('.')) == id)
                {
                    modIds = p.Key.RemoveExtensionId();
                    if (!tuple.Item1.ports.ContainsKey(modIds))
                        tuple.Item1.ports.Add(modIds, p.Value);
                    Port.Handler.UnregisterPort(p.Key);
                }

            var events = new List<Tuple<Action<PathfinderEvent>, string, string, int>>();
            foreach (var v in EventManager.eventListeners.Values)
                events.AddRange(v.FindAll(t => t.Item3 == id));
            foreach (var list in EventManager.eventListeners.ToArray())
            {
                if(!tuple.Item1.eventListeners.ContainsKey(list.Key))
                    tuple.Item1.eventListeners.Add(list.Key, new List<Tuple<Action<PathfinderEvent>, string, string, int>>());
                foreach (var e in events)
                {
                    if(!tuple.Item1.eventListeners[list.Key].Contains(e))
                        tuple.Item1.eventListeners[list.Key].Add(e);
                    list.Value.Remove(e);
                }
            }
        }

        internal static string RemoveExtensionId(this string id) => id.Substring(id.LastIndexOf('.') + 1);
    }
}
