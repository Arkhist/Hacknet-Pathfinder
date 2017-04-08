using System;
using System.Collections.Generic;
using Pathfinder.Event;

namespace Pathfinder.Executable
{
    public static class Handler
    {
        private static Dictionary<string, Interface> interfaces = new Dictionary<string, Interface>();
        private static Dictionary<string, string> nameToFileData = new Dictionary<string, string>();

        public static bool AddExecutable(string exeName, Interface inter)
        {
            if (interfaces.ContainsKey(exeName))
                return false;
            var type = inter.GetType();
            var fileData = "ldloc.args\ncall Pathfinder.Executable.Instance [" + type.Assembly.GetName().Name + "::" + type.Module.Name + "]"
                                                     + type.FullName + "=" + exeName + "()";
            if (fileData.Length < 1 || nameToFileData.ContainsValue(fileData))
                throw new ArgumentException("exeName and inter combined is not unique");
            inter.FileData = fileData;
            interfaces.Add(exeName, inter);
            nameToFileData.Add(exeName, fileData);
            return true;
        }

        internal static void ExecutableListener(ExecutableExecuteEvent e)
        {
            Interface i;
            if (interfaces.TryGetValue(e.ExecutableName, out i)
                && e.ExecutableData.Equals(i.FileData))
            {
                e.OsInstance.addExe(new Instance(e.Location, e.OsInstance, e.Parameters, i));
                e.IsCancelled = true;
            }
        }

        internal static void ExecutableListInsertListener(CommandSentEvent e)
        {
            if (e.Args[0].Equals("exe"))
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
                    if (!alreadyHandled && nameToFileData.ContainsValue(file.name))
                        os.write(name);
                }
                os.write(" ");
            }
        }
    }
}
