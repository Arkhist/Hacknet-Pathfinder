using System;

namespace Pathfinder.Event
{
    public class NetworkMapLoadContentEvent : PathfinderEvent
    {
        public Hacknet.NetworkMap NetMap { get; }
        [Obsolete("Use NetMap")]
        public Hacknet.NetworkMap NetMapInstance => NetMap;
        public NetworkMapLoadContentEvent(Hacknet.NetworkMap netmap) { NetMap = netmap; }
    }
}
