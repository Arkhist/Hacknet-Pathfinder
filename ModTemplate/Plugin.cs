using BepInEx;
using BepInEx.Hacknet;

#if (csharpFeature_FileScopedNamespaces)
namespace ModTemplate;
#else
namespace ModTemplate
{
#endif
[BepInPlugin(ModGUID, ModName, ModVersion)]
#if (includePathfinderAPI)
[BepInDependency("com.Pathfinder.API", BepInDependency.DependencyFlags.HardDependency)]
#endif
#if (includePathfinderUpdater)
[BepInDependency("com.Pathfinder.Updater", BepInDependency.DependencyFlags.SoftDependency)]
#endif
public class Plugin : HacknetPlugin
{
    public const string ModGUID = "$(ModGUID)";
    public const string ModName = "$(ModName)";
    public const string ModVersion = "$(ModVersion)";

    new internal static Harmony HarmonyInstance;
    new internal static ConfigFile Config;

    public override bool Load()
    {
        return true;
    }
}
#if (!csharpFeature_FileScopedNamespaces)
}
#endif