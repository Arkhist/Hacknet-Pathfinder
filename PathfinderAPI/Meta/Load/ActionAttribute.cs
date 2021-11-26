using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Action;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ActionAttribute : BaseAttribute
    {
        public string XmlName { get; }

        public ActionAttribute(string xmlName)
        {
            this.XmlName = xmlName;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            ActionManager.RegisterAction((Type)targettedInfo, XmlName);
        }
    }
}