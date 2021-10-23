using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Action;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConditionAttribute : BaseAttribute
    {
        public string XmlName { get; }

        public ConditionAttribute(string xmlName)
        {
            this.XmlName = xmlName;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            ConditionManager.RegisterCondition((Type)targettedInfo, XmlName);
        }
    }
}