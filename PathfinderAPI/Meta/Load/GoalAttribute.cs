using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Mission;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class GoalAttribute : BaseAttribute
    {
        public string XmlName { get; }

        public GoalAttribute(string xmlName)
        {
            this.XmlName = xmlName;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            GoalManager.RegisterGoal((Type)targettedInfo, XmlName);
        }
    }
}