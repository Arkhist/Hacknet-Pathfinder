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
        private static readonly string OPTION_TAG = "Pathfinder";
        private static ConfigFile configFile = null;

        internal static OptionCheckbox PreloadAllThemes = new OptionCheckbox("Preload All Themes",
            "Preload all themes at extension start\nimproves performance at the cost of memory");
        
        [Util.Initialize]
        static void Initialize()
        {
            OptionsManager.AddOption(OPTION_TAG, PreloadAllThemes);
            EventManager<CustomOptionsSaveEvent>.AddHandler(onOptionsSave);
            initConfig();
        }

        private static void initConfig()
        {
            configFile = new ConfigFile(BepInEx.Paths.ConfigPath + "/PathfinderAPI.cfg", false, null);
            var preloadAllThemesDef = configFile.Bind<bool>("PathfinderAPI", "PreloadAllThemes", false);
            PreloadAllThemes.Value = preloadAllThemesDef.Value;
        }

        private static void onOptionsSave(CustomOptionsSaveEvent _)
        {
            configFile.TryGetEntry<bool>("PathfinderAPI", "PreloadAllThemes", out var preloadAllThemesVal);
            preloadAllThemesVal.Value = PreloadAllThemes.Value;
            configFile.Save();
        }
    }
}
