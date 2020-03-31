using System;
using Pathfinder.ModManager;

namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class CommandModuleAttribute : AbstractPathfinderAttribute
    {
        public CommandModuleAttribute(Type mod = null) : base(mod) {}
    }
}
