using HarmonyLib;
using Hacknet;

namespace Pathfinder.Event.Loading
{
    [HarmonyPatch]
    public class ComputerLoadedEvent : PathfinderEvent
    {
        public Computer Comp { get; private set; }
        public string Filename { get; private set; }
        public bool PreventAddToNetmap { get; private set; }
        public bool DontInitDaemons { get; private set; }

        public ComputerLoadedEvent(Computer comp, string filename, bool preventNetmap, bool dontInit)
        {
            Comp = comp;
            Filename = filename;
            PreventAddToNetmap = preventNetmap;
            DontInitDaemons = dontInit;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadComputer))]
        internal static void ComputerLoadPostfix(ref object __result, string filename, bool preventAddingToNetmap, bool preventInitDaemons)
        {
            var compLoadedEvent = new ComputerLoadedEvent((Computer)__result, filename, preventAddingToNetmap, preventInitDaemons);
            EventManager<ComputerLoadedEvent>.InvokeAll(compLoadedEvent);
        }
    }
}
