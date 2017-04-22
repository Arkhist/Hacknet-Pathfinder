using System;
using System.Collections.Generic;
using System.IO;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public static class EventManager
    {
        private static Dictionary<Type, List<Tuple<Action<PathfinderEvent>, string>>> eventListeners =
            new Dictionary<Type, List<Tuple<Action<PathfinderEvent>, string>>>();

        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener, string methodName = null)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Tuple<Action<PathfinderEvent>, string>>());
            }
            if (methodName == null)
                methodName = "[" + Path.GetFileName(listener.Method.Module.Assembly.Location) + "] "
                                       + listener.Method.DeclaringType.FullName + "." + listener.Method.Name;
            eventListeners[pathfinderEventType].Add(new Tuple<Action<PathfinderEvent>, string>(listener, methodName));
        }

        public static void RegisterListener<T>(Action<T> listener) where T : PathfinderEvent
        {
            RegisterListener(typeof(T), (e) => listener.Invoke((T)e),
                             "[" + Path.GetFileName(listener.Method.Module.Assembly.Location) + "] "
                                       + listener.Method.DeclaringType.FullName + "." + listener.Method.Name);
        }

        public static void UnregisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Tuple<Action<PathfinderEvent>, string>>());
            }
            var i = eventListeners[pathfinderEventType].FindIndex(l => l.Item1 == listener);
            eventListeners[pathfinderEventType].RemoveAt(i);
        }

        public static void UnregisterListener<T>(Action<T> listener) where T : PathfinderEvent
        {
            Type pathfinderEventType = typeof(T);
            if (!eventListeners.ContainsKey(pathfinderEventType))
            {
                eventListeners.Add(pathfinderEventType, new List<Tuple<Action<PathfinderEvent>, string>>());
            }
            for (var i = eventListeners[pathfinderEventType].Count-1; i >= 0; i--)
            {
                var l = eventListeners[pathfinderEventType][i];
                try
                {
                    if(l.Item1.Method.Module.ResolveMethod(listener.Method.MetadataToken).Equals(listener.Method))
                        eventListeners[pathfinderEventType].Remove(l);
                }
                catch (Exception) {}
            }
        }

        public static void CallEvent(PathfinderEvent pathfinderEvent)
        {
            var eventType = pathfinderEvent.GetType();
            Logger.Verbose("Event Type contains check '[{0}] {1}'", Path.GetFileName(eventType.Assembly.Location), eventType.FullName);
            if (eventListeners.ContainsKey(eventType))
            {
                Logger.Verbose("Attempting Event call '[{0}] {1}'", Path.GetFileName(eventType.Assembly.Location), eventType.FullName);
                foreach (var listener in eventListeners[eventType])
                {
                    try
                    {
                        Logger.Verbose("Attempting Event Listener call '{0}'", listener.Item2);
                        listener.Item1(pathfinderEvent);
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Event Listener Call Failed");
                        Logger.Error("Exception: ", ex);
                    }
                }
            }
        }
    }
}
