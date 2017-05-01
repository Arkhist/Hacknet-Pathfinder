using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Extension
{
    public static class Handler
    {
        private static Dictionary<string, Info> idToInfo = new Dictionary<string, Info>();

        public static bool RegisterExtension(string id, Info extensionInfo)
        {
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to register extension {1} with id {2}",
                           Utility.GetPreviousStackFrameIdentity(),
                           extensionInfo.GetType().FullName,
                           id);
            if (idToInfo.ContainsKey(id))
                return false;

            extensionInfo.Id = id;
            idToInfo.Add(id, extensionInfo);
            return true;
        }
    }
}
