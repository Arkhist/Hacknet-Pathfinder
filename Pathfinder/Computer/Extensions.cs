using System;
using System.Collections.Generic;
using Pathfinder.Util;
using Pathfinder.NetworkMap;
using Pathfinder.Port;

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

        /// <summary>
        /// Adds from this comp connecting to newLink.
        /// </summary>
        /// <returns><c>true</c>, if the link was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="newLink">The New link.</param>
        /// <param name="netmap">The NetworkMap or the main client NetworkMap from <see cref="T:Utility.GetClientNetMap"/> if <c>null</c>.</param>
        public static bool AddLink(this Hacknet.Computer comp, Hacknet.Computer newLink, Hacknet.NetworkMap netmap = null)
        {
            if (netmap == null)
                netmap = Utility.GetClientNetMap();
            return netmap?.AddLink(comp, newLink) ?? false;
        }

        public static bool AddVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info, bool unlocked = false)
        {
            if (info.IsEmpty)
                return false;
            comp.ports.Add(info.PortNumber);
            comp.portsOpen.Add(unlocked ? (byte)1 : (byte)0);
            return true;
        }

        public static bool AddVanillaPort(this Hacknet.Computer comp, string portName, bool unlocked = false)
        {
            return comp.AddVanillaPort(ExeInfoManager.GetExecutableInfo(portName), unlocked);
        }

        public static bool AddVanillaPort(this Hacknet.Computer comp, int portNum, bool unlocked = false)
        {
            return comp.AddVanillaPort(ExeInfoManager.GetExecutableInfo(portNum), unlocked);
        }

        public static bool AddModPort(this Hacknet.Computer comp, PortType port, bool unlocked = false)
        {
            return port.AssignTo(comp, unlocked);
        }

        public static bool AddModPort(this Hacknet.Computer comp, string id, bool unlocked = false)
        {
            return Instance.AssignTo(id, comp, unlocked);
        }

        public static bool RemoveVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info)
        {
            if (info.IsEmpty)
                return false;
            var index = comp.ports.IndexOf(info.PortNumber);
            comp.ports.RemoveAt(index);
            comp.portsOpen.RemoveAt(index);
            return true;
        }

        public static bool RemoveVanillaPort(this Hacknet.Computer comp, string portName)
        {
            return comp.RemoveVanillaPort(ExeInfoManager.GetExecutableInfo(portName));
        }

        public static bool RemoveVanillaPort(this Hacknet.Computer comp, int portNum)
        {
            return comp.RemoveVanillaPort(ExeInfoManager.GetExecutableInfo(portNum));
        }

        public static bool RemoveModPort(this Hacknet.Computer comp, PortType port)
        {
            return port.RemoveFrom(comp);
        }

        public static bool RemoveModPort(this Hacknet.Computer comp, string id)
        {
            return Instance.RemoveFrom(id, comp);
        }

        public static bool HasPort(this Hacknet.Computer comp, PortType port)
        {
            return port.GetWithin(comp) != null;
        }

        public static bool IsPortOpen(this Hacknet.Computer comp, PortType port)
        {
            return port.GetWithin(comp)?.Unlocked == true;
        }

        public static void OpenPort(this Hacknet.Computer comp, PortType port, string ipFrom)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            i.Unlocked |= !comp.silent;
            comp.log(ipFrom + " Opened Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port.PortDisplay + " " + port.PortName);
        }

        public static void ClosePort(this Hacknet.Computer comp, PortType port, string ipFrom)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            var wasOpen = i.Unlocked;
            i.Unlocked &= comp.silent;
            if(wasOpen)
                comp.log(ipFrom + " Closed Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port.PortDisplay + " " + port.PortName);
        }
    }
}
