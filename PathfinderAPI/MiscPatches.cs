using System;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Hacknet;

namespace Pathfinder
{
    [HarmonyPatch]
    internal static class MiscPatches
    {
        internal static void Initialize(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(StackTrace), nameof(StackTrace.ToString), new Type[] { typeof(StackTrace).GetNestedType("TraceFormat", System.Reflection.BindingFlags.NonPublic) });
            var manipulator = AccessTools.Method(typeof(MiscPatches), nameof(MiscPatches.IncludeILOffsetInTrace));

            harmony.Patch(original, ilmanipulator: new HarmonyMethod(manipulator));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.Unload))]
        internal static void OnPluginUnload(ref HacknetPlugin __instance) => Event.EventManager.InvokeOnPluginUnload(__instance.GetType().Assembly);
        
        internal static void IncludeILOffsetInTrace(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int frameLoc = 6;
            ILLabel targetLabel = null;

            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(out frameLoc),
                x => x.MatchCallvirt(AccessTools.Method(typeof(StackFrame), nameof(StackFrame.GetILOffset))),
                x => x.MatchLdcI4(-1),
                x => x.MatchBeq(out targetLabel)
            );

            c.GotoLabel(targetLabel);

            c.Emit(OpCodes.Ldloc, frameLoc);
            c.Emit(OpCodes.Ldloc, 4);
            c.EmitDelegate<Action<StackFrame, StringBuilder>>((frame, builder) =>
            {
                builder.Append($" IL<0x{frame.GetILOffset().ToString("X4")}>");
            });
        }
    }
}
