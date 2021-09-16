using System;
using System.Collections.Generic;
using System.Threading;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.Misc;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance
{
    [HarmonyPatch]
    internal static class NodeLookup
    {
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SAChangeIP), nameof(SAChangeIP.Trigger))]
        internal static void OnChangeIPTrigger(SAChangeIP __instance, out string __state)
        {
            __state = ComputerLookup.Find(__instance.TargetComp)?.ip;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SAChangeIP), nameof(SAChangeIP.Trigger))]
        internal static void OnChangeIPTrigger2(SAChangeIP __instance, string __state)
        {
            var comp = ComputerLookup.Find(__instance.TargetComp);
            if (comp != null && comp.ip != __state)
            {
                ComputerLookup.RebuildLookups();
            }
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ISPDaemon), nameof(ISPDaemon.DrawIPEntryScreen))]
        internal static void OnISPChangeIP(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            c.GotoNext(MoveType.After, x => x.MatchStfld(AccessTools.Field(typeof(Computer), nameof(Computer.ip))));

            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(ComputerLookup), nameof(ComputerLookup.RebuildLookups)));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AircraftDaemon), nameof(AircraftDaemon.CrashAircraft))]
        internal static void OnAircraftDaemonChangeIP()
        {
            ComputerLookup.RebuildLookups();
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ExtensionLoader), nameof(ExtensionLoader.LoadNewExtensionSession))]
        [HarmonyPatch(typeof(OS), nameof(OS.loadMissionNodes))]
        internal static void RebuildBeforePostLoad(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.AfterLabel, x => x.MatchLdsfld(typeof(ComputerLoader), nameof(ComputerLoader.postAllLoadedActions)));

            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(ComputerLookup), nameof(ComputerLookup.RebuildLookups)));
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SaveFixHacks), nameof(SaveFixHacks.FixSavesWithTerribleHacks))]
        internal static void RebuildBeforePostLoadDLC(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdsfld(typeof(ComputerLoader), nameof(ComputerLoader.postAllLoadedActions)),
                x => x.MatchCallOrCallvirt(typeof(System.Action), nameof(System.Action.Invoke))
            );

            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(ComputerLookup), nameof(ComputerLookup.RebuildLookups)));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NetworkMap), nameof(NetworkMap.generateGameNodes))]
        internal static void RebuildAfterNodeGen()
        {
            ComputerLookup.RebuildLookups();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
        internal static void RebuildOnEndLoad(OS __instance)
        {
            ComputerLookup.RebuildLookups();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.quitGame))]
        internal static void ClearOnQuitGame()
        {
            ComputerLookup.ClearLookups();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Programs), nameof(Programs.scan))]
        internal static bool ScanReplacement(string[] args, OS os)
        {
            if (args.Length > 1)
            {
                Computer computer = ComputerLookup.FindByIp(args[1]);
                if (computer == null)
                {
                    computer = ComputerLookup.FindByName(args[1]);
                }
                if (computer != null)
                {
                    os.netMap.discoverNode(computer);
                    os.write("Found Terminal : " + computer.name + "@" + computer.ip);
                }
                return false;
            }
            return true;
        }
    }
}
