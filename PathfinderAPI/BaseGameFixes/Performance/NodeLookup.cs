using System;
using System.Collections.Generic;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
                ComputerLookup.NotifyIPChange(__state, comp.ip);
            }
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ISPDaemon), nameof(ISPDaemon.DrawIPEntryScreen))]
        internal static void OnISPChangeIP(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(NetworkMap), nameof(NetworkMap.generateRandomIP))));

            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ISPDaemon), nameof(ISPDaemon.scannedComputer)));
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(Computer), nameof(Computer.ip)));
            c.EmitDelegate<Action<string, string>>((newIp, oldIp) =>
            {
                if (ComputerLookup.FindByIp(newIp) == null)
                    ComputerLookup.NotifyIPChange(oldIp, newIp);
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.quitGame))]
        internal static void ClearOnQuitGame()
        {
            ComputerLookup.ClearLookups();
        }
    }
}