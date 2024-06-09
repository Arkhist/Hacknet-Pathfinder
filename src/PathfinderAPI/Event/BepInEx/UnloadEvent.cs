using BepInEx.Hacknet;
using HarmonyLib;

namespace Pathfinder.Event.BepInEx;

[HarmonyPatch]
public class UnloadEvent : PathfinderEvent
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.Unload))]
    private static void OnPluginUnload(ref HacknetPlugin __instance)
    {
        var evt = new UnloadEvent();
        EventManager<UnloadEvent>.InvokeAssembly(__instance.GetType().Assembly, evt);
    }
}