using System;
using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class StartingActionsAfterNodes
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ExtensionLoader), nameof(ExtensionLoader.LoadNewExtensionSession))]
        internal static void FixStartingActionsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(RunnableConditionalActions), nameof(RunnableConditionalActions.LoadIntoOS)))
            );

            c.Remove();
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Pop);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
        internal static void RunStartingActions(ref OS __instance)
        {
            if (!OS.WillLoadSave && Settings.IsInExtensionMode && ExtensionLoader.ActiveExtensionInfo.StartingActionsPath != null)
                RunnableConditionalActions.LoadIntoOS(ExtensionLoader.ActiveExtensionInfo.StartingActionsPath, __instance);
        }
    }
}