using BepInEx.Configuration;
using HarmonyLib;
using Hacknet;
using BepInEx.Hacknet;
using System.Collections.ObjectModel;
using BepInEx;

namespace Pathfinder.Options;

[HarmonyPatch]
public static class OptionsManager
{
    [Obsolete("Use PluginTabs")]
    public readonly static Dictionary<string, OptionsTab> Tabs = new Dictionary<string, OptionsTab>();
    private readonly static Dictionary<string, PluginOptionTab> _PluginTabs = new Dictionary<string, PluginOptionTab>();
    private static ReadOnlyDictionary<string, PluginOptionTab> _readonlyPluginTabs;
    public static ReadOnlyDictionary<string, PluginOptionTab> PluginTabs => _readonlyPluginTabs ??= new ReadOnlyDictionary<string, PluginOptionTab>(_PluginTabs);

    public const string PluginOptionTabIdPostfix = "Options";
    public static (string Name, string Id) MakeTabDataFrom(HacknetPlugin plugin, string tabName)
    {
        var metadata = MetadataHelper.GetMetadata(plugin);
        var pair = HacknetChainloader.Instance.Plugins.First(dataPair => dataPair.Value.Metadata.GUID == metadata.GUID);
        return (tabName ?? pair.Value.Metadata.Name, pair.Value.Metadata.GUID+PluginOptionTabIdPostfix);
    }

    public static string GetIdFrom(HacknetPlugin plugin, string name, string id = null)
        => id ?? (MakeTabDataFrom(plugin, name).Id);

    public static bool TryGetTab<TabT>(string tabId, out TabT tab)
        where TabT : PluginOptionTab
    {
        tab = null;
        if(_PluginTabs.TryGetValue(tabId, out var possibleTab))
            tab = possibleTab as TabT;
        return tab != null;
    }

    public static PluginOptionTab GetTab<TabT>(string tabId, bool shouldThrow = false)
        where TabT : PluginOptionTab
    {
        if(!TryGetTab(tabId, out TabT tab))
            return tab;
        if(shouldThrow)
            throw new KeyNotFoundException($"The given key '{tabId}' was not present in the dictionary.");
        return null;
    }

    public static PluginOptionTab RegisterTab(HacknetPlugin plugin = null, string tabName = null, string tabId = null)
    {
        var pair = MakeTabDataFrom(plugin, tabName);
        tabName ??= pair.Name;
        tabId ??= pair.Id;
        Console.WriteLine($"Registering tab {tabId} with name {tabName}");
        if(GetTab<PluginOptionTab>(GetIdFrom(plugin, tabName, tabId)) != null)
            ThrowDuplicateIdAttempt(tabId);
        return RegisterTab(plugin, new PluginOptionTab(plugin, tabName, tabId));
    }

    public static TabT RegisterTab<TabT>(HacknetPlugin plugin, TabT tab)
        where TabT : PluginOptionTab
    {
        if(GetTab<TabT>(tab.Id) != null)
            ThrowDuplicateIdAttempt(tab.Id);
        tab.Plugin = plugin;
        _PluginTabs[tab.Id] = tab;
        return tab;
    }

    public static TabT RegisterTab<TabT>(HacknetPlugin plugin)
        where TabT : PluginOptionTab, new()
        => RegisterTab<TabT>(plugin, new TabT());

    public static PluginOptionTab GetOrRegisterTab(HacknetPlugin plugin, string tabName = null)
    {
        var pair = MakeTabDataFrom(plugin, tabName);
        if(!TryGetTab(GetIdFrom(plugin, pair.Name, pair.Id), out PluginOptionTab tab))
            tab = RegisterTab(plugin, pair.Name, pair.Id);
        return tab;
    }

    public static PluginOptionTab GetOrRegisterTab(HacknetPlugin plugin, string tabName, string tabId = null)
    {
        if(!TryGetTab(GetIdFrom(plugin, tabName, tabId), out PluginOptionTab tab))
            tab = RegisterTab(plugin, tabName, tabId);
        return tab;
    }

    public static TabT GetOrRegisterTab<TabT>(HacknetPlugin plugin, string tabName, string tabId, Func<TabT> genFunc)
        where TabT : PluginOptionTab
    {
        if(!TryGetTab(GetIdFrom(plugin, tabName, tabId), out TabT tab))
            tab = RegisterTab(plugin, genFunc());
        return tab;
    }

    public static TabT GetOrRegisterTab<TabT>(HacknetPlugin plugin, Func<TabT> genFunc)
        where TabT : PluginOptionTab
    {
        var pair = MakeTabDataFrom(plugin, null);
        return GetOrRegisterTab(plugin, pair.Name, pair.Id, genFunc);
    }

    public static T RegisterOption<T>(HacknetPlugin plugin, string tabName, string tabId, T option)
        where T : IPluginOption
    {
        GetOrRegisterTab(plugin, tabName, tabId).AddOption(option);
        return option;
    }

    public static T RegisterOption<T>(HacknetPlugin plugin, string tabName, T option)
        where T : IPluginOption
    {
        return RegisterOption(plugin, tabName, null, option);
    }

    public static T RegisterOption<T>(HacknetPlugin plugin, string tabName, string tabId = null)
        where T : IPluginOption, new()
    {
        return RegisterOption(plugin, tabName, tabId, (T)Activator.CreateInstance(typeof(T)));
    }

    public static void OnConfigSave(string tabId, ConfigFile config)
    {
        _PluginTabs.GetValueSafe(tabId)?.Save();
    }

    public static void OnConfigLoad(string tabId, ConfigFile config)
    {
        _PluginTabs.GetValueSafe(tabId)?.Load();
    }

    [Obsolete("Use RegisterOption or PluginOptionTab.AddOption")]
    public static void AddOption(string tag, Option opt)
    {
        if (!Tabs.TryGetValue(tag, out var tab)) {
            tab = new OptionsTab(tag);
            Tabs.Add(tag, tab);
        }
        tab.Options.Add(opt);
    }


    private static InvalidOperationException ThrowDuplicateIdAttempt(string id)
        => throw new InvalidOperationException($"Can not register tab '{id}', tab id was already registered.");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game1), nameof(Game1.LoadContent))]
    private static void OnPostGame1LoadContent(Game1 __instance)
    {
        foreach(var tab in _PluginTabs)
            tab.Value.LoadContent();
    }
}
