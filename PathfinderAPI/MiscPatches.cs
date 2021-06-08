using System;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Hacknet;
using BepInEx.Logging;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder
{
    [HarmonyPatch]
    internal static class MiscPatches
    {
        [Util.Initialize]
        internal static void Initialize()
        {
            var original = AccessTools.Method(typeof(StackTrace), nameof(StackTrace.ToString), new Type[] { typeof(StackTrace).GetNestedType("TraceFormat", System.Reflection.BindingFlags.NonPublic) });
            var manipulator = AccessTools.Method(typeof(MiscPatches), nameof(MiscPatches.IncludeILOffsetInTrace));

            PathfinderAPIPlugin.HarmonyInstance.Patch(original, ilmanipulator: new HarmonyMethod(manipulator));
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
        
#if DEBUG
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(Program), nameof(Program.Main))]
        internal static void StopCatchingExceptionsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            foreach (var exHandler in il.Body.ExceptionHandlers)
            {
                c.Goto(exHandler.HandlerEnd, MoveType.After);
                var end = c.Index;
                c.Goto(exHandler.HandlerStart, MoveType.Before);
                c.RemoveRange(end - c.Index);
            }
            
            il.Body.ExceptionHandlers.Clear();
            
            foreach (var brokenLabel in il.Labels.Where(x => !c.Instrs.Contains(x.Target))) brokenLabel.Target = c.Instrs.Last();
        }
#endif
    }
}
