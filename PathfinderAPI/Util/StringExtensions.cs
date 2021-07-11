using System;
using System.IO;
using Hacknet;
using Hacknet.Extensions;

namespace Pathfinder.Util
{
    public static class StringExtensions
    {
        public static bool HasContent(this string s) => !string.IsNullOrWhiteSpace(s);

        public static bool ContentFileExists(this string filename) => File.Exists(filename.ContentFilePath());

        public static string ContentFilePath(this string filename)
        {
            if (Settings.IsInExtensionMode)
                return ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + filename;
            return filename.StartsWith("Content/") ? filename : "Content/" + filename;
        }

        public static string Filter(this string s) => ComputerLoader.filter(s);
    }
}