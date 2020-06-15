using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pathfinder.Attribute;
using Pathfinder.Util;

namespace Pathfinder.ModManager
{
    public static class ModAttributeHandler
    {
        internal static Dictionary<Assembly, Type> DefaultMods = new Dictionary<Assembly, Type>();
        internal static Dictionary<Type, List<MethodInfo>> ModToCommandMethods = new Dictionary<Type, List<MethodInfo>>();
        internal static Dictionary<Type, List<MethodInfo>> ModToEventMethods = new Dictionary<Type, List<MethodInfo>>();

        internal static void Reset()
        {
            ModToCommandMethods.Clear();
            ModToEventMethods.Clear();
        }

        public static bool ShouldIgnoreAssembly(Assembly asm)
            => !asm.GetExportedTypes().Any(t => t.IsValidForCommandRegister() || t.IsValidForEventRegister());

        internal static void TryAddDefaultMod(Type modType, bool needsDefaultAttrib)
        {
            if (ShouldIgnoreAssembly(modType.Assembly) || needsDefaultAttrib && !modType.HasAttribute<DefaultModAttribute>()) return;
            if (DefaultMods.TryGetValue(modType.Assembly, out var defaultMod))
            {
                Logger.Error($"More then one mod type found in '{modType.Assembly.GetName()}' " +
                $"{(!defaultMod.HasAttribute<DefaultModAttribute>() ? "without any DefaultModAttribute applied to IMod classes" : "with DefaultModAttribute applied to multiple IMod classes")}");
                return;
            }
            DefaultMods[modType.Assembly] = modType;
        }

        public static void HandleType(Type modType, bool needsDefaultAttrib)
        {
            TryAddDefaultMod(modType, needsDefaultAttrib);
            foreach (var t in modType.Assembly.GetExportedTypes()) 
            {
                BuildAttributesFor<CommandModuleAttribute, CommandAttribute>(modType, t, ModToCommandMethods, needsDefaultAttrib);
                BuildAttributesFor<EventModuleAttribute, EventModuleAttribute>(modType, t, ModToEventMethods, needsDefaultAttrib);
            }
        }

        public static bool IsValidRegister<ModuleAT, InnerAT>(this Type moduleType)
            where ModuleAT : System.Attribute
            where InnerAT : System.Attribute
            => moduleType.HasAttribute<ModuleAT>() || moduleType.GetMethods().Any(m => m.HasAttribute<InnerAT>());

        public static void AddMethodsFor(this Dictionary<Type, List<MethodInfo>> d, Type t, IEnumerable<MethodInfo> info)
        {
            if (d.ContainsKey(t))
                d[t].AddRange(info);
            else d.Add(t, info.ToList());
        }

        public static void AddMethodFor(this Dictionary<Type, List<MethodInfo>> d, Type t, MethodInfo info)
        {
            if (d.ContainsKey(t))
                d[t].Add(info);
            else d.Add(t, new List<MethodInfo> { info });
        }

        internal static List<MethodInfo> GetMethods<InnerAT>(Type type, bool modExplicits = false)
            where InnerAT : AbstractPathfinderAttribute
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Where(m => {
                                if (m.HasAttribute<IgnoreRegister>()) return false;
                                var at = m.GetFirstAttribute<InnerAT>();
                                return at != null && (at.Mod == null && !modExplicits || at.Mod != null && modExplicits);
                            }).ToList();
        }

        internal static void BuildAttributesFor<ModuleAT, InnerAT>(Type modType, Type exportedType, Dictionary<Type, List<MethodInfo>> dict, bool needsDefault)
            where ModuleAT : AbstractPathfinderAttribute
            where InnerAT : AbstractPathfinderAttribute
        {
            if (exportedType.HasAttribute<IgnoreRegister>()) return;
            var attrib = exportedType.GetFirstAttribute<ModuleAT>();
            if (dict.Values.Any(list => list.Any(i => exportedType == i.DeclaringType))) return;
            foreach (var meth in GetMethods<CommandAttribute>(exportedType, true))
                dict.AddMethodFor(meth.GetFirstAttribute<InnerAT>().Mod, meth);
            if (attrib?.Mod != null)
                dict.AddMethodsFor(attrib.Mod, GetMethods<InnerAT>(exportedType));
            else if (needsDefault && modType.HasAttribute<DefaultModAttribute>() || !needsDefault)
                dict.AddMethodsFor(modType, GetMethods<InnerAT>(exportedType));
        }

        public static bool IsValidForCommandRegister(this Type type)
            => type.IsValidRegister<CommandModuleAttribute, CommandAttribute>();

        public static bool IsValidForEventRegister(this Type type)
            => type.IsValidRegister<EventModuleAttribute, EventAttribute>();
    }
}
