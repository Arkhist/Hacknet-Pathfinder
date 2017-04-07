using System;
using System.Collections.Generic;

namespace Pathfinder.Event
{
    public static class EventManager
    {
        private static Dictionary<Type, List<Action<PathfinderEvent>>> eventListeners =
            new Dictionary<Type, List<Action<PathfinderEvent>>>();

        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Action<PathfinderEvent>>());
            }
            eventListeners[pathfinderEventType].Add((Action<PathfinderEvent>)listener);
        }

        public static void RegisterListener<T>(Action<PathfinderEvent> listener) where T : PathfinderEvent
        {
            RegisterListener(typeof(T), listener);
        }

        public static void RegisterListener<T>(Action<T> listener) where T : PathfinderEvent
        {
            RegisterListener<T>((e) => listener.Invoke((T)e));
        }

        public static void UnregisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Action<PathfinderEvent>>());
            }
            eventListeners[pathfinderEventType].Remove(listener);
        }

        public static void UnregisterListener<T>(Action<PathfinderEvent> listener) where T : PathfinderEvent
        {
            UnregisterListener(typeof(T), listener);
        }

        public static void UnregisterListener<T>(Action<T> listener) where T : PathfinderEvent
        {
            Type pathfinderEventType = typeof(T);
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Action<PathfinderEvent>>());
            }
            for (var i = eventListeners[pathfinderEventType].Count-1; i >= 0; i--)
            {
                var l = eventListeners[pathfinderEventType][i];
                try
                {
                    l.Method.Module.ResolveMethod(listener.Method.MetadataToken);
                    eventListeners[pathfinderEventType].Remove(l);
                }
                catch (Exception) {}
            }
        }

        public static void CallEvent(PathfinderEvent pathfinderEvent)
        {
            var eventType = pathfinderEvent.GetType();
            if (eventListeners.ContainsKey(eventType))
            {
                foreach (Action<PathfinderEvent> listener in eventListeners[eventType])
                {
                    listener(pathfinderEvent);
                }
            }
        }
    }
}
