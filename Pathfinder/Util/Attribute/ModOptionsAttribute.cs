using System;
namespace Pathfinder.Util.Attribute
{
    public class ModOptionsAttribute : System.Attribute
    {
        public string ModOptionsFullName { get; }
        public ModOptionsAttribute(string name) { ModOptionsFullName = name; }
    }
}
