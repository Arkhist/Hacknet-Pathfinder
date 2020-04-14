using System;
using Mono.Cecil;
using Mono.Cecil.Inject;
using Pathfinder.Attribute;
using Pathfinder.Util;

namespace Pathfinder.Internal.Patcher
{
    internal static class Executor
    {
        internal static void Main(AssemblyDefinition gameAssembly)
        {
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
                }
                method = gameAssembly.MainModule.GetType(sig.Remove(sig.LastIndexOf('.')))?.GetMethod(sig.Substring(sig.LastIndexOf('.') + 1));
                if(method == null)
                {
                    Console.WriteLine($"Method signature '{sig}' could not be found, method hook patching failed, skipping {nameof(PatchAttribute)} on '{sig}'.");
                    continue;
                }

                try
                {
                    method.InjectWith(
                        gameAssembly.MainModule.ImportReference(hooks.GetMethod(meth.Name)).Resolve(),
                        attrib.Offset,
                        attrib.Tag,
                        (InjectFlags) attrib.Flags,
                        attrib.After ? InjectDirection.After : InjectDirection.Before,
                        attrib.LocalIds);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error applying patch for {sig}", ex);
                }
            }
        }
    }
}
