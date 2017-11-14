using System;
using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Game.NetworkMap;
using Pathfinder.Util;

namespace Pathfinder.Game.Computer
{
    public static class Extensions
    {
        /// <summary>
        /// Adds a modded Daemon via the interface id string to the Computer.
        /// </summary>
        /// <returns>The modded Daemon instance.</returns>
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
        /// Retrieves a List of daemons exactly of Daemon from the Computer
        /// </summary>
        /// /// <param name="daemonType">The exact Type to search for</param>
        public static List<Hacknet.Daemon> GetDaemonList(this Hacknet.Computer comp, Type daemonType)
        {
            var result = new List<Hacknet.Daemon>();
            foreach (var d in comp.daemons)
                if (d.GetType() == daemonType)
                    result.Add(d);
            return result;
        }

        /// <summary>
        /// Retrieves a List of Daemons whose Type is or is derived from T, pulled from Computer
        /// </summary>
        /// <typeparam name="T">The Type or derivative of the type to search for in the Daemon List</typeparam>
        public static List<T> GetDaemonList<T>(this Hacknet.Computer comp) where T : Hacknet.Daemon =>
            comp.daemons.Select((arg) => arg as T).ToList();

        /// <summary>
        /// Retrieves the first Daemon.Instance whose interface Type is exactly modInterface.
        /// </summary>
        /// <param name="modInterface">The exact Type to find</param>
        public static Daemon.Instance GetModdedDaemon(this Hacknet.Computer comp, Type modInterface) =>
            comp.GetModdedDaemonList(modInterface).ElementAtOrDefault(0);

        /// <summary>
        /// Retrieves the first mod Daemon.Instance whose interface Type is or is derived from T.
        /// </summary>
        /// <typeparam name="T">The Type or derivative of the type to search for in the Daemon List</typeparam>
        public static Daemon.Instance GetModdedDaemon<T>(this Hacknet.Computer comp) where T : Daemon.IInterface =>
            comp.GetModdedDaemonList<T>().ElementAtOrDefault(0);

        /// <summary>
        /// Retrieves a List of Daemon.Instance whose interface Type is exactly modInterface.
        /// </summary>
        /// /// <param name="modInterface">The exact Type to find</param>
        public static List<Daemon.Instance> GetModdedDaemonList(this Hacknet.Computer comp, Type modInterface)
        {
            var result = new List<Daemon.Instance>();
            foreach (var d in comp.daemons)
                if ((d as Daemon.Instance)?.Interface?.GetType() == modInterface)
                    result.Add(d as Daemon.Instance);
            return result;
        }

        /// <summary>
        /// Retrieves a List of mod Daemon.Instance whose interface Type is or is derived from T
        /// </summary>
        /// <typeparam name="T">The Type or derivative of the type to search for in the Daemon List</typeparam>
        public static List<Daemon.Instance> GetModdedDaemonList<T>(this Hacknet.Computer comp) where T : Daemon.IInterface
        {
            var result = new List<Daemon.Instance>();
            foreach (var d in comp.daemons)
                if ((d as Daemon.Instance)?.Interface is T)
                    result.Add(d as Daemon.Instance);
            return result;
        }

        /// <summary>
        /// Adds a link on the NetworkMap from Computer connecting to newLink.
        /// </summary>
        /// <returns><c>true</c>, if the link was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="newLink">The New link.</param>
        public static bool AddLink(this Hacknet.Computer comp, Hacknet.Computer newLink) =>
            comp.GetNetworkMap().AddLink(comp, newLink);

        /// <summary>
        /// Retireves the OS of Computer.
        /// </summary>
        /// <returns>The OS.</returns>
        /// <param name="comp">The Computer</param>
        public static Hacknet.OS GetOS(this Hacknet.Computer comp) => comp.os;

        /// <summary>
        /// Retrieves the NetworkMap of Computer.
        /// </summary>
        /// <returns>The NetworkMap.</returns>
        /// <param name="comp">The Computer</param>
        public static Hacknet.NetworkMap GetNetworkMap(this Hacknet.Computer comp) => comp.GetOS().netMap;

        /// <summary>
        /// Adds a vanilla port by ExecutableInfo.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="info">The ExecutableInfo for the port</param>
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
        /// <param name="port">The Port.Type to add to Computer</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddModPort(this Hacknet.Computer comp, Port.Type port, bool unlocked = false) =>
            port?.AssignTo(comp, unlocked) == true;

        /// <summary>
        /// Adds the mod port by the modded port's registry id.
        /// </summary>
        /// <returns><c>true</c>, if mod port was added, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="id">The modded port's registry id to add to Computer</param>
        /// <param name="unlocked">If set to <c>true</c> then sets the port to be unlocked.</param>
        public static bool AddModPort(this Hacknet.Computer comp, string id, bool unlocked = false) =>
            comp.AddModPort(Port.Handler.GetPort(id), unlocked);

        /// <summary>
        /// Removes a vanilla port by ExecutableInfo.
        /// </summary>
        /// <returns><c>true</c>, if vanilla port was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="info">The ExecutableInfo for the port</param>
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
        /// Determines whether Computer has a certain vanilla port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="info">The ExecutableInfo to search by.</param>
        public static bool HasVanilaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info) =>
            comp.ports.Contains(info.PortNumber);

        /// <summary>
        /// Determines whether Computer has a certain vanilla port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="portName">The port name to search by.</param>
        public static bool HasVanillaPort(this Hacknet.Computer comp, string portName) =>
            comp.HasVanilaPort(ExeInfoManager.GetExecutableInfo(portName));

        /// <summary>
        /// Determines whether Computer has a certain vanilla port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="portNum">The port number to search by.</param>
        public static bool HasVanillaPort(this Hacknet.Computer comp, int portNum) =>
            comp.HasVanilaPort(ExeInfoManager.GetExecutableInfo(portNum));

        /// <summary>
        /// Determines whether Computer has a certain mod port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="port">The Port.Type to search by.</param>
        public static bool HasModPort(this Hacknet.Computer comp, Port.Type port) =>
            port.GetWithin(comp) != null;

        /// <summary>
        /// Determines whether Computer has a certain mod port.
        /// </summary>
        /// <returns><c>true</c>, if Computer has the port, <c>false</c> otherwise.</returns>
        /// <param name="id">The modded port's registry id to search by.</param>
        public static bool HasModPort(this Hacknet.Computer comp, string id) => comp.HasModPort(Port.Handler.GetPort(id));

        /// <summary>
        /// Determines whether the vanilla port is open.
        /// </summary>
        /// <returns><c>true</c>, if the port exists and is open, <c>false</c> otherwise.</returns>
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
        /// <returns><c>true</c>, if the port exists and is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portName">The port name to search by</param>
        public static bool IsVanillaPortOpen(this Hacknet.Computer comp, string portName) =>
            comp.IsVanillaPortOpen(ExeInfoManager.GetExecutableInfo(portName));

        /// <summary>
        /// Determines whether the vanilla port is open.
        /// </summary>
        /// <returns><c>true</c>, if the port exists and is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="portNum">The port number to search by</param>
        public static bool IsVanillaPortOpen(this Hacknet.Computer comp, int portNum) =>
            comp.IsVanillaPortOpen(ExeInfoManager.GetExecutableInfo(portNum));

        /// <summary>
        /// Determines whether the modded port is open.
        /// </summary>
        /// <returns><c>true</c>, if the port exists and is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="port">The Port.Type to find</param>
        public static bool IsModPortOpen(this Hacknet.Computer comp, Port.Type port) =>
            port.GetWithin(comp)?.Unlocked == true;

        /// <summary>
        /// Determines whether the mod port is open.
        /// </summary>
        /// <returns><c>true</c>, if the port exists and is open, <c>false</c> otherwise.</returns>
        /// <param name="comp">The Computer</param>
        /// <param name="id">The modded port's registry id to search by</param>
        public static bool IsModPortOpen(this Hacknet.Computer comp, string id) =>
            comp.IsModPortOpen(Port.Handler.GetPort(id));

        /// <summary>
        /// Opens a vanilla port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="info">The ExecutableInfo to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void OpenVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info, string ipFrom = null)
        {
            var i = comp.ports.IndexOf(info.PortNumber);
            if (i < 0)
                return;
            if (!comp.silent)
                comp.portsOpen[i] = 1;
            if (!String.IsNullOrEmpty(ipFrom))
                comp.log(ipFrom + " Opened Port#" + info.PortNumber);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + info.PortNumber);
        }

        /// <summary>
        /// Opens a vanilla port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="portName">The port name to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void OpenVanillaPort(this Hacknet.Computer comp, string portName, string ipFrom = null) =>
            comp.OpenVanillaPort(ExeInfoManager.GetExecutableInfo(portName), ipFrom);

        /// <summary>
        /// Opens a vanilla port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="portNum">The port number to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void OpenVanillaPort(this Hacknet.Computer comp, int portNum, string ipFrom = null) =>
            comp.OpenVanillaPort(ExeInfoManager.GetExecutableInfo(portNum), ipFrom);

        /// <summary>
        /// Opens a mod port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="port">The Port.Type to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void OpenModPort(this Hacknet.Computer comp, Port.Type port, string ipFrom = null)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            i.Unlocked |= !comp.silent;
            if (!String.IsNullOrEmpty(ipFrom))
                comp.log(ipFrom + " Opened Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port);
        }

        /// <summary>
        /// Opens a mod port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="id">The Type id to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void OpenModPort(this Hacknet.Computer comp, string id, string ipFrom = null) =>
            comp.OpenModPort(Port.Handler.GetPort(id), ipFrom);

        /// <summary>
        /// Closes a vanilla port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="info">The ExecutableInfo to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void CloseVanillaPort(this Hacknet.Computer comp, ExeInfoManager.ExecutableInfo info, string ipFrom = null)
        {
            var i = comp.ports.IndexOf(info.PortNumber);
            if (i < 0)
                return;
            var wasOpen = comp.portsOpen[i] >= 1;
            if (!comp.silent)
                comp.portsOpen[i] = 0;
            if (wasOpen && !String.IsNullOrEmpty(ipFrom))
                comp.log(ipFrom + " Closes Port#" + info.PortNumber);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + info.PortNumber);
        }

        /// <summary>
        /// Closes a vanilla port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="portName">The port name to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void CloseVanillaPort(this Hacknet.Computer comp, string portName, string ipFrom = null) =>
            comp.CloseVanillaPort(ExeInfoManager.GetExecutableInfo(portName), ipFrom);

        /// <summary>
        /// Closes a vanilla port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="portNum">The port number to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void CloseVanillaPort(this Hacknet.Computer comp, int portNum, string ipFrom = null) =>
            comp.CloseVanillaPort(ExeInfoManager.GetExecutableInfo(portNum), ipFrom);

        /// <summary>
        /// Closes a mod port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="port">The Port.Type to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void CloseModPort(this Hacknet.Computer comp, Port.Type port, string ipFrom = null)
        {
            var i = port.GetWithin(comp);
            if (i == null)
                return;
            var wasOpen = i.Unlocked;
            i.Unlocked &= comp.silent;
            if (wasOpen & !String.IsNullOrEmpty(ipFrom))
                comp.log(ipFrom + " Closed Port#" + port.PortName + "/" + port.PortDisplay);
            comp.sendNetworkMessage("cPortOpen " + comp.ip + " " + ipFrom + " " + port);
        }

        /// <summary>
        /// Closes a mod port. Doesn't add a port
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="id">The Type id to search for.</param>
        /// <param name="ipFrom">The ip responsible for the change.</param>
        public static void CloseModPort(this Hacknet.Computer comp, string id, string ipFrom = null) =>
            comp.CloseModPort(Port.Handler.GetPort(id), ipFrom);

        /// <summary>
        /// Adds a EOS Device connection represented by device
        /// </summary>
        /// <param name="comp">The Computer.</param>
        /// <param name="device">The Computer device to link to.</param>
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

        /// <summary>
        /// Creates the EOS Device connected to Computer.
        /// </summary>
        /// <returns>The created EOS Device.</returns>
        /// <param name="comp">The Computer.</param>
        /// <param name="name">The device's name.</param>
        /// <param name="ip">The device's ip.</param>
        /// <param name="icon">The device's icon.</param>
        /// <param name="location">The device's NetworkMap position.</param>
        /// <param name="password">The device's password, by game default should be alpine.</param>
        /// <param name="vanillaPorts">The device's closed vanilla port numbers.</param>
        /// <param name="portCracksRequired">The device's cracked ports required to unlock.</param>
        /// <param name="eosFolder">The device's eos folder.</param>
        /// <param name="modPorts">The device's closed modded Port.Type List.</param>
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

        /// <summary>
        /// Gets the EOS Devices sorted by retType.
        /// </summary>
        /// <returns>The EOS Devices.</returns>
        /// <param name="comp">The Computer.</param>
        /// <param name="retType">Determines how to sort the devices.</param>
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

        /// <summary>
        /// Sorts EOS Devices by ids
        /// </summary>
        /// <see cref="GetEOSDevicesBy"/>
        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesById(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.ID);

        /// <summary>
        /// Sorts EOS Devices by ips
        /// </summary>
        /// <see cref="GetEOSDevicesBy"/>
        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesByIp(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.ADDRESS);

        /// <summary>
        /// Sorts EOS Devices by names
        /// </summary>
        /// <see cref="GetEOSDevicesBy"/>
        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesByName(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.NAME);

        /// <summary>
        /// Sorts EOS Devices by NetworkMap indexes
        /// </summary>
        /// <see cref="GetEOSDevicesBy"/>
        public static Dictionary<string, Hacknet.Computer> GetEOSDevicesByIndex(this Hacknet.Computer comp) =>
            comp.GetEOSDevicesBy(RetrieveType.INDEX);
    }
}
