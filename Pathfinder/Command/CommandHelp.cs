using System.Collections.Generic;
using System.Text;
using Hacknet;

namespace Pathfinder.Command
{
    public static class Help
    {
        internal static Dictionary<string, string> help = new Dictionary<string, string>();

        /// <summary>
        /// Gets the help page count.
        /// </summary>
        /// <value>The help page count.</value>
        public static int PageCount => HelpCount / Helpfile.ITEMS_PER_PAGE + 1;
        public static int HelpCount => Helpfile.help.Count + help.Count;

        private static string GenerateString(string cmd, string description) => cmd + "\n    " + description;
        public static string GenerateString(KeyValuePair<string, string> pair) => GenerateString(pair.Key, pair.Value);

        /// <summary>
        /// Gets the help string for a command.
        /// </summary>
        /// <returns>The resulting help string or <c>null</c> if it doesn't exist.</returns>
        /// <param name="cmd">The Command key.</param>
        public static string GetStringFor(string cmd)
        {
            if (!help.ContainsKey(cmd))
                return null;
            return GenerateString(cmd, help[cmd]);
        }

        /// <summary>
        /// Gets the help string for a page.
        /// </summary>
        /// <returns>The string for a page.</returns>
        /// <param name="page">The page to get the string for.</param>
        public static string GetPageString(int page = 0)
        {
            if (Helpfile.LoadedLanguage != Settings.ActiveLocale)
                Helpfile.init();
            if (page == 0) page = 1;
            int num = (page - 1) * Helpfile.ITEMS_PER_PAGE;
            if (num >= HelpCount)
                num = 0;
            var sb = new StringBuilder();
            sb.Append(Helpfile.prefix.Replace("[PAGENUM]", page.ToString()).Replace("[TOTALPAGES]", PageCount.ToString()));
            int position = num;
            var enumer = help.GetEnumerator();
            while (position < num + Helpfile.ITEMS_PER_PAGE)
            {
                if (position < Helpfile.help.Count)
                    sb.Append("-" + Helpfile.help[position] + "\n  \n");
                else if (enumer.MoveNext())
                    sb.Append("-" + GenerateString(enumer.Current) + "\n  \n");
                else break;
                position++;
            }
            return sb.Append("\n" + Helpfile.postfix).ToString();
        }
    }
}
