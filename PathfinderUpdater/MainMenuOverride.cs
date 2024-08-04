﻿using Hacknet;
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
    private static OptionCheckbox IsEnabledBox = new OptionCheckbox("Enabled", "Enables the auto updater", PathfinderUpdaterPlugin.IsEnabled.Value);
    private static OptionCheckbox IncludePrerelease = new OptionCheckbox("IncludePreReleases", "Autoupdate to pre-releases", PathfinderUpdaterPlugin.EnablePreReleases.Value);
    internal static OptionCheckbox NoRestartPrompt = new OptionCheckbox("NoRestartPrompt", "Prevents a restart prompt appearing");
    private static PFButton CheckForUpdate = new PFButton(10, 10, 160, 30, "Check For Update", new Color(255, 255, 87));
    private const string UPDATE_STRING = "Update";
    private static PFButton PerformUpdate = new PFButton(180, 10, 160, 30, UPDATE_STRING);
    private static RestartPopupScreen popupScreen;


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Program), nameof(Program.Main))]
    internal static void PFAPILoaded()
    {
        OptionsManager.AddOption("Updater", IsEnabledBox);
        OptionsManager.AddOption("Updater", IncludePrerelease);
        OptionsManager.AddOption("Updater", NoRestartPrompt);
        EventManager<CustomOptionsSaveEvent>.AddHandler(OptionsSaved);
        EventManager<DrawMainMenuEvent>.AddHandler(OnDrawMainMenu);
        CanPerformUpdate = PathfinderUpdaterPlugin.NeedsUpdate;
    }

    internal static void OptionsSaved(CustomOptionsSaveEvent args)
    {
        PathfinderUpdaterPlugin.IsEnabled.Value = IsEnabledBox.Value;
        PathfinderUpdaterPlugin.EnablePreReleases.Value = IncludePrerelease.Value;
        PathfinderUpdaterPlugin.NoRestartPrompt.Value = NoRestartPrompt.Value;
        if(popupScreen != null)
            popupScreen.NoRestartPrompt.Value = PathfinderUpdaterPlugin.NoRestartPrompt.Value;
    }

    internal static async Task PerformCheckAndUpdateButtonAsync(PFButton updateButton = null, PFButton checkButton = null)
    {
        if(updateButton == null) updateButton = PerformUpdate;
        if(checkButton == null) checkButton = CheckForUpdate;
        updateButton.Text = UPDATE_STRING;
        CanCheckForUpdate = false;
        var oldText = checkButton.Text;
        checkButton.Text = "Finding Latest Update...";
        CanPerformUpdate = PathfinderUpdaterPlugin.NeedsUpdate
            = (await PathfinderUpdaterPlugin.PerformCheckAsync(true)).Length > 0;
        checkButton.Text = oldText;
        CanCheckForUpdate = true;
        updateButton.Text += $" (v{PathfinderUpdaterPlugin.PathfinderUpdater.LatestVersion})";
    }

    private static async Task PerformUpdateAndUpdateButtonAsync(GameScreen menu, PFButton updateButton = null)
    {
        if(updateButton == null) updateButton = PerformUpdate;
        var couldCheckForUpdate = CanCheckForUpdate;
        CanCheckForUpdate = false;
        CanPerformUpdate = false;
        var oldText = updateButton.Text;
        updateButton.Text = "Currently Updating...";
        await PathfinderUpdaterPlugin.PerformUpdateAsync();
        updateButton.Text = oldText;
        if(!menu.ScreenManager.screens.Contains(popupScreen) && !PathfinderUpdaterPlugin.NoRestartPrompt.Value)
            menu.ScreenManager.AddScreen(popupScreen ??= new RestartPopupScreen());
        CanPerformUpdate = !menu.ScreenManager.screens.Contains(popupScreen);
        CanCheckForUpdate = couldCheckForUpdate;
    }

    private static bool CanCheckForUpdate { get; set; } = true;
    private static bool CanPerformUpdate { get; set; }
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