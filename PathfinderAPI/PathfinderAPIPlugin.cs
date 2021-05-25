using System;
using System.Diagnostics;
using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;

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

            MiscPatches.Initialize(HarmonyInstance);
            HarmonyInstance.PatchAll(typeof(PathfinderAPIPlugin).Assembly);

            return true;
        }
    }
}
