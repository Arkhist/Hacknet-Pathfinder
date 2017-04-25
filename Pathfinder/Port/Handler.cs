using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public static class Handler
    {
        private static Dictionary<string, PortType> idToPortType = new Dictionary<string, PortType>();

        public static bool AddPort(string portId, PortType port)
        {
            portId = Utility.GetId(portId);
            Logger.Verbose("Mod {0} attempting to register port [{1}] with id {2}",
                           Utility.GetPreviousStackFrameIdentity(), port, portId);
            if (idToPortType.ContainsKey(portId))
                return false;
            port.PortId = portId;
            idToPortType.Add(portId, port);
            return true;
        }

        public static PortType GetPort(string id)
        {
            id = Utility.GetId(id);
            PortType p = null;
            idToPortType.TryGetValue(id, out p);
            return p;
        }
    }
}
