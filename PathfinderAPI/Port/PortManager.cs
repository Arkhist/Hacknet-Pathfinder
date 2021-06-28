using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using HarmonyLib;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    [HarmonyPatch]
    public static class PortManager
    {
        public struct PortInfo
        {
            public string PortName;
            public int PortNumber;
        }

        private static readonly AssemblyAssociatedList<PortInfo?> CustomPorts = new AssemblyAssociatedList<PortInfo?>();

        static PortManager()
        {
            EventManager<PortsAddedEvent>.AddHandler(OnPortsAdded);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnPortsAdded(PortsAddedEvent e)
        {
            var allPorts = CustomPorts.AllItems;
            foreach (var portToAdd in e.PortsList)
            {
                PortInfo? info = null;
                if (int.TryParse(portToAdd, out int portNum))
                    info = allPorts.FirstOrDefault(x => x.Value.PortNumber == portNum);
                else
                    info = allPorts.FirstOrDefault(x => x.Value.PortName == portToAdd);

                if (info != null)
                {
                    e.Comp.ports.Add(info.Value.PortNumber);
                    e.Comp.portsOpen.Add(0);
                }
            }
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            if (CustomPorts.RemoveAssembly(pluginAsm, out var removed))
            {
                foreach (var port in removed)
                {
                    if (portExploitsInit)
                        PortExploits.services.Remove(port.Value.PortNumber);
                    else
                        portsToAdd.RemoveAll(x => x.PortNumber == port.Value.PortNumber);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(string name, int number) => RegisterPortInternal(new PortInfo { PortName = name, PortNumber = number }, Assembly.GetCallingAssembly());
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterPort(PortInfo info) => RegisterPortInternal(info, Assembly.GetCallingAssembly());

        private static void RegisterPortInternal(PortInfo info, Assembly portAsm) 
        {
            CustomPorts.Add(info, portAsm);

            if (portExploitsInit)
                PortExploits.services.Add(info.PortNumber, info.PortName);
            else
                portsToAdd.Add(info);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnregisterPort(int portNumber, Assembly pluginAsm = null)
        {
            pluginAsm = pluginAsm ?? Assembly.GetCallingAssembly();

            if (portExploitsInit)
                PortExploits.services.Remove(portNumber);
            else
                portsToAdd.RemoveAll(x => x.PortNumber == portNumber);

            CustomPorts.RemoveAll(x => x.Value.PortNumber == portNumber, pluginAsm);
        }

        private static readonly List<PortInfo> portsToAdd = new List<PortInfo>();
        private static bool portExploitsInit = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortExploits), nameof(PortExploits.populate))]
        private static void PortExploitsPopulatePostfix()
        {
            portExploitsInit = true;
            foreach (var port in portsToAdd)
            {
                PortExploits.services.Add(port.PortNumber, port.PortName);
            }
        }
    }
}
