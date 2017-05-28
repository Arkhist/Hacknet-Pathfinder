namespace Pathfinder.Util.Attribute
{
    /// <summary>
    /// Event priority attribute for designating event listener call order.
    /// </summary>
    public class EventPriorityAttribute : System.Attribute
    {
        public int Priority { get; }
        public EventPriorityAttribute(int priority) { Priority = priority; }
    }
}
