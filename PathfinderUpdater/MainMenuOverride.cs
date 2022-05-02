using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Event.Menu;
using Pathfinder.GUI;
using Pathfinder.Options;

namespace PathfinderUpdater;

internal static class MainMenuOverride
{
    internal static PluginOptionTab UpdaterTab;

    internal static PluginCheckbox IsEnabledBox = new PluginCheckbox(
        "Enabled",
        "Enables the auto updater",
        true,
        "Enables or disables automatic updates for PathfinderAPI"
    );
    internal static PluginCheckbox IncludePrerelease = new PluginCheckbox(
        "Include Pre Releases",
        "Autoupdate to pre-releases",
        configDesc: "Whether or not to automatically update to beta versions"
    );
    internal static PluginCheckbox NoRestartPrompt = new PluginCheckbox(
        "No Restart Prompt",
        "Prevents a restart prompt from appearing",
        configDesc: "Whether ot not the restart prompt will appear when the update is finished"
    );

    private static PFButton CheckForUpdate = new PFButton(10, 10, 200, 30, "Check For Update", new Color(255, 255, 87));
    private const string UPDATE_STRING = "Update";
    private static PFButton PerformUpdate = new PFButton(220, 10, 170, 30, UPDATE_STRING);
    private static RestartPopupScreen popupScreen;

    internal static void PFAPILoaded()
    {
        UpdaterTab = OptionsManager.GetOrRegisterTab(PathfinderUpdaterPlugin.Instance, "Updater")
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
        if(!menu.ScreenManager.screens.Contains(popupScreen) && !NoRestartPrompt.Value)
            menu.ScreenManager.AddScreen(popupScreen ??= new RestartPopupScreen(PathfinderUpdaterPlugin.Config));
        CanPerformUpdate = !menu.ScreenManager.screens.Contains(popupScreen);
        CanCheckForUpdate = couldCheckForUpdate;
    }

    private static bool CanCheckForUpdate = true;
    private static bool CanPerformUpdate;
    internal static void OnDrawMainMenu(MainMenuEvent args)
    {
        if(!IsEnabledBox.Value) return;

        if(CheckForUpdate.Do() && CanCheckForUpdate)
            Task.Run(async () => await PerformCheckAndUpdateButtonAsync());

        if (!PathfinderUpdaterPlugin.NeedsUpdate)
            return;

        if(PerformUpdate.Do() && CanPerformUpdate)
            Task.Run(async () => await PerformUpdateAndUpdateButtonAsync(args.MainMenu));
    }
}