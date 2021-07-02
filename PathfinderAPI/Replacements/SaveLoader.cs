using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hacknet;
using Hacknet.Localization;
using Hacknet.PlatformAPI.Storage;
using HarmonyLib;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class SaveLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.loadSaveFile))]
        internal static bool SaveLoadReplacementPrefix(ref OS __instance)
        {
            __instance.FirstTimeStartup = false;
            
            Stream saveStream = __instance.ForceLoadOverrideStream ?? SaveFileManager.GetSaveReadStream(__instance.SaveGameUserName);
            if (saveStream == null)
            {
                return false;
            }

            var os = __instance;
            
            var executor = new EventExecutor(new StreamReader(saveStream).ReadToEnd(), false);
            executor.RegisterExecutor("HacknetSave", (exec, info) =>
            {
                MissionGenerator.generationCount = info.Attributes.GetInt("generatedMissionCount", 100);
                os.username = os.defaultUser.name = info.Attributes.GetString("Username");
                os.LanguageCreatedIn = info.Attributes.GetString("Language", "en-us");
                os.IsInDLCMode = info.Attributes.GetBool("DLCMode") && Settings.EnableDLC;
                os.DisableEmailIcon = info.Attributes.GetBool("DisableMailIcon") && Settings.EnableDLC;

                if (os.LanguageCreatedIn != Settings.ActiveLocale)
                {
                    LocaleActivator.ActivateLocale(os.LanguageCreatedIn, os.content);
                    Settings.ActiveLocale = os.LanguageCreatedIn;
                }
            });
            executor.RegisterExecutor("HacknetSave.DLC", (exec, info) =>
            {
                os.IsDLCSave = true;

                os.IsInDLCMode = info.Attributes.GetBool("Active");
                os.HasLoadedDLCContent = info.Attributes.GetBool("LoadedContent");

                if (os.HasLoadedDLCContent && !DLC1SessionUpgrader.HasDLC1Installed)
                {
                    MainMenu.AccumErrors = "LOAD ERROR: Save " + os.SaveGameUserName + " is configured for Labyrinths DLC, but it is not installed on this computer.\n\n\n";
                    os.ExitScreen();
                    os.IsExiting = true;
                }
            });
            executor.RegisterExecutor("HackentSave.DLC.Flags", 
                (exec, info) => os.PreDLCFaction = info.Attributes.GetString("OriginalFaction"));
            executor.RegisterExecutor("Hacknet.DLC.OriginalVisibleNodes", 
                (exec, info) => os.PreDLCVisibleNodesCache = info.Content,
                ParseOption.ParseInterior);
            // todo: conditional actions loader
            executor.RegisterExecutor("HacknetSave.Flags", (exec, info) =>
            {
                os.Flags.Flags.Clear();

                foreach (var flag in info.Content.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    os.Flags.Flags.Add(flag
                        .Replace("[%%COMMAREPLACED%%]", ",")
                        .Replace("décrypté", "decypher"));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("HacknetSave.NetworkMap", (exec, info) =>
            {
                Enum.TryParse(info.Attributes.GetString("sort"), out os.netMap.SortingAlgorithm);
            });
            executor.RegisterExecutor("HacknetSave.NetworkMap", (exec, info) =>
            {
                foreach (var daemon in os.netMap.nodes.SelectMany(x => x.daemons))
                    daemon.loadInit();
                
                os.netMap.loadAssignGameNodes();
            }, ParseOption.FireOnEnd);
            
            executor.Parse();
            
            return false;
        }
    }
}
