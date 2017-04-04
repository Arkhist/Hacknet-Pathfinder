using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Event
{

    public delegate void PathfinderEventHandler(PathfinderEvent pathfinderEvent);

    public abstract class PathfinderEvent
    {
        private static event PathfinderEventHandler eventListeners;

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

        public static event PathfinderEventHandler EventListeners
        {
            add
            {
                if(eventListeners != null)
                {
                    lock (eventListeners)
                        eventListeners += value;
                }
                else
                    eventListeners += value;

            }
            remove
            {
                if (eventListeners != null)
                {
                    lock (eventListeners)
                        eventListeners -= value;
                }
                else
                    eventListeners -= value;
            }
        }

        public void CallEvent()
        {
            if (eventListeners != null)
                eventListeners(this);
        }
    }
}
