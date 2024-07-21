using System.Text.Json.Serialization;

namespace PathfinderInstaller;

public class GitHubRelease
{
    public required string HtmlUrl { get; set; }
    public required string TagName { get; set; }
    public required bool Prerelease { get; set; }
    public required GitHubAsset[] Assets { get; set; }
}

public class GitHubAsset
{
    public required string Name { get; set; }
    public required string BrowserDownloadUrl { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease[]))]
public sealed partial class GitHubSerializerContext : JsonSerializerContext;
