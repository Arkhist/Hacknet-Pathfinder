using System;

namespace Pathfinder.Util
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class InitializeAttribute : Attribute { }
}
