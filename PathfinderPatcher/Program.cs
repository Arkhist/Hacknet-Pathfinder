using System;
using System.Linq;
using System.Security;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PathfinderPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            using (AssemblyDefinition hn = AssemblyDefinition.ReadAssembly("Hacknet.exe"))
            {
                // Make everything public
                foreach (var type in hn.MainModule.Types)
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

                // We want to run before everything else, including the Main program function, so we make a static module constructor
                var hnEntryType = hn.MainModule.Types.First(x => x.Name == "<Module>");

                var ctor = new MethodDefinition(".cctor",
                    Mono.Cecil.MethodAttributes.Public
                    | Mono.Cecil.MethodAttributes.Static
                    | Mono.Cecil.MethodAttributes.HideBySig
                    | Mono.Cecil.MethodAttributes.SpecialName
                    | Mono.Cecil.MethodAttributes.RTSpecialName,
                    hn.MainModule.TypeSystem.Void
                );

                // Inject the call
                var processor = ctor.Body.GetILProcessor();

                // Get absolute path of BepInEx.Hacknet.dll
                processor.Emit(OpCodes.Ldstr, "./BepInEx/core/BepInEx.Hacknet.dll");
                processor.Emit(OpCodes.Call, hn.MainModule.ImportReference(typeof(System.IO.Path).GetMethod("GetFullPath", new Type[] { typeof(string) })));
                // Load BepInEx.Hacknet.dll
                processor.Emit(OpCodes.Call, hn.MainModule.ImportReference(typeof(Assembly).GetMethod("LoadFile", new Type[] { typeof(string) })));
                // Get Entrypoint type
                processor.Emit(OpCodes.Ldstr, "BepInEx.Hacknet.Entrypoint");
                processor.Emit(OpCodes.Call, hn.MainModule.ImportReference(typeof(Assembly).GetMethod("GetType", new Type[] { typeof(string) })));
                // Get bootstrap method
                processor.Emit(OpCodes.Ldstr, "Bootstrap");
                processor.Emit(OpCodes.Call, hn.MainModule.ImportReference(typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string) })));
                // Call bootstrap method
                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Callvirt, hn.MainModule.ImportReference(typeof(MethodBase).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) })));
                processor.Emit(OpCodes.Pop);
                // Return
                processor.Emit(OpCodes.Ret);

                hnEntryType.Methods.Add(ctor);

                // Hacknet needs to be able to access *everything*, this ensures that
                var unverifiableType = hn.MainModule.ImportReference(typeof(UnverifiableCodeAttribute)).Resolve();
                var unverifiableCtor = hn.MainModule.ImportReference(unverifiableType.Methods.First(x => x.IsConstructor && x.Parameters.Count == 0));
                hn.MainModule.CustomAttributes.Add(new CustomAttribute(unverifiableCtor));

                // Write modified assembly to disk
                hn.Write("HacknetPathfinder.exe");
                
                Console.WriteLine("Successfully wrote HacknetPathfinder.exe to disk!");
            }
        }
    }
}
