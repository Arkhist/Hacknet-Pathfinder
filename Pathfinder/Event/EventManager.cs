using Pathfinder.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Event
{
    public static class EventManager
    {
        private static Dictionary<Type, List<Action<PathfinderEvent>>> eventListeners = 
            new Dictionary<Type, List<Action<PathfinderEvent>>>();


        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if(!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Action<PathfinderEvent>>());
            }
            eventListeners[pathfinderEventType].Add(listener);
        }

        public static void UnregisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Action<PathfinderEvent>>());
            }
            eventListeners[pathfinderEventType].Remove(listener);
        }

        public static void CallEvent(PathfinderEvent pathfinderEvent)
        {
            Type eventType = pathfinderEvent.GetType();
            if(eventListeners.ContainsKey(eventType))
            {
                foreach(Action<PathfinderEvent> listener in eventListeners[eventType])
                {
                    listener(pathfinderEvent);
                }
            }
        }
    }
}
