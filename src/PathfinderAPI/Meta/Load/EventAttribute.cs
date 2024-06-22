using System.Reflection;
using Pathfinder.Event;
using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class EventAttribute : BaseAttribute
{
    private EventHandlerOptions eventHandlerOptions;
    public int Priority
    {
        set => eventHandlerOptions.Priority = value;
    }
    public bool ContinueOnCancel
    {
        set => eventHandlerOptions.ContinueOnCancel = value;
    }
    public bool ContinueOnThrow
    {
        set => eventHandlerOptions.ContinueOnThrow = value;
    }

    public EventAttribute()
    {
    }

    public EventAttribute(int priority, bool continueOnCancel = false, bool continueOnThrow = false)
    {
        eventHandlerOptions = new EventHandlerOptions{
            Priority = priority,
            ContinueOnCancel = continueOnCancel,
            ContinueOnThrow = continueOnThrow
        };
    }

    public EventAttribute(bool continueOnCancel = false, bool continueOnThrow = false)
    {
        eventHandlerOptions = new EventHandlerOptions{
            ContinueOnCancel = continueOnCancel,
            ContinueOnThrow = continueOnThrow
        };
    }

    private static void CallOn(HacknetPlugin plugin, MethodInfo info)
    {
        if(!info.IsStatic)
            throw new InvalidOperationException("EventAttribute can not register event handlers to instance methods");
        var eventType = info.GetParameters().FirstOrDefault()?.ParameterType;
        EventManager.AddHandler(eventType, info);
    }

    private static void CallOn(HacknetPlugin plugin, Type type)
    {
        foreach(var method in type.GetMethods(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Static
                ))
        {
            if(method.GetCustomAttribute<IgnoreEventAttribute>() != null)
                continue;
            var parameter = method.GetParameters().FirstOrDefault();
            if(method.ReturnType == typeof(void)
               && (parameter?.ParameterType?.IsSubclassOf(typeof(PathfinderEvent)) ?? false))
                CallOn(plugin, method);
        }
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        if(targettedInfo is MethodInfo method)
            CallOn(plugin, method);
        else
            CallOn(plugin, (Type) targettedInfo);
    }
}