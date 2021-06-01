using HarmonyLib;
using Hacknet;

namespace Pathfinder.Event.Loading
{
    [HarmonyPatch]
    public class PortsAddedEvent : PathfinderEvent
    {
        public Computer Comp { get; }
        public string[] PortsList { get; }

        public PortsAddedEvent(Computer comp, string[] portsList)
        {
            Comp = comp;
            PortsList = portsList;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadPortsIntoComputer))]
        public static void PortsAddedPostfix(ref object computer_obj, string portsList)
        {
            var portsAddedEvent = new PortsAddedEvent((Computer)computer_obj, portsList.Split(' ', ','));
            EventManager<PortsAddedEvent>.InvokeAll(portsAddedEvent);
        }
    }
}
