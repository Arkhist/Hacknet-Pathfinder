using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.NetworkMap;
using Pathfinder.Util;

namespace Pathfinder.Computer
{
    public static class Extensions
    {
        /// <summary>
        /// Adds a modded daemon by interface id to the Computer.
        /// </summary>
        /// <returns>The modded daemon instance.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="interfaceId">Interface Identifier.</param>
        /// <param name="input">The input for the LoadInstance interface function.</param>
        public static Daemon.Instance AddModdedDaemon(this Hacknet.Computer comp,
                                                      string interfaceId,
                                                      Dictionary<string, string> input = null)
        {
            var i = Daemon.Instance.CreateInstance(interfaceId, comp, input);
            if (i == null)
                return null;
            comp.daemons.Add(i);
            return i;
        }

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
        public static bool AddLink(this Hacknet.Computer comp, Hacknet.Computer newLink) =>
            comp.GetNetworkMap().AddLink(comp, newLink);

        /// <summary>
        /// Gets the NetworkMap the computer is part of.
        /// </summary>
        /// <returns>The NetworkMap.</returns>
        /// <param name="comp">The Computer</param>
        public static Hacknet.NetworkMap GetNetworkMap(this Hacknet.Computer comp) => comp.os.netMap;

        /// <summary>
        /// Adds a vanilla port by ExecutableInfo.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="info">The info for the port</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info, bool unlocked = false)
        {
            if (info.IsEmpty)
                return false;
            comp.ports.Add(info.PortNumber);
            comp.portsOpen.Add(unlocked ? (byte)1 : (byte)0);
            return true;
        }

        /// <summary>
        /// Adds a vanilla port by port name.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portName">The name for the port</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddVanillaPort(this Hacknet.Computer comp, string portName, bool unlocked = false) =>
            comp.AddVanillaPort(ExeInfoManager.GetExecutableInfo(portName), unlocked);

        /// <summary>
        /// Adds a vanilla port by port number.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portNum">The number for the port</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddVanillaPort(this Hacknet.Computer comp, int portNum, bool unlocked = false) =>
            comp.AddVanillaPort(ExeInfoManager.GetExecutableInfo(portNum), unlocked);

        /// <summary>
        /// Adds the mod port by port type.
        /// </summary>
        /// <returns><c>true</c>, if mod port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="port">The port type to add to the computer</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddModPort(this Hacknet.Computer comp, Port.Type port, bool unlocked = false) =>
            port?.AssignTo(comp, unlocked) == true;

        /// <summary>
        /// Adds the mod port by port registry id.
        /// </summary>
        /// <returns><c>true</c>, if mod port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="id">The port's registry id to add to the computer</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddModPort(this Hacknet.Computer comp, string id, bool unlocked = false) =>
            comp.AddModPort(Port.Handler.GetPort(id), unlocked);

        /// <summary>
        /// Removes a vanilla port by ExecutableInfo.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="info">The info for the port</param>
        public static bool RemoveVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info)
        {
            if (info.IsEmpty)
                return false;
            var index = comp.ports.IndexOf(info.PortNumber);
            if (index < 0)
                return false;
            comp.ports.RemoveAt(index);
            comp.portsOpen.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes a vanilla port by port name.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portName">The name for the port</param>
        public static bool RemoveVanillaPort(this Hacknet.Computer comp, string portName) =>
            comp.RemoveVanillaPort(ExeInfoManager.GetExecutableInfo(portName));

        /// <summary>
        /// Removes a vanilla port by port number.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portNum">The number for the port</param>
        public static bool RemoveVanillaPort(this Hacknet.Computer comp, int portNum) =>
            comp.RemoveVanillaPort(ExeInfoManager.GetExecutableInfo(portNum));

        public static bool RemoveModPort(this Hacknet.Computer comp, Port.Type port) =>
            port?.RemoveFrom(comp) == true;

        public static bool RemoveModPort(this Hacknet.Computer comp, string id) =>
            comp.RemoveModPort(Port.Handler.GetPort(id));

        /// <summary>
        /// Gets a read-only list of modded ports.
        /// </summary>
        /// <returns>The list of modded ports.</returns>
        /// <param name="comp">The Computer</param>
        public static IList<Port.Instance> GetModdedPortList(this Hacknet.Computer comp)
        {
            List<Port.Instance> res;
            Port.Instance.compToInst.TryGetValue(comp, out res);
            if (res == null)
                res = new List<Port.Instance>();
            return res.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the Computer has a certain vanilla port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="info">The ExecutableInfo to search by.</param>
        public static bool HasVanilaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info) =>
            comp.ports.Contains(info.PortNumber);

        /// <summary>
        /// Determines whether the Computer has a certain vanilla port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="portName">The port name to search by.</param>
        public static bool HasVanillaPort(this Hacknet.Computer comp, string portName) =>
            comp.HasVanilaPort(ExeInfoManager.GetExecutableInfo(portName));

        /// <summary>
        /// Determines whether the Computer has a certain vanilla port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="portNum">The port number to search by.</param>
        public static bool HasVanillaPort(this Hacknet.Computer comp, int portNum) =>
            comp.HasVanilaPort(ExeInfoManager.GetExecutableInfo(portNum));

        /// <summary>
        /// Determines whether the Computer has a certain mod port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="port">The port type to search by.</param>
        public static bool HasModPort(this Hacknet.Computer comp, Port.Type port) =>
            port.GetWithin(comp) != null;

        /// <summary>
        /// Determines whether the Computer has a certain mod port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="id">The registry port id to search by.</param>
        public static bool HasModPort(this Hacknet.Computer comp, string id) => comp.HasModPort(Port.Handler.GetPort(id));

        /// <summary>
        /// Determines whether the vanilla port is open.
        /// </summary>
        /// <returns><c>true</c>, if port open is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="info">The ExecutableInfo to search by</param>
        public static bool IsVanillaPortOpen(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info)
        {
            var i = comp.ports.IndexOf(info.PortNumber);
            if (i < 0)
                return false;
            return comp.portsOpen[i] >= 1;
        }

        /// <summary>
        /// Determines whether the vanilla port is open.
        /// </summary>
        /// <returns><c>true</c>, if port open is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portName">The port name to search by</param>
        public static bool IsVanillaPortOpen(this Hacknet.Computer comp, string portName) =>
            comp.IsVanillaPortOpen(ExeInfoManager.GetExecutableInfo(portName));

        /// <summary>
        /// Determines whether the vanilla port is open.
        /// </summary>
        /// <returns><c>true</c>, if port open is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portNum">The port number to search by</param>
        public static bool IsVanillaPortOpen(this Hacknet.Computer comp, int portNum) =>
            comp.IsVanillaPortOpen(ExeInfoManager.GetExecutableInfo(portNum));

        /// <summary>
        /// Determines whether the mod port is open.
        /// </summary>
        /// <returns><c>true</c>, if port open is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="port">The port type to find</param>
        public static bool IsModPortOpen(this Hacknet.Computer comp, Port.Type port) =>
            port.GetWithin(comp)?.Unlocked == true;

        /// <summary>
        /// Determines whether the mod port is open.
        /// </summary>
        /// <returns><c>true</c>, if port open is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="id">The registered port id to search by</param>
        public static bool IsModPortOpen(this Hacknet.Computer comp, string id) =>
            comp.IsModPortOpen(Port.Handler.GetPort(id));

        public static void OpenVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info, string ipFrom)
        {
            var i = comp.ports.IndexOf(info.PortNumber);
            if (i < 0)
                return;
            if (!comp.silent)
                comp.portsOpen[i] = 1;
            comp.log(ipFrom + " Opened Port#" + info.PortNumber);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + info.PortNumber);
        }

        public static void OpenVanillaPort(this Hacknet.Computer comp, string portName, string ipFrom) =>
            comp.OpenVanillaPort(ExeInfoManager.GetExecutableInfo(portName), ipFrom);

        public static void OpenVanillaPort(this Hacknet.Computer comp, int portNum, string ipFrom) =>
            comp.OpenVanillaPort(ExeInfoManager.GetExecutableInfo(portNum), ipFrom);

        public static void OpenModPort(this Hacknet.Computer comp, Port.Type port, string ipFrom)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            i.Unlocked |= !comp.silent;
            comp.log(ipFrom + " Opened Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port);
        }

        public static void OpenModPort(this Hacknet.Computer comp, string id, string ipFrom) =>
            comp.OpenModPort(Port.Handler.GetPort(id), ipFrom);

        public static void CloseVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info, string ipFrom)
        {
            var i = comp.ports.IndexOf(info.PortNumber);
            if (i < 0)
                return;
            var wasOpen = comp.portsOpen[i] >= 1;
            if (!comp.silent)
                comp.portsOpen[i] = 0;
            if (wasOpen)
                comp.log(ipFrom + " Closes Port#" + info.PortNumber);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + info.PortNumber);
        }

        public static void CloseVanillaPort(this Hacknet.Computer comp, string portName, string ipFrom) =>
            comp.CloseVanillaPort(ExeInfoManager.GetExecutableInfo(portName), ipFrom);

        public static void ClosePort(this Hacknet.Computer comp, int portNum, string ipFrom) =>
            comp.CloseVanillaPort(ExeInfoManager.GetExecutableInfo(portNum), ipFrom);

        public static void ClosePort(this Hacknet.Computer comp, Port.Type port, string ipFrom)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            var wasOpen = i.Unlocked;
            i.Unlocked &= comp.silent;
            if (wasOpen)
                comp.log(ipFrom + " Closed Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port);
        }

        public static void CloseModPort(this Hacknet.Computer comp, Port.Type port, string ipFrom)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            var wasOpen = i.Unlocked;
            i.Unlocked &= comp.silent;
            if (wasOpen)
                comp.log(ipFrom + " Closed Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port);
        }

        public static void CloseModPort(this Hacknet.Computer comp, string id, string ipFrom) =>
            comp.CloseModPort(Port.Handler.GetPort(id), ipFrom);

        public static void AddEOSDevice(this Hacknet.Computer comp, Hacknet.Computer device)
        {
            if (comp.attatchedDeviceIDs == null)
                comp.attatchedDeviceIDs = device.idName;
            else
                comp.attatchedDeviceIDs += ',' + device.idName;

            if (!comp.GetNetworkMap().nodes.Contains(device))
                comp.GetNetworkMap().nodes.Add(device);

            device.AddLink(comp);
        }

        public static Hacknet.Computer CreateEOSDeviceOn(this Hacknet.Computer comp,
                                                         string name = "Unregistered eOS Device",
                                                         string ip = null,
                                                         string icon = "ePhone",
                                                         Vector2? location = null,
                                                         string password = "alpine",
                                                         List<int> vanillaPorts = null,
                                                         int portCracksRequired = 2,
                                                         Folder eosFolder = null,
                                                         List<Port.Type> modPorts = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var c = new Hacknet.Computer(name, comp.idName + "_eos", location ??
                                         comp.location + Corporation.getNearbyNodeOffset(comp.location,
                                                                                          Utils.random.Next(12),
                                                                                          12,
                                                                                          comp.GetNetworkMap(),
                                                                                          0f), 0, 5, comp.os);
            c.icon = icon;
            c.setAdminPassword(password);
            vanillaPorts = vanillaPorts ?? new List<int>(new int[] { 22, 3659 });
            foreach (var p in vanillaPorts)
                c.AddVanillaPort(p);
            if (modPorts != null) foreach (var p in modPorts)
                c.AddModPort(p);
            c.portsNeededForCrack = portCracksRequired;
            if (eosFolder != null)
                c.files.root.folders.Add(eosFolder);
            else EOSComp.GenerateEOSFilesystem(c);
            comp.AddEOSDevice(c);
            return c;
        }

        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesBy(this Hacknet.Computer comp, RetrieveType retType)
        {
            var res = new Dictionary<string, Hacknet.Computer>();
            if (comp.attatchedDeviceIDs != null)
                foreach (var id in comp.attatchedDeviceIDs.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var c = comp.GetNetworkMap().GetComputerById(id);
                    if (c != null)
                    {
                        switch (retType)
                        {
                        case RetrieveType.ID:
                                res[id] = c;
                                break;
                        case RetrieveType.ADDRESS:
                                res[c.ip] = c;
                                break;
                        case RetrieveType.NAME:
                                res[c.name] = c;
                                break;
                        case RetrieveType.INDEX:
                                res[c.GetNetworkMap().nodes.IndexOf(c).ToString()] = c;
                                break;
                        }
                    }
            }
            return res;
        }

        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesById(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.ID);

        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesByIp(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.ADDRESS);

        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesByName(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.NAME);

        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesByIndex(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.INDEX);
    }
}
