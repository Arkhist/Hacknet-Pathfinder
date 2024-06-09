using Hacknet;
using Hacknet.Effects;
using Hacknet.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Options;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance;

[HarmonyPatch]
public static class ThemeCaching
{
    private static FixedSizeCacheDict<string, CachedCustomTheme> CachedThemes = null;
    private static List<string> ThemeTasks = [];

    private static readonly object cacheLock = new object();

    private static string TargetTheme = null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
    internal static void QueueUpCustomThemes(OS __instance)
    {
        TargetTheme = null;
        ThemeTasks = [];
            
        if (!Settings.IsInExtensionMode)
        {
            CachedThemes = new FixedSizeCacheDict<string, CachedCustomTheme>(3);
            return;
        }

        CachedThemes = new FixedSizeCacheDict<string, CachedCustomTheme>(PathfinderOptions.PreloadAllThemes.Value ? 0 : 16);
            
        if (PathfinderOptions.PreloadAllThemes.Value)
        {
            var themesDir = Path.Combine(ExtensionLoader.ActiveExtensionInfo.FolderPath, "Themes");
            if (!Directory.Exists(themesDir))
                return;

            lock (cacheLock)
            {
                foreach (var theme in Directory.GetFiles(themesDir, "*.xml", SearchOption.AllDirectories))
                {
                    var path = theme.Substring(ExtensionLoader.ActiveExtensionInfo.FolderPath.Length + 1).Replace('\\', '/');
                    var task = new Task(() =>
                    {
                        var result = new CachedCustomTheme(path);
                        result.Load(false);
                        lock (cacheLock)
                        {
                            CachedThemes.Register(path, result);
                            ThemeTasks.Remove(path);
                        }
                    });
                    ThemeTasks.Add(path);
                    task.Start();
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.switchTheme), [typeof(object), typeof(string)])]
    internal static bool SwitchThemeReplacement(object osObject, string customThemePath)
    {
        var os = (OS) osObject;
        lock (cacheLock)
        {
            if (CachedThemes.TryGetCached(customThemePath, out var cached))
            {
                if (!cached.Loaded)
                    cached.Load(true);
                cached.ApplyTo(os);
                TargetTheme = null;
            }
            else if (!Settings.IsInExtensionMode)
            {
                var theme = new CachedCustomTheme(customThemePath);
                theme.Load(true);
                theme.ApplyTo(os);
                CachedThemes.Register(customThemePath, theme);
            }
            else if (!ThemeTasks.Contains(customThemePath))
            {
                TargetTheme = customThemePath;
                var path = customThemePath;
                var task = new Task(() =>
                {
                    var result = new CachedCustomTheme(path);
                    result.Load(false);
                    lock (cacheLock)
                    {
                        CachedThemes.Register(path, result);
                        ThemeTasks.Remove(path);
                    }
                });
                ThemeTasks.Add(path);
                task.Start();
            }
            else
            {
                TargetTheme = customThemePath;
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.Update))]
    internal static void UpdateTheme()
    {
        if (TargetTheme != null)
        {
            SwitchThemeReplacement(OS.currentInstance, TargetTheme);
        }
    }
        
    // this is ONLY done by DLCIntroExe.UpdateUIFlickerIn and nowhere else, this is so so so so dumb
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.switchTheme), [typeof(object), typeof(OSTheme)])]
    internal static bool SwitchToLastCustomTheme(object osObject, OSTheme theme)
    {
        if (theme == OSTheme.Custom)
        {
            lock (cacheLock)
            {
                // just dont question it
                var lastTheme = CachedThemes.BackingListHead?.Value;
                if (lastTheme != null)
                {
                    if (!lastTheme.Loaded)
                        lastTheme.Load(true);
                    lastTheme.ApplyTo((OS) osObject);
                }
            }

            return false;
        }

        return true;
    }
        
    // AAAAAAAAAAAAAAAAAAAAAA
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.getThemeForDataString))]
    internal static void LoadStartingCustomThemeWithReplacement(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(CustomTheme), nameof(CustomTheme.Deserialize))));

        c.Remove();
        c.EmitDelegate<Func<string, CustomTheme>>(theme =>
        {
            lock (cacheLock)
            {
                if (!CachedThemes.TryGetCached(theme, out _))
                {
                    var cacheTheme = new CachedCustomTheme(theme);
                    CachedThemes.Register(theme, cacheTheme);
                }
            }
            return null;
        });
    }

    private static float themeFpsLimitCounter = 0f;
    private delegate bool MakeSlowerDelegate(float timeSince, ref float timeRemaining);
        
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ActiveEffectsUpdater), nameof(ActiveEffectsUpdater.Update))]
    internal static void MakeThemeTransitionSlower(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        FieldReference swapTimeRemaining = null;
        ILLabel afterThemeSwap = null;
            
        c.GotoNext(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld(out swapTimeRemaining),
            x => x.MatchLdcR4(0f),
            x => x.MatchCgt(),
            x => x.MatchLdcI4(0),
            x => x.MatchCeq(),
            x => x.MatchStloc(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchBrtrue(out afterThemeSwap)
        );

        c.RemoveRange(9);
            
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldflda, swapTimeRemaining);
        c.EmitDelegate<MakeSlowerDelegate>((float timeSince, ref float currentTime) =>
        {
            if (currentTime <= 0)
                return false;
                
            themeFpsLimitCounter += timeSince;
            if (themeFpsLimitCounter > 1f / 30f)
            {
                currentTime -= themeFpsLimitCounter;
                themeFpsLimitCounter = 0f;

                return true;
            }
            return false;
        });
        c.Emit(OpCodes.Brfalse, afterThemeSwap);
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.drawBackgroundImage))]
    internal static void FixDrawHexGridBackground(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        // LastLoadedCustomTheme is null now
        c.GotoNext(MoveType.Before,
            x => x.MatchLdsfld(AccessTools.Field(typeof(ThemeManager), nameof(ThemeManager.LastLoadedCustomTheme))),
            x => x.MatchLdfld(AccessTools.Field(typeof(CustomTheme), nameof(CustomTheme.moduleColorBacking)))
        );
        c.RemoveRange(2);

        // Replicating base game behavior of loading these colors from LastLoadedCustomTheme instead of from OS
        c.EmitDelegate<Func<Color>>(() =>
            Utils.convertStringToColor(CachedThemes?.BackingListHead?.Value?.ThemeInfo?.Children?.Find(x => x.Name == "moduleColorBacking")?.Content)
        );

        c.GotoNext(MoveType.Before,
            x => x.MatchLdsfld(AccessTools.Field(typeof(ThemeManager), nameof(ThemeManager.LastLoadedCustomTheme))),
            x => x.MatchLdfld(AccessTools.Field(typeof(CustomTheme), nameof(CustomTheme.moduleColorStrong)))
        );
        c.RemoveRange(2);

        c.EmitDelegate<Func<Color>>(() =>
            Utils.convertStringToColor(CachedThemes?.BackingListHead?.Value?.ThemeInfo?.Children?.Find(x => x.Name == "moduleColorStrong")?.Content)
        );
    }
}