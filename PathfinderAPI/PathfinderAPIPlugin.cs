using BepInEx;
using BepInEx.Configuration;
using BepInEx.Hacknet;
using HarmonyLib;
using Pathfinder.Meta;
using Pathfinder.Util;
using System.Globalization;
using SDL2;

namespace Pathfinder;

[BepInPlugin(ModGUID, ModName, HacknetChainloader.VERSION)]
[BepInDependency("com.Pathfinder.Updater", BepInDependency.DependencyFlags.SoftDependency)]
[Updater(
    "https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases",
    "Pathfinder.Release.zip",
    "BepInEx/plugins/PathfinderAPI.dll"
)]
public class PathfinderAPIPlugin : HacknetPlugin
{
    public const string ModGUID = "com.Pathfinder.API";
    public const string ModName = "PathfinderAPI";

    new internal static Harmony HarmonyInstance;
    new internal static ConfigFile Config;

    public static readonly bool GameIsSteamVersion = typeof(Hacknet.PlatformAPI.Storage.SteamCloudStorageMethod).GetField("deserialized") != null;

    public override bool Load()
    {
        PathfinderAPIPlugin.HarmonyInstance = base.HarmonyInstance;
        Logger.LogSource = base.Log;
        PathfinderAPIPlugin.Config = base.Config;
        
        Environment.SetEnvironmentVariable("LD_PRELOAD", $"./lib{(Environment.Is64BitProcess ? "64" : "")}/libcef.so");

        foreach (var initMethod in typeof(PathfinderAPIPlugin).Assembly.GetTypes().SelectMany(AccessTools.GetDeclaredMethods))
        {
            if (initMethod.GetCustomAttributes(false).Any(x => x is InitializeAttribute) && initMethod.IsStatic && initMethod.GetParameters().Length == 0)
                initMethod.Invoke(null, null);
        }

        HarmonyInstance.PatchAll(typeof(PathfinderAPIPlugin).Assembly);
        
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        
        // Expose textures and renderbuffers to other OpenGL contexts for funky mods =D
        SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 1);
        
        return true;
    }
}
