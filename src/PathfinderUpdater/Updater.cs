using System.Reflection;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using BepInEx.Hacknet;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;

using Version = SemanticVersioning.Version;
using Pathfinder.Meta;

namespace PathfinderUpdater;

public class Updater
{
    private static Dictionary<string, HttpResponseMessage> UrlToResponseCache
        = new Dictionary<string, HttpResponseMessage>();

    public Type PluginType { get; private set; }

    public static Updater Create<PluginT>(string githubApiUrl, string assetFile, string zipPath, bool includePrereleases = false)
        where PluginT : HacknetPlugin
    {
        return new Updater(typeof(PluginT), githubApiUrl, assetFile, zipPath, includePrereleases);
    }

    public delegate Task<Version> FindVersionAction();
    private FindVersionAction _FindVersion;
    public FindVersionAction FindVersion { set => _FindVersion = value; }

    public delegate Task<Stream> GetUpdateStreamAction(Version latest);
    private GetUpdateStreamAction _GetUpdateStream;
    public GetUpdateStreamAction GetUpdateStream { set => _GetUpdateStream = value; }

    public delegate Task<string> HandleStreamDownloadAction(Stream stream);
    private HandleStreamDownloadAction _HandleStreamDownload;
    public HandleStreamDownloadAction HandleStreamDownload { set => _HandleStreamDownload = value; }

    public string GithubApiUrl { get; set; }
    public string AssetFileName { get; set; }
    public string ZipEntryPath { get; set; }
    public bool IncludePrerelease { get; set; }
    private JToken _ReleaseData;

    public string CurrentVersion { get; set; }
    public Version LatestVersion { get; private set; }
    public string PathToUpdate { get; private set; }
    public string DownloadedTempPath { get; private set; }

    public ManualLogSource Log { get; set; }

    public Updater(
        Type pluginType,
        FindVersionAction findVersion,
        GetUpdateStreamAction getUpdateStream = null,
        HandleStreamDownloadAction handeStreamDownload = null
    )
    {
        ArgumentNullException.ThrowIfNull(pluginType);

        PluginType = pluginType;
        FindVersion = findVersion ?? FindVersionDefault;
        GetUpdateStream = getUpdateStream ?? GetUpdateStreamDefault;
        HandleStreamDownload = handeStreamDownload ?? HandleStreamDownloadDefault;
    }

    public Updater(
        Type pluginType,
        string githubApiUrl,
        string assetFileName,
        string zipEntryPath,
        bool includePrerelease = false) : this(pluginType, (FindVersionAction)null)
    {
        GithubApiUrl = githubApiUrl;
        AssetFileName = assetFileName;
        ZipEntryPath = zipEntryPath;
        IncludePrerelease = includePrerelease;
    }

    public Updater(Type pluginType) : this(pluginType, (FindVersionAction)null)
    {
        var attribute = pluginType.GetCustomAttribute<UpdaterAttribute>();
        if(attribute == null)
            throw new InvalidOperationException($"No UpdaterAttribute found on {pluginType.FullName}");

        GithubApiUrl = attribute.GithubApiUrl;
        AssetFileName = attribute.AssetFileName;
        ZipEntryPath = attribute.ZipEntryPath;
        IncludePrerelease = attribute.IncludePrerelease;
    }

    public static bool TryCreate(Type pluginType, out Updater updater, bool canSupportUpdaterData = false)
    {
        updater = null;
        if(canSupportUpdaterData)
        {
            var memberInfos = pluginType.GetMember("__UpdaterData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            Tuple<string, string, string, bool> data;
            if(memberInfos.Length > 0)
            {
                switch(memberInfos[0])
                {
                    case PropertyInfo prop:
                        if(!prop.CanRead) return false;
                        if(prop.PropertyType == typeof(Tuple<string, string, string>))
                        {
                            var shortTuple = (Tuple<string, string, string>)prop.GetValue(null);
                            data = new Tuple<string, string, string, bool>(shortTuple.Item1, shortTuple.Item2, shortTuple.Item3, false);
                        }
                        else if(prop.PropertyType == typeof(Tuple<string, string, string, bool>))
                            data = (Tuple<string, string, string, bool>)prop.GetValue(null);
                        else
                        {
                            return false;
                        }
                        break;
                    case FieldInfo field:
                        if(field.FieldType == typeof(Tuple<string, string, string>))
                        {
                            var shortTuple = (Tuple<string, string, string>)field.GetValue(null);
                            data = new Tuple<string, string, string, bool>(shortTuple.Item1, shortTuple.Item2, shortTuple.Item3, false);
                        }
                        else if(field.FieldType == typeof(Tuple<string, string, string, bool>))
                            data = (Tuple<string, string, string, bool>)field.GetValue(null);
                        else
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
                updater = new Updater(pluginType, data.Item1, data.Item2, data.Item3, data.Item4);
                return true;
            }
        }

        if(pluginType.GetCustomAttribute<UpdaterAttribute>() == null)
            return false;
        updater = new Updater(pluginType);
        return true;
    }

    public async Task<bool> CheckForUpdate()
    {
        if(_FindVersion == null)
            throw new InvalidOperationException("Missing FindVersion behavior");

        LatestVersion = await _FindVersion.Invoke();
        if(LatestVersion == null)
            return false;

        if(CurrentVersion == null)
        {
            Log.LogError($"CurrentVersion for {PluginType.FullName} is null");
            return true;
        }

        if (Version.Parse(CurrentVersion) == LatestVersion)
            return false;

        return true;
    }

    public async Task PerformDownload()
    {
        if(_GetUpdateStream == null)
            throw new InvalidOperationException("Missing GetUpdateStream behavior");

        if(_HandleStreamDownload == null)
            throw new InvalidOperationException("Mission HandleStreamDownload behavior");

        var stream = await _GetUpdateStream(LatestVersion);
        PathToUpdate = await _HandleStreamDownload(stream);
        CurrentVersion = LatestVersion.ToString();
    }

    public void PerformUpdate()
    {
        var origPath = PluginType.Assembly.Location;
        File.Delete(origPath+".bak");
        File.Move(origPath, origPath+".bak");
        File.Move(PathToUpdate, origPath);
    }

    public bool TryDeleteDownloadedTempFile()
    {
        if(PathToUpdate == null)
            return false;
        File.Delete(PathToUpdate);
        return !File.Exists(PathToUpdate);
    }

    public bool TryDeleteDllTempFile()
    {
        var executionTempPath = PluginType.Assembly.Location + ".bak";
        if(File.Exists(executionTempPath))
            File.Delete(executionTempPath);
        return !File.Exists(executionTempPath);
    }

    public HttpResponseMessage ForceResetReleaseData()
    {
        var task = Task.Run(async () => await ForceResetReleaseDataAsync());
        task.Wait();
        return task.Result;
    }

    public async Task<HttpResponseMessage> ForceResetReleaseDataAsync()
    {
        using(var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("a", "1"));
            return UrlToResponseCache[GithubApiUrl] = await client.GetAsync(GithubApiUrl);
        }
    }

    public async Task<Version> FindVersionDefault()
    {
        JArray releasesList;
        try
        {
            HttpResponseMessage response;
            if(!UrlToResponseCache.TryGetValue(GithubApiUrl, out response))
                UrlToResponseCache[GithubApiUrl] = response = await ForceResetReleaseDataAsync();
            var releaseDataContent = await response.Content.ReadAsStringAsync();
            releasesList = JArray.Parse(releaseDataContent);

        }
        catch (Exception e)
        {
            Log?.Log(LogLevel.Error, e);
            return null;
        }

        Version latestVersion = null;
        foreach (var possibleRelease in releasesList)
        {
            var possibleTag = Version.Parse(possibleRelease.Value<string>("tag_name").Substring(1));
            if (possibleTag.PreRelease != null && !IncludePrerelease)
                continue;

            latestVersion = possibleTag;
            _ReleaseData = possibleRelease;
            break;
        }
        return latestVersion;
    }

    public async Task<Stream> GetUpdateStreamDefault(Version latest)
    {
        using(var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("a", "1"));
            var releaseUrlData = await client.GetAsync(
                _ReleaseData["assets"]
                    .First(x => x.Value<string>("name") == AssetFileName)
                    .Value<string>("browser_download_url")
            );
            return await releaseUrlData.Content.ReadAsStreamAsync();
        }
    }

    public async Task<string> HandleStreamDownloadDefault(Stream stream)
    {
        var assemblyFileName = PluginType.Assembly.Location;
        var tempFileName = assemblyFileName+".temp";
        using(var archive = new ZipArchive(stream))
        using(var tempFile = File.OpenWrite(tempFileName))
        {
            var entryStream = archive.GetEntry(ZipEntryPath).Open();
            await entryStream.CopyToAsync(tempFile);
            await tempFile.FlushAsync();
            return tempFileName;
        }
    }

    public static void ForceResetAllReleaseData()
    {
        Task.Run(async () => await ForceResetAllReleaseDataAsync());
    }

    public static async Task ForceResetAllReleaseDataAsync()
    {
        var keys = UrlToResponseCache.Keys.ToArray();
        using(var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("a", "1"));
            foreach(var data in keys)
                UrlToResponseCache[data] = await client.GetAsync(data);
        }
    }
}