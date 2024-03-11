using Hacknet;
using Hacknet.Extensions;

namespace Pathfinder.Util;

public static class PFStringExtensions
{
    public static bool HasContent(this string s) => !string.IsNullOrWhiteSpace(s);

    public static bool ContentFileExists(this string filename) => File.Exists(filename.ContentFilePath());

    public static string ContentFilePath(this string filename)
    {
        filename = filename.Replace("\\", "/");
        if (Settings.IsInExtensionMode)
        {
            var extFolder = ExtensionLoader.ActiveExtensionInfo.FolderPath.Replace("\\", "/");
            return filename.StartsWith(extFolder) ? filename : Path.Combine(extFolder, filename);
        }
        return filename.StartsWith("Content/") ? filename : Path.Combine("Content", filename);
    }

    public static string Filter(this string s) => s == null ? null : ComputerLoader.filter(s);
}