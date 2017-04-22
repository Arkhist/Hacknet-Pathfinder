using System;
using Hacknet;

namespace Pathfinder.Event
{
    public class NetworkMapLoadContentEvent : PathfinderEvent
    {
        public Hacknet.NetworkMap NetMap { get; private set; }
        [Obsolete("Use NetMap")]
        public Hacknet.NetworkMap NetMapInstance => NetMap;
        public NetworkMapLoadContentEvent(Hacknet.NetworkMap netmap) { NetMap = netmap; }
    }
}
