using System;
using Pathfinder.Command;

namespace Pathfinder
{
    [Obsolete("Use Pathfinder.Command.Handler")]
    public static class CommandHandler
    {
        [Obsolete("Use Pathfinder.Command.Handler.AddCommand")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function)
        {
            Handler.modBacktrack += 1;
            return Handler.AddCommand(key, function);
        }
        [Obsolete("Use Pathfinder.Command.Handler.AddCommand")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function, bool auto)
        {
            Handler.modBacktrack += 1;
            return Handler.AddCommand(key, function, auto);
        }
        [Obsolete("Use Pathfinder.Command.Handler.AddCommand")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function, string des, bool auto)
        {
            Handler.modBacktrack += 1;
            return Handler.AddCommand(key, function, des, auto);
        }
    }
}
