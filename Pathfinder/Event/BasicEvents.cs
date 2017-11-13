namespace Pathfinder.Event
{
    public class NetworkMapLoadContentEvent : PathfinderEvent
    {
        public Hacknet.NetworkMap NetMap { get; }
        public NetworkMapLoadContentEvent(Hacknet.NetworkMap netmap) { NetMap = netmap; }
    }
}
