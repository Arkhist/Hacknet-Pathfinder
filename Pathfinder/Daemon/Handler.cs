using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Pathfinder.Event;

namespace Pathfinder.Daemon
{
    public static class Handler
    {
        private static Dictionary<string, IInterface> interfaces = new Dictionary<string, IInterface>();

        public static bool AddDaemon(string daemonId, IInterface inter)
        {
            var asm = new StackFrame(1).GetMethod().Module.Assembly;
            if (asm == MethodBase.GetCurrentMethod().Module.Assembly)
                daemonId = "Pathfinder." + daemonId;
            else
                daemonId = Pathfinder.GetModByAssembly(asm).GetIdentifier() + "." + daemonId;

            if (interfaces.ContainsKey(daemonId))
                return false;

            interfaces.Add(daemonId, inter);
            return true;
        }

        internal static void DaemonLoadListener(LoadComputerXmlReadEvent e)
        {
            IInterface i;
            if (interfaces.TryGetValue(e.Reader.Name, out i))
            {
                e.Computer.daemons.Add(Instance.CreateInstance(e, i));
            }
        }
    }
}
