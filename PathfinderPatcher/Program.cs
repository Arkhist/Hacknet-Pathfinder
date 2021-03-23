using System;
using System.Linq;
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
                
                // We want to run before everything else, including the Main program function, so we insert ourselves in the static constructor of Program
                var hnEntryType = hn.MainModule.EntryPoint.DeclaringType;

                var ctor = hnEntryType.Methods.First(x => x.IsConstructor);

                // Inject the call
                var processor = ctor.Body.GetILProcessor();

                processor.RemoveAt(2);
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

                // Write modified assembly to disk
                hn.Write("HacknetPathfinder.exe");
            }
        }
    }
}
