using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ComputerExecutorAttribute : BaseAttribute
    {
        public string Element { get; }
        public ParseOption ParseOptions { get; set; }

        public ComputerExecutorAttribute(string element, ParseOption parseOptions = ParseOption.None)
        {
            Element = element;
            ParseOptions = parseOptions;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            ContentLoader.RegisterExecutor((Type)targettedInfo, Element, ParseOptions);
        }
    }
}