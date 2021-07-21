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
        [Util.Initialize]
        internal static void Initialize()
        {
            EventManager<SaveComputerLoadedEvent>.AddHandler(PopulateOnComputerLoad);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadComputer))]
        internal static void PopulateOnComputerCreation(object __result)
        {
            if (__result == null)
                return;
            ComputerLookup.PopulateLookups((Computer) __result);
        }

        internal static void PopulateOnComputerLoad(SaveComputerLoadedEvent args)
        {
            ComputerLookup.PopulateLookups(args.Comp);
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