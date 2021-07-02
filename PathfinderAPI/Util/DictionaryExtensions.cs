using System.Collections.Generic;

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
    }
}