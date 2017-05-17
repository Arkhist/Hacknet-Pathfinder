namespace Pathfinder.Util.Attribute
{
    public class EventPriorityAttribute : System.Attribute
    {
        public int Priority { get; }

        public EventPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
