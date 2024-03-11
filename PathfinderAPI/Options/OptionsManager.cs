using Pathfinder.GUI;

namespace Pathfinder.Options;

public static class OptionsManager
{
    public readonly static Dictionary<string, OptionsTab> Tabs = new Dictionary<string, OptionsTab>();

    static OptionsManager() { }

    public static void AddOption(string tag, Option opt)
    {
        if (!Tabs.TryGetValue(tag, out var tab)) {
            tab = new OptionsTab(tag);
            Tabs.Add(tag, tab);
        }
        tab.Options.Add(opt);
    }
}

public class OptionsTab
{
    public string Name;

    public List<Option> Options = [];

    internal int ButtonID = PFButton.GetNextID();

    public OptionsTab(string name) {
        Name = name;
    }
}