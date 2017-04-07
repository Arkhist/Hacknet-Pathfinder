using System;
using System.Collections.Generic;
using Hacknet;
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
            var fileData = "ldloc.args\ncall Pathfinder.Executable.Instance [" + type.Assembly.FullName + "::" + type.Module.Name + "]"
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
                e.OsInstance.addExe(new Instance(e.Location, e.OsInstance, e.Parameters, i, i.FileData));
            }
        }
    }
}
