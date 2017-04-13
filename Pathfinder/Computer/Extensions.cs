using System;
using System.Collections.Generic;

namespace Pathfinder.Computer
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieves a list of daemon of the exact type daemonType from comp
        /// </summary>
        public static List<Hacknet.Daemon> GetDaemonList(this Hacknet.Computer comp, Type daemonType)
        {
            var result = new List<Hacknet.Daemon>();
            foreach (var d in comp.daemons)
                if (d.GetType() == daemonType)
                    result.Add(d);
            return result;
        }

        /// <summary>
        /// Retrieves a list of daemon whose type is either T or derived from T, pulled from comp
        /// </summary>
        public static List<T> GetDaemonList<T>(this Hacknet.Computer comp) where T : Hacknet.Daemon
        {
            var result = new List<T>();
            foreach (var d in comp.daemons)
                if (d is T)
                    result.Add(d as T);
            return result;
        }

        /// <summary>
        /// Gets the first mod daemon instance whose interface type is exactly modInterface.
        /// </summary>
        public static Daemon.Instance GetModdedDaemon(this Hacknet.Computer comp, Type modInterface)
        {
            return comp.GetModdedDaemonList(modInterface)[0];
        }

        /// <summary>
        /// Gets the first mod daemon instance whose interface type derives or is of type modInterface.
        /// </summary>
        public static Daemon.Instance GetModdedDaemon<T>(this Hacknet.Computer comp) where T : Daemon.IInterface
        {
            return comp.GetModdedDaemonList<T>()[0];
        }

        /// <summary>
        /// Retrieves a list of mod daemon instances whose interface is exactly of type modInterface
        /// </summary>
        public static List<Daemon.Instance> GetModdedDaemonList(this Hacknet.Computer comp, Type modInterface)
        {
            var result = new List<Daemon.Instance>();
            foreach (var d in comp.daemons)
                if ((d as Daemon.Instance)?.Interface?.GetType() == modInterface)
                    result.Add(d as Daemon.Instance);
            return result;
        }

        /// <summary>
        /// Retrieves a list of mod daemon instances whose interface type is either T or derived from T, pulled from comp
        /// </summary>
        public static List<Daemon.Instance> GetModdedDaemonList<T>(this Hacknet.Computer comp) where T : Daemon.IInterface
        {
            var result = new List<Daemon.Instance>();
            foreach (var d in comp.daemons)
                if ((d as Daemon.Instance)?.Interface is T)
                    result.Add(d as Daemon.Instance);
            return result;
        }
    }
}
