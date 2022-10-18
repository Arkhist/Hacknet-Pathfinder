using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Administrator;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AdministratorAttribute : BaseAttribute
{
    public string XmlName { get; }

    public AdministratorAttribute()
    {
    }
    public AdministratorAttribute(string xmlName)
    {
        this.XmlName = xmlName;
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        if (XmlName == null)
            AdministratorManager.RegisterAdministrator((Type)targettedInfo);
        else
            AdministratorManager.RegisterAdministrator((Type)targettedInfo, XmlName);
    }
}
