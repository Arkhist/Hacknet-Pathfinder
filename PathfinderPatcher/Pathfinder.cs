using System;
using System.IO;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Inject;

namespace PathfinderPatcher
{
    // This will be the modloader.
    public static class PatcherProgram
    {
        internal static void Main(string[] args)
        {
            AssemblyDefinition ad = null;
            try
            {
                Console.WriteLine(String.Join("|", args));
                string pathfinderDir = "", exeDir = "";
                bool spitOutHacknetOnly = false;
                int index = 0;

                char separator = Path.DirectorySeparatorChar;
                foreach (var arg in args)
                {
                    if (arg.Equals("-pathfinderDir"))
                        pathfinderDir = args[index + 1] + separator;
                    if (arg.Equals("-exeDir"))
                        exeDir = args[index + 1] + separator;
                    if (arg.Equals("-spit"))
                        spitOutHacknetOnly = true;
                    index++;
                }

                // Opens Hacknet.exe, mods it, and then HacknetPathfinder.dll's Assembly
                ad = LoadAssembly(exeDir + "Hacknet.exe");

                ad.AddAssemblyAttribute<InternalsVisibleToAttribute>("Pathfinder");
                ad.RemoveInternals();

                if (spitOutHacknetOnly)
                {
                    ad.Write("HacknetPathfinder.exe");
                    return;
                }

                var pathfinder = LoadAssembly(pathfinderDir + "Pathfinder.dll");

                var hooks = pathfinder.MainModule.GetType("Pathfinder.PathfinderHooks");

                // Hook to the Program.Main
                ad.EntryPoint.InjectWith(hooks.GetMethod("onMain"),
                    flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn);

                // Hook to the Game1.LoadContent
                ad.MainModule.GetType("Hacknet.Game1").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadContent"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                ad.MainModule.GetType("Hacknet.ProgramRunner").GetMethod("ExecuteProgram").InjectWith(
                    hooks.GetMethod("onCommandSent"),
                    flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn
                );

                ad.MainModule.GetType("Hacknet.OS").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadSession"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn
                );

                ad.MainModule.GetType("Hacknet.OS").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onPostLoadSession"),
                    flags: InjectFlags.PassInvokingInstance,
                    dir: InjectDirection.After
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.MainMenu").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onMainMenuDraw"),
                    120,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassParametersVal
                );

                ad.MainModule.GetType("Hacknet.MainMenu").GetMethod("drawMainMenuButtons").InjectWith(
                    hooks.GetMethod("onMainMenuButtonsDraw"),
                    flags: InjectFlags.PassInvokingInstance,
                    dir: InjectDirection.After
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.OS").GetMethod("loadSaveFile").InjectWith(
                    hooks.GetMethod("onLoadSaveFile"),
                    33,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.ModifyReturn,
                    localsID: new int[] { 0, 1 }
                );

                ad.MainModule.GetType("Hacknet.OS").GetMethod("writeSaveGame").InjectWith(
                    hooks.GetMethod("onSaveFile"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal | InjectFlags.ModifyReturn
                );

                ad.MainModule.GetType("Hacknet.NetworkMap").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadNetmapContent"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.OS").GetMethod("launchExecutable").InjectWith(
                    hooks.GetMethod("onExecutableExecute"),
                    48,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 2 }
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                var mainMenu = ad.MainModule.GetType("Hacknet.MainMenu");
                mainMenu.GetMethod("DrawBackgroundAndTitle").InjectWith(
                    hooks.GetMethod("onDrawMainMenuTitles"),
                    7,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 0 }
                );
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                Console.WriteLine("Press enter to end...");
                Console.ReadLine();
            }
            if (ad != null)
                ad.Write("HacknetPathfinder.exe");
        }

        internal static void AddAssemblyAttribute<T>(this AssemblyDefinition ad, params object[] attribArgs)
        {
            var paramTypes = attribArgs.Length > 0 ? new Type[attribArgs.Length] : Type.EmptyTypes;
            int index = 0;
            foreach (var param in attribArgs)
            {
                paramTypes[index] = param.GetType();
                index++;
            }
            var attribCtor = ad.MainModule.ImportReference(typeof(T).GetConstructor(paramTypes));
            var attrib = new CustomAttribute(attribCtor);
            foreach (var param in attribArgs)
            {
                attrib.ConstructorArguments.Add(
                    new CustomAttributeArgument(ad.MainModule.ImportReference(param.GetType()), param)
                );
            }
            ad.CustomAttributes.Add(attrib);
        }

        internal static void RemoveInternals(this AssemblyDefinition ad)
        {
            foreach (TypeDefinition type in ad.MainModule.Types)
            {
                if (type.IsNotPublic)
                    type.IsNotPublic = false;
                if (!type.IsPublic)
                    type.IsPublic = true;

                /*if (type.HasFields)
                    foreach (FieldDefinition field in type.Fields)
                        if (field.IsPublic)
                            field.IsAssembly = false;*/
            }
        }

        internal static AssemblyDefinition LoadAssembly(string fileName, ReaderParameters parameters = null)
        {
            parameters = parameters ?? new ReaderParameters(ReadingMode.Deferred);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName is null/empty");
            }
            Stream stream = new FileStream(fileName, FileMode.Open, parameters.ReadWrite ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read);
            if (parameters.InMemory)
            {
                var memoryStream = new MemoryStream(stream.CanSeek ? ((int)stream.Length) : 0);
                using (stream)
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0L;
                stream = memoryStream;
            }
            ModuleDefinition result;
            try
            {
                result = ModuleDefinition.ReadModule(stream, parameters);
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
            return result.Assembly;
        }
    }
}
