using System.Reflection;
using BepInEx.Hacknet;
using Pathfinder.Daemon;

namespace Pathfinder.Meta.Load;

[AttributeUsage(AttributeTargets.Class)]
public class DaemonAttribute : BaseAttribute
{
    public DaemonAttribute()
    {
    }

    protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
    {
        DaemonManager.RegisterDaemon((Type)targettedInfo);
    }
}