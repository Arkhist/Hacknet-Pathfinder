using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Pathfinder.Util.XML
{
    public class ElementInfo
    {
        private static ulong freeId = 0;
        
        public string Name;
        public string Content;
        public ElementInfo Parent;
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();
        public List<ElementInfo> Children = new List<ElementInfo>();
        public readonly ulong NodeID = freeId++;

        public override string ToString()
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true
            };
            using (var writer = XmlWriter.Create(builder, settings))
            {
                WriteToXML(writer);
            }

            return builder.Replace("\t", "  ").ToString();
        }

        public void WriteToXML(XmlWriter writer)
        {
            writer.WriteStartElement(Name, "");
            foreach (var attr in Attributes)
                writer.WriteAttributeString(attr.Key, attr.Value);
            if (Content == null)
            {
                foreach (var child in Children)
                    child.WriteToXML(writer);
            }
            else
            {
                writer.WriteValue(Content);
            }
            writer.WriteEndElement();
        }
    }

    public static class ListExtensions
    {
        public static ElementInfo GetElement(this List<ElementInfo> list, string elementName)
        {
            foreach (var possibleInfo in list)
            {
                if (possibleInfo.Name == elementName)
                {
                    return possibleInfo;
                }
            }

            return null;
        }
        public static bool TryGetElement(this List<ElementInfo> list, string elementName, out ElementInfo info)
        {
            info = GetElement(list, elementName);
            return info != null;
        }
    }
}
