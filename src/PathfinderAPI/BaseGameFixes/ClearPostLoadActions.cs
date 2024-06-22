using Hacknet;
using HarmonyLib;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
internal static class ClearPostLoadActions
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
    internal static void ClearComputerPostLoadActionsPostfix()
    {
        ComputerLoader.postAllLoadedActions = null;
    }
}