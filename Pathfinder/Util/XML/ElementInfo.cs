using System;
using System.Collections.Generic;
using System.Xml;

namespace Pathfinder.Util.XML
{
    public class ElementInfo : IEquatable<ElementInfo>
    {
        public ElementInfo Parent { get; internal set; }
        public string Name { get; internal set; }
        public string Value { get; internal set; }
        public int Depth { get; internal set; }
        public IDictionary<string, string> Attributes { get; internal set; }
        public List<ElementInfo> Children { get; internal set; }
        internal int LocalNodeId { get; set; }

        public override bool Equals(object obj)
            => obj is ElementInfo ei && Equals(ei);

        public bool Equals(ElementInfo other)
            => Name == other?.Name
                && Value == other?.Value
                && Depth == other?.Depth
                && Parent.LocalNodeId == other?.Parent?.LocalNodeId
                && LocalNodeId == other?.LocalNodeId;

        public bool RepresentsNode(XmlReader reader)
            => (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
                && Name == reader.Name
                && (reader.NodeType == XmlNodeType.EndElement
                    && Depth == reader.Depth)
                || (reader.NodeType == XmlNodeType.Element
                    && Depth == reader.Depth
                    && Attributes.Count == reader.AttributeCount);

        public override int GetHashCode()
        {
            var hashCode = 1922712106;
            hashCode = hashCode * -1521134295 + Parent.LocalNodeId.GetHashCode();
            hashCode = hashCode * -1521134295 + LocalNodeId.GetHashCode();
            hashCode = hashCode * -1521134295 + Depth.GetHashCode();
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }

        internal void ConcatValue(string value) => Value += value;

        public static bool operator ==(ElementInfo lhs, ElementInfo rhs)
            => (lhs != null ? lhs.Equals(rhs) : (rhs == null));

        public static bool operator !=(ElementInfo lhs, ElementInfo rhs)
            => !(lhs == rhs);
    }
}