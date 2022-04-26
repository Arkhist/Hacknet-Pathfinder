using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Options;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class OptionAttribute : BaseAttribute
{
    [Obsolete("Use TabName")]
    public string Tag { get => TabName; set => TabName = value; }
    public string TabName { get; set; }
    public string TabId { get; set; }

    public OptionAttribute(string tag = null, string tabId = null)
    {
        TabName = tag;
        TabId = tabId;
    }

    public OptionAttribute(Type pluginType)
    {
        var tabAttr = pluginType.GetCustomAttribute<OptionsTabAttribute>();
        TabName = tabAttr.TabName;
        TabId = tabAttr.TabId;
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        if(TabName == null)
        {
            if(!OptionsTabAttribute.pluginToOptTabAttribute.TryGetValue(plugin, out var tab))
                throw new InvalidOperationException($"Could not find Pathfinder.Meta.Load.OptionsTabAttribute for {targettedInfo.DeclaringType.FullName}");
            TabName = tab.TabName;
            TabId = tab.TabId;
        }

        if(targettedInfo.DeclaringType != plugin.GetType())
            throw new InvalidOperationException($"Pathfinder.Meta.Load.OptionAttribute is only valid in a class derived from BepInEx.Hacknet.HacknetPlugin");

        IPluginOption option = null;
        switch(targettedInfo)
        {
            case PropertyInfo propertyInfo:
                if(!typeof(IPluginOption).IsAssignableFrom(propertyInfo.PropertyType))
                    throw new InvalidOperationException($"Property {propertyInfo.Name}'s type does not derive from Pathfinder.Options.IPluginOption");
                option = (IPluginOption)(propertyInfo.GetGetMethod()?.Invoke(plugin, null));
                break;
            case FieldInfo fieldInfo:
                if(!typeof(IPluginOption).IsAssignableFrom(fieldInfo.FieldType))
                    throw new InvalidOperationException($"Field {fieldInfo.Name}'s type does not derive from Pathfinder.Options.IPluginOption");
                option = (IPluginOption)fieldInfo.GetValue(plugin);
                break;
        }

        if(option == null)
            throw new InvalidOperationException($"IPluginOption not set to a default value, IPluginOption members should be set before HacknetPlugin.Load() is called");

        OptionsManager.GetOrRegisterTab(TabName, TabId).AddOption(option);
    }
}