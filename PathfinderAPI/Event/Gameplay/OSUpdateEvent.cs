using Hacknet;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Pathfinder.Event.Gameplay
{
    [HarmonyPatch]
    public class OSUpdateEvent : PathfinderEvent
    {
        public OS OS { get; }
        public GameTime GameTime { get; }

        public OSUpdateEvent(OS os, GameTime gameTime)
        {
            OS = os;
            GameTime = gameTime;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.Update))]
        internal static void OSUpdatePostfix(OS __instance, GameTime gameTime)
        {
            var osUpdate = new OSUpdateEvent(__instance, gameTime);
            EventManager<OSUpdateEvent>.InvokeAll(osUpdate);
        }
    }
}
