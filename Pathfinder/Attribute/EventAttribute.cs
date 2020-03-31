using System;
namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventAttribute : AbstractPathfinderAttribute
    {
        public string DebugName;
        public int Priority;

        public EventAttribute(string debugName = null, int priority = 0, Type mod = null) : base(mod)
        {
            DebugName = debugName;
            Priority = priority;
        }
    }
}
