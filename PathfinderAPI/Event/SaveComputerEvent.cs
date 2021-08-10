using System.Xml.Linq;
using Hacknet;

namespace Pathfinder.Event
{
    public class SaveComputerEvent : PathfinderEvent
    {
        public OS Os { get; }
        public Computer Comp { get; }
        public XElement Element { get; set; }

        public SaveComputerEvent(OS os, Computer comp, XElement element)
        {
            Os = os;
            Comp = comp;
            Element = element;
        }
    }
}
