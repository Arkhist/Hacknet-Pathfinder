using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hacknet;

namespace Pathfinder.Util
{
    public static class Locale
    {
        static Regex regex = new Regex(@"""""([^""\\]*(?:\\.[^""\\]*)*)""=""([^""\\]*(?:\\.[^""\\]*)*)""""");
        static Regex secondRegex = new Regex(@"""([^""\\]*(?:\\.[^""\\]*)*)=""([^""\\]*(?:\\.[^""\\]*)*)""""");
        static Dictionary<string, string> StoredLocale = new Dictionary<string, string>();

        /// <summary>
        /// Retrieves the localized formatted (based on the extra arguments) string of the input along.
        /// </summary>
        /// <returns>The localized formatted string of the input, otherwise defaulting to LocaleTerms.Loc.</returns>
        /// <param name="input">The input string to search in the locale for.</param>
        /// <param name="extraArgs">Any extra formatting arguments.</param>
        public static string Get(string input, params object[] extraArgs)
        {
            if (StoredLocale.ContainsKey(input))
            {
                var result = StoredLocale[input];
                if (extraArgs != null && extraArgs.Length > 0)
                    try
                    {
                        return string.Format(result, extraArgs);
                    }
                    catch (FormatException) {}
                return result;

            }
            return LocaleTerms.Loc(input);
        }

        internal static void ParseInput(string input)
        {
            foreach (var i in input.Split('\n'))
            {
                var match = secondRegex.Match(input);
                // if input line is <input>="<input>"
                if (match.Success)
                    StoredLocale.Add(match.Captures[0].Value.Replace("\\\"","\"").Replace("\\\\", "\\"),
                                     match.Captures[1].Value.Replace("\\\"","\"").Replace("\\\\", "\\"));
                // if input line is "<input>"="<input>"
                if ((match = regex.Match(input)).Success)
                    StoredLocale.Add(match.Captures[0].Value.Replace("\\\"","\"").Replace("\\\\", "\\"),
                                     match.Captures[1].Value.Replace("\\\"","\"").Replace("\\\\", "\\"));
                // the Replace methods strip the extra escape characters from the output
                // otherwise split using '='
                else
                {
                    var sarr = i.Split('=');
                    StoredLocale.Add(sarr[0], sarr[1]);
                }
                // in any other case the input is invalid
            }
        }

        internal static void Reset() => StoredLocale.Clear();
    }
}