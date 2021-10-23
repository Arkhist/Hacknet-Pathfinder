using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SaveExecutorAttribute : BaseAttribute
    {
        public string Element { get; }
        public ParseOption ParseOptions { get; set; }

        public SaveExecutorAttribute(string element, ParseOption parseOptions = ParseOption.None)
        {
            Element = element;
            ParseOptions = parseOptions;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            SaveLoader.RegisterExecutor((Type)targettedInfo, Element, ParseOptions);
        }
    }
}