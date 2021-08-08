using Hacknet;
using HarmonyLib;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class RandomIPNoRepeats
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkMap), nameof(NetworkMap.generateRandomIP))]
        internal static bool GenerateRandomIPReplacement(out string __result)
        {
            while (true)
            {
                var ip = Utils.random.Next(254) + 1 + "." + (Utils.random.Next(254) + 1) + "." + (Utils.random.Next(254) + 1) + "." + (Utils.random.Next(254) + 1);
                if (ComputerLookup.FindByIp(ip) == null)
                {
                    __result = ip;
                    return false;
                }
            }
        }
    }
}
