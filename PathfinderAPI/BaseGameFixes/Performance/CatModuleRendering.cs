using System;
using Hacknet;
using HarmonyLib;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes.Performance
{
    [HarmonyPatch]
    internal static class CatModuleRendering
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(DisplayModule), nameof(DisplayModule.doCatDisplay))]
        internal static void DoCatDisplayNoWrapIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(5),
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(LocalizedFileLoader), nameof(LocalizedFileLoader.SafeFilterString)))
            );

            c.Index += 1;
            c.RemoveRange(10);
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(Programs), nameof(Programs.cat))]
        [HarmonyPatch(typeof(Programs), nameof(Programs.replace))]
        [HarmonyPatch(typeof(Programs), nameof(Programs.replace2))]
        internal static void WrapOnceInCommandIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdfld(AccessTools.Field(typeof(FileEntry), nameof(FileEntry.data))),
                x => x.MatchStfld(AccessTools.Field(typeof(OS), nameof(OS.displayCache)))
            );

            c.Index += 1;

            c.EmitDelegate<Func<string, string>>(fileData =>
            {
                return Utils.SuperSmartTwimForWidth(LocalizedFileLoader.SafeFilterString(fileData), OS.currentInstance.display.bounds.Width - 40, GuiData.tinyfont);
            });
        }
    }
}