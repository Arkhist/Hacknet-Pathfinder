using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pathfinder.Event;

namespace Pathfinder
{
    static class Pathfinder
    {
        class ExceptionInvalidId : ArgumentException
        {
            public ExceptionInvalidId(string msg) : base(msg) { }
        }

        private static Dictionary<string, PathfinderMod> mods = new Dictionary<string, PathfinderMod>();
        //private static bool currentSaveMissingMods = false;

        public static readonly string ModFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                                          + Path.DirectorySeparatorChar + "Mods";

        public static void init()
        {
            EventManager.RegisterListener<CommandSentEvent>(Command.Handler.CommandListener);

            EventManager.RegisterListener<DrawMainMenuEvent>(GUI.PathfinderMainMenu.drawMainMenu);
            EventManager.RegisterListener<DrawMainMenuButtonsEvent>(GUI.PathfinderMainMenu.drawPathfinderButtons);

            EventManager.RegisterListener<ExecutableExecuteEvent>(Executable.Handler.ExecutableListener);
            EventManager.RegisterListener<CommandSentEvent>(Executable.Handler.ExecutableListInsertListener);

            EventManager.RegisterListener<SaveWriteEvent>(ManageSaveXml);

            EventManager.RegisterListener<LoadContentEvent>(Util.ExeInfoManager.LoadExecutableStruct);

            LoadMods();
        }

        public static void testEventListener(PathfinderEvent pathfinderEvent)
        {
            Console.WriteLine("HEY ! LISTEN !!");
        }

        public static void LoadMods()
        {
            var separator = Path.DirectorySeparatorChar;

            if (!Directory.Exists(ModFolderPath))
                Directory.CreateDirectory(ModFolderPath);

            foreach (string dll in Directory.GetFiles(ModFolderPath + separator, "*.dll"))
            {
                try
                {
                    var modAssembly = Assembly.LoadFile(dll);
                    Type modType = null;
                    foreach (Type t in (modAssembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(PathfinderMod)))))
                    {
                        modType = t;
                        break;
                    }
                    var modInstance = (PathfinderMod)Activator.CreateInstance(modType);

                    var methodInfo = modType.GetMethod("GetIdentifier");
                    if (methodInfo == null)
                        throw new NotSupportedException("Method 'GetIdentifier' doesn't exist : invalid Mod.dll");
                    var name = (string)methodInfo.Invoke(modInstance, null);
                    if (name == "Pathfinder" || name == "Hacknet" || mods.ContainsKey(name))
                        throw new ExceptionInvalidId("Mod with identifier '" + name + "' is either already loaded or is reserved");
                    if (name.Contains('.'))
                        throw new ExceptionInvalidId("Mod identifier '" + name + "' contains a period, mod identifiers may not contain a period (.)");
                    Console.WriteLine("Loading mod : " + name);

                    mods.Add(name, modInstance);

                    modInstance.Load();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Impossible to load mod " + dll + " : " + ex.Message);
                }
            }
        }

        internal static PathfinderMod GetModByAssembly(Assembly asm)
        {
            foreach (var pair in mods)
            {
                if (pair.Value.GetType().Assembly == asm)
                    return pair.Value;
            }
            return null;
        }

        public static void LoadModContent()
        {
            foreach (KeyValuePair<string, PathfinderMod> mod in mods)
            {
                mod.Value.LoadContent();
            }
        }

        internal static string[] LoadedModIdentifiers
        {
            get
            {
                return mods.Keys.ToArray();
            }
        }

        internal static void ManageSaveXml(SaveWriteEvent e)
        {
            var i = e.SaveString.IndexOf("</HacknetSave>", StringComparison.Ordinal);
            string modListStr = "";
            foreach (var pair in mods)
                modListStr += "\t<Mod assembly='" + pair.Value.GetType().Assembly.GetName().Name + "'>" + pair.Key + "</Mod>\n";
            e.SaveString = e.SaveString.Insert(i, "\n<PathfinderMods>\n" + modListStr + "</PathfinderMods>\n");
        }
    }
}
