using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Administrator;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AdministratorAttribute : BaseAttribute
    {
        public AdministratorAttribute()
        {
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            AdministratorManager.RegisterAdministrator((Type)targettedInfo);
        }
    }
}