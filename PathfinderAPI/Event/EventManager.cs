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
        public int PrioritySafe { get => Priority.GetValueOrDefault(0); }
        public bool ContinueOnCancel;
        public bool ContinueOnThrow;
    }
    
    internal class EventHandler<T> : IComparable<EventHandler<T>>, IEquatable<EventHandler<T>>, IEquatable<MethodInfo> where T : PathfinderEvent
    {
        internal Action<T> HandlerAction;
        internal MethodInfo HandlerInfo;
        internal EventHandlerOptions Options;

        internal EventHandler(Action<T> handlerAction, EventHandlerOptions options)
        {
            HandlerAction = handlerAction;
            Options = options;

            HandlerInfo = handlerAction.Method;
        }

        public int CompareTo(EventHandler<T> other) => Options.PrioritySafe.CompareTo(other.Options.PrioritySafe);

        public bool Equals(EventHandler<T> other) => HandlerInfo.Equals(other.HandlerInfo);

        public bool Equals(MethodInfo other) => HandlerInfo.Equals(other);
    }
    
    public static class EventManager
    {
        internal static List<object> Instances = new List<object>();
        internal static void AddEventManagerInstance(object managerObj)
        {
            Instances.Add(managerObj);
        }
        internal delegate void RemoveOnUnload(Assembly unloadedAssembly);
        internal static event RemoveOnUnload onPluginUnload;
        internal static void InvokeOnPluginUnload(Assembly pluginAsm) => onPluginUnload?.Invoke(pluginAsm);

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

    public class EventManager<T> where T : PathfinderEvent
    {
        private Dictionary<Assembly, List<EventHandler<T>>> handlers = new Dictionary<Assembly, List<EventHandler<T>>>();

        private static EventManager<T> _instance = null;

        public static EventManager<T> Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventManager<T>();
                return _instance;
            }
        }

        EventManager()
        {
            EventManager.AddEventManagerInstance(this);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddHandler(Action<T> handler)
        {
            Instance.AddHandlerInternal(new EventHandler<T>(handler, new EventHandlerOptions()), Assembly.GetCallingAssembly());
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddHandler(Action<T> handler, EventHandlerOptions options)
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RemoveHandler(Action<T> handler)
        {
            Instance.RemoveHandlerInternal(handler.Method, Assembly.GetCallingAssembly());
        }
        public static void RemoveHandler(Action<T> handler, Assembly eventAssembly)
        {
            Instance.RemoveHandlerInternal(handler.Method, eventAssembly);
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

        internal static void InvokeAllTesting() => InvokeAll((T)Activator.CreateInstance(typeof(T)));
    }
}
