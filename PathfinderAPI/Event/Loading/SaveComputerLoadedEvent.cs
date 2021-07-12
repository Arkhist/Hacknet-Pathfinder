using HarmonyLib;
using Hacknet;
using Pathfinder.Util.XML;

namespace Pathfinder.Event.Loading
{
    [HarmonyPatch]
    public class SaveComputerLoadedEvent : PathfinderEvent
    {
        public OS Os { get; }
        public Computer Comp { get; }
        public ElementInfo Info { get; }

        public SaveComputerLoadedEvent(OS os, Computer comp, ElementInfo info)
        {
            Os = os;
            Comp = comp;
            Info = info;
        }
    }
}
