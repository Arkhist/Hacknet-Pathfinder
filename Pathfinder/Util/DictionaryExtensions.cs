using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Pathfinder.Game;

namespace Pathfinder.Util
{
    public static class DictionaryExtensions
    {
        public static T2 GetValue<T1, T2>(this IDictionary<T1, T2> a, T1 key)
            where T2 : class
        {
            if (a == null) return default;
            a.TryGetValue(key, out var v);
            return v;
        }

        public static string GetValue<T1>(this IDictionary<T1, string> d, T1 key, bool convert = false)
            => convert ? d.GetValue(key)?.HacknetFilter() : d.GetValue(key);

        public static T2 GetValueOrDefault<T1, T2>(this IDictionary<T1, T2> a, T1 key, T2 defaultVal = default)
        {
            if (a == null) return defaultVal;
            return a.TryGetValue(key, out var v) ? v : defaultVal;
        }

        public static string GetValueOrDefault<T1>(this IDictionary<T1, string> a, T1 key, string defaultVal = default, bool convert = false)
            => convert ? a.GetValueOrDefault(key, defaultVal).HacknetFilter() : a.GetValueOrDefault(key, defaultVal);

        public static bool GetBool<T1>(this IDictionary<T1, string> a, T1 key, bool defaultVal = false)
            => a.GetValue(key)?.ToBool(defaultVal) == true;

        public static byte GetByte<T1>(this IDictionary<T1, string> a, T1 key, byte defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && byte.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static int GetInt<T1>(this IDictionary<T1, string> a, T1 key, int defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && int.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static uint GetUInt<T1>(this IDictionary<T1, string> a, T1 key, uint defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && uint.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static char GetChar<T1>(this IDictionary<T1, string> a, T1 key, char defaultVal = default)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && char.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static short GetShort<T1>(this IDictionary<T1, string> a, T1 key, short defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && short.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static long GetLong<T1>(this IDictionary<T1, string> a, T1 key, long defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && long.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static ushort GetUShort<T1>(this IDictionary<T1, string> a, T1 key, ushort defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && ushort.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static ulong GetULong<T1>(this IDictionary<T1, string> a, T1 key, ulong defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && ulong.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static float GetFloat<T1>(this IDictionary<T1, string> a, T1 key, float defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && float.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static double GetDouble<T1>(this IDictionary<T1, string> a, T1 key, double defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && double.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static decimal GetDecimal<T1>(this IDictionary<T1, string> a, T1 key, decimal defaultVal = 0)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) && decimal.TryParse(a.GetValue(key), out var b) ? b : defaultVal;
        }

        public static DateTime GetDateTime<T1>(this IDictionary<T1, string> a, T1 key,
                                           DateTime defaultVal = default,
                                           IFormatProvider format = null)
        {
            if (a == null) return defaultVal;
            return a.ContainsKey(key) 
                && ((format != null && DateTime.TryParse(a.GetValue(key), format, DateTimeStyles.None, out var d))
                || DateTime.TryParse(a.GetValue(key), out d))
                    ? d : defaultVal;
        }

        public static Vector2 GetVector2(this IDictionary<string, string> a,
            string xAttrib = "Origin",
            string yAttrib = null,
            Vector2 defaultVal = default,
            bool capital = true)
        {
            if (a == null || string.IsNullOrEmpty(xAttrib)) return defaultVal;
            if (string.IsNullOrEmpty(yAttrib))
            {
                yAttrib = xAttrib + (capital ? 'Y' : 'y');
                xAttrib += capital ? 'X' : 'x';
            }

            return new Vector2(a.GetFloat(xAttrib, defaultVal.X), a.GetFloat(yAttrib, defaultVal.Y));
        }

        public static Vector3 GetVector3(this IDictionary<string, string> a,
                                         string xAttrib = "Origin",
                                         string yAttrib = null,
                                         string zAttrib = null,
                                         Vector3 defaultVal = default,
                                         bool capital = true)
        {
            if (a == null || string.IsNullOrEmpty(xAttrib)) return defaultVal;
            if (string.IsNullOrEmpty(yAttrib) && !string.IsNullOrEmpty(zAttrib)
                || !string.IsNullOrEmpty(yAttrib) && string.IsNullOrEmpty(zAttrib))
                throw new InvalidOperationException($"{nameof(yAttrib)} and {nameof(zAttrib)} must both be null or non-null.");
            if (string.IsNullOrEmpty(yAttrib) && string.IsNullOrEmpty(zAttrib))
            {
                zAttrib = xAttrib + (capital ? 'Z' : 'z');
                yAttrib = xAttrib + (capital ? 'Y' : 'y');
                xAttrib += (capital ? 'X' : 'x');
            }
            

            return new Vector3(a.GetFloat(xAttrib, defaultVal.X), a.GetFloat(yAttrib, defaultVal.Y), a.GetFloat(zAttrib, defaultVal.Z));
        }

        public static Vector4 GetVector4(this IDictionary<string, string> a,
                                         string xAttrib = "Origin",
                                         string yAttrib = null,
                                         string zAttrib = null,
                                         string wAttrib = null,
                                         Vector4 defaultVal = default,
                                         bool capital = true)
        {
            if (a == null || string.IsNullOrEmpty(xAttrib)) return defaultVal;
            bool allNull = true;
            foreach (var at in new[] { yAttrib, zAttrib, wAttrib })
            {
                if (!allNull && string.IsNullOrEmpty(at) || allNull && !string.IsNullOrEmpty(at))
                    throw new InvalidOperationException($"{nameof(yAttrib)}, {nameof(zAttrib)}, and {nameof(wAttrib)} must all be null or non-null.");
                allNull &= string.IsNullOrEmpty(at);
            }
            if (string.IsNullOrEmpty(yAttrib) && string.IsNullOrEmpty(zAttrib) && string.IsNullOrEmpty(wAttrib))
            {
                wAttrib = xAttrib + (capital ? 'W' : 'w');
                zAttrib = xAttrib + (capital ? 'Z' : 'z');
                yAttrib = xAttrib + (capital ? 'Y' : 'y');
                xAttrib += (capital ? 'X' : 'x');
            }

            return new Vector4(a.GetFloat(xAttrib, defaultVal.X),
                               a.GetFloat(yAttrib, defaultVal.Y),
                               a.GetFloat(zAttrib, defaultVal.Z),
                               a.GetFloat(wAttrib, defaultVal.W));
        }

        public static Color GetColor<T1>(this IDictionary<T1, string> a, T1 key, Color? defaultColor = null)
            => a != null ? Utility.GetColorFromString(a.GetValue(key), defaultColor)
                                      : defaultColor ?? Color.White;

        public static Color? GetColor<T1>(this IDictionary<T1, string> a, T1 key, bool includeNull, Color? defaultColor = null)
            => a != null ? Utility.GetColorFromString(a.GetValue(key), includeNull, defaultColor) : defaultColor;
    }
}
