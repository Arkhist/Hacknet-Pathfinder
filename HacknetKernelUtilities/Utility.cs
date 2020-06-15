using System;
using System.Collections.Generic;
using System.Linq;

namespace KernelUtilities
{
    public static class Utility
    {
        public enum Signal
        {
            TERM = 15
        }

        public static readonly IReadOnlyList<Signal> SignalValues = GetValues<Signal>();
        public static readonly IReadOnlyList<Signal?> NullableSignalValues = (IReadOnlyList<Signal?>)SignalValues.Cast<Signal?>().Concat(null);

        public static Signal CurrentSignal { get; set; }
        public static void SetCurrentSignal(int value)
        {
            CurrentSignal = SignalValues.FirstOrDefault(s => (int)s == value);
        }

        public static Signal GetSignalByName(string name, bool ignoreCase = false) =>
            SignalValues.FirstOrDefault(s => ignoreCase
                                        ? name.ToLowerInvariant() == s.ToString().ToLowerInvariant()
                                        : name == s.ToString());

        public static Signal GetSignal(string nameOrNum, bool ignoreCase = false) =>
            SignalValues.FirstOrDefault(s => (ignoreCase
                                        ? nameOrNum.ToLowerInvariant() == s.ToString().ToLowerInvariant()
                                        : nameOrNum == s.ToString()) || nameOrNum == ((int)s).ToString());
        public static Signal? GetSignal(string nameOrNum, bool isNullable, bool ignoreCase = false)
        {
            if (isNullable)
                return NullableSignalValues.FirstOrDefault(s => (ignoreCase
                                                          ? nameOrNum.ToLowerInvariant() == s.ToString().ToLowerInvariant()
                                                          : nameOrNum == s.ToString()) || nameOrNum == ((int)s).ToString());
            return SignalValues.FirstOrDefault(s => (ignoreCase
                                                     ? nameOrNum.ToLowerInvariant() == s.ToString().ToLowerInvariant()
                                                     : nameOrNum == s.ToString()) || nameOrNum == ((int)s).ToString());
        }
        public static Signal GetSignal(int signal) => GetSignal(signal.ToString());


        public static Dictionary<string, string> ParseArguments(List<string> input, out List<string> args, string[] prefixes = null)
        {
            var result = new Dictionary<string, string>();
            prefixes = prefixes ?? new string[] { "-", "--" };
            for (var i = 0; i < input.Count; i++)
            {
                var prefix = prefixes.First(input[i].StartsWith);
                if (string.IsNullOrWhiteSpace(prefix)) continue;
                var argName = input[i].Remove(0, prefix.Length).Trim();
                var equalIndex = input[i].IndexOf('=');
                var nextNotStarts = input.Count > i + 1 && !prefixes.Any(input[i + 1].StartsWith);
                if (equalIndex != -1)
                    argName = argName.Remove(equalIndex);
                if (input[i].Length > equalIndex + 1)
                {
                    result.Add(argName, input[i].Substring(equalIndex + 1).Trim());
                    input[i] = null;
                }
                else if (equalIndex != -1 && nextNotStarts)
                {
                    result.Add(argName, input[i + 1].Trim());
                    input[i] = null;
                    input[i + 1] = null;
                }
                else if (nextNotStarts)
                {
                    result.Add(argName, input[i + 1].Trim());
                    input[i] = null;
                    input[i + 1] = null;
                }
                else
                {
                    result.Add(argName, string.Empty);
                    input[i] = null;
                }
            }
            input.RemoveAll(s => s == null);
            args = input;
            return result;
        }

        public static bool In<T>(this T source, params T[] list)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return list.Contains(source);
        }

        public static IReadOnlyList<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>().ToList().AsReadOnly();

        public static int IndexOf<T1>(this ICollection<T1> source, T1 value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = 0;
            if (source.FirstOrDefault(a => { result++; return value.Equals(a); }).GetHashCode() != value.GetHashCode()) return -1;
            return result;
        }

        public static T2 FirstOrDefault<T1, T2>(this IDictionary<T1, T2> source, T1 key) where T1 : IEquatable<T1>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.FirstOrDefault(a => a.Key.Equals(key)).Value;
        }

        public static KeyValuePair<T1, T2> PairOfFirstKey<T1, T2>(this Dictionary<T1, T2> source, params T1[] list)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.FirstOrDefault(p => list.Contains(p.Key));
        }

        public static bool KeysIn<T1, T2>(this Dictionary<T1, T2> source, params T1[] list)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Keys.Any(list.Contains);
        }

        public static bool ValuesIn<T1, T2>(this Dictionary<T1, T2> source, params T2[] list)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Values.Any(list.Contains);
        }
    }
}
