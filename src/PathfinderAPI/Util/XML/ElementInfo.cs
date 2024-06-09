using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Pathfinder.Util.XML;

public class ElementInfo
{
    private static ulong freeId = 0;
        
    public string Name { get; set; }
    public string Content {
        get;
        set;
    } = null;
    public ElementInfo Parent { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    public List<ElementInfo> Children { get; set; } = [];
    public ulong NodeID { get; } = freeId++;

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

    public bool ContentAsBoolean()
        => bool.TryParse(Content, out var value)
            ? value
            : throw new FormatException($"Value of '{Name}' is not true or false");

    public int ContentAsInt()
        => int.TryParse(Content, out var value)
            ? value
            : throw new FormatException($"Value of '{Name}' is not an integer, e.g.: 0, 1, 2");
        
    public float ContentAsFloat()
        => float.TryParse(Content, out var value)
            ? value
            : throw new FormatException($"Value of '{Name}' is not a float, e.g.: 1.0");

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

    public XElement ConvertToXElement()
    {
        XElement Xel = new(Name, Content);
        foreach (KeyValuePair<string, string> elAt in Attributes)
        {
            Xel.SetAttributeValue(elAt.Key, elAt.Value);
        }
        foreach (ElementInfo elx in Children)
        {
            Xel.Add(elx.ConvertToXElement());
        }
        return Xel;
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
