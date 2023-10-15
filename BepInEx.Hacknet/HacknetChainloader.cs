using System.Reflection;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using Hacknet.Extensions;
using Hacknet.Screens;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using HN = global::Hacknet;
using Version = SemanticVersioning.Version;

namespace BepInEx.Hacknet;

public class HacknetChainloader : BaseChainloader<HacknetPlugin>
{
    private static Tuple<string, string, string> __UpdaterData
        = new Tuple<string, string, string>(
            "https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases",
            "Pathfinder.Release.zip",
            "BepInEx/core/BepInEx.Hacknet.dll"
        );

    public const string VERSION = "5.3.2";
    public static readonly Version Version = Version.Parse(VERSION);
        
    public static HacknetChainloader Instance;

    internal ManualLogSource Log = Logger.CreateLogSource("HacknetChainloader");
    internal List<string> TemporaryPluginGUIDs = new List<string>();

    internal Harmony HarmonyInstance;
    private bool _firstLoadDone = false;
        
    public override void Initialize(string gameExePath = null)
    {
        base.Initialize(gameExePath);
        Instance = this;

        HarmonyInstance = new Harmony("BepInEx.Hacknet.Chainloader");

        HarmonyInstance.PatchAll(typeof(ExtensionPluginPatches));
        LogWriteLineToDisk.LogWriter = (Logger.Listeners.FirstOrDefault(x => x is DiskLogListener) as DiskLogListener)?.LogWriter;
        HarmonyInstance.PatchAll(typeof(LogWriteLineToDisk));
    }

    protected override IList<PluginInfo> DiscoverPlugins()
    {
        if (!_firstLoadDone)
        {
            _firstLoadDone = true;
            return base.DiscoverPlugins();
        }

        var plugins = base.DiscoverPlugins();
        return plugins;
    }

    public override HacknetPlugin LoadPlugin(PluginInfo pluginInfo, Assembly pluginAssembly)
    {
        var type = pluginAssembly.GetType(pluginInfo.TypeName);

        HacknetPlugin pluginInstance = (HacknetPlugin) Activator.CreateInstance(type);
        if (!pluginInstance.Load())
        {
            throw new Exception($"{pluginInfo.Metadata.GUID} returned false on its load method");
        }
        if (HN.Settings.IsInExtensionMode)
            TemporaryPluginGUIDs.Add(pluginInfo.Metadata.GUID);

        return pluginInstance;
    }

    public override void Execute()
    {
        base.Execute();
        foreach(var p in Plugins)
            ((HacknetPlugin)p.Value.Instance).PostLoad();
    }

    internal void UnloadTemps()
    {
        Log.LogMessage("Unloading extension plugins...");
            
        foreach (var temp in TemporaryPluginGUIDs.Reverse<string>())
        {
            if (!((HacknetPlugin)this.Plugins[temp].Instance).Unload())
            {
                Log.LogError($"{temp} failed to unload, this could cause problems without a game restart!");
            }
            this.Plugins.Remove(temp);
                
            Log.LogMessage($"Unloaded {temp}");
        }
        TemporaryPluginGUIDs.Clear();
        ChainloaderFix.ClearRemaps();

        Log.LogMessage("Finished unloading extension plugins");
    }
}

[HarmonyPatch]
internal static class ExtensionPluginPatches
{
    // BepInEx.Paths's setters are private, this creates setters we can use
    private static readonly MethodInfo PluginPathSetter = AccessTools.PropertySetter(typeof(Paths), nameof(Paths.PluginPath));
    private static readonly MethodInfo ConfigPathSetter = AccessTools.PropertySetter(typeof(Paths), nameof(Paths.ConfigPath));

    private static bool FirstExtensionLoaded = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.ActivateExtensionPage))]
    internal static void LoadTempPluginsPrefix(ExtensionInfo info, ref bool __runOriginal)
    {
        if (!__runOriginal)
            return;
            
        if (!FirstExtensionLoaded)
        {
            HacknetChainloader.Instance.HarmonyInstance.PatchAll(typeof(ChainloaderFix));
            FirstExtensionLoaded = true;
        }
            
        try
        {
            HN.Settings.IsInExtensionMode = true;
            ExtensionLoader.ActiveExtensionInfo = info;

            var newPluginPath = Path.Combine(info.GetFullFolderPath(), "Plugins");
            var newConfigPath = Path.Combine(newPluginPath, "Configs");

            if (Directory.Exists(newPluginPath) && Directory.GetFiles(newPluginPath, "*.dll", SearchOption.AllDirectories).Length > 0)
            {
                PluginPathSetter.Invoke(null, new object[] { newPluginPath });
                ConfigPathSetter.Invoke(null, new object[] { newConfigPath });
                HacknetChainloader.Instance.Execute();
            }

            __runOriginal = true;
        }
        catch (Exception ex)
        {
            HN.Settings.IsInExtensionMode = false;

            HacknetChainloader.Instance.Log.LogError($"A fatal exception occured while loading extension plugins, aborting:\n{ex}");

            __runOriginal = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HN.OS), nameof(HN.OS.quitGame))]
    internal static void UnloadOnOSQuitPostfix() => HacknetChainloader.Instance.UnloadTemps();
        
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.DrawExtensionInfoDetail))]
    internal static void OnBackButtonPressIL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchLdnull(),
            x => x.MatchStfld(AccessTools.Field(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.ExtensionInfoToShow)))
        );

        c.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(HacknetChainloader), nameof(HacknetChainloader.Instance)));
        c.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(HacknetChainloader), nameof(HacknetChainloader.UnloadTemps)));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.ExitExtensionsScreen))]
    private static void OnMainMenuButtonPressPrefix() => HacknetChainloader.Instance.UnloadTemps();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AppDomain), "get_BaseDirectory")]
    private static bool ReturnCorrectDirectoryPrefix(out string __result)
    {
        __result = Paths.GameRootPath;
        return false;
    }
}

[HarmonyPatch]
internal static class ChainloaderFix
{
    internal static Dictionary<string, Assembly> Remaps = new Dictionary<string, Assembly>();
    internal static Dictionary<string, AssemblyDefinition> RemapDefinitions = new Dictionary<string, AssemblyDefinition>();

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(BaseChainloader<HacknetPlugin>), "Execute")]
    internal static void PluginCecilHacks(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(Assembly), "LoadFile", new Type[] { typeof(string) }))
        );

        c.Remove();
        c.EmitDelegate<Func<string, Assembly>>(path =>
        {
            byte[] asmBytes;
            string name;

            var asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters()
            {
                AssemblyResolver = new RenamedAssemblyResolver()
            });
            name = asm.Name.Name;
            asm.Name.Name = asm.Name.Name + "-" + DateTime.Now.Ticks;

            using (var ms = new MemoryStream())
            {
                asm.Write(ms);
                asmBytes = ms.ToArray();
            }

            var loaded = Assembly.Load(asmBytes);
            Remaps[name] = loaded;
            RemapDefinitions[name] = asm;

            return loaded;
        });
    }
    internal static void ClearRemaps()
    {
        Remaps.Clear();
        foreach (var asm in RemapDefinitions.Values)
            asm.Dispose();
        RemapDefinitions.Clear();
    }
}
internal class RenamedAssemblyResolver : DefaultAssemblyResolver
{
    public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) =>
        ChainloaderFix.RemapDefinitions.TryGetValue(name.Name, out var asm)
            ? asm
            : base.Resolve(name, parameters);
}

[HarmonyPatch]
internal static class LogWriteLineToDisk
{
    internal static TextWriter LogWriter = null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Console), nameof(Console.WriteLine), new Type[] { typeof(string) })]
    internal static void WriteWriteLineToLog(string value)
    {
        LogWriter?.WriteLine(value);
    }
}
