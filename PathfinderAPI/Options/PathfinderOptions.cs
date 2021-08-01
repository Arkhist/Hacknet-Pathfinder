using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Hacknet;
using BepInEx.Configuration;
using BepInEx.Logging;
using Hacknet;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Gui;
using Hacknet.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pathfinder.Event;
using Pathfinder.Event.Options;

namespace Pathfinder.Options
{
    internal static class PathfinderOptions
    {
        private const string OPTION_TAG = "Pathfinder";

        internal static OptionCheckbox PreloadAllThemes = new OptionCheckbox("Preload All Themes",
            "Preload all themes at extension start\nimproves performance at the cost of memory");

        internal static OptionCheckbox DisableSteamCloudError = new OptionCheckbox("Disable Steam Cloud Message", 
            "Disables the Steam Cloud disabled message on the main menu");
        
        [Util.Initialize]
        static void Initialize()
        {
            OptionsManager.AddOption(OPTION_TAG, PreloadAllThemes);
            OptionsManager.AddOption(OPTION_TAG, DisableSteamCloudError);
            EventManager<CustomOptionsSaveEvent>.AddHandler(onOptionsSave);
            initConfig();
        }

        private static void initConfig()
        {
            var preloadAllThemesDef = PathfinderAPIPlugin.Config.Bind<bool>("PathfinderAPI", "PreloadAllThemes", false);
            var disableSteamCloudDef = PathfinderAPIPlugin.Config.Bind<bool>("PathfinderAPI", "DisableSteamCloudMessage", false);
            PreloadAllThemes.Value = preloadAllThemesDef.Value;
            DisableSteamCloudError.Value = disableSteamCloudDef.Value;
        }

        private static void onOptionsSave(CustomOptionsSaveEvent _)
        {
            PathfinderAPIPlugin.Config.TryGetEntry<bool>("PathfinderAPI", "PreloadAllThemes", out var preloadAllThemesVal);
            PathfinderAPIPlugin.Config.TryGetEntry<bool>("PathfinderAPI", "DisableSteamCloudMessage", out var disableCloudMessage);
            
            preloadAllThemesVal.Value = PreloadAllThemes.Value;
            disableCloudMessage.Value = DisableSteamCloudError.Value;
            PathfinderAPIPlugin.Config.Save();
        }
    }
}
