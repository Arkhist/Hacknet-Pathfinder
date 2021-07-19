using Hacknet;
using HarmonyLib;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance
{
    [HarmonyPatch]
    internal static class NodeLookup
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadComputer))]
        internal static void PopulateOnComputerCreation(ref object __result)
        {
            ComputerLookup.PopulateLookups((Computer) __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.load))]
        internal static void PopulateOnComputerLoad(ref Computer __result)
        {
            ComputerLookup.PopulateLookups(__result);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.findComp))]
        internal static bool ModifyComputerLoaderLookup(ref Computer __result, ref string target)
        {
            __result = ComputerLookup.FindById(target);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Programs), nameof(Programs.getComputer))]
        internal static bool ModifyProgramsLookup(ref Computer __result, ref string ip_Or_ID_or_Name)
        {
            __result = ComputerLookup.Find(ip_Or_ID_or_Name);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.quitGame))]
        internal static void ClearOnQuitGame()
        {
            ComputerLookup.ClearLookups();
        }
    }
}