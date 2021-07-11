using System.Collections.Generic;
using Mono.Cecil;

namespace PathfinderPatcher
{
    public static class PathfinderPatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Hacknet.exe" };

        public static void Patch(AssemblyDefinition assembly)
        {
            // Make everything public
            foreach (var type in assembly.MainModule.Types)
            {
                type.IsPublic = true;
                foreach (var method in type.Methods)
                    method.IsPublic = true;
                foreach (var field in type.Fields)
                    field.IsPublic = true;
                foreach (var property in type.Properties)
                {
                    if (property.GetMethod != null)
                        property.GetMethod.IsPublic = true;
                    if (property.SetMethod != null)
                        property.SetMethod.IsPublic = true;
                }
            }
        }
    }
}