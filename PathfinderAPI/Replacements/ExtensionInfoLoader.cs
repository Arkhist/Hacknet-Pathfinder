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
            private ExtensionInfoHolder _extensionInfo;

            public ExtensionInfo ExtensionInfo => _extensionInfo;

            public virtual void Init(ref ExtensionInfoHolder extensionInfo)
            {
                _extensionInfo = extensionInfo;
            }

            public abstract void Execute(IExecutor exec, ElementInfo info);
        }
        
        public class ExtensionInfoHolder
        {
            internal ExtensionInfo ExtensionInfo;
            internal ExtensionInfoHolder() {}

            public static implicit operator ExtensionInfo(ExtensionInfoHolder holder) => holder.ExtensionInfo;
        }
        
        private struct ExtensionInfoExecutorHolder
        {
            public string Element;
            public Type ExecutorType;
            public ParseOption Options;
        }
        
        private static readonly List<ExtensionInfoExecutorHolder> CustomExecutors = new List<ExtensionInfoExecutorHolder>();
        
        public static void RegisterExecutor<T>(string element, ParseOption options = ParseOption.None) where T : ExtensionInfoExecutor, new()
        {
            CustomExecutors.Add(new ExtensionInfoExecutorHolder()
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

            Logger.Log(LogLevel.Info, filepath);

            if (!File.Exists(filepath))
                throw new FileNotFoundException($"Could not find ExtensionInfo.xml in folder {folderpath}");

            var extInfo = new ExtensionInfo();
            extInfo.FolderPath = folderpath;
            extInfo.Language = "en-us";
            
            var logopath = folderpath + "Logo.";
            string extension = null;

            if (File.Exists(logopath + "png"))
                extension = "png";
            else if (File.Exists(logopath + "jpg"))
                extension = "jpg";

            if (extension != null)
                using (var input = File.OpenRead(logopath + extension))
                    extInfo.LogoImage = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, input);

            var executor = new EventExecutor(LocalizedFileLoader.GetLocalizedFilepath(filepath), true);

            var holder = new ExtensionInfoHolder();
            
            foreach (var custom in CustomExecutors)
            {
                var instance = (ExtensionInfoExecutor)Activator.CreateInstance(custom.ExecutorType);
                instance.Init(ref holder);
                executor.RegisterExecutor(custom.Element, instance.Execute, custom.Options);
            }
            
            executor.RegisterExecutor("HacknetExtension", (exec, info) =>
            {
                foreach (var child in info.Children) {
                    Logger.Log(LogLevel.Info, $"{child.Name} {child.Content}");
                    switch (child.Name)
                    {
                        case "Name":
                            extInfo.Name = Utils.CleanStringToLanguageRenderable(child.Content);
                            break;
                        case "Language":
                            extInfo.Language = child.Content;
                            break;
                        case "AllowSaves":
                            extInfo.AllowSave = bool.TryParse(child.Content, out var allowSave)
                                ? allowSave
                                : throw new FormatException($"Value specified for AllowSave in {filepath} was not a boolean (true/false)");
                            break;
                        case "StartingVisibleNodes":
                            extInfo.StartingVisibleNodes = Regex
                                .Replace(child.Content ?? throw new FormatException($"Extension info in {folderpath} has invalid starting visible nodes"),
                                    @"\s+", "")
                                .Split(',');
                            break;
                        case "StartingMission":
                            extInfo.StartingMissionPath = child.Content != "NONE" ? child.Content : null;
                            break;
                        case "StartingActions":
                            extInfo.StartingActionsPath = child.Content != "NONE" ? child.Content : null;
                            break;
                        case "Description":
                            extInfo.Description = Utils.CleanFilterStringToRenderable(child.Content);
                            break;
                        case "Faction":
                            extInfo.FactionDescriptorPaths.Add(child.Content);
                            break;
                        case "StartsWithTutorial":
                            extInfo.StartsWithTutorial = bool.TryParse(child.Content, out var startsWithTutorial)
                                ? startsWithTutorial
                                : throw new FormatException($"Value specified for StartsWithTutorial in {filepath} was not a boolean (true/false)");
                            break;
                        case "HasIntroStartup":
                            extInfo.HasIntroStartup = bool.TryParse(child.Content, out var hasIntroStartup)
                                ? hasIntroStartup
                                : throw new FormatException($"Value specified for HasIntroStartup in {filepath} was not a boolean (true/false)");
                            break;
                        case "StartingTheme":
                            extInfo.Theme = child.Content.ToLower();
                            break;
                        case "IntroStartupSong":
                            extInfo.IntroStartupSong = child.Content;
                            break;
                        case "IntroStartupSongDelay":
                            extInfo.IntroStartupSongDelay = float.TryParse(child.Content, out var introStartupSongDelay)
                                ? introStartupSongDelay
                                : throw new FormatException($"Value specified for IntroStartupSongDelay in {filepath} was not a float (e.g.: 0.0)");
                            break;
                        case "SequencerSpinUpTime":
                            extInfo.SequencerSpinUpTime = float.TryParse(child.Content, out var sequencerSpinUpTime)
                                ? sequencerSpinUpTime
                                : throw new FormatException($"Value specified for SequencerSpinUpTime in {filepath} was not a float (e.g.: 0.0)");
                            break;
                        case "ActionsToRunOnSequencerStart":
                            extInfo.ActionsToRunOnSequencerStart = child.Content;
                            break;
                        case "SequencerFlagRequiredForStart":
                            extInfo.SequencerFlagRequiredForStart = child.Content;
                            break;
                        case "SequencerTargetID":
                            extInfo.SequencerTargetID = child.Content;
                            break;
                        case "WorkshopDescription":
                            extInfo.WorkshopDescription = child.Content;
                            break;
                        case "WorkshopVisibility":
                            extInfo.WorkshopVisibility = byte.TryParse(child.Content, out var workshopVisibility)
                                ? workshopVisibility
                                : throw new FormatException("");
                            break;
                        case "WorkshopTags":
                            extInfo.WorkshopTags = child.Content;
                            break;
                        case "WorkshopPreviewImagePath":
                            extInfo.WorkshopPreviewImagePath = child.Content;
                            break;
                        case "WorkshopLanguage":
                            extInfo.WorkshopLanguage = child.Content;
                            break;
                        case "WorkshopPublishID":
                            extInfo.WorkshopPublishID = child.Content;
                            break;
                    }
                }
                holder.ExtensionInfo = extInfo;
            });
            
            if (!executor.TryParse(out var ex))
                throw new FormatException($"An exception occurred while trying to parse {filepath}", ex);

            Logger.Log(LogLevel.Info, $"{extInfo.Name} {extInfo.Description}");
            return extInfo;
        }
    }
}