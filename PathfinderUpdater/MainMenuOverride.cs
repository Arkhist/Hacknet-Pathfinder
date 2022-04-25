using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Event.Menu;
using Pathfinder.Event.Options;
using Pathfinder.GUI;
using Pathfinder.Options;
using HarmonyLib;

namespace PathfinderUpdater;

[HarmonyPatch]
internal static class MainMenuOverride
{
    internal static PluginCheckbox IsEnabledBox = new PluginCheckbox(
        "Enabled",
        "Enables the auto updater",
        true,
        "Enables or disables automatic updates for PathfinderAPI"
    );
    internal static PluginCheckbox IncludePrerelease = new PluginCheckbox(
        "IncludePreReleases",
        "Autoupdate to pre-releases",
        configDesc: "Whether or not to automatically update to beta versions"
    );
    internal static PluginCheckbox NoRestartPrompt = new PluginCheckbox(
        "NoRestartPrompt",
        "Prevents a restart prompt appearing",
        configDesc: "Whether ot not the restart prompt will automatically appear when the update is finished"
    );

    private static PFButton CheckForUpdate = new PFButton(10, 10, 160, 30, "Check For Update", new Color(255, 255, 87));
    private const string UPDATE_STRING = "Update";
    private static PFButton PerformUpdate = new PFButton(180, 10, 160, 30, UPDATE_STRING);
    private static RestartPopupScreen popupScreen;


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Program), nameof(Program.Main))]
    internal static void PFAPILoaded()
    {
        OptionsManager.GetOrRegisterTab("Updater", "AutoUpdater")
        .AddOption(IsEnabledBox)
        .AddOption(IncludePrerelease)
        .AddOption(NoRestartPrompt);
        EventManager<DrawMainMenuEvent>.AddHandler(OnDrawMainMenu);
        CanPerformUpdate = PathfinderUpdaterPlugin.NeedsUpdate;
    }

    internal static async Task PerformCheckAndUpdateButtonAsync()
    {
        PerformUpdate.Text = UPDATE_STRING;
        CanCheckForUpdate = false;
        var oldText = CheckForUpdate.Text;
        CheckForUpdate.Text = "Finding Latest Update...";
        CanPerformUpdate = PathfinderUpdaterPlugin.NeedsUpdate
            = (await PathfinderUpdaterPlugin.PerformCheckAsync(true)).Length > 0;
        CheckForUpdate.Text = oldText;
        CanCheckForUpdate = true;
        PerformUpdate.Text += $" (v{PathfinderUpdaterPlugin.PathfinderUpdater.LatestVersion})";
    }

    private static async Task PerformUpdateAndUpdateButtonAsync(MainMenu menu)
    {
        var couldCheckForUpdate = CanCheckForUpdate;
        CanCheckForUpdate = false;
        CanPerformUpdate = false;
        var oldText = PerformUpdate.Text;
        PerformUpdate.Text = "Currently Updating...";
        await PathfinderUpdaterPlugin.PerformUpdateAsync();
        PerformUpdate.Text = oldText;
        if(!menu.ScreenManager.screens.Contains(popupScreen) && !PathfinderUpdaterPlugin.NoRestartPrompt.Value)
            menu.ScreenManager.AddScreen(popupScreen ??= new RestartPopupScreen());
        CanPerformUpdate = !menu.ScreenManager.screens.Contains(popupScreen);
        CanCheckForUpdate = couldCheckForUpdate;
    }

    private static bool CanCheckForUpdate = true;
    private static bool CanPerformUpdate;
    internal static void OnDrawMainMenu(MainMenuEvent args)
    {
        if(CheckForUpdate.Do() && CanCheckForUpdate)
            Task.Run(async () => await PerformCheckAndUpdateButtonAsync());

        if (!PathfinderUpdaterPlugin.NeedsUpdate)
            return;

        if(PerformUpdate.Do() && CanPerformUpdate)
            Task.Run(async () => await PerformUpdateAndUpdateButtonAsync(args.MainMenu));
    }
}