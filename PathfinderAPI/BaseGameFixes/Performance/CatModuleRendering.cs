using Hacknet;
using Hacknet.Effects;
using HarmonyLib;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes.Performance;

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
            lastFileData = fileData;
            return Utils.SuperSmartTwimForWidth(LocalizedFileLoader.SafeFilterString(fileData), OS.currentInstance.display.bounds.Width - 40, GuiData.tinyfont);
        });
    }

    private static bool isFlickering = false;
        
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActiveEffectsUpdater), nameof(ActiveEffectsUpdater.Update))]
    internal static void CheckThemeSwapFlicker(ActiveEffectsUpdater __instance) => isFlickering = __instance.themeSwapTimeRemaining > 0f;

    private static string displayCache2 = null;
    private static string lastFileData = null;
        
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.switchTheme), [typeof(object), typeof(OSTheme)])]
    internal static void CacheDisplayStringForThemeSwitch(object osObject)
    {
        var os = (OS) osObject;

        if (os.displayCache == null || lastFileData == null || (os.display.command != "cat" && os.display.command != "less"))
            return;

        if (isFlickering)
        {
            if (displayCache2 == null)
            {
                displayCache2 = os.displayCache;
                    
                os.displayCache = Utils.SuperSmartTwimForWidth(LocalizedFileLoader.SafeFilterString(lastFileData), os.display.bounds.Width - 40, GuiData.tinyfont);
            }
            else
            {
                var temp = displayCache2;
                displayCache2 = os.displayCache;
                os.displayCache = temp;
            }
        }
        else
        {
            os.displayCache = Utils.SuperSmartTwimForWidth(LocalizedFileLoader.SafeFilterString(lastFileData), os.display.bounds.Width - 40, GuiData.tinyfont);
        }

        isFlickering = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActiveEffectsUpdater), nameof(ActiveEffectsUpdater.CompleteThemeSwap))]
    internal static void ClearCache2OnFlickerFinish() => displayCache2 = null;
}