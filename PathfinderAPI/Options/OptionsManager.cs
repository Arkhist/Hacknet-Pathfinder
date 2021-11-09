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

    public static bool TryGetTab(string tabName, out PluginOptionTab tab)
    {
        tab = PluginTabs.Find(t => t.Id == tabName);
        return tab != null;
    }

    public static PluginOptionTab GetTab(string tabName)
    {
        if(!TryGetTab(tabName, out var tab))
            return tab;
        return null;
    }

    public static PluginOptionTab RegisterTab(string tabName, string tabId = null)
    {
        if(GetTab(tabName) != null)
            throw new InvalidOperationException("Can not deliberately register an existing tab");
        PluginOptionTab tab;
        PluginTabs.Add(tab = new PluginOptionTab(tabName, tabId));
        tab.OnRegistered();
        return tab;
    }

    public static PluginOptionTab GetOrRegisterTab(string tabName, string tabId = null)
    {
        if(!TryGetTab(tabName, out var tab))
            tab = RegisterTab(tabName, tabId);
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
