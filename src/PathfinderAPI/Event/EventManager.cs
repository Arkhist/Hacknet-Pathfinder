using System.Reflection;
using HarmonyLib;
using System.Runtime.CompilerServices;
using Pathfinder.Util;

namespace Pathfinder.Event;

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
    internal static readonly List<object> Instances = [];
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
    /// <param name="options">Options to use for the handler</param>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddHandler(Type pathfinderEvent, MethodInfo handler, EventHandlerOptions options = default)
    {
        pathfinderEvent.ThrowNotInherit<PathfinderEvent>(nameof(pathfinderEvent));
        var parameters = handler.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != pathfinderEvent)
        {
            throw new ArgumentException("Handler method must have one parameter of the same type of event you're subscribing to!", nameof(handler));
        }

        object handlerDelegate = typeof(AccessTools)
            .GetMethod("MethodDelegate", [typeof(MethodInfo), typeof(object), typeof(bool)])
            .MakeGenericMethod(typeof(Action<>).MakeGenericType(pathfinderEvent))
            .Invoke(null, [handler, null, true]);

        object eventHandler = Activator.CreateInstance(
            typeof(EventHandler<>).MakeGenericType(pathfinderEvent),
            AccessTools.all,
            default,
            [handlerDelegate, options],
            default
        );

        var eventManagerType = typeof(EventManager<>).MakeGenericType(pathfinderEvent);

        object instance = eventManagerType.GetProperty("Instance").GetGetMethod().Invoke(null, null);
            
        eventManagerType
            .GetMethod("AddHandlerInternal", AccessTools.all)
            .Invoke(instance, [eventHandler, handler.Module.Assembly]);
    }
}

/// <summary>
/// Manager for <see cref="PathfinderEvent"/> handlers
/// </summary>
/// <typeparam name="T">The type of <see cref="PathfinderEvent"/></typeparam>
public class EventManager<T> where T : PathfinderEvent
{
    private readonly AssemblyAssociatedList<EventHandler<T>> handlers = new AssemblyAssociatedList<EventHandler<T>>();

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
        handlers.Add(handler, eventAssembly);
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
        handlers.RemoveAll(x => x.Equals(handler), eventAssembly);
    }

    /// <summary>
    /// Number of event handlers attached to this event type
    /// </summary>
    public static int HandlerCount => Instance.handlers.AllItems.Count;

    private void OnPluginUnload(Assembly pluginAsm)
    {
        handlers.RemoveAssembly(pluginAsm, out _);
    }

    private static T InvokeOn(IEnumerable<EventHandler<T>> list, T eventArgs)
    {
        foreach (var handler in list)
        {
            try
            {
                if ((handler.Options.ContinueOnThrow || !eventArgs.Thrown) && (handler.Options.ContinueOnCancel || !eventArgs.Cancelled))
                    handler.HandlerAction(eventArgs);
            }
            catch (Exception e)
            {
                Logger.Log(global::BepInEx.Logging.LogLevel.Error, $"{handler.HandlerInfo.DeclaringType.FullName}::{handler.HandlerInfo.FullDescription()}");
                Logger.Log(global::BepInEx.Logging.LogLevel.Error, e);
                eventArgs.Thrown = true;
            }
        }
        return eventArgs;
    }

    public static T InvokeAll(T eventArgs)
    {
        var allHandlers = Instance.handlers.AllItems.ToList();
        allHandlers.Sort();
        return InvokeOn(allHandlers, eventArgs);
    }

    public static T InvokeAssembly(Assembly asm, T eventArgs)
    {
        var asmHandlers = Instance.handlers[asm].ToList();
        asmHandlers.Sort();
        return InvokeOn(asmHandlers, eventArgs);
    }
}