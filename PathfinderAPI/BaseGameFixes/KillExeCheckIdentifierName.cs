using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
public static class KillExeCheckIdentifierName
{
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(SAKillExe), nameof(SAKillExe.Trigger))]
    internal static void SAKillExeTriggerIL(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        
        // if (oS.exes[i].name.ToLower().Contains(ExeName.ToLower()))
        c.GotoNext(MoveType.After,
            // (...)
            x => x.MatchLdfld(AccessTools.Field(typeof(SAKillExe), nameof(SAKillExe.ExeName))),
            x => x.MatchCallvirt(AccessTools.Method(typeof(string), nameof(string.ToLower))),
            x => x.MatchCallvirt(AccessTools.Method(typeof(string), nameof(string.Contains), [typeof(string)])),
            x => x.MatchLdcI4(0),
            x => x.MatchCeq()
        );
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc_0);
        c.Emit(OpCodes.Ldloc_1);
        c.EmitDelegate<Func<SAKillExe, OS, int, bool>>((__instance, oS, i) =>
            oS.exes[i].IdentifierName.ToLower().Contains(__instance.ExeName.ToLower())
        );
        c.Emit(OpCodes.Ldc_I4_0);
        c.Emit(OpCodes.Ceq);
        c.Emit(OpCodes.And);
    }
}
