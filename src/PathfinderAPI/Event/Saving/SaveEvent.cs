using System.Xml.Linq;
using Hacknet;

namespace Pathfinder.Event.Saving;

public class SaveEvent : PathfinderEvent
{
    public OS Os { get; }
    public XElement Save { get; }
    public string Filename { get; }

    public SaveEvent(OS os, XElement save, string filename)
    {
        Os = os;
        Save = save;
        Filename = filename;
    }
}