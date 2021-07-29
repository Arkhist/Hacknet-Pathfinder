using System;
using System.Linq;
using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;
using Pathfinder.Util;

namespace Pathfinder
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class PathfinderAPIPlugin : HacknetPlugin
    {
        public const string ModGUID = "com.Pathfinder.API";
        public const string ModName = "PathfinderAPI";
        public const string ModVer = "5.0.0";

        new internal static Harmony HarmonyInstance;

        public override bool Load()
        {
            PathfinderAPIPlugin.HarmonyInstance = base.HarmonyInstance;
            Logger.LogSource = base.Log;

            foreach (var initMethod in typeof(PathfinderAPIPlugin).Assembly.GetTypes().SelectMany(AccessTools.GetDeclaredMethods))
            {
                if (initMethod.GetCustomAttributes(false).Any(x => x is Util.InitializeAttribute) && initMethod.IsStatic && initMethod.GetParameters().Length == 0)
                    initMethod.Invoke(null, null);
            }

            HarmonyInstance.PatchAll(typeof(PathfinderAPIPlugin).Assembly);
            
            if (Environment.GetCommandLineArgs().Any(x => x.ToLower() == "-enabledebug"))
                Command.DebugCommands.AddCommands();

            return true;
        }
    }
}
