using System.Collections.Generic;
using Hacknet;
using HarmonyLib;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance
{
    [HarmonyPatch]
    internal static class NodeLookup
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(List<Computer>), nameof(List<Computer>.Add))]
        [HarmonyPatch(typeof(List<Computer>), nameof(List<Computer>.Insert))]
        internal static void AddComputerReference(List<Computer> __instance, Computer item)
        {
            if (object.ReferenceEquals(__instance, OS.currentInstance?.netMap?.nodes))
            {
                ComputerLookup.PopulateLookups(item);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(List<Computer>), nameof(List<Computer>.AddRange))]
        internal static void AddComputerReferenceRange(List<Computer> __instance, IEnumerable<Computer> collection)
        {
            if (object.ReferenceEquals(__instance, OS.currentInstance?.netMap?.nodes))
            {
                foreach (var comp in collection)
                    ComputerLookup.PopulateLookups(comp);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.findComp))]
        internal static bool ModifyComputerLoaderLookup(out Computer __result, string target)
        {
            __result = ComputerLookup.FindById(target);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Programs), nameof(Programs.getComputer))]
        internal static bool ModifyProgramsLookup(out Computer __result, string ip_Or_ID_or_Name)
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