using Pathfinder.Util.XML;

namespace Pathfinder.Event
{
    public class SaveEvent : PathfinderEvent
    {
        public ElementInfo Save { get; }
        public string Filename { get; }

        public SaveEvent(ElementInfo save, string filename)
        {
            Save = save;
            Filename = filename;
        }
    }
}