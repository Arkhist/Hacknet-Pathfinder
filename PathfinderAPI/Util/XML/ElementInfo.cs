using System.Collections.Generic;
using System.Xml;

namespace Pathfinder.Util.XML
{
    public class ElementInfo
    {
        private static ulong freeId = 0;
        
        public string Name;
        public string Content;
        public ElementInfo Parent;
        public readonly Dictionary<string, string> Attributes = new Dictionary<string, string>();
        public readonly List<ElementInfo> Children = new List<ElementInfo>();
        public readonly ulong NodeID = freeId++;
    }
}
