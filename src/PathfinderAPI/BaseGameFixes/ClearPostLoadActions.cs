using Hacknet;
using HarmonyLib;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
internal class ClearPostLoadActions
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
    internal static void ClearComputerPostLoadActionsPostfix()
    {
        ComputerLoader.postAllLoadedActions = null;
    }
}