using System;
using System.Reflection;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
internal static class AvoidNullDerefOnThemeChange {

	[HarmonyILManipulator]
	[HarmonyPatch(typeof(ThemeChangerExe), nameof(ThemeChangerExe.ApplyTheme))]
	internal static void ForcePlayerXServerToExist(ILContext context, MethodBase original) {
		if (original is null)
			return;

		ILCursor il = new(context);
		ILLabel fileIsNull = null!;
		ILLabel afterConditionals = il.DefineLabel();
		Type str = typeof(string);
		FieldInfo strEmpty = AccessTools.Field(str, nameof(string.Empty));
		ConstructorInfo fileCtor = AccessTools.Constructor(typeof(FileEntry), new[]{str, str});
		FieldInfo folderFiles = AccessTools.Field(typeof(Folder), nameof(Folder.files));
		MethodInfo fileListAdd = AccessTools.Method(folderFiles.FieldType, nameof(Folder.files.Add));

		il.GotoNext( // doesn't matter where we move relative to these, we just need the label at the end of this instruction set
			i => i.MatchLdloc(2),
			i => i.MatchLdnull(),
			i => i.MatchCeq(),
			i => i.MatchStloc(6),
			i => i.MatchLdloc(6),
			i => i.MatchBrtrue(out fileIsNull)
		);

		il.GotoLabel(fileIsNull, MoveType.Before); // we need to insert a branch after making the backup, in order to NOT make a duplicate file
		il.Emit(OpCodes.Br, afterConditionals); // coming in from the part that ran when FileEntry is NOT null, making the backup theme file
		il.MoveAfterLabels(); // now we need to inject AT the jump target for when the FileEntry IS null

		il.Emit(OpCodes.Ldsfld, strEmpty); // [string.Empty]
		il.Emit(OpCodes.Ldstr, "x-server.sys"); // [string.Empty, "x-server.sys"]
		il.Emit(OpCodes.Newobj, fileCtor); // [FileEntry] new FileEntry(string.Empty, "x-server.sys")
		il.Emit(OpCodes.Stloc_2); // []
		il.Emit(OpCodes.Ldloc_1); // [Folder]
		il.Emit(OpCodes.Ldfld, folderFiles); // [Folder.files]
		il.Emit(OpCodes.Ldloc_2); // [Folder.files, FileEntry]
		il.Emit(OpCodes.Callvirt, fileListAdd); // [] Folder.files.Add(FileEntry)

		afterConditionals.Target = il.Next;
	}

}
