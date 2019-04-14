using System;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Command
{
    public static class Handler
    {
        public delegate bool CommandFunc(OS os, List<string> args);

        internal static Dictionary<string, CommandFunc> ModCommands = new Dictionary<string, CommandFunc>();
        internal static Dictionary<string, List<string>> ModIdToCommandKeyList = new Dictionary<string, List<string>>();

        public static string ActiveCommand { get; internal set; }

        /// <summary>
        /// Adds a command to the game.
        /// </summary>
        /// <returns>The full mod command id if added to the game, <c>null</c> otherwise</returns>
        /// <param name="key">The key used to run the command.</param>
        /// <param name="function">The function run when command is input.</param>
        /// <param name="description">A description to input when help is command is run (if not null).</param>
        /// <param name="autoComplete">If set to <c>true</c> then autocomplete for command is enabled.</param>
        public static string RegisterCommand(string key,
                                             CommandFunc function,
                                             string description = null,
                                             bool autoComplete = false)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterCommand can not be called outside of mod or extension loading.");
            var id = Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id;
            Logger.Verbose("{0} {1} is attempting to add command {2}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension", id, key);
            if (ModCommands.ContainsKey(key))
                return null;
            ModCommands.Add(key, function);
            if (!ModIdToCommandKeyList.ContainsKey(id))
                ModIdToCommandKeyList.Add(id, new List<string>());
            ModIdToCommandKeyList[id].Add(key);
            if (description != null)
                //Helpfile.help.Add(key + "\n    " + description);
                Help.help.Add(key, description);
            if (autoComplete && !ProgramList.programs.Contains(key))
                ProgramList.programs.Add(key);
            return id + '.' + key;
        }

        internal static bool UnregisterCommand(string key)
        {
            if (!ModCommands.ContainsKey(key))
                return true;
            ModIdToCommandKeyList[Utility.ActiveModId].Remove(key);
            Help.help.Remove(key);
            ProgramList.programs.Remove(key);
            return ModCommands.Remove(key);
        }
    }
}
