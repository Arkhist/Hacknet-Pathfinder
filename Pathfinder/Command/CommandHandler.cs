using System;
using Hacknet;
using Pathfinder.Command;

namespace Pathfinder
{
    [Obsolete("Use Pathfinder.Command.Handler")]
    public static class CommandHandler
    {
        [Obsolete("Use Pathfinder.Command.Handler.AddCommand")]
        public static bool AddCommand(string key, Func<OS, string[], bool> function) { return Handler.AddCommand(key, function); }
        [Obsolete("Use Pathfinder.Command.Handler.AddCommand")]
        public static bool AddCommand(string key, Func<OS, string[], bool> function, bool auto) { return Handler.AddCommand(key, function, auto); }
        [Obsolete("Use Pathfinder.Command.Handler.AddCommand")]
        public static bool AddCommand(string key, Func<OS, string[], bool> function, string des, bool auto) { return Handler.AddCommand(key, function, des, auto); }
    }
}
