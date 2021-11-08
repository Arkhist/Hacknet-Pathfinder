using BepInEx.Hacknet;
using HarmonyLib;

namespace Pathfinder.Event.BepInEx
{
    [HarmonyPatch]
    public class PostLoadEvent : PathfinderEvent
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.PostLoad))]
        private static void OnPluginPostLoad(ref HacknetPlugin __instance)
        {
            var evt = new PostLoadEvent();
            EventManager<PostLoadEvent>.InvokeAssembly(__instance.GetType().Assembly, evt);
        }
    }
}