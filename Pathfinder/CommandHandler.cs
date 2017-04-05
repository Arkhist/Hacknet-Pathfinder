using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hacknet;
using Pathfinder.Event;

namespace Pathfinder
{
    public static class CommandHandler
    {
        private static Dictionary<string, Func<OS, string[], bool>> commands =
            new Dictionary<string, Func<Hacknet.OS, string[], bool>>();


        public static bool AddCommand(string key, Func<OS, string[], bool> function)
        {
            if (commands.ContainsKey(key))
                return false;
            commands.Add(key, function);
            return true;
        }

        public static bool AddCommand(string key, Func<OS, string[], bool> function, bool autoComplete)
        {
            if (commands.ContainsKey(key))
                return false;
            commands.Add(key, function);
            if (autoComplete && !ProgramList.programs.Contains(key))
                ProgramList.programs.Add(key);
            return true;
        }

        public static bool AddCommand(string key, Func<OS, string[], bool> function, string description, bool autoComplete)
        {
            if (commands.ContainsKey(key))
                return false;
            commands.Add(key, function);
            if (autoComplete && !ProgramList.programs.Contains(key))
                ProgramList.programs.Add(key);
            Helpfile.help.Add(description);
            return true;
        }

        /// <summary>
        /// Do not use this method outside of Pathfinder.dll
        /// </summary>
        public static void CommandListener(PathfinderEvent pathfinderEvent)
        {
            var commandSentEvent = (CommandSentEvent)pathfinderEvent;
            foreach (KeyValuePair<string, Func<OS, string[], bool>> entry in commands)
                if (commandSentEvent.Args[0].ToLower() == entry.Key.ToLower())
                {
                    bool disconnects = entry.Value(commandSentEvent.OsInstance, commandSentEvent.Args);
                    commandSentEvent.IsCancelled = true;
                    commandSentEvent.Disconnects = disconnects;
                    break;
                }
        }
    }
}
