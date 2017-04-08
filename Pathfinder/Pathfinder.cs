using Pathfinder.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pathfinder
{
    static class Pathfinder
    {
        static Dictionary<string, PathfinderMod> mods = new Dictionary<string, PathfinderMod>();

        public static void init()
        {
            EventManager.RegisterListener<CommandSentEvent>(Command.Handler.CommandListener);

            EventManager.RegisterListener<DrawMainMenuEvent>(GUI.PathfinderMainMenu.drawMainMenu);
            EventManager.RegisterListener<DrawMainMenuButtonsEvent>(GUI.PathfinderMainMenu.drawPathfinderButtons);

            EventManager.RegisterListener<ExecutableExecuteEvent>(Executable.Handler.ExecutableListener);
            EventManager.RegisterListener<CommandSentEvent>(Executable.Handler.ExecutableListInsertListener);

            LoadMods();
        }

        public static void testEventListener(PathfinderEvent pathfinderEvent)
        {
            Console.WriteLine("HEY ! LISTEN !!");
        }

        public static void LoadMods()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            char separator = Path.DirectorySeparatorChar;

            if (!Directory.Exists(path + separator + "Mods"))
                Directory.CreateDirectory(path + separator + "Mods");

            foreach (string dll in Directory.GetFiles(path + separator + "Mods" + separator, "*.dll"))
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
    }
}
