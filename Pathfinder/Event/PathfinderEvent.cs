namespace Pathfinder.Event
{
    public abstract class PathfinderEvent
    {
        public bool IsCancelled { get; set; }
        public bool PreventCall { get; set; }
        public virtual void CallEvent() { EventManager.CallEvent(this); }
    }
}
