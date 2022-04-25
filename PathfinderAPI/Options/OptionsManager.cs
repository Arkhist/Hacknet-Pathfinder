using Pathfinder.GUI;
using BepInEx.Configuration;

namespace Pathfinder.Options;

public static class OptionsManager
{
    [Obsolete("Use PluginTabs")]
    public readonly static Dictionary<string, OptionsTab> Tabs = new Dictionary<string, OptionsTab>();
    public readonly static List<PluginOptionTab> PluginTabs = new List<PluginOptionTab>();

    [Obsolete("Use RegisterOption or PluginOptionTab.AddOption")]
    public static void AddOption(string tag, Option opt)
    {
        if (!Tabs.TryGetValue(tag, out var tab)) {
            tab = new OptionsTab(tag);
            Tabs.Add(tag, tab);
        }
        tab.Options.Add(opt);
    }

    public static bool TryGetTab<TabT>(string tabId, out TabT tab)
        where TabT : PluginOptionTab
    {
        tab = PluginTabs.Find(t => t.Id == tabId) as TabT;
        return tab != null;
    }

    public static PluginOptionTab GetTab<TabT>(string tabId)
        where TabT : PluginOptionTab
    {
        if(!TryGetTab(tabId, out TabT tab))
            return tab;
        return null;
    }

    public static PluginOptionTab RegisterTab(string tabName, string tabId = null)
    {
        if(GetTab(tabId ?? string.Concat(tabName.Where(c => !char.IsWhiteSpace(c) && c != '='))) != null)
            throw new InvalidOperationException("Can not register tabs with a registered id");
        return RegisterTab(new PluginOptionTab(tabName, tabId));
    }

    public static TabT RegisterTab<TabT>(TabT tab)
        where TabT : PluginOptionTab
    {
        if(GetTab(tab.Id) != null)
            throw new InvalidOperationException("Can not register tabs with a registered id");
        PluginTabs.Add(tab);
        tab.OnRegistered();
        return tab;
    }

    public static PluginOptionTab GetOrRegisterTab(string tabName, string tabId = null)
    {
        if(!TryGetTab(tabId ?? string.Concat(tabName.Where(c => !char.IsWhiteSpace(c) && c != '=')), out PluginOptionTab tab))
            tab = RegisterTab(tabName, tabId);
        return tab;
    }

    public static TabT GetOrRegisterTab<TabT>(string tabName, string tabId, Func<TabT> generatorFunc)
        where TabT : PluginOptionTab
    {
        if(!TryGetTab(tabId ?? string.Concat(tabName.Where(c => !char.IsWhiteSpace(c) && c != '=')), out TabT tab))
            tab = RegisterTab(generatorFunc());
        return tab;
    }

    public static T RegisterOption<T>(string tabName, string tabId, T option)
        where T : IPluginOption
    {
        GetOrRegisterTab(tabName, tabId).AddOption(option);
        return option;
    }

    public static T RegisterOption<T>(string tabName, T option)
        where T : IPluginOption
    {
        return RegisterOption(tabName, null, option);
    }

    public static T RegisterOption<T>(string tabName, string tabId = null)
        where T : IPluginOption, new()
    {
        return RegisterOption(tabName, tabId, (T)Activator.CreateInstance(typeof(T)));
    }

    public static void OnConfigSave(ConfigFile config)
    {
        foreach(var tab in PluginTabs)
            tab.OnSave(config);
    }

    public static void OnConfigLoad(ConfigFile config)
    {
        foreach(var tab in PluginTabs)
            tab.OnLoad(config);
    }
}

[Obsolete("Use PluginOptionTab")]
public class OptionsTab
{
    public string Name;

    public List<Option> Options = new List<Option>();

    internal int ButtonID = PFButton.GetNextID();

    public OptionsTab(string name) {
        Name = name;
    }
}
