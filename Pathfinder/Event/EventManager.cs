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
        public struct ListenerOptions
        {
            public string DebugName { get; internal set; }
            public int? PriorityStore { get; internal set; }
            public int Priority => PriorityStore.GetValueOrDefault(0);
            public bool ContinueOnCancel { get; set; }
            public bool ContinueOnThrow { get; set; }
        }

        public class ListenerObject
        {
            public InternalUtility.MethodInvoker Func { get; private set; }
            public MethodInfo Info { get; private set; }
            public string ModId { get; }
            public ListenerOptions Options;

            public ListenerObject(MethodInfo info, ListenerOptions options, int? priority = null, string modId = null)
            {
                Info = info;
                Func = Info.GetMethodInvoker();

                var attrib = info.GetFirstAttribute<EventAttribute>();

                if (string.IsNullOrEmpty(modId))
                    ModId = Utility.ActiveModId;
                else ModId = modId;

                if (string.IsNullOrEmpty(options.DebugName))
                    options.DebugName = "[" + Path.GetFileName(info.Module.Assembly.Location + "] "
                                          + info.DeclaringType.FullName + "." + info.Name);

                options.PriorityStore = options.PriorityStore ?? attrib?.Priority ?? 0;
                options.ContinueOnCancel = options.ContinueOnCancel || (attrib?.ContinueOnCancel ?? false);
                options.ContinueOnThrow = options.ContinueOnThrow || (attrib?.ContinueOnThrow ?? false);
            }
        }

        internal static Dictionary<Type, List<ListenerObject>> eventListeners = new Dictionary<Type, List<ListenerObject>>();

        private static void RegisterExpressionListener(Type pathfinderEventType, ListenerObject listenerObj)
        {
            InternalUtility.ValidateNoId("Event Listener", listenerObj.Options.DebugName, $" with priority {listenerObj.Options.Priority}");
            if (!eventListeners.ContainsKey(pathfinderEventType))
                eventListeners.Add(pathfinderEventType, new List<ListenerObject>());
            var list = eventListeners[pathfinderEventType];
            list.Add(listenerObj);
            list.Sort((x, y) => y.Options.Priority - x.Options.Priority);
        }

        public static void RegisterListener(Type pathfinderEventType, Action<PathfinderEvent> listener, ListenerOptions options)
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterExpressionListener(pathfinderEventType, new ListenerObject(listener.Method, options));
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
            RegisterExpressionListener(listener.Method.GetParameters()[0].ParameterType,
                new ListenerObject(listener.Method, new ListenerOptions { PriorityStore = priority }));
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
            RegisterExpressionListener(typeof(T), new ListenerObject(listener.Method,
                new ListenerOptions { DebugName = debugName, PriorityStore = priority }));
        }

        public static void RegisterListener<T>(Action<T> listener, string debugName = null)
            where T : PathfinderEvent
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterListener(listener, debugName, null);
        }

        public static void RegisterListener<T>(Action<T> listener, ListenerOptions options)
            where T : PathfinderEvent
        {
            InternalUtility.ValidateNoId(log: false);
            RegisterExpressionListener(typeof(T), new ListenerObject(listener.Method, options));
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
        public static Dictionary<string, Exception> CallEvent(PathfinderEvent pathfinderEvent)
        {
            var exceptionDict = new Dictionary<string, Exception>();
            if (pathfinderEvent.PreventCall) return exceptionDict;
            var eventType = pathfinderEvent.GetType();
            var log = !Logger.IgnoreEventTypes.Contains(eventType);
            if(log)
                Logger.Verbose("Event Type contains check '[{0}] {1}'", Path.GetFileName(eventType.Assembly.Location), eventType.FullName);
            if (eventListeners.ContainsKey(eventType))
            {
                if(log)
                    Logger.Verbose("Attempting Event call '[{0}] {1}'", Path.GetFileName(eventType.Assembly.Location), eventType.FullName);
                var cancelled = false;
                var thrown = false;
                foreach (var listener in eventListeners[eventType])
                {
                    if (cancelled && listener.Options.ContinueOnCancel) continue;
                    if (thrown && listener.Options.ContinueOnThrow) continue;
                    try
                    {
                        if(log) Logger.Verbose("Attempting Event Listener call '{0}'", listener.Options.DebugName);
                        Manager.CurrentMod = Manager.GetLoadedMod(listener.ModId);
                        listener.Func(null, pathfinderEvent);
                        Manager.CurrentMod = null;
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Event Listener Call Failed");
                        Logger.Error("Exception: {0}", ex);
                        exceptionDict[listener.Options.DebugName] = ex;
                        thrown = true;
                    }
                    cancelled |= pathfinderEvent.IsCancelled;
                }
            }
            return exceptionDict;
        }
    }
}
