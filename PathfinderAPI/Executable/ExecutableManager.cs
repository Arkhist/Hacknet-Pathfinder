using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Event.Gameplay;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable
{
    public static class ExecutableManager
    {
        private struct CustomExeInfo
        {
            public string ExeData;
            public string XmlId;
            public Type ExeType;
        }

        private static readonly List<CustomExeInfo?> CustomExes = new List<CustomExeInfo?>();
        
        static ExecutableManager()
        {
            EventManager<TextReplaceEvent>.AddHandler(GetTextReplacementExe);
            EventManager<ExecutableExecuteEvent>.AddHandler(OnExeExecute);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void GetTextReplacementExe(TextReplaceEvent e)
        {
            var exe = CustomExes.FirstOrDefault(x => x.Value.XmlId == e.Original);
            if (!exe.HasValue)
                return;
            e.Replacement = exe.Value.ExeData;
        }
        private static void OnExeExecute(ExecutableExecuteEvent e)
        {
            var exe = CustomExes.FirstOrDefault(x => x.Value.ExeData == e.ExecutableData);
            if (!exe.HasValue)
                return;
            var location = new Rectangle(e.OS.ram.bounds.X, e.OS.ram.bounds.Y + RamModule.contentStartOffset, RamModule.MODULE_WIDTH, (int)OS.EXE_MODULE_HEIGHT);
            e.OS.addExe((BaseExecutable)Activator.CreateInstance(exe.Value.ExeType, new object[] { location, e.OS, e.Arguments.ToArray() }));
            e.Result = ExecutionResult.StartupSuccess;
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            CustomExes.RemoveAll(x => x.Value.ExeType.Assembly == pluginAsm);
        }

        public static void RegisterExecutable<T>(string xmlName) where T : BaseExecutable => RegisterExecutable(typeof(T), xmlName);
        public static void RegisterExecutable(Type executableType, string xmlName)
        {
            if (!typeof(BaseExecutable).IsAssignableFrom(executableType))
                throw new ArgumentException("Type of exe registered must inherit from Pathfinder.Executable.BaseExecutable!", nameof(executableType));

            var builder = new StringBuilder();
            foreach (var exeByte in Encoding.ASCII.GetBytes("PathfinderExe:" + executableType.FullName))
                builder.Append(Convert.ToString(exeByte, 2));
            CustomExes.Add(new CustomExeInfo
            {
                ExeData = builder.ToString(),
                XmlId = xmlName,
                ExeType = executableType
            });
        }

        public static string GetCustomExeData(string xmlName) => CustomExes.FirstOrDefault(x => x.Value.XmlId == xmlName)?.ExeData;

        public static void UnregisterExecutable(string xmlName)
        {
            CustomExes.RemoveAll(x => x.Value.XmlId == xmlName);
        }
        public static void UnregisterExecutable<T>() => UnregisterExecutable(typeof(T));
        public static void UnregisterExecutable(Type exeType)
        {
            CustomExes.RemoveAll(x => x.Value.ExeType == exeType);
        }
    }
}
