using BepInEx;
using BepInEx.Configuration;
using BepInEx.Hacknet;
using HarmonyLib;
using Pathfinder.Meta;
using Pathfinder.Event;
using Pathfinder.Event.Options;
using Pathfinder.Options;
using Pathfinder.Util;
using System.Globalization;

namespace Pathfinder;

[BepInPlugin(ModGUID, ModName, HacknetChainloader.VERSION)]
[BepInDependency("com.Pathfinder.Updater", BepInDependency.DependencyFlags.SoftDependency)]
[Updater(
    "https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases",
    "Pathfinder.Release.zip",
    "BepInEx/plugins/PathfinderAPI.dll"
)]
[HarmonyPatch]
public class PathfinderAPIPlugin : HacknetPlugin
{
    public const string ModGUID = "com.Pathfinder.API";
    public const string ModName = "PathfinderAPI";

    new internal static Harmony HarmonyInstance;
    new internal static ConfigFile Config;
    internal static PathfinderAPIPlugin Instance;

    public override bool Load()
    {
        PathfinderAPIPlugin.HarmonyInstance = base.HarmonyInstance;
        Logger.LogSource = base.Log;
        PathfinderAPIPlugin.Config = base.Config;
        Instance = this;

        foreach (var initMethod in typeof(PathfinderAPIPlugin).Assembly.GetTypes().SelectMany(AccessTools.GetDeclaredMethods))
        {
            if (initMethod.GetCustomAttributes(false).Any(x => x is InitializeAttribute) && initMethod.IsStatic && initMethod.GetParameters().Length == 0)
                initMethod.Invoke(null, null);
        }

        HarmonyInstance.PatchAll(typeof(PathfinderAPIPlugin).Assembly);

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.PostLoad))]
    private static void PrefixPostLoad(HacknetPlugin __instance)
    {
        var pluginInfo = MetadataHelper.GetMetadata(__instance);
        OptionsManager.OnConfigLoad(pluginInfo.GUID, __instance.Config);
        EventManager<CustomOptionsLoadEvent>.AddHandler(_ => OptionsManager.OnConfigLoad(pluginInfo.GUID, __instance.Config));
        EventManager<CustomOptionsSaveEvent>.AddHandler(_ =>
        {
            OptionsManager.OnConfigSave(pluginInfo.GUID, __instance.Config);
            __instance.Config.Save();
        });
    }
}