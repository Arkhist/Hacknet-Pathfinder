using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Internal;
using Pathfinder.Util;

namespace Pathfinder.Executable
{
    public static class Handler
    {
        internal static readonly Dictionary<string, Tuple<Interface, string>> ModExecutables =
            new Dictionary<string, Tuple<Interface, string>>();

        /// <summary>
        /// Adds an executable interface by id.
        /// </summary>
        /// <returns>The full mod id if added to the game, <c>null</c> otherwise.</returns>
        /// <param name="id">The Executable Identifier to try and add.</param>
        /// <param name="inter">The interface object.</param>
        public static string RegisterExecutable(string id, Interface inter)
        {
            id = InternalUtility.Validate(id, "Executable Interface", inter.GetType().FullName, true);
            if (ModExecutables.ContainsKey(id))
                return null;
            var type = inter.GetType();
            var fileData = GenerateFileDataString(type.Assembly.GetName().Name, type.FullName, id);
            if (fileData.Length < 1 || ModExecutables.Any(pair => pair.Value.Item2 == fileData))
                throw new ArgumentException("created data for '" + id + "' is not unique");
            ModExecutables.Add(id, new Tuple<Interface, string>(inter, fileData));
            return id;
        }

        const string LOAD_PREFIX = "6C 64 6C 6F 63 2E 61 72 67 73";  // \nldloc.args\n
        const string CALL_PREFIX =
                        "63 61 6C 6C 20 50 61 74 68 66 69 6E 64 65 72 2E 45 " +  // call Pathfinder.E
                        "78 65 63 75 74 61 62 6C 65 2E 49 6E 73 74 61 6E 63 65"; // xecutable.Instance
        const string SPACE = "20";
        const string LEFT_BRACKET = "5B";    // [
        const string EXT = "2E 64 6C 6C 5D"; // .dll
        const string RIGHT_BRACKET = "5D";   // ]
        const string EQUAL = "3D";           // =
        const string PARENTHESIS = "28 29";  // ()

        /// <summary>
        /// Generates a file data string for inputs.
        /// </summary>
        /// <returns>The resulting file data string.</returns>
        /// <param name="assemblyName">The Assembly's Name.</param>
        /// <param name="typeFullname">The Type's FullName value.</param>
        /// <param name="id">The current mod's Identifier.</param>
        public static string GenerateFileDataString(string assemblyName, string typeFullname, string id)
        {
            return (id = Utility.ConvertToHexBlocks(id))
                 + $"\n{LOAD_PREFIX}\n{CALL_PREFIX} {SPACE} {LEFT_BRACKET} "
                 + Utility.ConvertToHexBlocks(assemblyName) + $" {EXT} {RIGHT_BRACKET} "
                 + Utility.ConvertToHexBlocks(typeFullname) + $" {EQUAL} " + id + $" {PARENTHESIS}";

        }

        /// <summary>
        /// Removes an executable by it's id
        /// </summary>
        /// <param name="id">The full id of the executable</param>
        /// <returns>If successful</returns>
        public static bool UnregisterExecutable(string id)
        {
            id = Utility.GetId(id);
            return !ModExecutables.ContainsKey(id) || ModExecutables.Remove(id);
        }

        /// <summary>
        /// Determines whether the file data is for a mod interface.
        /// </summary>
        /// <returns><c>true</c>, if the file data is for a mod interface, <c>false</c> otherwise.</returns>
        /// <param name="fileData">The Filedata to check against.</param>
        public static bool IsFileDataForModExe(string fileData)
        {
            var dataLines = fileData.Split('\n');
            return dataLines.Length >= 3
                            &&
                                (dataLines[1].Equals(LOAD_PREFIX, StringComparison.OrdinalIgnoreCase)
                                && dataLines[2].StartsWith(CALL_PREFIX, StringComparison.OrdinalIgnoreCase)
                                && dataLines[2].EndsWith($"{EQUAL} {dataLines[0]} {PARENTHESIS}", StringComparison.OrdinalIgnoreCase))
                            ||
                                (dataLines[1] == "ldloc.args"
                                && dataLines[2].StartsWith("call Pathfinder.Executable.Instance",
                                                           StringComparison.Ordinal)
                                && dataLines[2].EndsWith("=" + dataLines[0] + "()", StringComparison.Ordinal))
                            && ModExecutables.Any(pair => pair.Value.Item2 == fileData
                                                  || Utility.ConvertToHexBlocks(pair.Value.Item2) == fileData);
        }

        /// <summary>
        /// Gets the standard file data by id.
        /// </summary>
        /// <returns>The standard file data or <c>null</c> if it doesn't exist.</returns>
        /// <param name="id">Executable Identifier.</param>
        /// <param name="requiresModId">If set to <c>true</c> id requires a prefixing mod identifier delimated by period.</param>
        public static string GetStandardFileDataBy(string id, bool requiresModId = false)
        {
            if (requiresModId && id.IndexOf('.') == -1)
                throw new ArgumentException("must contain a mod id and delimter (.)", nameof(id));
            id = Utility.GetId(id, requiresModId, true);
            return ModExecutables.TryGetValue(id, out var result) ? result.Item2 : null;
        }

        /// <summary>
        /// Gets the first standard file data by interface.
        /// </summary>
        /// <returns>The standard file data or <c>null</c> if it doesn't exist.</returns>
        /// <param name="inter">The Executable Interface</param>
        public static string GetStandardFileDataBy(Interface inter)
        {
            foreach (var pair in ModExecutables)
                if (pair.Value.Item1 == inter)
                    return GetStandardFileDataBy(pair.Key);
            return null;
        }
    }
}
