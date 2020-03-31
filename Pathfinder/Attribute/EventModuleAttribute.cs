using System;
namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class EventModuleAttribute : AbstractPathfinderAttribute
    {
        public EventModuleAttribute(Type mod = null) : base(mod) {}
    }
}
