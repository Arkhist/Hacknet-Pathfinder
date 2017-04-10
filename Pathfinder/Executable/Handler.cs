using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;

namespace Pathfinder.Executable
{
    public static class Handler
    {
        private static Dictionary<string, Interface> interfaces = new Dictionary<string, Interface>();
        private static Dictionary<string, string> idToFileData = new Dictionary<string, string>();

        public static bool AddExecutable(string exeIdentity, Interface inter)
        {
            if (interfaces.ContainsKey(exeIdentity))
                return false;
            var type = inter.GetType();
            var fileData = exeIdentity + "\nldloc.args\ncall Pathfinder.Executable.Instance [" + type.Assembly.GetName().Name + ".dll]"
                                                     + type.FullName + "=" + exeIdentity + "()";
            if (fileData.Length < 1 || idToFileData.ContainsValue(fileData))
                throw new ArgumentException("exeName and inter combined is not unique");
            inter.FileData = fileData;
            interfaces.Add(exeIdentity, inter);
            idToFileData.Add(exeIdentity, fileData);
            return true;
        }

        public static bool IsFileDataForMod(string fileData)
        {
            var dataLines = fileData.Split('\n');
            return dataLines.Length == 2 && dataLines[1] == "ldloc.args"
                            && dataLines[2].StartsWith("call Pathfinder.Executable.Instance ", StringComparison.Ordinal)
                            && dataLines[2].EndsWith("=" + dataLines[0] + "()", StringComparison.Ordinal)
                            && idToFileData.ContainsValue(dataLines[0]);
        }

        internal static void ExecutableListener(ExecutableExecuteEvent e)
        {
            if (IsFileDataForMod(e.ExeFile.data))
            {
                Interface i;
                if (interfaces.TryGetValue(e.ExeFile.data.Split('\n')[0], out i) && e.ExeFile.data == i.FileData)
                {
                    int num = e.OsInstance.ram.bounds.Y + RamModule.contentStartOffset;
                    foreach (var exe in e.OsInstance.exes)
                            num += exe.bounds.Height;
                    Rectangle location = new Rectangle(e.OsInstance.ram.bounds.X, num, RamModule.MODULE_WIDTH, (int)OS.EXE_MODULE_HEIGHT);
                    e.OsInstance.addExe(Instance.CreateInstance(i, e.OsInstance, e.Arguments, location));
                    e.Result = ExecutionResult.StartupSuccess;
                }
            }
        }

        internal static void ExecutableListInsertListener(CommandSentEvent e)
        {
            if (e.Arguments[0].Equals("exe"))
            {
                e.IsCancelled = true;
                e.Disconnects = false;
                var os = e.OsInstance;
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
                    foreach (var num in Hacknet.PortExploits.exeNums)
                        if (file.data == Hacknet.PortExploits.crackExeData[num]
                            || file.data == Hacknet.PortExploits.crackExeDataLocalRNG[num])
                        {
                            os.write (name);
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
