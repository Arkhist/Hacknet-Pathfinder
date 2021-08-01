using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Event;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class ExtensionInfoLoader
    {
        public abstract class ExtensionInfoExecutor
        {
            public ExtensionInfo ExtensionInfo;

            public virtual void Init(ref ExtensionInfo extensionInfo)
            {
                ExtensionInfo = extensionInfo;
            }

            public abstract void Execute(IExecutor exec, ElementInfo info);
        }

        private struct ExtensionInfoExecutorHolder
        {
            public string Element;
            public Type ExecutorType;
            public ParseOption Options;
        }

        private static readonly List<string> ValidLanguages = new List<string>
        {
            "en-us", "de-de", "fr-be", "ru-ru", "es-ar", "ko-kr", "ja-jp", "zh-cn"
        };

        public static void AddLanguage(string language) => ValidLanguages.Add(language);
        
        private static readonly List<ExtensionInfoExecutorHolder> CustomExecutors = new List<ExtensionInfoExecutorHolder>();
        
        public static void RegisterExecutor<T>(string element, ParseOption options = ParseOption.None) where T : ExtensionInfoExecutor, new()
        {
            CustomExecutors.Add(new ExtensionInfoExecutorHolder
            {
                Element = element,
                ExecutorType = typeof(T),
                Options = options
            });
        }
        
        public static void UnregisterExecutor<T>() where T : ExtensionInfoExecutor, new()
        {
            var tType = typeof(T);
            CustomExecutors.RemoveAll(x => x.ExecutorType == tType);
        }
        
        static ExtensionInfoLoader()
        {
            EventManager.onPluginUnload += OnPluginUnload;
        }
        
        private static void OnPluginUnload(Assembly pluginAsm)
        {
            var allTypes = AccessTools.GetTypesFromAssembly(pluginAsm);
            CustomExecutors.RemoveAll(x => allTypes.Contains(x.ExecutorType));
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtensionInfo), nameof(ExtensionInfo.ReadExtensionInfo))]
        private static bool LoadExtensionInfoPrefix(string folderpath, ref ExtensionInfo __result)
        {
            __result = LoadExtensionInfo(folderpath);
            return false;
        }

        public static ExtensionInfo LoadExtensionInfo(string folderpath)
        {
            var filepath = folderpath + "/ExtensionInfo.xml";

            if (!File.Exists(filepath))
                throw new FileNotFoundException($"Could not find ExtensionInfo.xml in folder {folderpath}");

            var extInfo = new ExtensionInfo();
            extInfo.FolderPath = folderpath;
            extInfo.Language = "en-us";
            
            var logopath = folderpath + "/Logo.";
            string extension = null;

            if (File.Exists(logopath + "png"))
                extension = "png";
            else if (File.Exists(logopath + "jpg"))
                extension = "jpg";

            if (extension != null)
                using (var input = File.OpenRead(logopath + extension))
                    extInfo.LogoImage = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, input);

            var exe = new EventExecutor(LocalizedFileLoader.GetLocalizedFilepath(filepath), true);

            foreach (var custom in CustomExecutors)
            {
                var instance = (ExtensionInfoExecutor)Activator.CreateInstance(custom.ExecutorType);
                instance.Init(ref extInfo);
                exe.RegisterExecutor(custom.Element, instance.Execute, custom.Options);
            }

            exe.Reg("Name", (exec, info) => extInfo.Name = Utils.CleanStringToLanguageRenderable(info.Content));
            exe.Reg("Language", (exec, info) =>
            {
                var language = info.Content;
                if (!ValidLanguages.Contains(language))
                    throw new FormatException($"Invalid language in '{filepath}': {language}");
                extInfo.Language = language;
            });
            exe.Reg("AllowSaves", (exec, info) => extInfo.AllowSave = info.ContentAsBoolean(filepath));
            exe.Reg("StartingVisibleNodes", (exec, info) =>
                extInfo.StartingVisibleNodes = info.Content
                    .Split(new[] {',', ' ', '\t', '\n', '\r', '/'}, StringSplitOptions.RemoveEmptyEntries));
            exe.Reg("StartingMission", (exec, info) => extInfo.StartingMissionPath = info.Content != "NONE" ? info.Content : null);
            exe.Reg("StartingActions", (exec, info) => extInfo.StartingActionsPath = info.Content != "NONE" ? info.Content : null);
            exe.Reg("Description", (exec, info) => extInfo.Description = Utils.CleanFilterStringToRenderable(info.Content));
            exe.Reg("Faction", (exec, info) => extInfo.FactionDescriptorPaths.Add(info.Content));
            exe.Reg("StartsWithTutorial", (exec, info) => extInfo.StartsWithTutorial = info.ContentAsBoolean(filepath));
            exe.Reg("HasIntroStartup", (exec, info) => extInfo.HasIntroStartup = info.ContentAsBoolean(filepath));
            exe.Reg("StartingTheme", (exec, info) => extInfo.Theme = info.Content); // no more ToLower() :)
            exe.Reg("IntroStartupSong", (exec, info) => extInfo.IntroStartupSong = info.Content);
            exe.Reg("IntroStartupSongDelay", (exec, info) => extInfo.IntroStartupSongDelay = info.ContentAsFloat(filepath));
            exe.Reg("SequencerSpinUpTime", (exec, info) => extInfo.SequencerSpinUpTime = info.ContentAsFloat(filepath));
            exe.Reg("ActionsToRunOnSequencerStart", (exec, info) => extInfo.ActionsToRunOnSequencerStart = info.Content);
            exe.Reg("SequencerFlagRequiredForStart", (exec, info) => extInfo.SequencerFlagRequiredForStart = info.Content);
            exe.Reg("SequencerTargetID", (exec, info) => extInfo.SequencerTargetID = info.Content);
            exe.Reg("WorkshopDescription", (exec, info) => extInfo.WorkshopDescription = info.Content);
            exe.Reg("WorkshopVisibility", (exec, info) =>
            {
                byte value;
                switch (info.Content)
                {
                    case "public":
                        value = 0;
                        break;
                    case "friends":
                        value = 1;
                        break;
                    case "private":
                        value = 2;
                        break;
                    default:
                        value = (byte) info.ContentAsInt(filepath);
                        break;
                }
                extInfo.WorkshopVisibility = value;
            });
            exe.Reg("WorkshopTags", (exec, info) => extInfo.WorkshopTags = info.Content);
            exe.Reg("WorkshopPreviewImagePath", (exec, info) => extInfo.WorkshopPreviewImagePath = info.Content);
            exe.Reg("WorkshopLanguage", (exec, info) => extInfo.WorkshopLanguage = info.Content);
            exe.Reg("WorkshopPublishID", (exec, info) => extInfo.WorkshopPublishID = info.Content);
            
            if (!exe.TryParse(out var ex))
                throw new FormatException($"An exception occurred while trying to parse '{filepath}'", ex);
            
            return extInfo;
        }

        private static void Reg(this IExecutor executor, string childName, ReadExecution execution)
            => executor.RegisterExecutor("HacknetExtension." + childName, execution, ParseOption.ParseInterior);
    }
}