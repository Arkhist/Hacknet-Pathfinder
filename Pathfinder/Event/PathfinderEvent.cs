namespace Pathfinder.Event
{

    public delegate void PathfinderEventHandler(PathfinderEvent pathfinderEvent);

    public abstract class PathfinderEvent
    {
        private bool cancelled = false;

        public bool IsCancelled
        {
            get
            {
                return cancelled;
            }
            set
            {
                cancelled = value;
            }
        }

        public void CallEvent()
        {
            EventManager.CallEvent(this);
        }
    }
}
