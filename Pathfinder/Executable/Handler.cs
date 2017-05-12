using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Executable
{
    public static class Handler
    {
        internal static Dictionary<string, IInterface> idToInterface = new Dictionary<string, IInterface>();
        private static Dictionary<string, string> idToDataCache = new Dictionary<string, string>();

        /// <summary>
        /// Adds an executable interface by id.
        /// </summary>
        /// <returns>The full mod id if added to the game, <c>null</c> otherwise.</returns>
        /// <param name="id">The Executable Identifier to try and add.</param>
        /// <param name="inter">The interface object.</param>
        public static string RegisterExecutable(string id, IInterface inter)
        {
            if (Pathfinder.CurrentMod == null)
                throw new InvalidOperationException("RegisterExecutable can not be called outside of mod loading.\nMod Blame: "
                                                    + Utility.GetPreviousStackFrameIdentity());
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod '{0}' is attempting to add executable interface {1} with id {2}",
                           Utility.ActiveModId,
                           inter.GetType().FullName,
                           id);
            if (idToInterface.ContainsKey(id))
                return null;
            var type = inter.GetType();
            var fileData = id + "\nldloc.args\ncall Pathfinder.Executable.Instance [" + type.Assembly.GetName().Name + ".dll]"
                                                     + type.FullName + "=" + id + "()";
            if (fileData.Length < 1 || idToDataCache.ContainsValue(fileData))
                throw new ArgumentException("created data for '" + id + "' is not unique");
            idToInterface.Add(id, inter);
            idToDataCache.Add(id, fileData);
            return id;
        }

        [Obsolete("Use RegisterExecutable")]
        public static bool AddExecutable(string id, IInterface inter) => RegisterExecutable(id, inter) != null;

        internal static bool UnregisterExecutable(string id)
        {
            id = Utility.GetId(id);
            if (!idToInterface.ContainsKey(id))
                return true;
            idToDataCache.Remove(id);
            return idToInterface.Remove(id);
        }

        /// <summary>
        /// Determines whether the file data is for a mod interface.
        /// </summary>
        /// <returns><c>true</c>, if the file data is for a mod interface, <c>false</c> otherwise.</returns>
        /// <param name="fileData">File data.</param>
        public static bool IsFileDataForModExe(string fileData)
        {
            var dataLines = fileData.Split('\n');
            return dataLines.Length >= 3 && dataLines[1] == "ldloc.args"
                            && dataLines[2].StartsWith("call Pathfinder.Executable.Instance", StringComparison.Ordinal)
                            && dataLines[2].EndsWith("=" + dataLines[0] + "()", StringComparison.Ordinal)
                            && idToDataCache.ContainsKey(dataLines[0]);
        }

        /// <summary>
        /// Gets the standard file data by id.
        /// </summary>
        /// <returns>The standard file data or <c>nulll</c> if it doesn't exist.</returns>
        /// <param name="id">Executable Identifier.</param>
        /// <param name="requiresModId">If set to <c>true</c> id requires a prefixing mod identifier delimated by period.</param>
        public static string GetStandardFileDataBy(string id, bool requiresModId = false)
        {
            if (requiresModId && id.IndexOf('.') == -1)
                throw new ArgumentException("must contain a mod id and delimter (.)", nameof(id));
            id = Utility.GetId(id, requiresModId, true);
            string result;
            if (idToDataCache.TryGetValue(id, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Gets the first standard file data by interface.
        /// </summary>
        /// <returns>The standard file data or <c>null</c> if it doesn't exist.</returns>
        /// <param name="inter">The Executable Interface</param>
        public static string GetStandardFileDataBy(IInterface inter)
        {
            foreach (var pair in idToInterface)
                if (pair.Value == inter)
                    return GetStandardFileDataBy(pair.Key);
            return null;
        }
    }
}
