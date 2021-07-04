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
    }
}