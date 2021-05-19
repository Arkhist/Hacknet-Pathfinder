using HarmonyLib;
using BepInEx.Hacknet;

namespace Pathfinder
{
    [HarmonyPatch]
    internal static class MiscPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.Unload))]
        public static void OnPluginUnload(ref HacknetPlugin __instance) => Event.EventManager.InvokeOnPluginUnload(__instance.GetType().Assembly);
    }
}
