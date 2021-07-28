using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class DontLosePlayerCompAdmin
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SAChangeIP), nameof(SAChangeIP.Trigger))]
        internal static void NotifyOSOfPlayerCompIPChange(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(1),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(AccessTools.Field(typeof(SAChangeIP), nameof(SAChangeIP.NewIP))),
                x => x.MatchStfld(AccessTools.Field(typeof(Computer), nameof(Computer.ip)))
            );

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Castclass, typeof(OS));
            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<System.Action<OS, Computer>>((os, comp) =>
            {
                if (comp.idName == "playerComp")
                {
                    os.thisComputerIPReset();
                }
            });
        }
    }
}