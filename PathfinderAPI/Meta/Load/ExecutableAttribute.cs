using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Executable;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExecutableAttribute : BaseAttribute
    {
        public string XmlName { get; }

        public ExecutableAttribute(string xmlName)
        {
            XmlName = xmlName;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            ExecutableManager.RegisterExecutable((Type)targettedInfo, XmlName);
        }
    }
}