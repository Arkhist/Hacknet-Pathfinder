using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Inject;

namespace PathfinderPatcher
{
    internal static class Extensions
    {
        internal static void MakeFieldAccess(this FieldDefinition f, AccessLevel access = AccessLevel.Internal)
        {
            switch (access)
            {
                case AccessLevel.Private:
                    f.IsPrivate = true; break;
                case AccessLevel.Protected:
                    f.IsFamily = true; break;
                case AccessLevel.Public:
                    f.IsPublic = true; break;
                case AccessLevel.Internal:
                    f.IsAssembly = true; break;
                case AccessLevel.ProtectedInternal:
                    f.IsFamilyOrAssembly = true; break;
                case AccessLevel.PrivateProtected:
                    f.IsFamilyAndAssembly = true; break;
            }
        }

        internal static void MakeAllFieldAccess(this TypeDefinition t, AccessLevel access = AccessLevel.Internal, Predicate<FieldDefinition> skip = null)
        {
            if (skip == null) skip = x => !x.IsPrivate;
            foreach (var f in t.Fields)
            {
                if (skip(f)) continue;
                f.MakeFieldAccess(access);
            }
        }

        internal static void MakeAllFieldAccess(this ModuleDefinition m, string typename, AccessLevel access = AccessLevel.Internal, Predicate<FieldDefinition> skip = null)
            => m.GetType(typename).MakeAllFieldAccess(access, skip);

        internal static void MakeFieldAccess(this ModuleDefinition module, string typename, string fieldname, AccessLevel access = AccessLevel.Internal)
            => module.GetType(typename).GetField(fieldname).MakeFieldAccess(access);

        internal static void MakeFieldAccess(this ModuleDefinition module, string typename, AccessLevel access = AccessLevel.Internal, params string[] names)
            => module.GetType(typename).MakeAllFieldAccess(access, f => names.Length > 0 && !names.Contains(f.Name));

        internal static void MakeMethodAccess(this MethodDefinition m, AccessLevel access = AccessLevel.Internal)
        {
            switch (access)
            {
                case AccessLevel.Private:
                    m.IsPrivate = true; break;
                case AccessLevel.Protected:
                    m.IsFamily = true; break;
                case AccessLevel.Public:
                    m.IsPublic = true; break;
                case AccessLevel.Internal:
                    m.IsAssembly = true; break;
                case AccessLevel.ProtectedInternal:
                    m.IsFamilyOrAssembly = true; break;
                case AccessLevel.PrivateProtected:
                    m.IsFamilyAndAssembly = true; break;
            }
        }

        internal static void MakeMethodAccess(this ModuleDefinition module, string typename, string methodname, AccessLevel access = AccessLevel.Internal)
            => module.GetType(typename).GetMethod(methodname).MakeMethodAccess(access);

        internal static void MakeAllMethodAccess(this TypeDefinition t, AccessLevel access = AccessLevel.Internal, Predicate<MethodDefinition> skip = null)
        {
            if (skip == null) skip = x => !x.IsPrivate;
            foreach (var m in t.Methods)
            {
                if (skip(m)) continue;
                m.MakeMethodAccess(access);
            }
        }

        internal static void MakeAllMethodAccess(this ModuleDefinition m, string typename, AccessLevel access = AccessLevel.Internal, Predicate<MethodDefinition> skip = null)
            => m.GetType(typename).MakeAllMethodAccess(access, skip);

        internal static void MakeNestedAccess(this TypeDefinition t, AccessLevel access = AccessLevel.Internal)
        {
            switch (access)
            {
                case AccessLevel.Private:
                    t.IsNestedPrivate = true; break;
                case AccessLevel.Protected:
                    t.IsNestedFamily = true; break;
                case AccessLevel.Public:
                    t.IsNestedPublic = true; break;
                case AccessLevel.Internal:
                    t.IsNestedAssembly = true; break;
                case AccessLevel.ProtectedInternal:
                    t.IsNestedFamilyOrAssembly = true; break;
                case AccessLevel.PrivateProtected:
                    t.IsNestedFamilyAndAssembly = true; break;
            }
        }

        internal static void ModifyNestedVisible(this ModuleDefinition module, string typename, string methodname, AccessLevel access = AccessLevel.Internal)
            => module.GetType(typename).NestedTypes.First(t => t.FullName == methodname).MakeNestedAccess(access);

        internal static void MakeAllNestedAccess(this TypeDefinition t, AccessLevel access = AccessLevel.Internal, Predicate<TypeDefinition> skip = null)
        {
            if (skip == null) skip = x => !x.IsNestedPrivate;
            foreach (var m in t.NestedTypes)
            {
                if (skip(m)) continue;
                m.MakeNestedAccess(access);
            }
        }

        internal static void ModifyAllNestedsVisible(this ModuleDefinition m, string typename, AccessLevel access = AccessLevel.Internal, Predicate<TypeDefinition> skip = null)
            => m.GetType(typename).MakeAllNestedAccess(access, skip);

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

        internal static MethodDefinition AddRefConstructor(this TypeDefinition type,
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

        internal static Instruction Inst(this MethodDefinition self, int loc) => self.Body.Instructions[loc];

        internal static MethodDefinition AdjustInstruction(this MethodDefinition self, int loc, OpCode op = default(OpCode), object operand = null)
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
        internal static MethodDefinition InjectWithLocalFieldParameter(
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
            if (flags.PassTag) il.InsertBefore(inst, il.Create(OpCodes.Ldc_I4, token));
            if (flags.PassInvokingInstance) il.InsertBefore(inst, il.Create(OpCodes.Ldarg_0));
            if (flags.ModifyReturn && !isVoid) il.InsertBefore(inst, il.Create(OpCodes.Ldloca_S, returnDef));
            if (flags.PassLocals)
                foreach (int i in localsID) il.InsertBefore(inst, il.Create(OpCodes.Ldloca_S, (byte)i));
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


        public static string LineInfoForExcept(this IXmlLineInfo info)
        {
            return !info.HasLineInfo() ? "" : $" (Line {info.LineNumber}:{info.LinePosition})";
        }

        /* deconstruct adapters for this wierd combination of C# >= 7.0 and .NET = 4.0  */
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value) {
            key = pair.Key;
            value = pair.Value;
        }

        public static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple, out T1 Item1, out T2 Item2) {
            Item1 = tuple.Item1;
            Item2 = tuple.Item2;
        }
    }
}
