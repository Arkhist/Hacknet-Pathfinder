using Hacknet;
using Hacknet.Extensions;
using Hacknet.Misc;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance;

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

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ProgramRunner), nameof(ProgramRunner.ExecuteProgram))]
    internal static void RebuildInDebugCommand(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("practiceServer"),
            x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(NetworkMap), nameof(NetworkMap.discoverNode), [typeof(string)]))
        );

        c.Emit(OpCodes.Ldnull);
        c.Emit(OpCodes.Call, AccessTools.Method(typeof(ComputerLookup), nameof(ComputerLookup.RebuildLookups)));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MissionGenerator), nameof(MissionGenerator.generateComputer))]
    private static void AddMissionGenComputer(object __result)
    {
        var comp = (Computer)__result;
        comp.idName = "Gen" + MissionGenerator.generationCount;
        ComputerLookup.Add(comp);
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ISPDaemon), nameof(ISPDaemon.DrawIPEntryScreen))]
    internal static void ModifyISPDaemonDrawIPEntryScreen(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel loopHead = null;
        c.GotoNext(MoveType.Before,
            // flag = false;
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(2),
            // for (int i = 0; i < os.netMap.nodes.Count; i++)
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(3),
            // (no C# code)
            x => x.MatchBr(out loopHead)
        );
        int i = c.Index;

        c.GotoLabel(loopHead);
        c.GotoNext(MoveType.After,
            // for (int i = 0; i < os.netMap.nodes.Count; i++)
            // ...
            // for (int i = 0; i < os.netMap.nodes.Count; i++)
            // ...
            x => x.MatchBrtrue(out _)
        );
        int j = c.Index;

        c.Index = i;
        c.RemoveRange(j-i);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<ISPDaemon, bool>>(__instance => {
            Computer c = ComputerLookup.FindByIp(__instance.scannedComputer.ip, false);

            return (c != null) && (c.idName != __instance.scannedComputer.idName);
        });
        c.Emit(OpCodes.Stloc_2);

        c.Emit(OpCodes.Ldnull);
        c.Emit(OpCodes.Call, AccessTools.Method(typeof(ComputerLookup), nameof(ComputerLookup.RebuildLookups)));
    }
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ISPDaemon), nameof(ISPDaemon.DrawLoadingScreen))]
    internal static void ModifyISPDaemonDrawLoadingScreen(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel loopHead = null;
        c.GotoNext(MoveType.Before,
            // scannedComputer = null;
            x => x.MatchNop(),
            x => x.MatchLdarg(0),
            x => x.MatchLdnull(),
            x => x.MatchStfld(AccessTools.Field(typeof(ISPDaemon), nameof(ISPDaemon.scannedComputer))),
            // for (int i = 0; i < os.netMap.nodes.Count; i++)
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(1),
            // (no C# code)
            x => x.MatchBr(out loopHead)
        );
        int i = ++c.Index;

        c.GotoLabel(loopHead);
        c.GotoNext(MoveType.After,
            // for (int i = 0; i < os.netMap.nodes.Count; i++)
            // ...
            // for (int i = 0; i < os.netMap.nodes.Count; i++)
            // ...
            x => x.MatchBrtrue(out ILLabel _)
        );
        int j = c.Index;

        c.Index = i;
        c.RemoveRange(j-i);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<ISPDaemon>>(__instance =>
            __instance.scannedComputer = ComputerLookup.FindByIp(__instance.ipSearch, false));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MissionFunctions), nameof(MissionFunctions.findComp))]
    internal static bool ModifyMissionFunctionsFindComp(string target, ref Computer __result)
    {
        __result = ComputerLookup.FindById(target);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Multiplayer), nameof(Multiplayer.getComp))]
    internal static bool ModifyMultiplayerGetComp(string ip, OS os, ref Computer __result)
    {
        __result = ComputerLookup.FindByIp(ip, false);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetworkMap), nameof(NetworkMap.discoverNode), [typeof(string)])]
    internal static bool ModifyNetworkMapDiscoverNodeString(NetworkMap __instance, string cName)
    {
        Computer c = ComputerLookup.FindById(cName);
        if (c != null)
            __instance.discoverNode(c);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Programs), nameof(Programs.computerExists))]
    internal static bool ModifyProgramsComputerExists(OS os, string ip, ref bool __result)
    {
        __result = ComputerLookup.Find(ip, SearchType.Ip | SearchType.Name) != null;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Programs), nameof(Programs.scan))]
    internal static bool ScanReplacement(string[] args, OS os)
    {
        if (args.Length > 1)
        {
            Computer computer = ComputerLookup.Find(args[1], SearchType.Ip | SearchType.Name);
            if (computer != null)
            {
                os.netMap.discoverNode(computer);
                os.write("Found Terminal : " + computer.name + "@" + computer.ip);
            }
            return false;
        }
        return true;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Programs), nameof(Programs.connect))]
    internal static void ConnectILManipulator(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel incrementLabel = null;

        // Remove from here
        // for (int i = 0; i < os.netMap.nodes.Count; i++)
        c.GotoNext(MoveType.Before,
            x => x.MatchNop(),
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(1),
            x => x.MatchBr(out ILLabel _)
        );
        int i = c.Index;

        // if (!os.netMap.nodes[i].ip.Equals(args[1]) && !os.netMap.nodes[i].name.Equals(args[1]))
        // {
        //     continue;   
        // }
        // To here
        c.GotoNext(MoveType.After,
            x => x.MatchLdcI4(0),
            x => x.MatchNop(),
            x => x.MatchStloc(10),
            x => x.MatchLdloc(10),
            x => x.MatchBrtrue(out incrementLabel)
        );
        int j = c.Index;

        c.Index = i;
        c.RemoveRange(j-i);

        // Now before if (os.netMap.nodes[i].connect(os.thisComputer.ip))

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate<Func<string[], OS, int>>((args, os) => 
            os.netMap.nodes.IndexOf(ComputerLookup.Find(args[1], SearchType.Ip | SearchType.Name))
        );
        c.Emit(OpCodes.Stloc_1);

        c.Emit(OpCodes.Ldloc_1);
        c.Emit(OpCodes.Ldc_I4_M1);
        c.Emit(OpCodes.Ceq);
        // incrementLabel will be preserved but loop behavior removed
        c.Emit(OpCodes.Brtrue, incrementLabel);

        // Remove remaining loop behavior
        c.GotoLabel(incrementLabel);
        i = ++c.Index; // preserve label on nop
        c.GotoNext(MoveType.After,
            x => x.MatchStloc(10),
            x => x.MatchLdloc(10),
            x => x.MatchBrtrue(out ILLabel _)
        );
        j = c.Index;

        c.Index = i;
        c.RemoveRange(j-i);
    }
}
