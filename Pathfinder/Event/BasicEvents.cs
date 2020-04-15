namespace Pathfinder.Event
{
    public class ActionsLoadIntoOSEvent : PathfinderEvent
    {
        public Hacknet.OS OS { get; }
        public string FilePath { get; }

        public ActionsLoadIntoOSEvent(string filePath, Hacknet.OS os)
        {
            FilePath = filePath;
            OS = os;
        }
    }
    public class NetworkMapLoadContentEvent : PathfinderEvent
    {
        public Hacknet.NetworkMap NetMap { get; }
        public NetworkMapLoadContentEvent(Hacknet.NetworkMap netmap) { NetMap = netmap; }
    }
}
