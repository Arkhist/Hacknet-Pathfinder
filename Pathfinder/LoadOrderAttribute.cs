using System;

namespace Pathfinder
{
    [Obsolete("Use Pathfinder.Util.Attribute.LoadOrderAttribute instead")]
    public class LoadOrderAttribute : Util.Attribute.LoadOrderAttribute
    {
        public LoadOrderAttribute(string before = null, string after = null) : base(before, after) {}
    }
}
