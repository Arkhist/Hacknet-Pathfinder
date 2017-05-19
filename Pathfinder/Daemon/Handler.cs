using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Daemon
{
    public static class Handler
    {
        internal static Dictionary<string, IInterface> ModDaemons = new Dictionary<string, IInterface>();

        /// <summary>
        /// Registers a daemon interface.
        /// </summary>
        /// <returns>The daemon's full id if added to the game, <c>null</c> otherwise.</returns>
        /// <param name="id">The daemon interface id to insert.</param>
        /// <param name="inter">The interface to add.</param>
        public static string RegisterDaemon(string id, IInterface inter)
        {
            if (Pathfinder.CurrentMod == null)
                throw new InvalidOperationException("RegisterDaemon can not be called outside of mod loading.\nMod Blame: "
                                                    + Utility.GetPreviousStackFrameIdentity());
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to add daemon interface {1} with id {2}",
                           Utility.ActiveModId,
                           inter.GetType().FullName,
                           id);
            if (ModDaemons.ContainsKey(id))
                return null;
            ModDaemons.Add(id, inter);
            return id;
        }

        [Obsolete("Use RegisterDaemon")]
        public static bool AddDaemon(string id, IInterface inter) => RegisterDaemon(id, inter) != null;

        internal static bool UnregisterDaemon(string id)
        {
            id = Utility.GetId(id);
            if (!ModDaemons.ContainsKey(id))
                return true;
            return ModDaemons.Remove(id);
        }

        public static bool ContainsDaemon(string id) => ContainsDaemon(ref id);
        public static bool ContainsDaemon(ref string id) => ModDaemons.ContainsKey(id = Utility.GetId(id));

        public static IInterface GetDaemonById(string id) => GetDaemonById(ref id);
        public static IInterface GetDaemonById(ref string id)
        {
            id = Utility.GetId(id);
            IInterface i = null;
            ModDaemons.TryGetValue(id, out i);
            return i;
        }
    }
}
