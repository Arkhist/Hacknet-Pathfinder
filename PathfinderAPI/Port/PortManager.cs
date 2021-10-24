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
        private static readonly AssemblyAssociatedList<PortRecord> CustomPorts = new AssemblyAssociatedList<PortRecord>();

        static PortManager()
        {
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            CustomPorts.RemoveAssembly(pluginAsm, out _);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(string protocol, string displayName, int defaultPort = -1) => RegisterPortInternal(new PortRecord(protocol, displayName, defaultPort), Assembly.GetCallingAssembly());
        [Obsolete("PortData is obsolete, use RegisterPort(PortRecord)")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(PortData info) => RegisterPortInternal((PortRecord)info, Assembly.GetCallingAssembly());
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(PortRecord record) => RegisterPortInternal(record, Assembly.GetCallingAssembly());

        internal static void RegisterPortInternal(PortRecord info, Assembly portAsm)
        {
            CustomPorts.Add(info, portAsm);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnregisterPort(string protocol, Assembly pluginAsm = null)
        {
            pluginAsm = pluginAsm ?? Assembly.GetCallingAssembly();

            CustomPorts.RemoveAll(x => x.Protocol == protocol, pluginAsm);
        }

        public static bool IsPortRecordRegistered(PortRecord record)
            => CustomPorts.AllItems.Contains(record) || ComputerExtensions.OGPorts.ContainsValue(record);

        public static bool IsPortRegistered(string protocol)
            => CustomPorts.AllItems.Any(p => p.Protocol == protocol) || ComputerExtensions.OGPorts.ContainsKey(protocol);

        public static PortRecord GetPortRecordFromProtocol(string proto)
        {
            var port = CustomPorts.AllItems.FirstOrDefault(x => x.Protocol == proto);
            if (port == null)
                return port;
            ComputerExtensions.OGPorts.TryGetValue(proto, out port);
            return port;
        }

        public static PortRecord GetPortRecordFromNumber(int num)
        {
            var port = CustomPorts.AllItems.FirstOrDefault(x => x.DefaultPortNumber == num);
            if (port == null)
                return port;
            return ComputerExtensions.OGPorts.Values.FirstOrDefault(x => x.DefaultPortNumber == num);
        }

        [Obsolete("Use GetPortRecordFromProtocol(string)")]
        public static PortData GetPortDataFromProtocol(string proto)
        {
            return (PortData)GetPortRecordFromProtocol(proto);
        }

        [Obsolete("Use GetPortRecordFromNumber(int)")]
        public static PortData GetPortDataFromNumber(int num)
        {
            return (PortData)GetPortRecordFromNumber(num);
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
                    var record = GetPortRecordFromProtocol(portParts[0]);
                    if (record == null)
                        throw new ArgumentException($"Protocol '{portParts[0]}' does not exist");
                    if (record.DefaultPortNumber == -1)
                        throw new ArgumentException($"Protocol '{portParts[0]}' has no default port");
                    comp.AddPort(record);
                }
                else if (portParts.Length == 2)
                {
                    var record = GetPortRecordFromProtocol(portParts[0]);
                    if(record == null)
                        throw new ArgumentException($"Protocol '{portParts[0]}' does not exist");
                    if (!int.TryParse(portParts[1], out var portNum))
                        throw new FormatException($"Unable to parse port number for protocol '{portParts[0]}'");
                    comp.AddPort(record.CreateState(comp, portNumber: portNum));
                }
                else if (portParts.Length == 3)
                {
                    if (!int.TryParse(portParts[1], out var portNum))
                        throw new FormatException($"Unable to parse port number for protocol '{portParts[0]}'");
                    var record = GetPortRecordFromProtocol(portParts[0]);
                    if (record != null)
                    {
                        comp.AddPort(record.CreateState(comp, portParts[2].Replace('_', ' '), portNum));
                    }
                    else
                    {
                        comp.AddPort(portParts[0], portNum, portParts[2].Replace('_', ' '));
                    }
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
                var record = GetPortRecordFromNumber(portNum);
                if (record == null)
                {
                    Logger.Log(LogLevel.Warning, $"{comp.idName} has port {port}, which does not exist");
                    continue;
                }
                comp.AddPort(record);
            }
        }

        public static void LoadPortRemapsFromStringVanilla(Computer comp, string remap)
        {
            var ports = comp.GetAllPortStates();
            var remaps = PortRemappingSerializer.Deserialize(remap);
            if (remaps == null)
                return;
            foreach (var binding in remaps)
            {
                var port = ports.FirstOrDefault(x => x.PortNumber == binding.Key);
                if (port == null)
                {
                    port = ComputerExtensions.OGPorts.Values.FirstOrDefault(x => x.OriginalPortNumber == binding.Key)?.CreateState(comp);
                }
                if (port == null)
                {
                    throw new ArgumentException($"Invalid port remap, port {binding.Key} does not exist");
                }
                port.PortNumber = binding.Value;
            }
        }
    }
}
