using System;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Util;

namespace Pathfinder.Command
{
    public static class Handler
    {
        private static Dictionary<string, Func<Hacknet.OS, List<string>, bool>> commands =
            new Dictionary<string, Func<Hacknet.OS, List<string>, bool>>();

        public static bool AddCommand(string key,
                                      Func<Hacknet.OS, List<string>, bool> function,
                                      string description = null,
                                      bool autoComplete = false)
        {
            Logger.Verbose("Mod {0} is attempting to add command {1}", Utility.GetPreviousStackFrameIdentity(), key);
            if (commands.ContainsKey(key))
                return false;
            commands.Add(key, function);
            if (description != null)
                Helpfile.help.Add(key + "\n    " + description);
            if(autoComplete && !ProgramList.programs.Contains(key))
                    ProgramList.programs.Add(key);
            return false;
        }

        [Obsolete("The second argument of function should be a list instead of an array")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function)
        {
            return AddCommand(key, (os, l) => function(os, l.ToArray()));
        }

        [Obsolete("The second argument of function should be a list instead of an array")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function, bool autoComplete)
        {
            return AddCommand(key, (os, l) => function(os, l.ToArray()), autoComplete: autoComplete);
        }

        [Obsolete("The second argument of function should be a list instead of an array")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function, string description, bool autoComplete)
        {
            return AddCommand(key, (os, l) => function(os, l.ToArray()), description, autoComplete);
        }

        internal static void CommandListener(CommandSentEvent commandSentEvent)
        {
            Func<Hacknet.OS, List<string>, bool> f;
            if (commands.TryGetValue(commandSentEvent.Arguments[0], out f))
            {
                commandSentEvent.IsCancelled = true;
                commandSentEvent.Disconnects = f(commandSentEvent.OsInstance, commandSentEvent.Arguments);
            }
        }
    }
}
