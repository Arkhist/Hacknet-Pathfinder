using System.IO;
using System.Linq;
using System.Xml;
using Hacknet;
using Hacknet.PlatformAPI.Storage;
using HarmonyLib;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class SaveLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.loadSaveFile))]
        internal static bool SaveLoadReplacementPrefix(ref OS __instance)
        {
            __instance.FirstTimeStartup = false;
            
            Stream saveStream = __instance.ForceLoadOverrideStream ?? SaveFileManager.GetSaveReadStream(__instance.SaveGameUserName);
            if (saveStream == null)
            {
                return false;
            }
            
            
            
            return false;
        }
    }
}
