using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Event.Gameplay;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable
{
    public static class ExecutableHandler
    {
        private struct CustomExeInfo
        {
            public string ExeData;
            public string XmlId;
            public Type ExeType;
        }

        private static List<CustomExeInfo?> CustomExes = new List<CustomExeInfo?>();
        
        static ExecutableHandler()
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
            var pluginTypes = pluginAsm.GetTypes();
            CustomExes.RemoveAll(x => pluginTypes.Contains(x.Value.ExeType));
        }

        public static void RegisterExecutable<T>(string xmlName) where T : BaseExecutable => RegisterExecutable(typeof(T), xmlName);
        public static void RegisterExecutable(Type executableType, string xmlName)
        {
            if (executableType.BaseType != typeof(BaseExecutable))
                throw new ArgumentException("Type of exe registered must inherit from Pathfinder.Executable.BaseExecutable!", nameof(executableType));

            string hex = BitConverter.ToString(System.Text.Encoding.ASCII.GetBytes("PathfinderExe:" + executableType.FullName));
            CustomExes.Add(new CustomExeInfo
            {
                ExeData = hex,
                XmlId = xmlName,
                ExeType = executableType
            });
        }
    }
}
