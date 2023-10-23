using System.Reflection;
using Hacknet.Effects;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
public static class FlickeringTextReportNull {
	[HarmonyPatch(typeof(FlickeringTextEffect), nameof(FlickeringTextEffect.GetReportString))]
	[HarmonyILManipulator]
	private static void GetReportStringManipulator(ILContext context) {
		ILCursor cursor = new(context);

		ILLabel unskipLabel = context.DefineLabel();
		cursor.Emit(OpCodes.Ldsfld, typeof(FlickeringTextEffect).GetField(nameof(FlickeringTextEffect.LinedItemTarget), BindingFlags.Static | BindingFlags.Public));
		cursor.Emit(OpCodes.Brtrue, unskipLabel);
		cursor.Emit(OpCodes.Ldstr, "FlickeringTextEffect was not used in this execution.");
		cursor.Emit(OpCodes.Ret);
		unskipLabel.Target = cursor.Next;
	}
}
