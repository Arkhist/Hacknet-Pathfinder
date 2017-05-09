using System;
using System.Collections.Generic;
using System.Text;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.OS;
using Pathfinder.Util;

namespace Pathfinder.Command
{
    public static class Handler
    {
        internal static Dictionary<string, Func<Hacknet.OS, List<string>, bool>> commands =
                    new Dictionary<string, Func<Hacknet.OS, List<string>, bool>>();
        internal static Dictionary<string, List<string>> modToCommands = new Dictionary<string, List<string>>();

        private static Dictionary<string, string> help = new Dictionary<string, string>();

        /// <summary>
        /// Adds a command to the game.
        /// </summary>
        /// <returns>The full mod command id if added to the game, <c>null</c> otherwise</returns>
        /// <param name="key">The key used to run the command.</param>
        /// <param name="function">The function run when command is input.</param>
        /// <param name="description">A description to input when help is command is run (if not null).</param>
        /// <param name="autoComplete">If set to <c>true</c> then autocomplete for command is enabled.</param>
        public static string RegisterCommand(string key,
                                      Func<Hacknet.OS, List<string>, bool> function,
                                      string description = null,
                                      bool autoComplete = false)
        {
            Logger.Verbose("Mod {0} is attempting to add command {1}", Utility.ActiveModId, key);
            if (commands.ContainsKey(key))
                return null;
            commands.Add(key, function);
            if(!modToCommands.ContainsKey(Utility.ActiveModId))
                modToCommands.Add(Utility.ActiveModId, new List<string>());
            modToCommands[Utility.ActiveModId].Add(key);
            if (description != null)
                //Helpfile.help.Add(key + "\n    " + description);
                help.Add(key, description);
            if(autoComplete && !ProgramList.programs.Contains(key))
                    ProgramList.programs.Add(key);
            return Utility.ActiveModId + '.' + key;
        }

        internal static bool UnregisterCommand(string key)
        {
            Logger.Info("unreg 1 {0}", key);
            if (!commands.ContainsKey(key))
                return true;
            Logger.Info("unreg 2");
            modToCommands[Utility.ActiveModId].Remove(key);
            help.Remove(key);
            ProgramList.programs.Remove(key);
            return commands.Remove(key);
        }

        [Obsolete("Use RegisterCommand")]
        public static bool AddCommand(string key,
                                      Func<Hacknet.OS, List<string>, bool> function,
                                      string description = null,
                                      bool autoComplete = false) =>
            RegisterCommand(key, function, description, autoComplete) != null;

        [Obsolete("The second argument of function should be a list instead of an array")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function) =>
            AddCommand(key, (os, l) => function(os, l.ToArray()));
        [Obsolete("The second argument of function should be a list instead of an array")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function, bool autoComplete) =>
            AddCommand(key, (os, l) => function(os, l.ToArray()), autoComplete: autoComplete);
        [Obsolete("The second argument of function should be a list instead of an array")]
        public static bool AddCommand(string key, Func<Hacknet.OS, string[], bool> function, string description, bool autoComplete) =>
            AddCommand(key, (os, l) => function(os, l.ToArray()), description, autoComplete);

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
            else if (e.Arguments[0].ToLower() == "help" || e.Arguments[0].ToLower() == "man" || e.Arguments[0] == "?")
            {
                e.IsCancelled = true;
                int num4 = 0;
                if (e.Arguments.Count > 1)
                {
                    try
                    {
                        num4 = Convert.ToInt32(e.Arguments[1]);
                        if (num4 > GetHelpPages())
                        {
                            e.OS.write("Invalid Page Number - Displaying First Page");
                            num4 = 0;
                        }
                    }
                    catch (FormatException)
                    {
                        e.OS.write("Invalid Page Number");
                    }
                    catch (OverflowException)
                    {
                        e.OS.write("Invalid Page Number");
                    }
                }
                e.OS.write(GetHelpString(num4));
                e.Disconnects = false;
            }
        }

        public static int GetHelpPages()
        {
            return Helpfile.getNumberOfPages() + (help.Count / Helpfile.ITEMS_PER_PAGE + 1);
        }

        public static string GetHelpString(int page = 0)
        {
            if (Helpfile.LoadedLanguage != Settings.ActiveLocale)
                Helpfile.init();
            if (page == 0) page = 1;
            int num = (page - 1) * Helpfile.ITEMS_PER_PAGE;
            if (num >= Helpfile.help.Count + help.Count)
                num = 0;
            var sb = new StringBuilder();
            sb.Append(Helpfile.prefix.Replace("[PAGENUM]", page.ToString()).Replace("[TOTALPAGES]", GetHelpPages().ToString()));
            int count = num;
            var enumer = help.GetEnumerator();
            while (count < Helpfile.help.Count && count < num + Helpfile.ITEMS_PER_PAGE) {
                if (count < Helpfile.help.Count)
                    sb.Append((count == 0 ? " " : "") + Helpfile.help[count] + "\n  \n ");
                else if (enumer.MoveNext())
                {
                    var c = enumer.Current;
                    sb.Append((count == 0 ? " " : "") + c.Key + "\n    " + c.Value + "\n  \n ");
                }
                count++;
            }
            return sb.Append("\n" + Helpfile.postfix).ToString();
        }
    }
}
