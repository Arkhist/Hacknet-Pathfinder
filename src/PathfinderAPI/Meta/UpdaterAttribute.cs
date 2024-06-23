﻿namespace Pathfinder.Meta;

[AttributeUsage(AttributeTargets.Class)]
[Obsolete("PathfinderUpdater no longer exists in the same form, and does not update plugins.")]
public class UpdaterAttribute : Attribute
{
    public string GithubApiUrl { get; set; }
    public string AssetFileName { get; set; }
    public string ZipEntryPath { get; set; }
    public bool IncludePrerelease { get; set; }

    public UpdaterAttribute(string apiUrl, string assetName, string zipPath = null, bool includePrerlease = false)
    {
        GithubApiUrl = apiUrl;
        AssetFileName = assetName;
        ZipEntryPath = zipPath;
        IncludePrerelease = includePrerlease;
    }
}