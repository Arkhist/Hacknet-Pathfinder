namespace Pathfinder.Event
{
    public abstract class PathfinderEvent
    {
        public bool IsCancelled { get; set; }
        public bool PreventCall { get; set; }
        public void CallEvent() { EventManager.CallEvent(this); }
    }
}
