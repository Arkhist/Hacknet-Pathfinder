using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hacknet;

namespace Pathfinder.Util
{
    public static class Extensions
    {
        public static T GetFirstAttribute<T>(this MethodInfo info, bool inherit = false) where T : System.Attribute
            => info.GetCustomAttributes(inherit).Length > 0
                ? info.GetCustomAttributes(typeof(T), inherit)[0] as T
                : null;

        public static T GetFirstAttribute<T>(this Type type, bool inherit = false) where T : System.Attribute
            => type.GetCustomAttributes(inherit).Length > 0
                ? type.GetCustomAttributes(typeof(T), inherit)[0] as T
                : null;

        public static T GetFirstAttribute<T>(this FieldInfo info, bool inherit = false) where T : System.Attribute
            => info.GetCustomAttributes(inherit).Length > 0
                ? info.GetCustomAttributes(typeof(T), inherit)[0] as T
                : null;

        public static void ThrowNoLabyrinths(this string input, bool inputOnly = false)
        {
            if (DLC1SessionUpgrader.HasDLC1Installed)
                throw new NotSupportedException("Labyrinths DLC not found.\n"
                                                + (inputOnly
                                                   ? input
                                                   : input + " requires Hacknet Labyrinths to be installed."));
        }

        public static bool ToBool(this string input, bool defaultVal = false)
            => defaultVal ? input?.ToLower() != "false" && input != "0" : input?.ToLower() == "true" || input == "1";

        public static List<Tuple<string, bool>> SerialSplit(this string text, char selector, string enders = "\\w", char esc = '\\')
        {
            var tokens = new List<Tuple<string, bool>>();
            byte move = 0; // 0 normal, 1 group capture, 2 escape
            var frag = new StringBuilder(100);
            var checkForWhitespace = enders == "\\w";

            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (move)
                {
                    case 0:
                        if (c == selector)
                        {
                            if (text.IndexOf(selector, i + 1) == -1
                                // if second capture selector exists
                                || (checkForWhitespace
                                    && text.IndexOfAny(new char[] { ' ', '\t', '\n' }, i) < text.IndexOf(selector, i + 1))
                                // if whitespace
                                || (!checkForWhitespace
                                    && text.IndexOfAny(enders.ToCharArray(), i) < text.IndexOf(selector, i + 1))
                                // if voider of capture
                                || text.IndexOf(esc, i) == text.IndexOf(selector, i + 1) - 1)
                                // if capture is escaped
                                frag.Append(c); // continue seeking
                            else
                            {
                                // execute group capture
                                if (frag.Length != 0) tokens.Add(new Tuple<string, bool>(frag.ToString(), false));
                                frag.Clear();
                                move = 1;
                            }
                        }
                        else if (c == esc) move = 2; // escape character
                        else frag.Append(c); // continue seeking
                        break;
                    case 1:
                        if (c == selector)
                        {
                            // end group capture
                            if (frag.Length != 0) tokens.Add(new Tuple<string, bool>(frag.ToString(), true));
                            frag.Clear();
                            move = 0;
                        }
                        else frag.Append(c); // continue capture
                        break;
                    case 2:
                        // end escape
                        frag.Append(c);
                        move = 0;
                        break;
                }
            }
            if (frag.Length != 0)
                tokens.Add(new Tuple<string, bool>(frag.ToString(), false));
            return tokens;
        }
    }
}
