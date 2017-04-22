using System.Xml;

namespace Pathfinder.Event
{
    public class ComputerEvent : PathfinderEvent
    {
        public Hacknet.Computer Computer { get; private set; }
        public ComputerEvent(Hacknet.Computer com) { Computer = com; }
    }

    public class LoadComputerXmlReadEvent : ComputerEvent
    {
        public XmlReader Reader { get; private set; }
        public string Filename { get; private set; }
        public bool PreventNetmapAdd { get; private set; }
        public bool PreventDaemonInit { get; private set; }
        public LoadComputerXmlReadEvent(Hacknet.Computer com, XmlReader red, string fnam, bool prevNetmap, bool prevDaeInit) : base(com)
        {
            Reader = red;
            Filename = fnam;
            PreventNetmapAdd = prevNetmap;
            PreventDaemonInit = prevDaeInit;
        }
    }
}
