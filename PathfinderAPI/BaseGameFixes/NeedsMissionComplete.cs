using System;
using System.Linq;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Hacknet;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class NeedsMissionComplete
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SCInstantly), nameof(SCInstantly.Check))]
        internal static void ActiveMissionNullCheckIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdfld(AccessTools.Field(typeof(OS), nameof(OS.currentMission)))
            );

            c.RemoveRange(2);
            c.EmitDelegate<Func<ActiveMission, bool>>(mission => mission?.isComplete() ?? false);
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SCOnConnect), nameof(SCOnConnect.Check))]
        internal static void ActiveMissionLogicFixIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(AccessTools.Field(typeof(SCOnConnect), nameof(SCOnConnect.needsMissionComplete)))
            );

            c.RemoveRange((c.Instrs.Count - 1) - c.Index);
            c.Emit(OpCodes.Ldarg_0);
            var startInst = c.Prev;
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<Func<SCOnConnect, OS, Computer, bool>>((self, os, comp) => 
                (!self.needsMissionComplete || os.currentMission == null || os.currentMission.isComplete()) 
                && (os.connectedComp != null && os.connectedComp.ip == comp.ip)
            );
            c.Emit(OpCodes.Ret);

            foreach (var label in il.Labels)
            {
                if (!il.Instrs.Any(x => label.Target.Equals(x)))
                {
                    label.Target = startInst;
                }
            }
        }
    }
}
