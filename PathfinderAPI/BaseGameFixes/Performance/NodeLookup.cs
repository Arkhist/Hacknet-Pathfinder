using System;
using System.Collections.Generic;
using System.Threading;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Programs), nameof(Programs.scan))]
        public static bool ScanReplacement(string[] args, OS os)
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
            Computer computer2 = ((os.connectedComp != null) ? os.connectedComp : os.thisComputer);
            if (os.hasConnectionPermission(admin: true))
            {
                os.write("Scanning...");
                for (int i = 0; i < computer2.links.Count; i++)
                {
                    if (!os.netMap.visibleNodes.Contains(computer2.links[i]))
                    {
                        os.netMap.visibleNodes.Add(computer2.links[i]);
                    }
                    os.netMap.nodes[computer2.links[i]].highlightFlashTime = 1f;
                    os.write("Found Terminal : " + os.netMap.nodes[computer2.links[i]].name + "@" + os.netMap.nodes[computer2.links[i]].ip);
                    os.netMap.lastAddedNode = os.netMap.nodes[computer2.links[i]];
                    Thread.Sleep(400);
                }
                os.write("Scan Complete\n");
            }
            else
            {
                os.write("Scanning Requires Admin Access\n");
            }
			return false;
        }
    }
}
