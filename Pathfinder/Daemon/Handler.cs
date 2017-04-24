using System.Collections.Generic;
using Pathfinder.Event;
using Pathfinder.Util;

namespace Pathfinder.Daemon
{
    public static class Handler
    {
        private static Dictionary<string, IInterface> interfaces = new Dictionary<string, IInterface>();

        public static bool AddDaemon(string daemonId, IInterface inter)
        {
            daemonId = Utility.GetPreviousStackFrameIdentity() + "." + Utility.GetId(daemonId, true);
            Logger.Verbose("Mod {0} attempting to add daemon interface {1} with id {2}",
                           Utility.GetPreviousStackFrameIdentity(),
                           inter.GetType().FullName,
                           daemonId);
            if (interfaces.ContainsKey(daemonId))
                return false;

            interfaces.Add(daemonId, inter);
            return true;
        }

        public static bool ContainsDaemon(string id)
        {
            return interfaces.ContainsKey(Utility.GetId(id));
        }

        public static IInterface GetDaemonById(string id)
        {
            id = Utility.GetId(id);
            IInterface i = null;
            interfaces.TryGetValue(id, out i);
            return i;
        }

        internal static void DaemonLoadListener(LoadComputerXmlReadEvent e)
        {
            IInterface i;
            var id = e.Reader.GetAttribute("interfaceId");
            if (id != null && interfaces.TryGetValue(id, out i))
            {
                var objs = new Dictionary<string, string>();
                var storedObjects = e.Reader.GetAttribute("storedObjects")?.Split(' ');
                if (storedObjects != null)
                    foreach (var s in storedObjects)
                        objs[s.Remove(s.IndexOf('|'))] = s.Substring(s.IndexOf('|') + 1);
                e.Computer.daemons.Add(Instance.CreateInstance(id, e.Computer, objs));
            }
        }
    }
}
