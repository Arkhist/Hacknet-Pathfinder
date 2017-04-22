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
        private static Dictionary<string, IInterface> interfaces = new Dictionary<string, IInterface>();
        private static Dictionary<string, string> idToDataCache = new Dictionary<string, string>();

        public static bool AddExecutable(string exeIdentity, IInterface inter)
        {
            exeIdentity = Utility.GetPreviousStackFrameIdentity() + "." + exeIdentity;
            Logger.Verbose("Mod {0} is attempting to add executable interface {1} with id {2}",
                           Utility.GetPreviousStackFrameIdentity(),
                           inter.GetType().FullName,
                           exeIdentity);
            if (interfaces.ContainsKey(exeIdentity))
                return false;
            var type = inter.GetType();
            var fileData = exeIdentity + "\nldloc.args\ncall Pathfinder.Executable.Instance [" + type.Assembly.GetName().Name + ".dll]"
                                                     + type.FullName + "=" + exeIdentity + "()";
            if (fileData.Length < 1 || idToDataCache.ContainsValue(fileData))
                throw new ArgumentException("exeName and inter combined is not unique");
            interfaces.Add(exeIdentity, inter);
            idToDataCache.Add(exeIdentity, fileData);
            return true;
        }

        public static bool IsFileDataForMod(string fileData)
        {
            var dataLines = fileData.Split('\n');
            return dataLines.Length >= 3 && dataLines[1] == "ldloc.args"
                            && dataLines[2].StartsWith("call Pathfinder.Executable.Instance ", StringComparison.Ordinal)
                            && dataLines[2].EndsWith("=" + dataLines[0] + "()", StringComparison.Ordinal)
                            && idToDataCache.ContainsValue(dataLines[0]);
        }

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

        public static string GetStandardFileDataBy(IInterface inter)
        {
            foreach (var pair in interfaces)
                if (pair.Value == inter)
                    return GetStandardFileDataBy(pair.Key);
            return null;
        }

        internal static void ExecutableListener(ExecutableExecuteEvent e)
        {
            IInterface i;
            if (IsFileDataForMod(e.ExecutableFile.data) && interfaces.TryGetValue(e.ExecutableFile.data.Split('\n')[0], out i))
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
                    if (!alreadyHandled && IsFileDataForMod(file.data))
                        os.write(name);
                }
                os.write(" ");
            }
        }
    }
}
