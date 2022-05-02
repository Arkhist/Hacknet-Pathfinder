using System.Reflection;
using BepInEx.Hacknet;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Class)]
public class OptionsTabAttribute : BaseAttribute
{
    internal static readonly Dictionary<HacknetPlugin, OptionsTabAttribute> pluginToOptTabAttribute = new Dictionary<HacknetPlugin, OptionsTabAttribute>();

    [Obsolete("Use TabName")]
    public string Tag { get => TabName; set => TabName = value; }
    public string TabName { get; set; }
    public string TabId { get; set; }

    public OptionsTabAttribute(string tag, string tabId = null)
    {
        TabName = tag;
        TabId = tabId;
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        pluginToOptTabAttribute.Add(plugin, this);
    }
}