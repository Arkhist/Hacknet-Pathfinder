using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

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
    public List<ElementInfo> Children { get; set; } = new List<ElementInfo>();
    public ulong NodeID { get; } = freeId++;
    public XmlNodeType Type { get; set; }
    public bool IsText => Type == XmlNodeType.Text;

    public ElementInfo() { Type = XmlNodeType.Element; }

    public ElementInfo(string name, string content = null, Dictionary<string, string> attributes = null, List<ElementInfo> children = null, ElementInfo parent = null)
        : this()
    {
        Name = name;
        if(content != null) Content = content;
        Attributes = attributes ?? new Dictionary<string, string>();
        Children = children ?? new List<ElementInfo>();
        Parent = parent;
    }

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

    public bool TryContentAsBoolean(out bool result)
        => Content.TryAsBoolean(out result);

    public bool TryContentAsInt(out int result)
        => Content.TryAsInt(out result);

    public bool TryContentAsFloat(out float result)
        => Content.TryAsFloat(out result);

    public bool GetContentAsBoolean(bool defaultVal = default)
        => TryContentAsBoolean(out var result) ? result : defaultVal;

    public int GetContentAsInt(int defaultVal = default)
        => TryContentAsInt(out var result) ? result : defaultVal;

    public float GetContentAsFloat(float defaultVal = default)
        => TryContentAsFloat(out var result) ? result : defaultVal;

    public bool ContentAsBoolean()
        => Content.AsBoolean(nameof(Content));

    public int ContentAsInt()
        => Content.AsInt(nameof(Content));

    public float ContentAsFloat()
        => Content.AsFloat(nameof(Content));

    public bool TryAttributeAsBoolean(string attribName, out bool result)
        => Attributes.TryAsBoolean(attribName, out result);

    public bool TryAttributeAsInt(string attribName, out int result)
        => Attributes.TryAsInt(attribName, out result);

    public bool TryAttributeAsFloat(string attribName, out float result)
        => Attributes.TryAsFloat(attribName, out result);

    public bool GetAttributeAsBoolean(string attribName, bool defaultVal = default)
        => TryAttributeAsBoolean(attribName, out var result) ? result : defaultVal;

    public int GetAttributeAsInt(string attribName, int defaultVal = default)
        => TryAttributeAsInt(attribName, out var result) ? result : defaultVal;

    public float GetAttributeAsFloat(string attribName, float defaultVal = default)
        => TryAttributeAsFloat(attribName, out var result) ? result : defaultVal;

    public bool AttributeAsBoolean(string attribName)
        => Attributes.AsBoolean(attribName, $"{nameof(Attributes)}");

    public int AttributeAsInt(string attribName)
        => Attributes.AsInt(attribName, $"{nameof(Attributes)}");

    public float AttributeAsFloat(string attribName)
        => Attributes.AsFloat(attribName, $"{nameof(Attributes)}");

    public bool TryAddChild(ElementInfo info)
    {
        if(Children == null) return false;
        if(info.Parent != null)
            info.Parent.Children.Remove(info);
        Children.Add(info);
        info.Parent = this;
        return Children.Last() == info;
    }

    public bool TrySetParent(ElementInfo info)
    {
        return info.TryAddChild(this);
    }

    public bool TrySetAttribute(string key, string value)
    {
        if(Attributes == null) return false;
        Attributes[key] = value;
        return Attributes[key] == value;
    }

    public bool TryGetAttribute(string key, ref string value)
    {
        if(!Attributes?.TryGetValue(key, out value) ?? true) return false;
        return true;
    }

    public ElementInfo AddChild(ElementInfo info)
    {
        TryAddChild(info);
        return this;
    }

    public ElementInfo SetParent(ElementInfo info)
    {
        TrySetParent(info);
        return this;
    }

    public ElementInfo SetAttribute(string key, string value)
    {
        TrySetAttribute(key, value);
        return this;
    }

    public string GetAttribute(string key, string defaultValue = null)
    {
        TryGetAttribute(key, ref defaultValue);
        return defaultValue;
    }

    public void WriteToXML(XmlWriter writer)
    {
        if(IsText)
        {
            writer.WriteString(Content);
            return;
        }
        writer.WriteStartElement(Name, "");
        foreach (var attr in Attributes)
            writer.WriteAttributeString(attr.Key, attr.Value);
        foreach (var child in Children)
            child.WriteToXML(writer);
        writer.WriteEndElement();
    }

    public static ElementInfo FromText(string input)
        => new ElementInfo
        {
            Content = input,
            Type = XmlNodeType.Text,
            Attributes = null,
            Children = null
        };
}

public static class ElementInfoStringExtensions
{
    public static bool TryAsBoolean(this string content, out bool result)
        => bool.TryParse(content, out result);
    public static bool TryAsInt(this string content, out int result)
        => int.TryParse(content, out result);
    public static bool TryAsFloat(this string content, out float result)
        => float.TryParse(content, out result);

    public static bool AsBooleanSafe(this string content, bool defaultVal = default)
        => content.TryAsBoolean(out var value)
            ? value
            : defaultVal;
    public static int AsIntSafe(this string content, int defaultVal = default)
        => content.TryAsInt(out var value)
            ? value
            : defaultVal;
    public static float AsFloatSafe(this string content, float defaultVal = default)
        => content.TryAsFloat(out var value)
            ? value
            : defaultVal;

    public static bool AsBoolean(this string content, string valName = "content")
        => content.TryAsBoolean(out var value)
            ? value
            : throw new FormatException($"Value of '{valName}' is '{content}' which is not true or false");
    public static int AsInt(this string content, string valName = "content")
        => content.TryAsInt(out var value)
            ? value
            : throw new FormatException($"Value of '{valName}' is '{content}' which is not an integer, e.g.: 0, 1, 2");
    public static float AsFloat(this string content, string valName = "content")
        => content.TryAsFloat(out var value)
            ? value
            : throw new FormatException($"Value of '{valName}' is '{content}' which is not a float, e.g.: 1.0");
}

public static class ElementInfoListExtensions
{
    public static ElementInfo GetElement(IEnumerable<ElementInfo> list, string elementName)
    {
        return list.FirstOrDefault(e => e.Name == elementName);
    }
    public static bool TryGetElement(IEnumerable<ElementInfo> list, string elementName, out ElementInfo info)
    {
        info = GetElement(list, elementName);
        return info != null;
    }
}

[Obsolete("Use ElementInfoListExtensions")]
public static class ListExtensions
{
    public static ElementInfo GetElement(this IEnumerable<ElementInfo> list, string elementName)
        => ElementInfoListExtensions.GetElement(list, elementName);
    public static bool TryGetElement(this IEnumerable<ElementInfo> list, string elementName, out ElementInfo info)
        => ElementInfoListExtensions.TryGetElement(list, elementName, out info);
}

public static class ElementInfoDictionaryExtensions
{
    public static bool TryAsBoolean(this IDictionary<string, string> attribute, string key, out bool result)
    {
        result = default;
        if(attribute == null) return false;
        return attribute.TryGetValue(key, out var str) ? str.TryAsBoolean(out result) : false;
    }
    public static bool TryAsInt(this IDictionary<string, string> attribute, string key, out int result)
    {
        result = default;
        if(attribute == null) return false;
        return attribute.TryGetValue(key, out var str) ? str.TryAsInt(out result) : false;
    }
    public static bool TryAsFloat(this IDictionary<string, string> attribute, string key, out float result)
    {
        result = default;
        if(attribute == null) return false;
        return attribute.TryGetValue(key, out var str) ? str.TryAsFloat(out result) : false;
    }
    public static bool AsBoolean(this IDictionary<string, string> attribute, string key, string dictName = "attribute")
        => attribute[key].AsBoolean($"{dictName}[{key}]");
    public static int AsInt(this IDictionary<string, string> attribute, string key, string dictName = "attribute")
        => attribute[key].AsInt($"{dictName}[{key}]");
    public static float AsFloat(this IDictionary<string, string> attribute, string key, string dictName = "attribute")
        => attribute[key].AsFloat($"{dictName}[{key}]");
}