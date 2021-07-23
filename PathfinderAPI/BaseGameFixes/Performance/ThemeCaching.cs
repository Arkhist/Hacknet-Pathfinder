using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes.Performance
{
    [HarmonyPatch]
    public static class ThemeCaching
    {
        private static FixedSizeCacheDict<string, CachedCustomTheme> CachedThemes = null;
        private static List<Task> ThemeTasks = null;

        private static string TargetTheme = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
        internal static void QueueUpCustomThemes(OS __instance)
        {
            TargetTheme = null;
            ThemeTasks = new List<Task>();
            
            if (!Settings.IsInExtensionMode)
            {
                CachedThemes = new FixedSizeCacheDict<string, CachedCustomTheme>(3);
                return;
            }

            CachedThemes = new FixedSizeCacheDict<string, CachedCustomTheme>(int.MaxValue /*int.MaxValue on PreLoadAll*/);
            
            // replace with PreLoadAll check
            if (true)
            {
                var themesDir = Path.Combine(ExtensionLoader.ActiveExtensionInfo.FolderPath, "Themes");
                if (!Directory.Exists(themesDir))
                    return;

                foreach (var theme in Directory.GetFiles(themesDir, "*.xml", SearchOption.AllDirectories))
                {
                    var path = theme.Substring(ExtensionLoader.ActiveExtensionInfo.FolderPath.Length + 1);
                    var task = new Task(() =>
                    {
                        var result = new CachedCustomTheme(path);
                        result.Load(false);
                        lock (CachedThemes)
                        {
                            CachedThemes.Register(path.ToLower(), result);
                        }
                    });
                    task.Start();
                    ThemeTasks.Add(task);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ThemeManager), nameof(ThemeManager.switchTheme), new Type[] { typeof(object), typeof(string) })]
        internal static bool SwitchThemeReplacement(object osObject, string customThemePath)
        {
            var os = (OS) osObject;
            lock (CachedThemes)
            {
                if (CachedThemes.TryGetCached(customThemePath.ToLower(), out var cached))
                {
                    if (!cached.Loaded)
                        cached.Load(true);
                    cached.ApplyTo(os);
                }
                else
                {
                    TargetTheme = customThemePath;
                    var path = customThemePath;
                    var task = new Task(() =>
                    {
                        var result = new CachedCustomTheme(path);
                        result.Load(false);
                        lock (CachedThemes)
                        {
                            CachedThemes.Register(path, result);
                        }
                    });
                    task.Start();
                    ThemeTasks.Add(task);
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
                lock (CachedThemes)
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
