using System.Configuration;
using System.Net;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Hacknet;
using Hacknet;
using HarmonyLib;
using Version = SemanticVersioning.Version;
using System.Diagnostics;
using System.Reflection;
using Pathfinder.Meta;

namespace PathfinderUpdater;

[BepInPlugin(ModGUID, ModName, HacknetChainloader.VERSION)]
[Updater(
    "https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases",
    "Pathfinder.Release.zip",
    "BepInEx/plugins/PathfinderUpdater.dll"
)]
[HarmonyPatch]
public class PathfinderUpdaterPlugin : HacknetPlugin
{
    public const string ModGUID = "com.Pathfinder.Updater";
    public const string ModName = "AutoUpdater";

    new internal static ConfigFile Config;

    internal static ConfigEntry<bool> IsEnabled;
    internal static ConfigEntry<bool> EnablePreReleases;
    internal static ConfigEntry<bool> NoRestartPrompt;
    internal static ConfigEntry<string> AcceptedUpdate;
    internal static ConfigEntry<string> CurrentVersion;

    public static Version VersionToRequest = null;
    public static bool NeedsUpdate;
    internal static Updater PathfinderUpdater;

    public static readonly List<Updater> Updaters = [];
    private static readonly List<string> UpdaterWhitelistGuids =
    [
        "com.Pathfinder.API",
        "com.Pathfinder.Updater"
    ];

    public override bool Load()
    {
        Config = base.Config;

        IsEnabled = Config.Bind<bool>("AutoUpdater", "EnableAutoUpdates", true, "Enables or disables automatic updates for PathfinderAPI");
        EnablePreReleases = Config.Bind<bool>("AutoUpdater", "UsePreReleases", false, "Whether or not to automatically update to beta versions");
        AcceptedUpdate = Config.Bind<string>("AutoUpdater", "LatestAcceptedUpdate", "", "Used internally to keep track of whether you accepted the update or not");
        CurrentVersion = Config.Bind<string>("AutoUpdater", "CurrentVersion", HacknetChainloader.VERSION,
            "Used internally to keep track of version.\nIf you want to skip updating to a version but keep the updater on, set this manually to the latest verison.");
        NoRestartPrompt = Config.Bind<bool>("AutoUpdater", "NoRestartPrompt", false, "Whether ot not the restart prompt will automatically appear when the update is finished");

        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        if (!IsEnabled.Value)
            return true;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        return true;
    }

    public override void PostLoad()
    {
        if(!IsEnabled.Value) return;
        if(!Updater.TryCreate(typeof(HacknetChainloader), out var chainLoaderUpdater, true))
            throw new InvalidOperationException("HacknetChainloader missing __UpdaterData");
        Updaters.Add(chainLoaderUpdater);
        chainLoaderUpdater.TryDeleteDllTempFile();
        chainLoaderUpdater.IncludePrerelease = EnablePreReleases.Value;
        chainLoaderUpdater.CurrentVersion = CurrentVersion.Value;

        foreach(var kvPair in HacknetChainloader.Instance.Plugins)
        {
            if(!UpdaterWhitelistGuids.Contains(kvPair.Key)) continue;
            var type = kvPair.Value.Instance.GetType();
            if(!Updater.TryCreate(type, out var updater))
                continue;
            Updaters.Add(updater);
            updater.TryDeleteDllTempFile();
            updater.IncludePrerelease = EnablePreReleases.Value;
            updater.CurrentVersion = CurrentVersion.Value;
            if(kvPair.Key == UpdaterWhitelistGuids[0])
                PathfinderUpdater = updater;
        }
        PerformUpdateCheck();
    }

    public static void RestartForUpdate()
    {
        AcceptedUpdate.Value = VersionToRequest.ToString();
        MusicManager.stop();
        Game1.threadsExiting = true;
        Game1.getSingleton().Exit();
        var cmdLine = Environment.CommandLine.Replace(Environment.GetCommandLineArgs()[0] + " ", "");
        Process.Start(new ProcessStartInfo
        {
            FileName = Assembly.GetExecutingAssembly().Location,
            Arguments = cmdLine
        });
        Environment.Exit(0);
    }

    public static async Task PerformUpdateAsync()
    {
        foreach(var updater in Updaters)
        {
            await updater.PerformDownload();
            updater.PerformUpdate();
            updater.TryDeleteDownloadedTempFile();
        }
    }

    public static void PerformUpdateCheck()
    {
        Task.Run(async() => await MainMenuOverride.PerformCheckAndUpdateButtonAsync());
    }

    public static async Task<bool[]> PerformCheckAsync(bool forceData = false)
    {
        var checks = new List<bool>(Updaters.Count);
        if(forceData)
            await Updater.ForceResetAllReleaseDataAsync();
        foreach(var updater in Updaters)
        {
            checks.Add(await updater.CheckForUpdate());
            VersionToRequest = updater.LatestVersion;
        }
        return checks.ToArray();
    }
}