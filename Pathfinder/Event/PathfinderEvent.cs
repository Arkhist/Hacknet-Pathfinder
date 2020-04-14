using System;
using System.Collections.Generic;

namespace Pathfinder.Event
{
    public abstract class PathfinderEvent
    {
        public bool IsCancelled { get; set; }
        public bool PreventCall { get; set; }
        public virtual Dictionary<string, Exception> CallEvent() { return EventManager.CallEvent(this); }
    }
}
