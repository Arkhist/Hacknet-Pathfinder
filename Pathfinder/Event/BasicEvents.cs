using Hacknet;

namespace Pathfinder.Event
{
    public class NetworkMapLoadContentEvent : PathfinderEvent
    {
        public NetworkMap NetMapInstance { get; private set; }
        public NetworkMapLoadContentEvent(NetworkMap netmapInstance) { NetMapInstance = netmapInstance; }
    }
}
