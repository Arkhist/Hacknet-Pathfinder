using System;
namespace Pathfinder.Attribute
{
    public abstract class AbstractPathfinderAttribute : System.Attribute
    {
        public Type Mod { get; set; }

        protected AbstractPathfinderAttribute(Type mod)
        {
            Mod = mod;
        }
    }
}
