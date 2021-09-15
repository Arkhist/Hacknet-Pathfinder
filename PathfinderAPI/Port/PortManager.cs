using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using Pathfinder.Event;
using Hacknet;
using Hacknet.Security;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public static class PortManager
    {
        private static readonly AssemblyAssociatedList<PortData> CustomPorts = new AssemblyAssociatedList<PortData>();

        static PortManager()
        {
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            CustomPorts.RemoveAssembly(pluginAsm, out _);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(string protocol, string displayName, int defaultPort = -1) => RegisterPortInternal(new PortData(protocol, defaultPort, displayName), Assembly.GetCallingAssembly());
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(PortData info) => RegisterPortInternal(info, Assembly.GetCallingAssembly());

        private static void RegisterPortInternal(PortData info, Assembly portAsm) 
        {
            CustomPorts.Add(info, portAsm);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnregisterPort(string protocol, Assembly pluginAsm = null)
        {
            pluginAsm = pluginAsm ?? Assembly.GetCallingAssembly();

            CustomPorts.RemoveAll(x => x.Protocol == protocol, pluginAsm);
        }

        public static PortData GetPortDataFromProtocol(string proto)
        {
            var port = CustomPorts.AllItems.FirstOrDefault(x => x.Protocol == proto);
            if (port != null)
                return port;
            ComputerExtensions.OGPorts.TryGetValue(proto, out port);
            return port;
        }
        
        public static PortData GetPortDataFromNumber(int num)
        {
            var port = CustomPorts.AllItems.FirstOrDefault(x => x.Port == num);
            if (port != null)
                return port;
            return ComputerExtensions.OGPorts.Values.FirstOrDefault(x => x.Port == num);
        }
        
        public static void LoadPortsFromString(Computer comp, string portString, bool clearExisting)
        {
            if (clearExisting)
                comp.ClearPorts();
            foreach (var port in portString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var portParts = port.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (portParts.Length == 1)
                {
                    var data = GetPortDataFromProtocol(portParts[0]);
                    if (data is null)
                        throw new ArgumentException($"Protocol '{portParts[0]}' does not exist");
                    if (data.Port == -1)
                        throw new ArgumentException($"Protocol '{portParts[0]}' has no default port");
                    comp.AddPort(data.Clone());
                }
                else if (portParts.Length == 2)
                {
                    var data = GetPortDataFromProtocol(portParts[0]);
                    data = data?.Clone() ?? throw new ArgumentException($"Protocol '{portParts[0]}' does not exist");
                    if (!int.TryParse(portParts[1], out var portNum))
                        throw new FormatException($"Unable to parse port number for protocol '{portParts[0]}'");
                    data.Port = portNum;
                    comp.AddPort(data);
                }
                else if (portParts.Length == 3)
                {
                    if (!int.TryParse(portParts[1], out var portNum))
                        throw new FormatException($"Unable to parse port number for protocol '{portParts[0]}'");
                    comp.AddPort(new PortData(portParts[0], portNum, portParts[2].Replace('_', ' ')));
                }
                else
                {
                    throw new FormatException("PFPorts input string is in the wrong format!");
                }
            }
        }

        public static void LoadPortsFromStringVanilla(Computer comp, string portsList)
        {
            foreach (var port in portsList.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(port, out var portNum))
                    throw new FormatException($"Failed to parse port number from '{port}'");
                var data = GetPortDataFromNumber(portNum);
                if (data == null)
                {
                    Logger.Log(LogLevel.Warning, $"{comp.idName} has port {port}, which does not exist");
                    continue;
                }
                comp.AddPort(data.Clone());
            }
        }

        public static void LoadPortRemapsFromStringVanilla(Computer comp, string remap)
        {
            var ports = comp.GetAllPorts();
            foreach (var binding in PortRemappingSerializer.Deserialize(remap))
            {
                var port = ports.FirstOrDefault(x => x.Port == binding.Key);
                if (port == null)
                {
                    port = ComputerExtensions.OGPorts.Values.FirstOrDefault(x => x.OriginalPort == binding.Key).Clone();
                }
                if (port == null)
                {
                    throw new ArgumentException($"Invalid port remap, port {binding.Key} does not exist");
                }
                port.Port = binding.Value;
            }
        }
    }
}
