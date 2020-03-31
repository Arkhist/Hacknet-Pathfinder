using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Pathfinder.Attribute;
using Pathfinder.Internal;
using Pathfinder.ModManager;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public static class EventManager
    {
        public class ListenerTuple : Tuple<MethodInfo, string, string, int>
        {
            public InternalUtility.MethodInvoker Func { get; }
            public MethodInfo Info => Item1;
            public string DebugName => Item2;
            public string ModId => Item3;
            public int Priority => Item4;

            public ListenerTuple(MethodInfo item1, string item2, string item3, int item4) : base(item1, item2, item3, item4)
            {
                Func = Info.GetMethodInvoker();
            }
        }

        internal static Dictionary<Type, List<ListenerTuple>> eventListeners = new Dictionary<Type, List<ListenerTuple>>();

        private static void RegisterExpressionListener(Type pathfinderEventType, MethodInfo info, string debugName = null, int? priority = null)
        {
            if (string.IsNullOrEmpty(debugName))
                debugName = "[" + Path.GetFileName(info.Module.Assembly.Location + "] "
                                       + info.DeclaringType.FullName + "." + info.Name);
            InternalUtility.ValidateNoId("Event Listener", debugName, $" with priority {priority}");
            if (!eventListeners.ContainsKey(pathfinderEventType))
                eventListeners.Add(pathfinderEventType, new List<ListenerTuple>());
            var list = eventListeners[pathfinderEventType];
            list.Add(new ListenerTuple(info, debugName, Utility.ActiveModId, priority ?? info.GetFirstAttribute<EventAttribute>()?.Priority ?? 0));
            list.Sort((x, y) => y.Priority - x.Priority);
        }

        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener, string debugName = null, int? priority = null)
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterExpressionListener(pathfinderEventType, listener.Method, debugName, priority);
        }

        /// <summary>
        /// Registers an event listener by runtime type.
        /// </summary>
        /// <param name="pathfinderEventType">The PathfinderEvent Runtime Type to register for</param>
        /// <param name="listener">The listener function that will be executed on an event call</param>
        /// <param name="debugName">Name to assign for debug purposes</param>
        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener, string debugName = null)
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterListener(pathfinderEventType, listener, debugName);
        }

        public static void RegisterListener(Action<PathfinderEvent> listener, string debugName = null, int? priority = null)
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterExpressionListener(listener.Method.GetParameters()[0].ParameterType, listener.Method,
                string.IsNullOrEmpty(debugName)
                    ? "[" + Path.GetFileName(listener.Method.Module.Assembly.Location) + "] "
                        + listener.Method.DeclaringType.FullName + "." + listener.Method.Name
                    : debugName,
                priority);
        }

        /// <summary>
        /// Registers an event listener by compile time type.
        /// </summary>
        /// <param name="listener">The listener function that will be executed on an event call</param>
        /// <param name="debugName">Name to assign for debug purposes</param>
        /// <typeparam name="T">The PathfinderEvent Compile time Type to listen for</typeparam>
        public static void RegisterListener<T>(Action<T> listener, string debugName, int? priority)
            where T : PathfinderEvent
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterExpressionListener(typeof(T), listener.Method,
                string.IsNullOrEmpty(debugName)
                    ? "[" + Path.GetFileName(listener.Method.Module.Assembly.Location) + "] "
                        + listener.Method.DeclaringType.FullName + "." + listener.Method.Name
                    : debugName,
                priority);
        }

        public static void RegisterListener<T>(Action<T> listener, string debugName = null)
            where T : PathfinderEvent
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterListener(listener, debugName, null);
        }

        public static void UnregisterListener(Action<PathfinderEvent> listener)
            => UnregisterListener(listener.Method.GetParameters()[0].ParameterType, listener);


        /// <summary>
        /// Removes an event listener by runtime type.
        /// </summary>
        /// <param name="pathfinderEventType">The PathfinderEvent Runtime Type to remove for</param>
        /// <param name="listener">The listener function to remove</param>
        public static void UnregisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener)
        {
            if (!eventListeners.ContainsKey(pathfinderEventType)) return;
            eventListeners[pathfinderEventType].RemoveAll(l => l.Info.Equals(listener.Method));
        }

        /// <summary>
        /// Removes an event listener by compile time type.
        /// </summary>
        /// <param name="listener">The listener function to remove</param>
        /// <typeparam name="T">The PathfinderEvent Compile time Type to remove for</typeparam>
        public static void UnregisterListener<T>(Action<T> listener) where T : PathfinderEvent
            => UnregisterListener(typeof(T), (Action<PathfinderEvent>)listener);

        /// <summary>
        /// Calls a PathfinderEvent.
        /// </summary>
        /// <param name="pathfinderEvent">The PathfinderEvent to call.</param>
        public static void CallEvent(PathfinderEvent pathfinderEvent)
        {
            if (pathfinderEvent.PreventCall) return;
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
                        if(log) Logger.Verbose("Attempting Event Listener call '{0}'", listener.DebugName);
                        Manager.CurrentMod = Manager.GetLoadedMod(listener.ModId);
                        listener.Func(null, pathfinderEvent);
                        Manager.CurrentMod = null;
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Event Listener Call Failed");
                        Logger.Error("Exception: {0}", ex);
                    }
                    if (pathfinderEvent.IsCancelled) break;
                }
            }
        }
    }
}
