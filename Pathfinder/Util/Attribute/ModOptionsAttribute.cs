using System;
using Pathfinder.GUI.ModOptions;

namespace Pathfinder.Util.Attribute
{
    public class ModOptionsAttribute : System.Attribute
    {
        public Type ModOptionsType { get; }
        public ModOptionsAttribute(Type type)
        {
            ModOptionsType = type;
            if (!ModOptionsType.IsAssignableFrom(typeof(AbstractOptions)))
                throw new ArgumentException("must point to a derivative of AbstractOptions", nameof(type));
        }
        public ModOptionsAttribute(string name) : this(Type.GetType(name, true, false)) {}
    }
}
