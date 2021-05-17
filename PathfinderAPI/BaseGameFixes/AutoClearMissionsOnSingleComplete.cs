using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Hacknet;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class AutoClearMissionsOnSingleComplete
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(DLCHubServer), nameof(DLCHubServer.PlayerAttemptCompleteMission))]
        internal static void DontClearMissionsOnCompleteIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Stloc_3);

            c.GotoNext(MoveType.After,
                x => x.MatchLdcI4(1),
                x => x.MatchStloc(3)
            );
            c.Remove();

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(3)
            );
            c.RemoveRange(2);
        }
    }
}
