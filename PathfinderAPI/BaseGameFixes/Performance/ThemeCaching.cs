using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using Pathfinder.Options;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance
{
    [HarmonyPatch]
    public static class ThemeCaching
    {
        private static FixedSizeCacheDict<string, CachedCustomTheme> CachedThemes = null;
        private static List<string> ThemeTasks = new List<string>();

        private static readonly object cacheLock = new object();

        private static string TargetTheme = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
        internal static void QueueUpCustomThemes(OS __instance)
        {
            TargetTheme = null;
            ThemeTasks = new List<string>();
            
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
                                CachedThemes.Register(path.ToLower(), result);
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
        [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.switchTheme), new Type[] { typeof(object), typeof(string) })]
        internal static bool SwitchThemeReplacement(object osObject, string customThemePath)
        {
            var os = (OS) osObject;
            lock (cacheLock)
            {
                if (CachedThemes.TryGetCached(customThemePath.ToLower(), out var cached))
                {
                    if (!cached.Loaded)
                        cached.Load(true);
                    cached.ApplyTo(os);
                    TargetTheme = null;
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
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.Update))]
        internal static void UpdateTheme()
        {
            if (TargetTheme != null)
            {
                lock (cacheLock)
                {
                    if (CachedThemes.TryGetCached(TargetTheme, out var theme))
                    {
                        if (!theme.Loaded)
                            theme.Load(true);
                        theme.ApplyTo(OS.currentInstance);

                        TargetTheme = null;
                    }
                }
            }
        }
    }
}
