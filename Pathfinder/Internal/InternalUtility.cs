using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Hacknet;
using Pathfinder.ModManager;
using Pathfinder.Util;

namespace Pathfinder.Internal
{
    public static class InternalUtility
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetPreviousStackFrameIdentity(out MethodBase b)
            => GetPreviousStackFrameIdentity(2, out b);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetPreviousStackFrameIdentity(int frameSkip = 2)
            => GetPreviousStackFrameIdentity(frameSkip + 1, out MethodBase b);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetPreviousStackFrameIdentity(int frameSkip, out MethodBase method)
        {
            var result = "";
            method = new StackFrame(frameSkip + 1).GetMethod();
            var asm = method.Module.Assembly;
            if (asm == MethodBase.GetCurrentMethod().Module.Assembly)
                result = "Pathfinder";
            else if (asm == typeof(Program).Assembly)
                result = "Hacknet";
            else
                result = asm.GetFirstMod()?.GetCleanId();
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string Validate(string id, string noun = null, string instName = null, bool periodThrow = false, bool excludeExt = false, bool log = true, int frames = 0, bool ignoreId = false)
        {
            var frameId = GetPreviousStackFrameIdentity(frames + 1, out var caller);
            if(!ignoreId && id == null)
                throw new ArgumentNullException(nameof(id), $"{caller.Name} can not be called while {nameof(id)} is null.");
            if (frameId != "Pathfinder" && Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException($"{caller.Name} can not be called outside of mod or extension loading.\nMod Fault: {frameId}");
            if (!ignoreId)
                id = Pathfinder.CurrentMod != null || excludeExt ? Utility.GetId(id, throwFindingPeriod: periodThrow) : Extension.Handler.ActiveInfo.Id + "." + id;
            if(log)
                Logger.Verbose("{0}'{1}' is attempting to add {2} '{3}' with id '{4}'",
                               Pathfinder.CurrentMod != null ? "Mod " : (Extension.Handler.ActiveInfo != null ? "Extension " : ""),
                               Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo?.Id ?? "Pathfinder",
                               noun,
                               instName,
                               id);
            return id;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ValidateNoId(string typeName = null, string instName = null, string extra = null, bool log = true, int frames = 0)
        {
            Validate(null, frames: frames + 1, log: false, ignoreId: true);
            if(log)
                Logger.Verbose("{0}'{1}' is attempting to add {2} '{3}' {4}",
                               Pathfinder.CurrentMod != null ? "Mod " : (Extension.Handler.ActiveInfo != null ? "Extension " : ""),
                               Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo?.Id ?? "Pathfinder",
                               typeName,
                               instName,
                               extra);
        }

        /// <summary>
        /// Hardcoded Validatation behavior dependent on parameters
        /// </summary>
        /// <returns><paramref name="input"/> with validation changes if <paramref name="overrideIdBeh"/> is false</returns>
        /// <param name="input">Input string, commonly identifier, for validation.</param>
        /// <param name="additNoun">Additional noun that is being added.</param>
        /// <param name="instanceName">Instance Name of the additional noun.</param>
        /// <param name="periodThrow">If set to <c>true</c> will throw if a period is found in id and <paramref name="overrideIdBeh"/> is false.</param>
        /// <param name="excludeExt">If set to <c>true</c> will exclude extension references in id.</param>
        /// <param name="overrideIdBeh">If set to <c>true</c> overrides all identifier manipulation behavior.</param>
        /// <param name="ignoreAddLog">If set to <c>true</c> overrides logger behavior.</param>
        /// <param name="frames">Stack Frame backward steps to check for validation and user method.</param>
        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static string Validate(string input = null, string additNoun = null, string instanceName = null, bool periodThrow = false, bool excludeExt = false, bool overrideIdBeh = false, bool ignoreAddLog = false, int frames = 1)
        //{
        //    var frameId = GetPreviousStackFrameIdentity(frames, out var @base);
        //    if (!ignoreAddLog && !overrideIdBeh && input == null)
        //        throw new ArgumentNullException(nameof(input), $"{@base.Name} can not be called while {nameof(input)} is null.");
        //    if (frameId != "Pathfinder" && Pathfinder.CurrentMod == null && (excludeExt || !Extension.Handler.CanRegister))
        //        throw new InvalidOperationException($"{@base.Name} can not be called outside of mod or extension loading.\nMod Fault: {frameId}");
        //    if (!overrideIdBeh && input != null)
        //        input = Pathfinder.CurrentMod != null || excludeExt ? Utility.GetId(input, throwFindingPeriod: periodThrow) : Extension.Handler.ActiveInfo.Id + "." + input;
        //    if (!ignoreAddLog)
        //        Logger.Verbose($"{{0}} '{{1}}' is attempting to add {{2}} '{{3}}' {(overrideIdBeh ? "{4}" : "with id '{4}'")}",
        //                       Pathfinder.CurrentMod != null ? "Mod" : "Extension",
        //                       Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id,
        //                       additNoun,
        //                       instanceName,
        //                       input);
        //    return input;
        //}

        public delegate object MethodInvoker(object sender, params object[] parameters);

        public static MethodInvoker GetMethodInvoker(this MethodInfo methodInfo)
        {
            var dynamicMethod =
                new DynamicMethod(string.Empty, typeof(object),
                    new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
            var il = dynamicMethod.GetILGenerator();
            var ps = methodInfo.GetParameters();
            var paramTypes = new Type[ps.Length];
            var locals = new LocalBuilder[paramTypes.Length];
            var i = 0;
            for (i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef) paramTypes[i] = ps[i].ParameterType.GetElementType();
                else paramTypes[i] = ps[i].ParameterType;
                locals[i] = il.DeclareLocal(paramTypes[i], true);
                il.Emit(OpCodes.Ldarg_1);
            }
            i = 0;
            foreach (var p in paramTypes)
            {
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                if (p.IsValueType) il.Emit(OpCodes.Unbox_Any, p);
                else il.Emit(OpCodes.Castclass, p);
                il.Emit(OpCodes.Stloc, locals[i]);
                i++;
            }
            if(!methodInfo.IsStatic) il.Emit(OpCodes.Ldarg_0);

            for (i = 0; i < paramTypes.Length; i++)
                if (paramTypes[i].IsByRef) il.Emit(OpCodes.Ldloca_S, locals[i]);
                else il.Emit(OpCodes.Ldloc, locals[i]);
            if (methodInfo.IsStatic) il.EmitCall(OpCodes.Call, methodInfo, null);
            else il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void)) il.Emit(OpCodes.Ldnull);
            else if (methodInfo.ReturnType.IsValueType) il.Emit(OpCodes.Box, methodInfo.ReturnType);

            for (i = 0; i < paramTypes.Length; i++)
                if (paramTypes[i].IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType) il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }

            il.Emit(OpCodes.Ret);
            var invoder = (MethodInvoker)dynamicMethod.CreateDelegate(typeof(MethodInvoker));
            return invoder;
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0: il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1: il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2: il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3: il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4: il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5: il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6: il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7: il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8: il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128) il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            else il.Emit(OpCodes.Ldc_I4, value);
        }
    }
}
