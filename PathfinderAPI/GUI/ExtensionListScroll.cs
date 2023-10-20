using Hacknet;
using Hacknet.Screens;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.GUI;

[HarmonyPatch]
internal static class ExtensionListScroll {

	[HarmonyPatch(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.DrawExtensionList))]
	[HarmonyILManipulator]
	private static void AddExtensionScrollWheelListenerManipulator(ILContext context) {
		ILCursor cursor = new(context);

		/* Matches the loop header `for(int loc1 = this.ScrollStartIndex; … this.Extensions…)` */
		cursor.GotoNext(MoveType.Before,
			i => i.MatchLdarg(0),
			i => i.MatchLdfld<ExtensionsMenuScreen>("ScrollStartIndex"),
			i => i.MatchStloc(1),
			i =>
				i.MatchBr(out ILLabel label) &&
				label.Target.MatchLdloc(1) &&
				label.Target.Next.MatchLdarg(0) &&
				label.Target.Next.Next.MatchLdfld<ExtensionsMenuScreen>("Extensions")
		);

		cursor.Emit(OpCodes.Ldarg_0);
		cursor.Emit(OpCodes.Ldarg_1);
		cursor.Emit(OpCodes.Ldloc_0);
		cursor.EmitDelegate(ScrollUpdate);
	}

	private static void ScrollUpdate(ExtensionsMenuScreen self, Vector2 drawPos, Rectangle fullScreen) {
		const double bottomMargin = 140;
		const int itemHeight = 55;
		int extensionsOnScreen = (int) ((fullScreen.Height - drawPos.Y - bottomMargin) / itemHeight) + 1;
		int bottomMostPosition = self.Extensions.Count - extensionsOnScreen;

		int newPosition = self.ScrollStartIndex + (int) GuiData.getMouseWheelScroll();
		if(newPosition < 0)
			newPosition = 0;
		else if(newPosition > bottomMostPosition)
			newPosition = bottomMostPosition;
		self.ScrollStartIndex = newPosition;
	}

}
