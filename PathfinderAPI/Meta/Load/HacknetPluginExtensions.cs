using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load
{
    public static class HacknetPluginExtensions
    {
        public static string GetOptionsTag(this HacknetPlugin plugin)
        {
            if(!OptionsTabAttribute.pluginToOptionsTag.TryGetValue(plugin, out var tag))
                return null;
            return tag;
        }

        public static bool HasOptionsTag(this HacknetPlugin plugin)
        {
            return OptionsTabAttribute.pluginToOptionsTag.ContainsKey(plugin);
        }
    }
}