using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Port;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PortAttribute : BaseAttribute
{
    public PortAttribute()
    {
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        if(targettedInfo.DeclaringType != plugin.GetType())
            throw new InvalidOperationException($"Pathfinder.Meta.Load.PortAttribute is only valid in a class derived from BepInEx.Hacknet.HacknetPlugin");

        object portRecord = null;
        switch(targettedInfo)
        {
            case PropertyInfo propertyInfo:
                if(propertyInfo.PropertyType != typeof(PortRecord))
                    throw new InvalidOperationException($"Property {propertyInfo.Name}'s type does not derive from Pathfinder.Port.PortRecord");
                portRecord = propertyInfo.GetGetMethod()?.Invoke(plugin, null);
                break;
            case FieldInfo fieldInfo:
                if(fieldInfo.FieldType != typeof(PortRecord))
                    throw new InvalidOperationException($"Field {fieldInfo.Name}'s type does not derive from Pathfinder.Port.PortRecord");
                portRecord = fieldInfo.GetValue(plugin);
                break;
        }

        if(portRecord == null)
            throw new InvalidOperationException($"PortRecord not set to a default value, PortRecord should be set before HacknetPlugin.Load() is called");

        PortManager.RegisterPortInternal((PortRecord)portRecord, targettedInfo.Module.Assembly);
    }
}