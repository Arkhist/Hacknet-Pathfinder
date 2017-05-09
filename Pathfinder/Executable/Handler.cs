using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
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
            id = Utility.GetId(id, requiresModId, ignoreValidXml: true);
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

        internal static void ExecutableListener(ExecutableExecuteEvent e)
        {
            IInterface i;
            if (IsFileDataForModExe(e.ExecutableFile.data) && idToInterface.TryGetValue(e.ExecutableFile.data.Split('\n')[0], out i))
            {
                int num = e.OS.ram.bounds.Y + RamModule.contentStartOffset;
                foreach (var exe in e.OS.exes)
                    num += exe.bounds.Height;
                var location = new Rectangle(e.OS.ram.bounds.X, num, RamModule.MODULE_WIDTH, (int)Hacknet.OS.EXE_MODULE_HEIGHT);
                e.OS.addExe(Instance.CreateInstance(i, e.ExecutableFile, e.OS, e.Arguments, location));
                e.Result = ExecutionResult.StartupSuccess;
            }
        }

        internal static void ExecutableListInsertListener(CommandSentEvent e)
        {
            if (e.Arguments[0].Equals("exe"))
            {
                e.IsCancelled = true;
                e.Disconnects = false;
                var os = e.OS;
                var folder = os.thisComputer.files.root.searchForFolder("bin");
                os.write("Available Executables:\n");
                os.write("PortHack");
                os.write("ForkBomb");
                os.write("Shell");
                os.write("Tutorial");
                foreach (var file in folder.files)
                {
                    bool alreadyHandled = false;
                    var name = file.name.Contains(".") ? file.name.Remove(file.name.LastIndexOf('.')) : file.name;
                    foreach (var num in PortExploits.exeNums)
                        if (file.data == PortExploits.crackExeData[num]
                            || file.data == PortExploits.crackExeDataLocalRNG[num])
                        {
                            os.write(name);
                            alreadyHandled = true;
                            break;
                        }
                    if (!alreadyHandled && IsFileDataForModExe(file.data))
                        os.write(name);
                }
                os.write(" ");
            }
        }
    }
}
