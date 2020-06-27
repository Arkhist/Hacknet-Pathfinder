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
    public enum AccessMods
    {
        Private,
        Protected,
        Public,
        Internal,
        ProtectedInternal,
        PrivateProtected
    }

    // This will be the modloader.
    public static class PatcherProgram
    {
        const string PATCH_ATTRIBUTE_CLASSPATH = "Pathfinder.Attribute.PatchAttribute";

        internal static int Main(string[] args)
        {
            var separator = Path.DirectorySeparatorChar;

            Console.WriteLine($"Executing Patcher { (args.Length > 0 ? $"with arguments:\n{{\n\t{string.Join(",\n\t", args)}\n}}" : "without arguments.") }");

            string pathfinderDir = null, exeDir = "";
            var index = 0;
            var spitOutHacknetOnly = false;
            var skipLaunchers = false;
            foreach (var arg in args)
            {
                if (arg.Equals("-pathfinderDir")) // the Pathfinder.dll's directory
                    pathfinderDir = args[index + 1] + Path.DirectorySeparatorChar;
                if (arg.Equals("-exeDir")) // the Hacknet.exe's directory
                    exeDir = args[index + 1] + separator;
                spitOutHacknetOnly |= arg.Equals("-spit"); // spit modifications without injected code
                skipLaunchers |= arg.Equals("-nolaunch");
                index++;
            }

            AssemblyDefinition gameAssembly = null;
            try
            {
                if(!skipLaunchers) {
                   if (File.Exists(exeDir + "Hacknet"))
                   {
                        File.Copy(exeDir + "Hacknet", exeDir + "HacknetPathfinder", true);

                        var txt = File.ReadAllText(exeDir + "Hacknet");
                        txt = txt.Replace("Hacknet", "HacknetPathfinder");

                       File.WriteAllText(exeDir + "HacknetPathfinder", txt);
                   }

                   foreach (var n in new string[]{
                       exeDir + "Hacknet.bin.x86",
                       exeDir + "Hacknet.bin.x86_64",
                       exeDir + "Hacknet.bin.osx"
                    })
                    if (File.Exists(n))
                        File.Copy(n, exeDir + "HacknetPathfinder.bin" + Path.GetExtension(n), true);
                }
                // Loads Hacknet.exe's assembly
                gameAssembly = LoadAssembly(exeDir + "Hacknet.exe");
            }
            catch (Exception ex)
            {
                HandleExeception("Failure at Assembly Loading:", ex);
                return 2;
            }

            if (gameAssembly == null) throw new InvalidDataException("Hacknet Assembly could not be found");

            try
            {
                // Loads FNA.dll for some extra injections
                var fna = LoadAssembly(exeDir + "FNA.dll");

                // Adds Pathfinder internal attribute hack
                gameAssembly.AddAssemblyAttribute<InternalsVisibleToAttribute>("Pathfinder");
                // Removes internal visibility from types
                gameAssembly.RemoveInternals();
                // Ensure the os field is internal
                gameAssembly.MainModule.MakeFieldAccess("Hacknet.Computer", "os");
                // Ensure MissionListingServer's fields are internal
                gameAssembly.MainModule.MakeAllFieldAccess("Hacknet.MissionListingServer");
                // Ensure MissionHubServer's fields are internal
                gameAssembly.MainModule.MakeAllFieldAccess("Hacknet.MissionHubServer");

                // Ensure ActiveMission's methods are virtual
                var activeMission = gameAssembly.MainModule.GetType("Hacknet.ActiveMission");
                foreach (var m in activeMission.Methods)
                {
                    if (m.IsStatic || m.IsConstructor) continue;
                    m.IsVirtual = true;
                    m.IsNewSlot = true;
                }

                // Ensure Hacknet.Computer.sendNetworkMessage is public
                gameAssembly.MainModule.MakeMethodAccess("Hacknet.Computer", "sendNetworkMessage", AccessMods.Public);
                // Ensure important DisplayModule fields are public
                gameAssembly.MainModule.MakeFieldAccess("Hacknet.DisplayModule", AccessMods.Public, "x", "y", "openLockSprite", "lockSprite");
                // Ensure Button's methods are public
                gameAssembly.MainModule.MakeAllMethodAccess("Hacknet.Gui.Button", AccessMods.Public, (MethodDefinition m) => false);

                // Ensure ExtensionsMenuScreen's methods, fields, and nested type are public
                var type = gameAssembly.MainModule.GetType("Hacknet.Screens.ExtensionsMenuScreen");
                type.MakeAllFieldAccess(AccessMods.Public);
                type.MakeAllMethodAccess(AccessMods.Public);
                type.MakeAllNestedAccess(AccessMods.Public);

                // Ensure Helpfile's fields are internal
                gameAssembly.MainModule.MakeAllFieldAccess("Hacknet.Helpfile");
                // Retrieve FNA's Vector2 as a type reference
                var v2 = gameAssembly.MainModule.ImportReference(fna.MainModule.GetType("Microsoft.Xna.Framework.Vector2"));

                // Add simplified constructor implictedly referencing Hacknet.OS.currentInstance
                type = gameAssembly.MainModule.GetType("Hacknet.Computer");
                type.AddRefConstructor(type.GetMethod(".ctor"),
                new TypeReference[] {
                    gameAssembly.MainModule.TypeSystem.String,
                    gameAssembly.MainModule.TypeSystem.String,
                    v2,
                    gameAssembly.MainModule.TypeSystem.Int32,
                    gameAssembly.MainModule.TypeSystem.Byte
                },
                new Instruction[] {
                    Instruction.Create(OpCodes.Ldsfld, gameAssembly.MainModule.GetType("Hacknet.OS").GetField("currentInstance"))
                });

                // Add simplified constructor assigning the compType value to 0
                type.AddRefConstructor(type.GetMethod(".ctor", gameAssembly.MainModule.TypeSystem.String,
                                                      gameAssembly.MainModule.TypeSystem.String,
                                                      v2,
                                                      gameAssembly.MainModule.TypeSystem.Int32,
                                                      gameAssembly.MainModule.TypeSystem.Byte),
                new TypeReference[] {
                    gameAssembly.MainModule.TypeSystem.String,
                    gameAssembly.MainModule.TypeSystem.String,
                    v2,
                    gameAssembly.MainModule.TypeSystem.Int32
                },
                new Instruction[] {
                    Instruction.Create(OpCodes.Ldc_I4_0)
                });

                // Ensure OS's fields and methods are internal, also ensure introTextModule is public
                type = gameAssembly.MainModule.GetType("Hacknet.OS");
                type.MakeAllMethodAccess();
                type.MakeAllFieldAccess();
                type.GetField("introTextModule").IsPublic = true;

                // Ensure ComputerLoader's fields are internal and nest types nested public
                type = gameAssembly.MainModule.GetType("Hacknet.ComputerLoader");
                type.MakeAllFieldAccess();
                type.MakeAllNestedAccess(AccessMods.Public);
                type.GetMethod("findComp").IsPublic = true;

                // Ensure IntroTextModule's fields are public
                gameAssembly.MainModule.MakeAllFieldAccess("Hacknet.IntroTextModule", AccessMods.Public);
                // Ensure OptionsMenu's fields are public
                gameAssembly.MainModule.MakeAllFieldAccess("Hacknet.OptionsMenu", AccessMods.Public);
                // Ensure DatabaseDaemon's fields are public
                gameAssembly.MainModule.MakeAllFieldAccess("Hacknet.DatabaseDaemon", AccessMods.Public);

                // Create FileProperties Struct for FileType
                var nestedType = new TypeBuilder
                {
                    Namespace = "Hacknet",
                    Name = "FileProperties",
                    BaseType = typeof(ValueType),
                    Fields = {
                        new FieldBuilder { Name = "AccessedTime", Type = typeof(ulong) },
                        new FieldBuilder { Name = "ModifiedTime", Type = typeof(ulong) },
                        new FieldBuilder { Name = "ChangedTime", Type = typeof(ulong) },
                        new FieldBuilder { Name = "PrivilegeMask", Type = typeof(short) },
                    }
                }.Build(gameAssembly.MainModule);
                gameAssembly.MainModule.Types.Add(nestedType);

                type = gameAssembly.MainModule.GetType("Hacknet.FileType");


                // Create FileProperties Properties getter/setter Property in FileType
                type.AddUndefinedProperty("Properties", nestedType);

                // Add Properties Property to FileEntry
                type = gameAssembly.MainModule.GetType("Hacknet.FileEntry");
                type.ModifyConstructor(GetCommonConstructorModifier(nestedType,
                    type.AddFullProperty("Properties", nestedType),
                    type.GetField("secondCreatedAt")));

                // Add Properties Property to Folder
                type = gameAssembly.MainModule.GetType("Hacknet.Folder");
                type.ModifyConstructor(m => { m.Body.Variables.Add(new VariableDefinition(gameAssembly.MainModule.TypeSystem.UInt64)); }
                    + GetCommonConstructorModifier(nestedType,
                    type.AddFullProperty("Properties", nestedType),
                    gameAssembly.MainModule.GetType("Hacknet.OS").GetField("currentElapsedTime")));

                gameAssembly.MainModule.MakeFieldAccess("Hacknet.ProgressionFlags", "Flags", AccessMods.Public);
                gameAssembly.MainModule.MakeMethodAccess("Hacknet.NetworkMap", "loadAssignGameNodes", AccessMods.Public);
            }
            catch (Exception ex)
            {
                HandleExeception("Failure during Hacknet Pathfinder Assembly Tweaks:", ex);
                gameAssembly?.Write("HacknetPathfinder.exe");
                return 3;
            }
            if(!spitOutHacknetOnly) try
                {
                    using (var stream = new MemoryStream())
                    {
                        gameAssembly.Write(stream);
                        System.Reflection.Assembly.Load(stream.GetBuffer());
                        var assm = System.Reflection.Assembly.LoadFrom(
                            new FileInfo(string.IsNullOrEmpty(pathfinderDir) ? "Pathfinder.dll" : pathfinderDir + "Pathfinder.dll").FullName);
                        var t = assm.GetType("Pathfinder.Internal.Patcher.Executor");
                        t.GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                            .Invoke(null, new object[] { gameAssembly });
                    }
                }
                catch(Exception ex)
                {
                    HandleExeception("Failure during Pathfinder.dll's Patch Execution:", ex);
                    gameAssembly?.Write("HacknetPathfinder.exe");
                    return 1;
                }
            gameAssembly?.Write("HacknetPathfinder.exe");
            return 0;
        }

        private static void HandleExeception(string message, Exception e)
        {
            Console.WriteLine(message);
            Console.WriteLine(e);
            Console.WriteLine("Press any enter to terminate...");
            Console.ReadLine();
        }

        private static void AddUndefinedProperty(this TypeDefinition t, string propName, TypeDefinition propType)
        {
            var property = new PropertyDefinition(propName, PropertyAttributes.None, propType);
            t.Properties.Add(property);

            var method = new MethodDefinition($"get_{propName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Abstract | MethodAttributes.Virtual, propType);
            t.Methods.Add(method);
            property.GetMethod = method;

            method = new MethodDefinition($"set_{propName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Abstract | MethodAttributes.Virtual, t.Module.TypeSystem.Void);
            method.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propType));
            t.Methods.Add(method);
            property.SetMethod = method;
        }

        private static FieldDefinition AddFullProperty(this TypeDefinition t, string propName, TypeDefinition propType)
        {
            var field = new FieldDefinition($"<{propName}>k__BackingField", FieldAttributes.Private, propType);
            field.CustomAttributes.Add(new CustomAttribute(t.Module.ImportReference(typeof(CompilerGeneratedAttribute).GetConstructors()[0])));
            t.Fields.Add(field);

            var method = new MethodDefinition($"get_{propName}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                propType);

            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            t.Methods.Add(method);
            t.Properties.Add(new PropertyDefinition(propName, PropertyAttributes.None, propType)
            {
                GetMethod = method
            });

            method = new MethodDefinition($"set_{propName}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                t.Module.TypeSystem.Void);
            method.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propType));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            t.Methods.Add(method);
            t.Properties.Last().SetMethod = method;

            return field;
        }

        private static Action<MethodDefinition> GetCommonConstructorModifier(
            TypeDefinition nestedType,
            FieldDefinition propField,
            FieldDefinition secondCreatedField)
        {
            return m =>
            {
                var il = m.Body.GetILProcessor();
                il.Remove(m.Body.Instructions.Last());
                //m.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                //m.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, secondCreated));
                //m.Body.Instructions.Add(Instruction.Create(OpCodes.Conv_Ovf_U4));
                foreach (var f in nestedType.Fields)
                {
                    if (f.FieldType == nestedType.Module.TypeSystem.UInt64)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, propField);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, secondCreatedField);
                        il.Emit(OpCodes.Stfld, f);
                    }
                }
                m.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                m.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            };
        }

        private static void ModifyConstructor(this TypeDefinition t, Action<MethodDefinition> modifier)
            => t.GetMethods(".ctor").ForEach(modifier);

        private static AssemblyDefinition LoadAssembly(string fileName, ReaderParameters parameters = null)
        {
            parameters = parameters ?? new ReaderParameters(ReadingMode.Deferred);
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException($"{nameof(fileName)} is null/empty");
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

        public static int GetLineNumber(this Exception ex)
        {
            var lineNumber = 0;
            const string lineSearch = ":line ";
            var index = ex.StackTrace.LastIndexOf(lineSearch);
            if (index != -1)
            {
                var lineNumberText = ex.StackTrace.Substring(index + lineSearch.Length);
                if (int.TryParse(lineNumberText, out lineNumber))
                {
                }
            }
            return lineNumber;
        }
    }
}
