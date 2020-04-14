using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hacknet;

namespace Pathfinder.Util
{
    public static class Extensions
    {
        public static bool HasAttribute<InfoT, T>(this InfoT info, bool inherit = false)
            where InfoT : MemberInfo
            where T : System.Attribute
        {
            var attribs = info.GetCustomAttributes(typeof(T), inherit);
            return attribs.Length > 0 && attribs[0] != null;
        }

        public static bool HasAttribute<T>(this MethodInfo info, bool inherit = false)
            where T : System.Attribute
            => info.HasAttribute<MethodInfo, T>(inherit);

        public static bool HasAttribute<T>(this FieldInfo info, bool inherit = false)
            where T : System.Attribute
            => info.HasAttribute<FieldInfo, T>(inherit);

        public static bool HasAttribute<T>(this Type info, bool inherit = false)
            where T : System.Attribute
            => info.HasAttribute<Type, T>(inherit);

        public static T GetFirstAttribute<InfoT, T>(this InfoT info, bool inherit = false)
            where InfoT : MemberInfo
            where T : System.Attribute
        {
            var attribs = info.GetCustomAttributes(typeof(T), inherit);
            return attribs.Length > 0 ? attribs[0] as T : null;
        }

        public static T GetFirstAttribute<T>(this MethodInfo info, bool inherit = false)
            where T : System.Attribute
            => info.GetFirstAttribute<MethodInfo, T>(inherit);

        public static T GetFirstAttribute<T>(this FieldInfo info, bool inherit = false)
            where T : System.Attribute
            => info.GetFirstAttribute<FieldInfo, T>(inherit);

        public static T GetFirstAttribute<T>(this Type info, bool inherit = false)
            where T : System.Attribute
            => info.GetFirstAttribute<Type, T>(inherit);

        public static bool DLCchecked = false;
        public static void ThrowNoLabyrinths(this string input, bool inputOnly = false)
        {
            if (!CheckLabyrinths())
                throw new NotSupportedException("Labyrinths DLC not found.\n"
                                                + (inputOnly
                                                   ? input
                                                   : input + " requires Hacknet Labyrinths to be installed."));
        }
        public static bool CheckLabyrinths()
        {
            if (!DLCchecked)
            {
                DLC1SessionUpgrader.CheckForDLCFiles();
                DLCchecked = true;
            }
            return DLC1SessionUpgrader.HasDLC1Installed;
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
                            var nextEnder = checkForWhitespace ? 
                                text.IndexOfAny(new char[] { ' ', '\t', '\n' }, i)
                                : text.IndexOfAny(enders.ToCharArray(), i);
                            var nextSelector = text.IndexOf(selector, i + 1);
                            if (text.IndexOf(selector, i + 1) == -1
                                // if second capture selector exists
                                || (nextEnder > -1
                                    && nextEnder < nextSelector)
                                // if voider of capture
                                || text.IndexOf(esc, i) == nextSelector - 1)
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

        public static TDeg CreateDelegate<TDeg>(this MethodInfo info, object instance = null)
            where TDeg : Delegate
        {
            if (instance != null)
                return (TDeg)Delegate.CreateDelegate(typeof(TDeg), instance, info);
            return (TDeg)Delegate.CreateDelegate(typeof(TDeg), info);
        }

        public static Delegate CreateDelegate(this MethodInfo info, Type TDeg, object instance = null)
        {
            if (instance != null)
                return Delegate.CreateDelegate(TDeg, instance, info);
            return Delegate.CreateDelegate(TDeg, info);
        }

        public static string RemoveExtended(this string str, int? startInd = null, int count = -1)
        {
            if (startInd == null
                || startInd + (count > 0 ? count : 0) >= str.Length
                || startInd - (count > 0 ? count : 0) <= -str.Length)
                return str;
            if (startInd < 0)
                startInd = str.Length + startInd.Value;
            if (count == -1)
                return str.Remove(startInd.Value);
            return str.Remove(startInd.Value, count);
        }

        public static string RemoveAll(this string str, string toRemove) => str.Replace(toRemove, string.Empty);

        public static string RemoveFirst(this string str, string toRemove) 
            => str.RemoveExtended(str.IndexOf(toRemove, StringComparison.Ordinal), toRemove.Length);

        public static string RemoveLast(this string str, string toRemove)
            => str.RemoveExtended(str.LastIndexOf(toRemove, StringComparison.Ordinal), toRemove.Length);
    }
}
