using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load;

public static class HacknetPluginExtensions
{
    public static string GetOptionsTag(this HacknetPlugin plugin)
    {
        if(!OptionsTabAttribute.pluginToOptTabAttribute.TryGetValue(plugin, out var attr))
            return null;
        return attr.TabName;
    }

    public static bool HasOptionsTag(this HacknetPlugin plugin)
    {
        return OptionsTabAttribute.pluginToOptTabAttribute.ContainsKey(plugin);
    }
}