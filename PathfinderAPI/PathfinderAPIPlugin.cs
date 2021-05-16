using BepInEx;
using BepInEx.Hacknet;

namespace Pathfinder
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class PathfinderAPIPlugin : HacknetPlugin
    {
        public const string ModGUID = "com.Pathfinder.API";
        public const string ModName = "PathfinderAPI";
        public const string ModVer = "1.0.0";

        public override bool Load()
        {
            HarmonyInstance.PatchAll(typeof(PathfinderAPIPlugin).Assembly);
            return true;
        }
    }
}
