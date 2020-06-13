using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Inject;
using Pathfinder.Attribute;
using Pathfinder.Util;

namespace Pathfinder.Internal.Patcher
{
    internal static class Executor
    {
        private struct MethodStore
        {
            public string Name;
            public PatchAttribute Attribute;
            public MethodDefinition Target;
            public MethodStore(string name, PatchAttribute attrib, MethodDefinition target)
            {
                Name = name;
                Attribute = attrib;
                Target = target;
            }
        }

        private static void AddOrCreateTo<T>(this Dictionary<string, List<T>> input, string key, T val)
        {
            if (!input.ContainsKey(key))
                input.Add(key, new List<T>());
            input[key].Add(val);
        }

        internal static void Main(AssemblyDefinition gameAssembly)
        {
            var injectedList = new List<string>();
            var depDict = new Dictionary<string, List<MethodStore>>();

            // Retrieve the hook methods
            var hooks = typeof(PathfinderHooks);
            MethodDefinition method;
            PatchAttribute attrib;
            string sig;
            foreach (var meth in hooks.GetMethods())
            {
                attrib = meth.GetFirstAttribute<PatchAttribute>();
                if (attrib == null) continue;
                sig = attrib.MethodSig;
                if (sig == null)
                {
                    Console.WriteLine($"Null method signature found, skipping {nameof(PatchAttribute)} on method.");
                    continue;
                }
                method = gameAssembly.MainModule.GetType(sig.Remove(sig.LastIndexOf('.')))?.GetMethod(sig.Substring(sig.LastIndexOf('.') + 1));
                if(method == null)
                {
                    Console.WriteLine($"Method signature '{sig}' could not be found, method hook patching failed, skipping {nameof(PatchAttribute)} on '{sig}'.");
                    continue;
                }

                if (attrib.DependentSig != null && !injectedList.Contains(attrib.DependentSig))
                {
                    depDict.AddOrCreateTo(attrib.DependentSig, new MethodStore(meth.Name, attrib, method));
                    continue;
                }

                method.TryInject(
                    gameAssembly.MainModule.ImportReference(hooks.GetMethod(meth.Name)).Resolve(),
                    attrib
                );

                injectedList.Add(meth.Name);

                if (depDict.TryGetValue(meth.Name, out var storeList))
                {
                    foreach (var store in storeList)
                        store.Target.TryInject(
                            gameAssembly.MainModule.ImportReference(hooks.GetMethod(store.Name)).Resolve(),
                            store.Attribute
                        );
                    depDict.Remove(meth.Name);
                }

            }
        }

        private static void TryInject(this MethodDefinition def, MethodDefinition toInject, PatchAttribute attrib)
        {
            try
            {
                def.InjectWith(
                    toInject,
                    attrib.Offset,
                    attrib.Tag,
                    (InjectFlags)attrib.Flags,
                    attrib.After ? InjectDirection.After : InjectDirection.Before,
                    attrib.LocalIds);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error applying patch for '{attrib.MethodSig}'", ex);
            }
        }
    }
}
