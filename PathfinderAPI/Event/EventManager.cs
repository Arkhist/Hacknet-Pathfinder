using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace Pathfinder.Event
{
    public struct EventHandlerOptions
    {
        public int? Priority;
        public int PrioritySafe => Priority.GetValueOrDefault(0);
        public bool ContinueOnCancel;
        public bool ContinueOnThrow;
    }
    
    internal class EventHandler<T> : IComparable<EventHandler<T>>, IEquatable<EventHandler<T>>, IEquatable<MethodInfo> where T : PathfinderEvent
    {
        internal readonly Action<T> HandlerAction;
        internal readonly MethodInfo HandlerInfo;
        internal readonly EventHandlerOptions Options;

        internal EventHandler(Action<T> handlerAction, EventHandlerOptions options)
        {
            HandlerAction = handlerAction;
            Options = options;

            HandlerInfo = handlerAction.Method;
        }

        public int CompareTo(EventHandler<T> other) => Options.PrioritySafe.CompareTo(other.Options.PrioritySafe);

        public bool Equals(EventHandler<T> other) => HandlerInfo.Equals(other?.HandlerInfo);

        public bool Equals(MethodInfo other) => HandlerInfo.Equals(other);
    }
    
    public static class EventManager
    {
        internal static readonly List<object> Instances = new List<object>();
        internal static void AddEventManagerInstance(object managerObj)
        {
            Instances.Add(managerObj);
        }
        internal delegate void RemoveOnUnload(Assembly unloadedAssembly);
        internal static event RemoveOnUnload onPluginUnload;
        internal static void InvokeOnPluginUnload(Assembly pluginAsm) => onPluginUnload?.Invoke(pluginAsm);

        /// <summary>
        /// USE IS HEAVILY DISCOURAGED!
        /// Use the generic EventManager class instead unless you absolutely need to evaluate the type at runtime.
        /// </summary>
        /// <param name="pathfinderEvent">The type of event to subscribe to, must inherit from <see cref="PathfinderEvent"/>></param>
        /// <param name="handler">MethodInfo of the handler method</param>
        /// <exception cref="ArgumentException"></exception>
        public static void AddHandler(Type pathfinderEvent, MethodInfo handler)
        {
            if (pathfinderEvent.BaseType != typeof(PathfinderEvent))
            {
                throw new ArgumentException("Type must derive from PathfinderAPI.Event.PathfinderEvent!", nameof(pathfinderEvent));
            }
            var parameters = handler.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != pathfinderEvent)
            {
                throw new ArgumentException("Handler method must have one parameter of the same type of event you're subscribing to!", nameof(handler));
            }

            object handlerDelegate = typeof(AccessTools)
                .GetMethod("MethodDelegate")
                .MakeGenericMethod(typeof(Action<>).MakeGenericType(pathfinderEvent))
                .Invoke(null, new object[] { handler, null, true });

            typeof(EventManager<>)
                .MakeGenericType(pathfinderEvent)
                .GetMethod("AddHandler", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { handlerDelegate });
        }
    }

    /// <summary>
    /// Manager for <see cref="PathfinderEvent"/> handlers
    /// </summary>
    /// <typeparam name="T">The type of <see cref="PathfinderEvent"/></typeparam>
    public class EventManager<T> where T : PathfinderEvent
    {
        private readonly Dictionary<Assembly, List<EventHandler<T>>> handlers = new Dictionary<Assembly, List<EventHandler<T>>>();

        private static EventManager<T> _instance = null;

        public static EventManager<T> Instance => _instance ?? (_instance = new EventManager<T>());

        private EventManager()
        {
            EventManager.AddEventManagerInstance(this);
            EventManager.onPluginUnload += OnPluginUnload;
        }
        
        /// <summary>
        /// Adds an event handler, optionally with custom settings
        /// </summary>
        /// <param name="handler">The event handler to add</param>
        /// <param name="options">Options to use with the handler</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddHandler(Action<T> handler, EventHandlerOptions options = default)
        {
            Instance.AddHandlerInternal(new EventHandler<T>(handler, options), Assembly.GetCallingAssembly());
        }
        private void AddHandlerInternal(EventHandler<T> handler, Assembly eventAssembly)
        {
            if (!handlers.ContainsKey(eventAssembly))
                handlers.Add(eventAssembly, new List<EventHandler<T>>());
            var eventList = handlers[eventAssembly];
            eventList.Add(handler);
            eventList.Sort();
        }

        /// <summary>
        /// Removes a handler based on the handler method and its containing assembly
        /// </summary>
        /// <param name="handler">Handler method for the event</param>
        /// <param name="eventAssembly">Assembly associated with the event, by default the calling assembly</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RemoveHandler(Action<T> handler, Assembly eventAssembly = null)
        {
            Instance.RemoveHandlerInternal(handler.Method, eventAssembly ?? Assembly.GetCallingAssembly());
        }
        private void RemoveHandlerInternal(MethodInfo handler, Assembly eventAssembly)
        {
            if (handlers.TryGetValue(eventAssembly, out List<EventHandler<T>> handlersList))
            {
                handlersList.RemoveAll(x => x.HandlerInfo.Equals(handler));
                handlersList.Sort();
            }
        }

        private void OnPluginUnload(Assembly pluginAsm)
        {
            handlers.Remove(pluginAsm);
        }

        internal static T InvokeAll(T eventArgs)
        {
            foreach (var handler in Instance.handlers.Values.SelectMany(x => x))
            {
                try
                {
                    if ((handler.Options.ContinueOnThrow || !eventArgs.Thrown) && (handler.Options.ContinueOnCancel || !eventArgs.Cancelled))
                        handler.HandlerAction(eventArgs);
                }
                catch (Exception e)
                {
                    Logger.Log(BepInEx.Logging.LogLevel.Error, $"{handler.HandlerInfo.DeclaringType.FullName}::{handler.HandlerInfo.FullDescription()}");
                    Logger.Log(BepInEx.Logging.LogLevel.Error, e);
                    eventArgs.Thrown = true;
                }
            }

            return eventArgs;
        }
    }
}
