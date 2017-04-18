using System;
using System.Collections.Generic;
using Pathfinder.Util;

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
            eventListeners[pathfinderEventType].Add(listener);
        }

        public static void RegisterListener<T>(Action<T> listener) where T : PathfinderEvent
        {
            RegisterListener(typeof(T), (e) => listener.Invoke((T)e));
        }

        public static void UnregisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Action<PathfinderEvent>>());
            }
            eventListeners[pathfinderEventType].Remove(listener);
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
                    if(l.Method.Module.ResolveMethod(listener.Method.MetadataToken).Equals(listener.Method))
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
                Logger.Verbose("Attepting to call event {0}", eventType.FullName);
                foreach (var listener in eventListeners[eventType])
                {
                    try
                    {
                        Logger.Verbose("Attempting to call event listener {0}", listener.Method.Name);
                        listener(pathfinderEvent);
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Event Call Failed");
                        Logger.Error("Exception: ", ex);
                    }
                }
            }
        }
    }
}
