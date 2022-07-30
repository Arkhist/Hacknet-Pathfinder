using Pathfinder.GUI;

namespace Pathfinder.Options;

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
