using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
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

                var fna = LoadAssembly(exeDir + "FNA.dll");

                ad.AddAssemblyAttribute<InternalsVisibleToAttribute>("Pathfinder");
                ad.RemoveInternals();

                var mod = ad.MainModule;

                var osField = ad.MainModule.GetType("Hacknet.Computer").GetField("os");
                osField.IsPrivate = false;
                osField.IsAssembly = true;

                var missionServer = ad.MainModule.GetType("Hacknet.MissionListingServer");
                foreach (var f in missionServer.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                missionServer = ad.MainModule.GetType("Hacknet.MissionHubServer");
                foreach (var f in missionServer.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                var activeMission = ad.MainModule.GetType("Hacknet.ActiveMission");
                foreach (var m in activeMission.Methods)
                {
                    if (m.IsStatic || m.IsConstructor) continue;
                    m.IsVirtual = true;
                    m.IsNewSlot = true;
                }

                var compSendMsg = ad.MainModule.GetType("Hacknet.Computer").GetMethod("sendNetworkMessage");
                compSendMsg.IsPrivate = false;
                compSendMsg.IsPublic = true;

                var type = ad.MainModule.GetType("Hacknet.DisplayModule");
                var typeVars = type.GetField("x");
                typeVars.IsPrivate = false;
                typeVars.IsPublic = true;
                typeVars = type.GetField("y");
                typeVars.IsPrivate = false;
                typeVars.IsPublic = true;
                typeVars = type.GetField("openLockSprite");
                typeVars.IsPrivate = false;
                typeVars.IsPublic = true;
                typeVars = type.GetField("lockSprite");
                typeVars.IsPrivate = false;
                typeVars.IsPublic = true;

                type = ad.MainModule.GetType("Hacknet.Gui.Button");
                foreach (var m in type.Methods)
                {
                    m.IsPrivate = false;
                    m.IsPublic = true;
                }

                type = ad.MainModule.GetType("Hacknet.Screens.ExtensionsMenuScreen");
                foreach (var m in type.Methods)
                {
                    if (!m.IsPrivate) continue;
                    m.IsPrivate = false;
                    m.IsPublic = true;
                }

                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsPublic = true;
                }

                foreach (var t in type.NestedTypes)
                {
                    t.IsPublic = true;
                    t.IsNestedPublic = true;
                }

                type = ad.MainModule.GetType("Hacknet.Helpfile");
                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                var v2 = ad.MainModule.ImportReference(fna.MainModule.GetType("Microsoft.Xna.Framework.Vector2"));

                type = ad.MainModule.GetType("Hacknet.Computer");
                type.AddRefConstructor(type.GetMethod(".ctor"),
                new TypeReference[] {
                    ad.MainModule.TypeSystem.String,
                    ad.MainModule.TypeSystem.String,
                    v2,
                    ad.MainModule.TypeSystem.Int32,
                    ad.MainModule.TypeSystem.Byte
                },
                new Instruction[] {
                    Instruction.Create(OpCodes.Ldsfld, ad.MainModule.GetType("Hacknet.OS").GetField("currentInstance"))
                });

                type.AddRefConstructor(type.GetMethod(".ctor", ad.MainModule.TypeSystem.String,
                                                      ad.MainModule.TypeSystem.String,
                                                      v2,
                                                      ad.MainModule.TypeSystem.Int32,
                                                      ad.MainModule.TypeSystem.Byte),
                new TypeReference[] {
                    ad.MainModule.TypeSystem.String,
                    ad.MainModule.TypeSystem.String,
                    ad.MainModule.ImportReference(fna.MainModule.GetType("Microsoft.Xna.Framework.Vector2")),
                    ad.MainModule.TypeSystem.Int32
                },
                new Instruction[] {
                    Instruction.Create(OpCodes.Ldc_I4_0)
                });

                if (spitOutHacknetOnly)
                {
                    ad?.Write("HacknetPathfinder.exe");
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
                    13,
                    flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 1 }
                );

                ad.MainModule.GetType("Hacknet.OS").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadSession"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn
                );

                ad.MainModule.GetType("Hacknet.OS").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onPostLoadSession"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
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

                ad.MainModule.GetType("Hacknet.OS").GetMethod("writeSaveGame").InjectWith(
                    hooks.GetMethod("onSaveWrite"),
                    -5,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.PassParametersVal,
                    localsID: new int[] { 0 }
                );

                ad.MainModule.GetType("Hacknet.NetworkMap").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadNetmapContent"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.ProgramRunner").GetMethod("AttemptExeProgramExecution").InjectWith(
                    hooks.GetMethod("onExecutableExecute"),
                    54,
                    flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 1, 2, 6 }
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                var mainMenu = ad.MainModule.GetType("Hacknet.MainMenu");
                mainMenu.GetMethod("DrawBackgroundAndTitle").InjectWith(
                    hooks.GetMethod("onDrawMainMenuTitles"),
                    7,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 0 }
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.OS").GetMethod("launchExecutable").InjectWith(
                    hooks.GetMethod("onPortExecutableExecute"),
                    44,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 2 }
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.ComputerLoader").GetMethod("loadComputer").InjectWith(
                    hooks.GetMethod("onLoadComputer"),
                    197,
                    flags: InjectFlags.PassParametersVal | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 2 }
                );

                ad.MainModule.GetType("Hacknet.Game1").GetMethod("UnloadContent").InjectWith(
                    hooks.GetMethod("onGameUnloadContent"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                ad.MainModule.GetType("Hacknet.Game1").GetMethod("Update").InjectWith(
                    hooks.GetMethod("onGameUpdate"),
                    -5,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.DisplayModule").GetMethod("doProbeDisplay").InjectWith(
                    hooks.GetMethod("onPortNameDraw"),
                    -158,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 1, 10 }
                );

                ad.MainModule.GetType("Hacknet.DisplayModule").GetMethod("Update").InjectWith(
                    hooks.GetMethod("onDisplayModuleUpdate"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                ad.MainModule.GetType("Hacknet.DisplayModule").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onDisplayModuleDraw"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // SENSIBLE CODE, CHANGE OFFSET IF NEEDED
                ad.MainModule.GetType("Hacknet.Screens.ExtensionsMenuScreen").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onExtensionsMenuScreenDraw"),
                    71,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 3 }
                );

                ad.MainModule.GetType("Hacknet.Screens.ExtensionsMenuScreen").GetMethod("DrawExtensionList").InjectWith(
                    hooks.GetMethod("onExtensionsMenuListDraw"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                ad?.Write("HacknetPathfinder.exe");
            }
            catch (Exception ex)
            {
                try
                {
                    ad?.Write("HacknetPathfinder.exe");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Writing problem: " + e);
                }
                Console.Write(ex);
                Console.WriteLine("Press enter to end...");
                Console.ReadLine();
            }
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

        public static MethodDefinition AddRefConstructor(this TypeDefinition type,
                                                         MethodReference mref,
                                                         IEnumerable<TypeReference> args,
                                                         IEnumerable<Instruction> extraInstructions = null)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefinition(".ctor", methodAttributes, type.Module.TypeSystem.Void);
            foreach (var a in args)
                method.Parameters.Add(new ParameterDefinition(a));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            int i = 0;
            foreach (var a in args)
                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, method.Parameters[i++]));
            if (extraInstructions != null) foreach (var inst in extraInstructions)
                method.Body.Instructions.Add(inst);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, mref));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(method);
            return method;
        }
    }
}
