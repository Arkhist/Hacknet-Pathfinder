using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Game.OS;
using ModOptions = Pathfinder.GUI.ModOptions;

namespace Pathfinder.Internal
{
    static class HandlerListener
    {
        public static void CommandListener(CommandSentEvent e)
        {
            Command.Handler.CommandFunc f;
            if (Command.Handler.ModCommands.TryGetValue(e[0], out f))
            {
                e.IsCancelled = true;
                try
                {
                    e.Disconnects = f(e.OS, e.Arguments);
                }
                catch (Exception ex)
                {
                    e.OS.Write("Command {0} threw Exception:\n    {1}('{2}')", e.Arguments[0], ex.GetType().FullName, ex.Message);
                    throw;
                }
            }
        }

        public static void DaemonLoadListener(LoadComputerXmlReadEvent e)
        {
            Daemon.IInterface i;
            var id = e.Reader.GetAttribute("interfaceId");
            if (id != null && Daemon.Handler.ModDaemons.TryGetValue(id, out i))
            {
                var objs = new Dictionary<string, string>();
                var storedObjects = e.Reader.GetAttribute("storedObjects")?.Split(' ');
                if (storedObjects != null)
                    foreach (var s in storedObjects)
                        objs[s.Remove(s.IndexOf('|'))] = s.Substring(s.IndexOf('|') + 1);
                e.Computer.daemons.Add(Daemon.Instance.CreateInstance(id, e.Computer, objs));
            }
        }

        public static void ExecutableListener(ExecutableExecuteEvent e)
        {
            Tuple<Executable.IInterface, string> tuple;
            if (Executable.Handler.IsFileDataForModExe(e.ExecutableFile.data)
                && Executable.Handler.ModExecutables.TryGetValue(e.ExecutableFile.data.Split('\n')[0], out tuple))
            {
                int num = e.OS.ram.bounds.Y + RamModule.contentStartOffset;
                foreach (var exe in e.OS.exes)
                    num += exe.bounds.Height;
                var location = new Rectangle(e.OS.ram.bounds.X, num, RamModule.MODULE_WIDTH, (int)OS.EXE_MODULE_HEIGHT);
                e.OS.addExe(Executable.Instance.CreateInstance(tuple.Item1, e.ExecutableFile, e.OS, e.Arguments, location));
                e.Result = Executable.ExecutionResult.StartupSuccess;
            }
        }

        public static void UpdateDisplayModes(OptionsMenu m)
        {
            var list = new List<string>();
            foreach (var mode in m.ScreenManager.GraphicsDevice.Adapter.SupportedDisplayModes)
            {
                if (!list.Contains(mode.Width + "x" + mode.Height))
                    list.Add(mode.Width + "x" + mode.Height);
                if (m.getCurrentResolution().Equals(list[list.Count - 1]))
                    m.currentResIndex = list.Count - 1;
            }
            list.Sort();
            m.currentResIndex = list.FindIndex((obj) => obj.Equals(m.getCurrentResolution()));
            if (m.currentResIndex == -1) m.currentResIndex = 0;
        }

        public static void OptionsMenuLoadContentListener(OptionsMenuLoadContentEvent e)
        {
			UpdateDisplayModes(e.OptionsMenu);
            foreach (var o in ModOptions.Handler.ModOptions)
                o.Value.LoadContent(e.OptionsMenu);
        }

        public static void OptionsMenuApplyListener(OptionsMenuApplyEvent e)
        {
            foreach (var o in ModOptions.Handler.ModOptions)
                o.Value.Apply(e.OptionsMenu);
        }

        private static bool updateDisplayMode = false;
        public static void OptionsMenuUpdateListener(OptionsMenuUpdateEvent e)
        {
            if (updateDisplayMode && e.OptionsMenu.windowed == e.OptionsMenu.getIfWindowed())
            {
                UpdateDisplayModes(e.OptionsMenu);
                updateDisplayMode = false;
            }
            else updateDisplayMode |= e.OptionsMenu.windowed != e.OptionsMenu.getIfWindowed();
            // this updates the resolutions on the options menu on the next update so that it be accurate
            // according to the window mode
            foreach (var o in ModOptions.Handler.ModOptions)
                o.Value.Update(e.OptionsMenu, e.GameTime, e.ScreenNotFocused, e.ScreenIsCovered);
        }

        private static string selected;
        public static void OptionsMenuDrawListener(OptionsMenuDrawEvent e)
        {
            if (selected == null || !ModOptions.Handler.ModOptions.ContainsKey(selected)) return;
            e.IsCancelled = true;
            ModOptions.Handler.ModOptions[selected].Draw(e.OptionsMenu, e.GameTime);
        }
    }
}
