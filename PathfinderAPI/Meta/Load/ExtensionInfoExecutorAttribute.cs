using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExtensionInfoExecutorAttribute : BaseAttribute
{
    public string Element { get; }
    public ParseOption ParseOptions { get; set; }

    public ExtensionInfoExecutorAttribute(string element, ParseOption parseOptions = ParseOption.None)
    {
        Element = element;
        ParseOptions = parseOptions;
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        ExtensionInfoLoader.RegisterExecutor((Type)targettedInfo, Element, ParseOptions);
    }
}