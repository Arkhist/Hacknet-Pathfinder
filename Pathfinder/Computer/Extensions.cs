using System;

namespace Pathfinder.Computer
{
    public static class Extensions
    {
        public static Daemon.Instance GetModdedDaemon(this Hacknet.Computer comp, Type modInterface)
        {
            foreach (var d in comp.daemons)
                if ((d as Daemon.Instance)?.Interface?.GetType() == modInterface)
                    return d as Daemon.Instance;
            return null;
        }

        public static Daemon.Instance GetModdedDaemon<T>(this Hacknet.Computer comp)
        {
            return comp.GetModdedDaemon(typeof(T));
        }
    }
}
