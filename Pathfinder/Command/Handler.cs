using System;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Event;

namespace Pathfinder.Command
{
    public static class Handler
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
            if (AddCommand(key, function))
            {
                if (autoComplete && !ProgramList.programs.Contains(key))
                    ProgramList.programs.Add(key);
                return true;
            }
            return false;
        }

        public static bool AddCommand(string key, Func<OS, string[], bool> function, string description, bool autoComplete)
        {
            if (AddCommand(key, function, autoComplete))
            {
                Helpfile.help.Add(key + "\n    " + description);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Do not use this method outside of Pathfinder.dll
        /// </summary>
        internal static void CommandListener(CommandSentEvent commandSentEvent)
        {
            foreach (KeyValuePair<string, Func<OS, string[], bool>> entry in commands)
                if (commandSentEvent.Args[0].ToLower() == entry.Key.ToLower())
                {
                    commandSentEvent.IsCancelled = true;
                    commandSentEvent.Disconnects = entry.Value(commandSentEvent.OsInstance, commandSentEvent.Args);
                    break;
                }
        }
    }
}
