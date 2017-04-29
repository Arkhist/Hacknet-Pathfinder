using System;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.OS;
using Pathfinder.Util;

namespace Pathfinder.Command
{
    public static class Handler
    {
        private static Dictionary<string, Func<Hacknet.OS, List<string>, bool>> commands =
            new Dictionary<string, Func<Hacknet.OS, List<string>, bool>>();

        /// <summary>
        /// Adds a command to the game.
        /// </summary>
        /// <returns><c>true</c>, if command was added, <c>false</c> otherwise.</returns>
        /// <param name="key">The key used to run the command.</param>
        /// <param name="function">The function run when command is input.</param>
        /// <param name="description">A description to input when help is command is run (if not null).</param>
        /// <param name="autoComplete">If set to <c>true</c> then autocomplete for command is enabled.</param>
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

        internal static void CommandListener(CommandSentEvent e)
        {
            Func<Hacknet.OS, List<string>, bool> f;
            if (commands.TryGetValue(e.Arguments[0], out f))
            {
                e.IsCancelled = true;
                try
                {
                    e.Disconnects = f(e.OS, e.Arguments);
                }
                catch (Exception ex)
                {
                    e.OS.WriteF("Command {0} threw Exception:\n    {1}('{2}')", e.Arguments[0], ex.GetType().FullName, ex.Message);
                    throw ex;
                }
            }
        }
    }
}
