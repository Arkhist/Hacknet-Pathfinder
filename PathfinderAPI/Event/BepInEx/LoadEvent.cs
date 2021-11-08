using System.Reflection;
using BepInEx.Hacknet;
using HarmonyLib;

namespace Pathfinder.Event.BepInEx
{
    [HarmonyPatch]
    public class LoadEvent : PathfinderEvent
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HacknetChainloader), nameof(HacknetChainloader.LoadPlugin))]
        private static void OnPluginLoad(ref HacknetChainloader __instance, ref HacknetPlugin __result, Assembly pluginAssembly)
        {
            var evt = new LoadEvent();
            EventManager<LoadEvent>.InvokeAssembly(pluginAssembly, evt);
        }
    }
}