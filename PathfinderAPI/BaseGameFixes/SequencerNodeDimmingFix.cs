using Hacknet;

using HarmonyLib;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
public class SequencerNodeDimmingFix
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExtensionSequencerExe), "Killed")]
    static void FixSequencerNodeDimming(ExtensionSequencerExe __instance)
    {
        // This fixes a bug where nodes would continue to be dimmed after the sequencer is killed
        __instance.os.netMap.DimNonConnectedNodes = false;
    }
}