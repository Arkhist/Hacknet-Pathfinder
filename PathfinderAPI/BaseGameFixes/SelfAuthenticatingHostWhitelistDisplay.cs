using System;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class SelfAuthenticatingHostWhitelistDisplay
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(WhitelistConnectionDaemon), nameof(WhitelistConnectionDaemon.draw))]
        internal static void WhitelistDrawFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // bool flag = RemoteCompCanBeAccessed();
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(WhitelistConnectionDaemon), nameof(WhitelistConnectionDaemon.RemoteCompCanBeAccessed)))
            );

            // or RemoteCompCanBeAccessed() with RemoteSourceIP == null
            // Could be more efficient by checking RemoteSourceIP first and branching
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(WhitelistConnectionDaemon), nameof(WhitelistConnectionDaemon.RemoteSourceIP)));
            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Ceq);
            c.Emit(OpCodes.Or);
        }
    }
}
