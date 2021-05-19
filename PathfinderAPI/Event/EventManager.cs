using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;

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
        private List<EventHandler<T>> handlers = new List<EventHandler<T>>();

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
        }

        public static void AddHandler(Action<T> handler)
        {
            Instance.AddHandlerInternal(new EventHandler<T>(handler, new EventHandlerOptions()));
        }
        public static void AddHandler(Action<T> handler, EventHandlerOptions options)
        {
            Instance.AddHandlerInternal(new EventHandler<T>(handler, options));
        }
        private void AddHandlerInternal(EventHandler<T> handler)
        {
            handlers.Add(handler);
            handlers.Sort();
        }

        public static void RemoveHandler(Action<T> handler)
        {
            Instance.RemoveHandlerInternal(handler.Method);
        }
        private void RemoveHandlerInternal(MethodInfo handler)
        {
            handlers.RemoveAll(x => x.Equals(handler));
            handlers.Sort();
        }

        internal static T InvokeAll(T eventArgs)
        {
            foreach (var handler in Instance.handlers)
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
