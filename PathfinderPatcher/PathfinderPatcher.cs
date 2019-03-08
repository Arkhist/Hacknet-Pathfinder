using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                // pathfinder arguments
                foreach (var arg in args)
                {
                    if (arg.Equals("-pathfinderDir")) // the Pathfinder.dll's directory
                        pathfinderDir = args[index + 1] + separator;
                    if (arg.Equals("-exeDir")) // the Hacknet.exe's directory
                        exeDir = args[index + 1] + separator;
                    if (arg.Equals("-spit")) // spit modifications without injected code
                        spitOutHacknetOnly = true;
                    index++;
                }

                if(File.Exists("Hacknet"))
                {
                    var txt = File.ReadAllText("Hacknet");
                    txt = txt.Replace("Hacknet", "HacknetPathfinder");
                    File.WriteAllText("HacknetPathfinder", txt);
                }

                foreach (var n in new string[]{ "Hacknet.bin.x86", "Hacknet.bin.x86_64", "Hacknet.bin.osx" })
                    if(File.Exists(n)) File.Copy(n, n.Replace("Hacknet", "HacknetPathfinder"), true);

                // Loads Hacknet.exe's assembly
                ad = LoadAssembly(exeDir + "Hacknet.exe");

                // Loads FNA.dll for some extra injections
                var fna = LoadAssembly(exeDir + "FNA.dll");

                // Adds Pathfinder internal attribute hack
                ad.AddAssemblyAttribute<InternalsVisibleToAttribute>("Pathfinder");
                // Removes internal visibility from types
                ad.RemoveInternals();

                var mod = ad.MainModule;

                // Ensure the os field is internal
                var osField = ad.MainModule.GetType("Hacknet.Computer").GetField("os");
                osField.IsPrivate = false;
                osField.IsAssembly = true;

                // Ensure MissionListingServer's fields are internal
                var missionServer = ad.MainModule.GetType("Hacknet.MissionListingServer");
                foreach (var f in missionServer.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                // Ensure MissionHubServer's fields are internal
                missionServer = ad.MainModule.GetType("Hacknet.MissionHubServer");
                foreach (var f in missionServer.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                // Ensure ActiveMission's methods are virtual
                var activeMission = ad.MainModule.GetType("Hacknet.ActiveMission");
                foreach (var m in activeMission.Methods)
                {
                    if (m.IsStatic || m.IsConstructor) continue;
                    m.IsVirtual = true;
                    m.IsNewSlot = true;
                }

                // Ensure Hacknet.Computer.sendNetworkMessage is public
                var compSendMsg = ad.MainModule.GetType("Hacknet.Computer").GetMethod("sendNetworkMessage");
                compSendMsg.IsPrivate = false;
                compSendMsg.IsPublic = true;

                // Ensure important DisplayModule fields are public
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

                // Ensure Button's methods are public
                type = ad.MainModule.GetType("Hacknet.Gui.Button");
                foreach (var m in type.Methods)
                {
                    m.IsPrivate = false;
                    m.IsPublic = true;
                }

                // Ensure ExtensionsMenuScreen's methods, fields, and nested type are public
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

                // Ensure Helpfile's fields are internal
                type = ad.MainModule.GetType("Hacknet.Helpfile");
                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                // Retrieve FNA's Vector2 as a type reference
                var v2 = ad.MainModule.ImportReference(fna.MainModule.GetType("Microsoft.Xna.Framework.Vector2"));

                // Add simplified constructor implictedly referencing Hacknet.OS.currentInstance
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

                // Add simplified constructor assigning the compType value to 0
                type.AddRefConstructor(type.GetMethod(".ctor", ad.MainModule.TypeSystem.String,
                                                      ad.MainModule.TypeSystem.String,
                                                      v2,
                                                      ad.MainModule.TypeSystem.Int32,
                                                      ad.MainModule.TypeSystem.Byte),
                new TypeReference[] {
                    ad.MainModule.TypeSystem.String,
                    ad.MainModule.TypeSystem.String,
                    v2,
                    ad.MainModule.TypeSystem.Int32
                },
                new Instruction[] {
                    Instruction.Create(OpCodes.Ldc_I4_0)
                });

                // Ensure OS's fields and methods are internal, also ensure introTextModule is public
                type = ad.MainModule.GetType("Hacknet.OS");
                foreach (var f in type.Fields)
                {
                    if (f.Name == "introTextModule") f.IsPublic = true;
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }
                foreach (var m in type.Methods)
                {
                    if (!m.IsPrivate) continue;
                    m.IsPrivate = false;
                    m.IsAssembly = true;
                }

                // Ensure ComputerLoader's fields are internal and nest types nested public
                type = ad.MainModule.GetType("Hacknet.ComputerLoader");
                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsAssembly = true;
                }

                foreach (var t in type.NestedTypes) t.IsNestedPublic = true;

                type.GetMethod("findComp").IsPublic = true;

                // Ensure IntroTextModule's fields are public
                type = ad.MainModule.GetType("Hacknet.IntroTextModule");
                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsPublic = true;
                }

                // Ensure OptionsMenu's fields are public
                type = ad.MainModule.GetType("Hacknet.OptionsMenu");
                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsPublic = true;
                }

                type = ad.MainModule.GetType("Hacknet.DatabaseDaemon");
                foreach (var f in type.Fields)
                {
                    if (!f.IsPrivate) continue;
                    f.IsPrivate = false;
                    f.IsPublic = true;
                }

                // Spit out changes and exit
                if (spitOutHacknetOnly)
                {
                    ad?.Write("HacknetPathfinder.exe");
                    return;
                }

                // Load Pathfinder.dll's assembly
                var pathfinder = LoadAssembly(pathfinderDir + "Pathfinder.dll");

                // Retrieve the hook methods
                var hooks = pathfinder.MainModule.GetType("Pathfinder.PathfinderHooks");

                // Hook onMain to Program.Main
                ad.EntryPoint.InjectWith(hooks.GetMethod("onMain"),
                    flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn);

                // Hook onLoadContent to Game1.LoadContent
                ad.MainModule.GetType("Hacknet.Game1").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadContent"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                // Hook onCommandSent to ProgramRunner.ExecuteProgram
                ad.MainModule.GetType("Hacknet.ProgramRunner").GetMethod("ExecuteProgram").InjectWith(
                    hooks.GetMethod("onCommandSent"),
                    13,
                    flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 1 }
                );

                // Hook onLoadSession to OS.LoadContent
                ad.MainModule.GetType("Hacknet.OS").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadSession"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn
                );

                // Hook onPostLoadSession to OS.LoadContent
                ad.MainModule.GetType("Hacknet.OS").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onPostLoadSession"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                // Hook onUnloadSession to OS.UnloadContent
                ad.MainModule.GetType("Hacknet.OS").GetMethod("UnloadContent").InjectWith(
					hooks.GetMethod("onUnloadSession"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onMainMenuDraw to MainMenu.Draw
                ad.MainModule.GetType("Hacknet.MainMenu").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onMainMenuDraw"),
                    120,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassParametersVal
                );

                // Hook onMainMenuButtonsDraw to MainMenu.drawMainMenuButtons
                ad.MainModule.GetType("Hacknet.MainMenu").GetMethod("drawMainMenuButtons").InjectWith(
                    hooks.GetMethod("onMainMenuButtonsDraw"),
                    248,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 4 }
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onLoadSaveFile to OS.loadSaveFile
                ad.MainModule.GetType("Hacknet.OS").GetMethod("loadSaveFile").InjectWith(
                    hooks.GetMethod("onLoadSaveFile"),
                    33,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.ModifyReturn,
                    localsID: new int[] { 0, 1 }
                );

                // Hook onSaveFile to OS.writeSaveGame
                ad.MainModule.GetType("Hacknet.OS").GetMethod("writeSaveGame").InjectWith(
                    hooks.GetMethod("onSaveFile"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal | InjectFlags.ModifyReturn
                );

                // Hook onSaveWrite to OS.writeSaveGame
                ad.MainModule.GetType("Hacknet.OS").GetMethod("writeSaveGame").InjectWith(
                    hooks.GetMethod("onSaveWrite"),
                    -5,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.PassParametersVal,
                    localsID: new int[] { 0 }
                );

                // Hook onLoadNetmapContent to NetworkMap.LoadContent
                ad.MainModule.GetType("Hacknet.NetworkMap").GetMethod("LoadContent").InjectWith(
                    hooks.GetMethod("onLoadNetmapContent"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onExecutableExecute to ProgramRunner.AttemptExeProgramExecution
                ad.MainModule.GetType("Hacknet.ProgramRunner").GetMethod("AttemptExeProgramExecution").InjectWith(
                    hooks.GetMethod("onExecutableExecute"),
                    54,
                    flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 1, 2, 6 }
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onDrawMainMenuTitles to MainMenu.DrawBackgroundAndTitle
                ad.MainModule.GetType("Hacknet.MainMenu").GetMethod("DrawBackgroundAndTitle").InjectWith(
                    hooks.GetMethod("onDrawMainMenuTitles"),
                    7,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 0 }
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onPortExecutableExecute to OS.launchExecutable
                ad.MainModule.GetType("Hacknet.OS").GetMethod("launchExecutable").InjectWith(
                    hooks.GetMethod("onPortExecutableExecute"),
                    44,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 2 }
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onLoadComputer to ComputerLoader.loadComputer
                // adds the obsfuscated c value in <>c__DisplayClass4 as a parameter (seen as nearbyNodeOffset decompiled)
                // 232 puts it right before the nearbyNodeOffset.type == 4 if statement that erases all home folder data
                type = ad.MainModule.GetType("Hacknet.ComputerLoader");
                var method = type.GetMethod("loadComputer");

                method.InjectWith(
                    hooks.GetMethod("onLoadContentComputerStart"),
                    flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                method.InjectWith(
                    hooks.GetMethod("onLoadContentComputerEnd"),
                    -2,
                    flags: InjectFlags.PassParametersRef | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 153 }
                );

                type.GetMethod("filter").InjectWith(
                    hooks.GetMethod("onFilterString"),
                    flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                /*method.InjectWithLocalFieldParameter(
                    hooks.GetMethod("onLoadContentComputerEnd"),
                    -1,
                    152,
                    type.NestedTypes.First((arg) => arg.Name == "<>c__DisplayClass4").GetField("c"),
                    true,
                    InjectDirection.Before,
                    InjectFlags.PassParametersRef | InjectFlags.PassLocals,
                    new int[] { 0, 153 }
                );*/

                method = ad.MainModule.GetType("Hacknet.Computer").GetMethod("load");

                method.InjectWith(
                    hooks.GetMethod("onLoadSavedComputerStart"),
                    flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                method.InjectWith(
                    hooks.GetMethod("onLoadSavedComputerEnd"),
                    -2,
                    flags: InjectFlags.PassParametersRef | InjectFlags.PassLocals,
                    localsID: new int[] { 97 }
                );



                /*method.InjectWithLocalFieldParameter(
                    hooks.GetMethod("onLoadComputer"),
                    266,
                    152,
                    type.NestedTypes.First((arg) => arg.Name == "<>c__DisplayClass4").GetField("c"),
                    false,
                    InjectDirection.Before,
                    InjectFlags.PassParametersVal | InjectFlags.PassLocals,
                    new int[] { 1 }
                );
                method.AdjustInstruction(240, operand: method.Inst(265));*/

                // Hook onLoadSaveComputer to Hacknet.Computer
                /*ad.MainModule.GetType("Hacknet.Computer").GetMethod("load").InjectWith(
                    hooks.GetMethod("onLoadSaveComputer"),
                    355,
                    flags: InjectFlags.PassParametersRef | InjectFlags.PassLocals,
                    localsID: new int[] { 23 }
                );*/

                // Hook onGameUnloadContent to Game1.UnloadContent
                ad.MainModule.GetType("Hacknet.Game1").GetMethod("UnloadContent").InjectWith(
                    hooks.GetMethod("onGameUnloadContent"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                // Hook onGameUpdate to Game1.Update
                ad.MainModule.GetType("Hacknet.Game1").GetMethod("Update").InjectWith(
                    hooks.GetMethod("onGameUpdate"),
                    -5,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onPortNameDraw to DisplayModule.doProbeDisplay
                ad.MainModule.GetType("Hacknet.DisplayModule").GetMethod("doProbeDisplay").InjectWith(
                    hooks.GetMethod("onPortNameDraw"),
                    -158,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals,
                    localsID: new int[] { 0, 1, 10 }
                );

                // Hook onDisplayModuleUpdate to DisplayModule.Update
                ad.MainModule.GetType("Hacknet.DisplayModule").GetMethod("Update").InjectWith(
                    hooks.GetMethod("onDisplayModuleUpdate"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // Hook onDisplayModuleDraw to DisplayModule.Draw
                ad.MainModule.GetType("Hacknet.DisplayModule").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onDisplayModuleDraw"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onExtensionsMenuScreenDraw to ExtensionsMenuScreen.Draw
                ad.MainModule.GetType("Hacknet.Screens.ExtensionsMenuScreen").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onExtensionsMenuScreenDraw"),
                    71,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
                    localsID: new int[] { 3 }
                );

                // Hook onExtensionsMenuListDraw to ExtensionsMenuScreen.DrawExtensionList
                ad.MainModule.GetType("Hacknet.Screens.ExtensionsMenuScreen").GetMethod("DrawExtensionList").InjectWith(
                    hooks.GetMethod("onExtensionsMenuListDraw"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onOptionsMenuDraw to OptionsMenu.Draw
                ad.MainModule.GetType("Hacknet.OptionsMenu").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onOptionsMenuDraw"),
                    40,
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // Hook onOptionsMenuLoadContent to OptionsMenu.LoadContent
                ad.MainModule.GetType("Hacknet.OptionsMenu").GetMethod("LoadContent").InjectWith(
					hooks.GetMethod("onOptionsMenuLoadContent"),
                    -1,
                    flags: InjectFlags.PassInvokingInstance
                );

                // Hook onOptionsMenuUpdate to OptionsMenu.Update
                ad.MainModule.GetType("Hacknet.OptionsMenu").GetMethod("Update").InjectWith(
					hooks.GetMethod("onOptionsMenuUpdate"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // Hook onOptionsApply to OptionsMenu.apply
                ad.MainModule.GetType("Hacknet.OptionsMenu").GetMethod("apply").InjectWith(
                    hooks.GetMethod("onOptionsApply"),
                    flags: InjectFlags.PassInvokingInstance
                );

                ad.MainModule.GetType("Hacknet.OS").GetMethod("Draw").InjectWith(
                    hooks.GetMethod("onOSDraw"),
                    flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                ad.MainModule.GetType("Hacknet.RunnableConditionalActions").GetMethod("Deserialize").InjectWith(
                    hooks.GetMethod("onDeserializeRunnableConditionalActions"),
                    flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onAddSerializableConditions to SerializableCondition.Deserialize
                /*ad.MainModule.GetType("Hacknet.SerializableCondition").GetMethod("Deserialize").InjectWith(
                    hooks.GetMethod("onAddSerializableConditions"),
                    3,
                    flags: InjectFlags.PassLocals,
                    localsID:new int[]{ 0 }
                );

                // SENSITIVE CODE, CHANGE OFFSET IF NEEDED
                // Hook onAddSerializableConditions to SerializableCondition.Deserialize
                ad.MainModule.GetType("Hacknet.SerializableAction").GetMethod("Deserialize").InjectWith(
                    hooks.GetMethod("onAddSerializableActions"),
                    3,
                    flags: InjectFlags.PassLocals,
                    localsID: new int[] { 0 }
                );*/

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
            foreach (var type in ad.MainModule.Types)
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
            int i = 0;
            foreach (var a in args)
                method.Parameters.Add(new ParameterDefinition(mref.Parameters.Count > i ? mref.Parameters[i++].Name : null,
                                                              ParameterAttributes.None,
                                                              a
                                                             ));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            i = 0;
            foreach (var a in args)
                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, method.Parameters[i++]));
            if (extraInstructions != null)
                foreach (var inst in extraInstructions) method.Body.Instructions.Add(inst);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, mref));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            type.Methods.Add(method);
            return method;
        }

        public static Instruction Inst(this MethodDefinition self, int loc)
        {
            return self.Body.Instructions[loc];
        }

        public static MethodDefinition AdjustInstruction(this MethodDefinition self, int loc, OpCode op = default(OpCode), object operand = null)
        {
            var i = self.Inst(loc);
            if (op != default(OpCode)) i.OpCode = op;
            if (operand != null) i.Operand = operand;
            return self;
        }

        /// <summary>
        /// Injects like <see cref="Mono.Cecil.Inject.InjectionDefinition"/>, with less safety measures
        /// and injects a value from a local compiler class' field
        /// </summary>
        /// <returns>The modified self parameter.</returns>
        /// <param name="self">Self.</param>
        /// <param name="injectionMethod">The method to inject.</param>
        /// <param name="callLoc">The instruction position to inject at.</param>
        /// <param name="localClass">The Local class value whose field is to be injecting from.</param>
        /// <param name="fieldInClass">The field reference in local class.</param>
        /// <param name="f">The injection flags.</param>
        /// <param name="localsID">The other local value ids.</param>
        /// <param name="typeFields">The type fields.</param>
        public static MethodDefinition InjectWithLocalFieldParameter(
            this MethodDefinition self,
            MethodDefinition injectionMethod,
            int callLoc,
            int localClass,
            FieldDefinition fieldInClass,
            bool fieldByRef = false,
            InjectDirection direction = InjectDirection.Before,
            InjectFlags f = InjectFlags.None,
            int[] localsID = null,
	        FieldDefinition[] typeFields = null,
            int token = 0
        )
        {
            var flags = f.ToValues();
            if (flags.PassLocals && localsID == null) throw new ArgumentNullException(nameof(localsID));
            if (flags.PassFields && typeFields == null) throw new ArgumentNullException(nameof(typeFields));

            var body = self.Body;
            if (callLoc < 0) callLoc = body.Instructions.Count + callLoc;
            var il = body.GetILProcessor();
            var isVoid = self.ReturnType.FullName == "System.Void";
            var inst = body.Instructions[callLoc];
            var inst2 = inst;

            if (direction == InjectDirection.Before && callLoc != 0)
            {
                Instruction oldIns = ILUtils.CopyInstruction(inst);
                ILUtils.ReplaceInstruction(inst, il.Create(OpCodes.Nop));
                Instruction ins = body.Instructions[callLoc];
                il.InsertAfter(ins, oldIns);
                inst2 = body.Instructions[callLoc + 1];
            }
            else if (direction == InjectDirection.After)
            {
                il.InsertAfter(inst, il.Create(OpCodes.Nop));
                inst2 = body.Instructions[callLoc + 1];
            }

            VariableDefinition returnDef = null;
            if (flags.ModifyReturn && !isVoid)
            {
                body.InitLocals = true;
                returnDef = new VariableDefinition(self.ReturnType);
                body.Variables.Add(returnDef);
            }
            if (flags.PassTag) il.InsertBefore (inst, il.Create(OpCodes.Ldc_I4, token));
            if (flags.PassInvokingInstance) il.InsertBefore(inst, il.Create(OpCodes.Ldarg_0));
            if (flags.ModifyReturn && !isVoid) il.InsertBefore(inst, il.Create(OpCodes.Ldloca_S, returnDef));
            if (flags.PassLocals)
                foreach (int i in localsID) il.InsertBefore(inst, il.Create(OpCodes.Ldloca_S, (byte) i));
            if (flags.PassFields)
            {
                var memberRefs = typeFields.Select(t => t.Module.ImportReference(t));
                foreach (FieldReference t in memberRefs)
                {
                    il.InsertBefore(inst, il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(inst, il.Create(OpCodes.Ldflda, t));
                }
            }
            if (flags.PassParameters)
            {
                int prefixCount = Convert.ToInt32(flags.PassTag) +
                                    Convert.ToInt32(flags.PassInvokingInstance) +
                                    Convert.ToInt32(flags.ModifyReturn && !isVoid);
                int localsCount = flags.PassLocals ? localsID.Length : 0;
                int memberRefCount = flags.PassFields ? typeFields.Length : 0;
                int paramCount = flags.PassParameters ? self.Parameters.Count : 0;
                int parameters = injectionMethod.Parameters.Count - prefixCount - localsCount - memberRefCount - 1;
                int icr = Convert.ToInt32(!self.IsStatic);
                for (int i = 0; i < parameters; i++)
                    il.InsertBefore(
                        inst,
                        il.Create(flags.PassParametersByRef ? OpCodes.Ldarga_S : OpCodes.Ldarg_S, (byte)(i + icr))
                    );
            }
            il.InsertBefore(inst, il.Create(OpCodes.Ldloc_S, (byte)localClass));
            il.InsertBefore(inst, il.Create(fieldByRef ? OpCodes.Ldflda : OpCodes.Ldfld, fieldInClass));
            il.InsertBefore(inst, il.Create(OpCodes.Call, self.Module.ImportReference(injectionMethod)));
            if (flags.ModifyReturn)
            {
                il.InsertBefore(inst, il.Create(OpCodes.Brfalse_S, inst));
                if (!isVoid) il.InsertBefore(inst, il.Create(OpCodes.Ldloc_S, returnDef));
                il.InsertBefore(inst, il.Create(OpCodes.Ret));
            }
            // If we don't use the return value of InjectMethod, pop it from the ES
            else if (injectionMethod.ReturnType.FullName != "System.Void") il.InsertBefore(inst, il.Create(OpCodes.Pop));
            if (direction == InjectDirection.After) il.Remove(inst2);
            return self;
        }
    }
}
