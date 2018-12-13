using System;
using Microsoft.Xna.Framework;
using Pathfinder.Util;

namespace Sax.Net
{
    public static class Extensions
    {
        public static string GetValue(this IAttributes a, string key, bool convert)
            => convert ? a?.GetValue(key)?.HacknetConvert() : a?.GetValue(key);

        public static string GetValueOrDefault(this IAttributes a, string key, string defaultVal = null, bool convert = false)
            => (convert ? a?.GetValue(key)?.HacknetConvert() : a?.GetValue(key)) ?? defaultVal;

        public static bool GetBool(this IAttributes a, string key, bool defaultVal = false)
            => a?.GetValue(key)?.ToBool(defaultVal) == true;

        public static byte GetByte(this IAttributes a, string key, byte defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToByte(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a byte value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static int GetInt(this IAttributes a, string key, int defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToInt32(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a integer value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static uint GetUInt(this IAttributes a, string key, uint defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToUInt32(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to an unsigned integer value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static int GetChar(this IAttributes a, string key, char defaultVal = default(char))
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToChar(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a char value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static short GetShort(this IAttributes a, string key, short defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToInt16(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a short value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static long GetLong(this IAttributes a, string key, long defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToInt64(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a long value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static ushort GetUShort(this IAttributes a, string key, ushort defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToUInt16(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to an unsigned short value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static ulong GetULong(this IAttributes a, string key, ulong defaultVal = 0)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToUInt64(a.GetValue(key)) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to an unsigned long value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static float GetFloat(this IAttributes a, string key, float defaultVal = 0f)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToSingle(a.GetValue(key).Replace(',', '.')) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a float value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static double GetDouble(this IAttributes a, string key, double defaultVal = 0d)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? Convert.ToDouble(a.GetValue(key).Replace(',', '.')) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a double value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static decimal GetDecimal(this IAttributes a, string key, decimal defaultVal = 0)
        {
            try
            {
                return a.Contains(key) ? Convert.ToDecimal(a.GetValue(key).Replace(',', '.')) : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a decimal value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static DateTime GetDateTime(this IAttributes a, string key,
                                           DateTime defaultVal = default(DateTime),
                                           IFormatProvider format = null)
        {
            if (a == null) return defaultVal;
            try
            {
                return a.Contains(key) ? (format == null
                                          ? Convert.ToDateTime(a.GetValue(key))
                                          : Convert.ToDateTime(a.GetValue(key), format))
                            : defaultVal;
            }
            catch (FormatException e)
            {
                Logger.Warn("Unable to convert {0} to a DateTime value.\nError: {1}", key, e);
                return defaultVal;
            }
            catch (Exception) { return defaultVal; }
        }

        public static Vector2 GetVector2(this IAttributes a, string xAttrib = "Origin", string yAttrib = null,
                                         Vector2 defaultVal = default(Vector2), bool capital = true)
        {
            if (a == null) return defaultVal;
            if (!string.IsNullOrWhiteSpace(xAttrib) && string.IsNullOrWhiteSpace(yAttrib))
            {
                yAttrib = xAttrib + (capital ? 'Y' : 'y');
                xAttrib += (capital ? 'X' : 'x');
            }

            return new Vector2(a.GetFloat(xAttrib, defaultVal.X), a.GetFloat(yAttrib, defaultVal.Y));
        }

        public static Vector3 GetVector3(this IAttributes a,
                                         string xAttrib = "Origin",
                                         string yAttrib = null,
                                         string zAttrib = null,
                                         Vector3 defaultVal = default(Vector3),
                                         bool capital = true)
        {
            if (a == null) return defaultVal;
            if (!string.IsNullOrWhiteSpace(xAttrib) && string.IsNullOrWhiteSpace(yAttrib) && string.IsNullOrWhiteSpace(zAttrib))
            {
                zAttrib = xAttrib + (capital ? 'Z' : 'z');
                yAttrib = xAttrib + (capital ? 'Y' : 'y');
                xAttrib += (capital ? 'X' : 'x');
            }

            return new Vector3(a.GetFloat(xAttrib, defaultVal.X), a.GetFloat(yAttrib, defaultVal.Y), a.GetFloat(zAttrib, defaultVal.Z));
        }

        public static Vector4 GetVector4(this IAttributes a,
                                         string xAttrib = "Origin",
                                         string yAttrib = null,
                                         string zAttrib = null,
                                         string wAttrib = null,
                                         Vector4 defaultVal = default(Vector4),
                                         bool capital = true)
        {
            if (a == null) return defaultVal;
            if (!string.IsNullOrWhiteSpace(xAttrib) && string.IsNullOrWhiteSpace(yAttrib)
                && string.IsNullOrWhiteSpace(zAttrib) && string.IsNullOrWhiteSpace(wAttrib))
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

        public static Color GetColor(this IAttributes a, string key, Color? defaultColor = null)
            => a != null ? Utility.GetColorFromString(a.GetValue(key), defaultColor)
                                      : defaultColor.HasValue ? defaultColor.Value : Color.White;

        public static Color? GetColor(this IAttributes a, string key, bool includeNull, Color? defaultColor = null)
            => a != null ? Utility.GetColorFromString(a.GetValue(key), includeNull, defaultColor) : defaultColor;

        public static bool Contains(this IAttributes atts, string name)
        {
            for (var i = 0; i < (atts?.Length ?? 0); i++)
                if (atts.GetQName(i) == name) return true;
            return false;
        }
    }
}
