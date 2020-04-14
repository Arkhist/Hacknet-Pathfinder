using System;
using static Pathfinder.Event.EventManager;

namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventAttribute : AbstractPathfinderAttribute
    {
        public string DebugName;
        public int Priority { set => PriorityStore = value; get => PriorityStore.GetValueOrDefault(0); }
        public int? PriorityStore { get; set; } = null;
        public bool ContinueOnCancel;
        public bool ContinueOnThrow;

        internal ListenerOptions Options =>
            new ListenerOptions {
                DebugName = DebugName,
                PriorityStore = PriorityStore,
                ContinueOnCancel = ContinueOnCancel,
                ContinueOnThrow = ContinueOnCancel
            };

        public EventAttribute(string debugName = null, int priority = 0, Type mod = null) : base(mod)
        {
            DebugName = debugName;
            Priority = priority;
        }
    }
}
