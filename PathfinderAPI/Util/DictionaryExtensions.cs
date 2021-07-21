using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Util
{
    public static class DictionaryExtensions
    {
        public static string GetString<Key>(this Dictionary<Key, string> dict, Key key, string defaultVal = "")
        {
            return dict.TryGetValue(key, out var ret) ? ret : defaultVal;
        }

        public static string GetOrThrow<Key>(this Dictionary<Key, string> dict, Key key, string exMessage, Predicate<string> validator = null)
        {
            if (dict.TryGetValue(key, out var ret))
            {
                if (validator != null && !validator(ret))
                    throw new ArgumentException(exMessage);
                return ret;
            }

            throw new KeyNotFoundException(exMessage);
        }

        public static string GetAnyOrThrow<Key>(this Dictionary<Key, string> dict, Key[] keys, string exMessage, Predicate<string> validator = null)
        {
            foreach (var key in keys)
            {
                if (dict.TryGetValue(key, out var ret))
                {
                    if (validator != null && !validator(ret))
                        continue;
                    return ret;
                }
            }
            
            throw new KeyNotFoundException(exMessage);
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) =>
            dict.TryGetValue(key, out var value) ? value : default;

        public static int GetInt<Key>(this Dictionary<Key, string> dict, Key key, int defaultVal = 0)
        {
            if (dict.TryGetValue(key, out var val) && int.TryParse(val, out var ret))
                return ret;
            return defaultVal;
        }
        
        public static bool GetBool<Key>(this Dictionary<Key, string> dict, Key key, bool defaultVal = false)
        {
            if (dict.TryGetValue(key, out var val) && bool.TryParse(val, out var ret))
                return ret;
            return defaultVal;
        }

        public static float GetFloat<Key>(this Dictionary<Key, string> dict, Key key, float defaultVal = 0f)
        {
            if (dict.TryGetValue(key, out var val) && float.TryParse(val, out var ret))
                return ret;
            return defaultVal;
        }
        
        public static byte GetByte<Key>(this Dictionary<Key, string> dict, Key key, byte defaultVal = 0)
        {
            if (dict.TryGetValue(key, out var val) && byte.TryParse(val, out var ret))
                return ret;
            return defaultVal;
        }

        public static Vector2? GetVector<Key>(this Dictionary<Key, string> dict, Key x, Key y, Vector2? defaultVal = null)
        {
            if (dict.TryGetValue(x, out var x_string) && dict.TryGetValue(y, out var y_string) &&
                float.TryParse(x_string, out var x_float) && float.TryParse(y_string, out var y_float))
                return new Vector2(x_float, y_float);
            return defaultVal;
        }

        public static Color? GetColor<Key>(this Dictionary<Key, string> dict, Key key, Color? defaultVal = null)
        {
            if (dict.TryGetValue(key, out var color))
                return Utils.convertStringToColor(color);
            return defaultVal;
        }

        public static Computer GetComp<Key>(this Dictionary<Key, string> dict, Key key, SearchType searchType = SearchType.Any, string exMessage = null)
        {
            if (dict.TryGetValue(key, out var compString))
                return ComputerLookup.Find(compString.Filter(), searchType) ?? throw new NullReferenceException(exMessage ?? $"Could not find computer {compString} for attribute {key.ToString()}");
            throw new KeyNotFoundException(exMessage ?? $"Could not find attribute {key.ToString()}");
        }
    }
}