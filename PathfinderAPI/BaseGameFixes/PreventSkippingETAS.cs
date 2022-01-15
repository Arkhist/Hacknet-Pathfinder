
using System;
using Hacknet;
using Hacknet.Effects;
using HarmonyLib;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class PreventSkippingETAS
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Programs), nameof(Programs.reboot))]
        internal static bool RebootPrefix(string[] args, OS os)
        {
            if (os.TraceDangerSequence.IsActive && (os.connectedComp == null || os.connectedComp == os.thisComputer))
            {
                os.write("REBOOT ERROR: OS reports critical action already in progress.");
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.thisComputerCrashed))]
        [HarmonyPatch(typeof(OS), nameof(OS.rebootThisComputer))]
        internal static void ETASGameover(OS __instance)
        {
            if (__instance.TraceDangerSequence.IsActive)
            {
                __instance.TraceDangerSequence.timeThisState = 0f;
                __instance.TraceDangerSequence.state = TraceDangerSequence.TraceDangerState.Gameover;
                __instance.TraceDangerSequence.CancelTraceDangerSequence();
                Game1.getSingleton().Exit();
            }
        }
    }
}
