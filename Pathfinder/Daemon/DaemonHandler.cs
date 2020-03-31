using System;
using System.Collections.Generic;
using Pathfinder.Internal;
using Pathfinder.Util;

namespace Pathfinder.Daemon
{
    public static class Handler
    {
        internal static Dictionary<string, Interface> ModDaemons = new Dictionary<string, Interface>();

        /// <summary>
        /// Registers a daemon interface.
        /// </summary>
        /// <returns>The daemon's full id if added to the game, <c>null</c> otherwise.</returns>
        /// <param name="id">The daemon interface id to insert.</param>
        /// <param name="inter">The interface to add.</param>
        public static string RegisterDaemon(string id, Interface inter)
        {
            id = InternalUtility.Validate(id, "Daemon", inter.GetType().FullName, true);
            if (ModDaemons.ContainsKey(id))
                return null;
            ModDaemons.Add(id, inter);
            return id;
        }

        internal static bool UnregisterDaemon(string id)
        {
            id = Utility.GetId(id);
            if (!ModDaemons.ContainsKey(id))
                return true;
            return ModDaemons.Remove(id);
        }

        public static bool ContainsDaemon(string id) => ContainsDaemon(ref id);
        public static bool ContainsDaemon(ref string id) => ModDaemons.ContainsKey(id = Utility.GetId(id));

        public static Interface GetDaemonById(string id) => GetDaemonById(ref id);
        public static Interface GetDaemonById(ref string id)
        {
            id = Utility.GetId(id);
            ModDaemons.TryGetValue(id, out Interface i);
            return i;
        }
    }
}
