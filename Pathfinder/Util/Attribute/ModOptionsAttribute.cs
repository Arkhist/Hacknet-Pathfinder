using System;
using Pathfinder.GUI.ModOptions;

namespace Pathfinder.Util.Attribute
{
    /// <summary>
    /// Mod options attribute which allows assignment of a derived <see cref="AbstractOptions"/> class for a mod.
    /// </summary>
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
