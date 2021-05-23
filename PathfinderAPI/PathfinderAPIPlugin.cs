using BepInEx;
using BepInEx.Hacknet;

namespace Pathfinder
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class PathfinderAPIPlugin : HacknetPlugin
    {
        public const string ModGUID = "com.Pathfinder.API";
        public const string ModName = "PathfinderAPI";
        public const string ModVer = "5.0.0";

        public override bool Load()
        {
            Logger.LogSource = base.Log;

            HarmonyInstance.PatchAll(typeof(PathfinderAPIPlugin).Assembly);
            Executable.ExecutableHandler.Initialize();

            return true;
        }
    }
}
