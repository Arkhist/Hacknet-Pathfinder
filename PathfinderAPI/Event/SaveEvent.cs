using System.Xml.Linq;
using Pathfinder.Util.XML;

namespace Pathfinder.Event
{
    public class SaveEvent : PathfinderEvent
    {
        public XElement Save { get; }
        public string Filename { get; }

        public SaveEvent(XElement save, string filename)
        {
            Save = save;
            Filename = filename;
        }
    }
}