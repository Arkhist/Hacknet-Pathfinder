using System;

namespace Pathfinder.Util
{
    public static class StringExtensions
    {
        public static bool HasContent(this string s) => !string.IsNullOrWhiteSpace(s);
    }
}