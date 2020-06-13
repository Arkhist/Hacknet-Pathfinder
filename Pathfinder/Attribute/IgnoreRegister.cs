using System;
namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
    public class IgnoreRegister : System.Attribute {}
}
