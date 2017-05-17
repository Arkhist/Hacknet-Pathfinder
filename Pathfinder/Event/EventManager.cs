using System;
using System.Collections.Generic;
using System.IO;
using Pathfinder.Util;
using Pathfinder.Util.Attribute;

namespace Pathfinder.Event
{
    public static class EventManager
    {
        internal static Dictionary<Type, List<Tuple<Action<PathfinderEvent>, string, string, int>>> eventListeners =
            new Dictionary<Type, List<Tuple<Action<PathfinderEvent>, string, string, int>>>();

        private static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener, string debugName = null, int priority = 0)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
                eventListeners.Add(pathfinderEventType, new List<Tuple<Action<PathfinderEvent>, string, string, int>>());
            if (String.IsNullOrEmpty(debugName))
                debugName = "[" + Path.GetFileName(listener.Method.Module.Assembly.Location) + "] "
                                       + listener.Method.DeclaringType.FullName + "." + listener.Method.Name;
            var list = eventListeners[pathfinderEventType];
            list.Add(new Tuple<Action<PathfinderEvent>, string, string, int>(listener, debugName, Utility.ActiveModId, priority));
            list.Sort((x, y) => y.Item4 - x.Item4);
        }

        /// <summary>
        /// Registers an event listener by runtime type.
        /// </summary>
        /// <param name="pathfinderEventType">The PathfinderEvent Runtime Type to register for</param>
        /// <param name="listener">The listener function that will be executed on an event call</param>
        /// <param name="debugName">Name to assign for debug purposes</param>
        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener, string debugName = null)
        {
            RegisterListener(pathfinderEventType, listener, debugName,
                             listener.Method.GetPossibleFirstAttribute<EventPriorityAttribute>()?.Priority ?? 0);
        }

        /// <summary>
        /// Registers an event listener by compile time type.
        /// </summary>
        /// <param name="listener">The listener function that will be executed on an event call</param>
        /// <param name="debugName">Name to assign for debug purposes</param>
        /// <typeparam name="T">The PathfinderEvent Compile time Type to listen for</typeparam>
        public static void RegisterListener<T>(Action<T> listener, string debugName = null) where T : PathfinderEvent
        {
            RegisterListener(typeof(T), (e) => listener.Invoke((T)e),
                             String.IsNullOrEmpty(debugName) ?
                             "[" + Path.GetFileName(listener.Method.Module.Assembly.Location) + "] "
                             + listener.Method.DeclaringType.FullName + "." + listener.Method.Name : debugName,
                             listener.Method.GetPossibleFirstAttribute<EventPriorityAttribute>()?.Priority ?? 0);
        }

        /// <summary>
        /// Removes an event listener by runtime type.
        /// </summary>
        /// <param name="pathfinderEventType">The PathfinderEvent Runtime Type to remove for</param>
        /// <param name="listener">The listener function to remove</param>
        public static void UnregisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType))
                eventListeners.Add(pathfinderEventType, new List<Tuple<Action<PathfinderEvent>, string, string, int>>());
            var i = eventListeners[pathfinderEventType].FindIndex(l => l.Item1 == listener);
            eventListeners[pathfinderEventType].RemoveAt(i);
        }

        /// <summary>
        /// Removes an event listener by compile time type.
        /// </summary>
        /// <param name="listener">The listener function to remove</param>
        /// <typeparam name="T">The PathfinderEvent Compile time Type to remove for</typeparam>
        public static void UnregisterListener<T>(Action<T> listener) where T : PathfinderEvent
        {
            Type pathfinderEventType = typeof(T);
            if (!eventListeners.ContainsKey(pathfinderEventType))
                eventListeners.Add(pathfinderEventType, new List<Tuple<Action<PathfinderEvent>, string, string, int>>());
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

        /// <summary>
        /// Calls a PathfinderEvent.
        /// </summary>
        /// <param name="pathfinderEvent">The PathfinderEvent to call.</param>
        public static void CallEvent(PathfinderEvent pathfinderEvent)
        {

            var eventType = pathfinderEvent.GetType();
            var log = !Logger.IgnoreEventTypes.Contains(eventType);
            if(log)
                Logger.Verbose("Event Type contains check '[{0}] {1}'", Path.GetFileName(eventType.Assembly.Location), eventType.FullName);
            if (eventListeners.ContainsKey(eventType))
            {
                if(log)
                    Logger.Verbose("Attempting Event call '[{0}] {1}'", Path.GetFileName(eventType.Assembly.Location), eventType.FullName);
                foreach (var listener in eventListeners[eventType])
                {
                    try
                    {
                        if(log)
                            Logger.Verbose("Attempting Event Listener call '{0}'", listener.Item2);
                        Pathfinder.CurrentMod = Pathfinder.GetMod(listener.Item3);
                        listener.Item1(pathfinderEvent);
                        Pathfinder.CurrentMod = null;
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Event Listener Call Failed");
                        Logger.Error("Exception: {0}", ex);
                    }
                    if (pathfinderEvent.IsCancelled)
                        break;
                }
            }
        }
    }
}
