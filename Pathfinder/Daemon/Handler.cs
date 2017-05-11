using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Daemon
{
    public static class Handler
    {
        internal static Dictionary<string, IInterface> idToInterface = new Dictionary<string, IInterface>();

        public static string RegisterDaemon(string id, IInterface inter)
        {
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to add daemon interface {1} with id {2}",
                           Utility.ActiveModId,
                           inter.GetType().FullName,
                           id);
            if (idToInterface.ContainsKey(id))
                return null;
            idToInterface.Add(id, inter);
            return id;
        }

        [Obsolete("Use RegisterDaemon")]
        public static bool AddDaemon(string id, IInterface inter) => RegisterDaemon(id, inter) != null;

        internal static bool UnregisterDaemon(string id)
        {
            id = Utility.GetId(id);
            if (!idToInterface.ContainsKey(id))
                return true;
            return idToInterface.Remove(id);
        }

        public static bool ContainsDaemon(string id) => ContainsDaemon(ref id);
        public static bool ContainsDaemon(ref string id) => idToInterface.ContainsKey(id = Utility.GetId(id));

        public static IInterface GetDaemonById(string id) => GetDaemonById(ref id);
        public static IInterface GetDaemonById(ref string id)
        {
            id = Utility.GetId(id);
            IInterface i = null;
            idToInterface.TryGetValue(id, out i);
            return i;
        }
    }
}
