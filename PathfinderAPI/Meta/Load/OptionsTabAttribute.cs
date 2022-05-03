using System.Reflection;
using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Class)]
public class OptionsTabAttribute : BaseAttribute
{
    internal static readonly Dictionary<HacknetPlugin, string> pluginToOptionsTag = new Dictionary<HacknetPlugin, string>();

    public string Tag { get; }

    public OptionsTabAttribute(string tag)
    {
        this.Tag = tag;
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        pluginToOptionsTag.Add(plugin, Tag);
    }
}