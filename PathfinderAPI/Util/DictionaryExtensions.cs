using System.Collections.Generic;

namespace Pathfinder.Util
{
    public static class DictionaryExtensions
    {
        public static string GetString<Key>(this Dictionary<Key, string> dict, Key key, string defaultVal = "")
        {
            if (dict.TryGetValue(key, out var ret))
                return ret;
            return defaultVal;
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
    }
}