using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
public static class FixTutorialStartup
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AdvancedTutorial), nameof(AdvancedTutorial.Killed))]
    internal static bool AdvancedTutorialLoadStartingMission(AdvancedTutorial __instance)
    {
        if (!__instance.os.multiplayer && __instance.os.initShowsTutorial && Settings.IsInExtensionMode && __instance.os.currentMission is null)
            __instance.os.currentMission = (ActiveMission)ComputerLoader.readMission(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + ExtensionLoader.ActiveExtensionInfo.StartingMissionPath);
        return true;
    }
}
