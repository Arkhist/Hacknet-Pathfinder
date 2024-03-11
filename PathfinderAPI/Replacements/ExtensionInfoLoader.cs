using System.Reflection;
using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Event;
using Pathfinder.Util.XML;
using Pathfinder.Util;

namespace Pathfinder.Replacements;

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

        public abstract void Execute(EventExecutor exec, ElementInfo info);
    }

    private struct ExtensionInfoExecutorHolder
    {
        public string Element;
        public Type ExecutorType;
        public ParseOption Options;
    }

    private static readonly List<string> ValidLanguages = ["en-us", "de-de", "fr-be", "ru-ru", "es-ar", "ko-kr", "ja-jp", "zh-cn"];

    public static void AddLanguage(string language) => ValidLanguages.Add(language);
        
    private static readonly List<ExtensionInfoExecutorHolder> CustomExecutors = [];

    public static void RegisterExecutor<T>(string element, ParseOption options = ParseOption.None) where T : ExtensionInfoExecutor, new() => RegisterExecutor(typeof(T), element, options);
    public static void RegisterExecutor(Type executorType, string element, ParseOption options = ParseOption.None)
    {
        executorType.ThrowNotInherit<ExtensionInfoExecutor>(nameof(executorType));
        executorType.ThrowNoDefaultCtor(nameof(executorType));
        CustomExecutors.Add(new ExtensionInfoExecutorHolder
        {
            Element = element,
            ExecutorType = executorType,
            Options = options
        });
    }
        
    public static void UnregisterExecutor<T>() where T : ExtensionInfoExecutor, new()
    {
        var tType = typeof(T);
        CustomExecutors.RemoveAll(x => x.ExecutorType == tType);
    }
        
    private static void OnPluginUnload(Assembly pluginAsm)
    {
        CustomExecutors.RemoveAll(x => x.ExecutorType.Assembly == pluginAsm);
    }

    private static readonly EventExecutor executor = new EventExecutor();

    private static ExtensionInfo extInfo = null;
        
    static ExtensionInfoLoader()
    {
        EventManager.onPluginUnload += OnPluginUnload;
            
        executor.RegisterExecutor("Name", (exec, info) => extInfo.Name = Utils.CleanStringToLanguageRenderable(info.Content), ParseOption.ParseInterior);
        executor.RegisterExecutor("Language", (exec, info) =>
        {
            var language = info.Content;
            if (!ValidLanguages.Contains(language))
                throw new FormatException($"Invalid language: {language}");
            extInfo.Language = language;
        }, ParseOption.ParseInterior);
        executor.RegisterExecutor("AllowSaves", (exec, info) => extInfo.AllowSave = info.ContentAsBoolean(), ParseOption.ParseInterior);
        executor.RegisterExecutor("StartingVisibleNodes", (exec, info) =>
            extInfo.StartingVisibleNodes = info.Content
                ?.Split(new[] {',', ' ', '\t', '\n', '\r', '/'}, StringSplitOptions.RemoveEmptyEntries) ?? [], ParseOption.ParseInterior);
        executor.RegisterExecutor("StartingMission", (exec, info) => extInfo.StartingMissionPath = info.Content != "NONE" ? info.Content : null, ParseOption.ParseInterior);
        executor.RegisterExecutor("StartingActions", (exec, info) => extInfo.StartingActionsPath = info.Content != "NONE" ? info.Content : null, ParseOption.ParseInterior);
        executor.RegisterExecutor("Description", (exec, info) => extInfo.Description = Utils.CleanFilterStringToRenderable(info.Content), ParseOption.ParseInterior);
        executor.RegisterExecutor("Faction", (exec, info) => extInfo.FactionDescriptorPaths.Add(info.Content), ParseOption.ParseInterior);
        executor.RegisterExecutor("StartsWithTutorial", (exec, info) => extInfo.StartsWithTutorial = info.ContentAsBoolean(), ParseOption.ParseInterior);
        executor.RegisterExecutor("HasIntroStartup", (exec, info) => extInfo.HasIntroStartup = info.ContentAsBoolean(), ParseOption.ParseInterior);
        executor.RegisterExecutor("StartingTheme", (exec, info) => extInfo.Theme = info.Content, ParseOption.ParseInterior); // no more ToLower() :)
        executor.RegisterExecutor("IntroStartupSong", (exec, info) => extInfo.IntroStartupSong = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("IntroStartupSongDelay", (exec, info) => extInfo.IntroStartupSongDelay = info.ContentAsFloat(), ParseOption.ParseInterior);
        executor.RegisterExecutor("SequencerSpinUpTime", (exec, info) => extInfo.SequencerSpinUpTime = info.ContentAsFloat(), ParseOption.ParseInterior);
        executor.RegisterExecutor("ActionsToRunOnSequencerStart", (exec, info) => extInfo.ActionsToRunOnSequencerStart = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("SequencerFlagRequiredForStart", (exec, info) => extInfo.SequencerFlagRequiredForStart = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("SequencerTargetID", (exec, info) => extInfo.SequencerTargetID = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("WorkshopDescription", (exec, info) => extInfo.WorkshopDescription = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("WorkshopVisibility", (exec, info) =>
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
                    value = (byte) info.ContentAsInt();
                    break;
            }
            extInfo.WorkshopVisibility = value;
        }, ParseOption.ParseInterior);
        executor.RegisterExecutor("WorkshopTags", (exec, info) => extInfo.WorkshopTags = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("WorkshopPreviewImagePath", (exec, info) => extInfo.WorkshopPreviewImagePath = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("WorkshopLanguage", (exec, info) => extInfo.WorkshopLanguage = info.Content, ParseOption.ParseInterior);
        executor.RegisterExecutor("WorkshopPublishID", (exec, info) => extInfo.WorkshopPublishID = info.Content, ParseOption.ParseInterior);
    }
        
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExtensionInfo), nameof(ExtensionInfo.ReadExtensionInfo))]
    private static bool LoadExtensionInfoPrefix(string folderpath, out ExtensionInfo __result)
    {
        __result = LoadExtensionInfo(folderpath);
        return false;
    }

    public static ExtensionInfo LoadExtensionInfo(string folderpath)
    {
        var filepath = folderpath + "/ExtensionInfo.xml";

        if (!File.Exists(filepath))
            throw new FileNotFoundException($"Could not find ExtensionInfo.xml in folder {folderpath}");

        extInfo = new ExtensionInfo();
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

        executor.SetText(LocalizedFileLoader.GetLocalizedFilepath(filepath), true);

        foreach (var custom in CustomExecutors)
        {
            var instance = (ExtensionInfoExecutor)Activator.CreateInstance(custom.ExecutorType);
            instance.Init(ref extInfo);
            executor.RegisterTempExecutor(custom.Element, instance.Execute, custom.Options);
        }

        if (!executor.TryParse(out var ex))
            throw new FormatException($"An exception occurred while trying to parse '{filepath}'", ex);

        var ret = extInfo;
        extInfo = null;
        return ret;
    }
}
