using System;
using System.Collections.Generic;
using Pathfinder.Internal;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public static class Handler
    {
        internal static Dictionary<string, Type> PortTypes = new Dictionary<string, Type>();

        public static string RegisterPort(string id, Type port)
        {
            id = InternalUtility.Validate(id, "Port Type", $"[{port}]", true);
            if (PortTypes.ContainsKey(id))
                return null;
            port.PortId = id;
            PortTypes.Add(id, port);
            return id;
        }

        internal static bool UnregisterPort(string id)
        {
            id = Utility.GetId(id);
            if (!PortTypes.ContainsKey(id))
                return true;
            PortTypes[id].PortId = null;
            return PortTypes.Remove(id);
        }

        public static Type GetPort(string id)
        {
            id = Utility.GetId(id);
            PortTypes.TryGetValue(id, out var p);
            return p;
        }
    }
}
