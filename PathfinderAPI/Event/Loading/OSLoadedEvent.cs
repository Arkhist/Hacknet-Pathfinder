using Hacknet;
using HarmonyLib;

namespace Pathfinder.Event.Loading;

[HarmonyPatch]
public class OSLoadedEvent : PathfinderEvent
{
    public OS Os { get; }

    public OSLoadedEvent(OS os)
    {
        Os = os;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
    private static void OSLoadPostfix(OS __instance) => EventManager<OSLoadedEvent>.InvokeAll(new OSLoadedEvent(__instance));
}