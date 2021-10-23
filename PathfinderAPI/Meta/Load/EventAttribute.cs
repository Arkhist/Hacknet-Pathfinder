using System.Linq;
using System;
using System.Reflection;
using Pathfinder.Event;
using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
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

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            var methodInfo = (MethodInfo)targettedInfo;
            var eventType = methodInfo.GetParameters().FirstOrDefault()?.ParameterType;
            EventManager.AddHandler(eventType, methodInfo);
        }
    }
}