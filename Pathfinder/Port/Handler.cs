using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public static class Handler
    {
        internal static Dictionary<string, Type> idToPortType = new Dictionary<string, Type>();

        public static string RegisterPort(string id, Type port)
        {
            if (Pathfinder.CurrentMod == null)
                throw new InvalidOperationException("RegisterPort can not be called outside of mod loading.\nMod Blame: "
                                                    + Utility.GetPreviousStackFrameIdentity());
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to register port [{1}] with id {2}", Utility.ActiveModId, port, id);
            if (idToPortType.ContainsKey(id))
                return null;
            port.PortId = id;
            idToPortType.Add(id, port);
            return id;
        }

        [Obsolete("Use RegisterPort")]
        public static bool AddPort(string id, Type port) => RegisterPort(id, port) != null;

        internal static bool UnregisterPort(string id)
        {
            id = Utility.GetId(id);
            if (!idToPortType.ContainsKey(id))
                return true;
            idToPortType[id].PortId = null;
            return idToPortType.Remove(id);
        }

        public static Type GetPort(string id)
        {
            id = Utility.GetId(id);
            Type p = null;
            idToPortType.TryGetValue(id, out p);
            return p;
        }
    }
}
