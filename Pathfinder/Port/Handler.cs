using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public static class Handler
    {
        internal static Dictionary<string, Type> PortTypes = new Dictionary<string, Type>();

        public static string RegisterPort(string id, Type port)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterPort can not be called outside of mod or extension loading.");
            id = Pathfinder.CurrentMod != null ? Utility.GetId(id, throwFindingPeriod: true) : Extension.Handler.ActiveInfo.Id+"."+id;
            Logger.Verbose("{0} {1} is attempting to add port type [{2}] with id {3}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension",
                           Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id,
                           port,
                           id);
            if (PortTypes.ContainsKey(id))
                return null;
            port.PortId = id;
            PortTypes.Add(id, port);
            return id;
        }

        [Obsolete("Use RegisterPort")]
        public static bool AddPort(string id, Type port) => RegisterPort(id, port) != null;

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
            Type p = null;
            PortTypes.TryGetValue(id, out p);
            return p;
        }
    }
}
