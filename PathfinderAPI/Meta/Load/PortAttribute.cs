using System;
using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Meta.Load;
using Pathfinder.Port;

namespace Pathfinder.Meta.Load
{
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

            object portData = null;
            switch(targettedInfo)
            {
                case PropertyInfo propertyInfo:
                    if(propertyInfo.PropertyType != typeof(PortData))
                        throw new InvalidOperationException($"Property {propertyInfo.Name}'s type does not derive from Pathfinder.Port.PortData");
                    portData = propertyInfo.GetGetMethod()?.Invoke(plugin, null);
                    break;
                case FieldInfo fieldInfo:
                    if(fieldInfo.FieldType != typeof(PortData))
                        throw new InvalidOperationException($"Field {fieldInfo.Name}'s type does not derive from Pathfinder.Port.PortData");
                    portData = fieldInfo.GetValue(plugin);
                    break;
            }

            if(portData == null)
                throw new InvalidOperationException($"PortData not set to a default value, PortData should be set before HacknetPlugin.Load() is called");

            PortManager.RegisterPortInternal((PortData)portData, targettedInfo.Module.Assembly);
        }
    }
}