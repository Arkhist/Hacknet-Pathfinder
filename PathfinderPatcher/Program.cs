using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PathfinderPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            using (AssemblyDefinition hn = AssemblyDefinition.ReadAssembly("Hacknet.exe"))
            using (AssemblyDefinition bootstrap = AssemblyDefinition.ReadAssembly(typeof(Program).Assembly.Location))
            {
                // We want to run before everything else, including the Main program function, so we insert ourselves in the static constructor of Program
                var hnEntryType = hn.MainModule.EntryPoint.DeclaringType;

                var ctor = hnEntryType.Methods.First(x => x.IsConstructor);

                // Get a reference to our bootstrap method that we can call
                var bootstrapMethod = bootstrap.MainModule.Types.First(x => x.Name == "Entrypoint").Methods.First(x => x.Name == "Bootstrap");

                var injectedBootstrap = hn.MainModule.ImportReference(bootstrapMethod);

                // Inject the call
                var processor = ctor.Body.GetILProcessor();

                processor.RemoveAt(2);
                processor.Emit(OpCodes.Call, injectedBootstrap);
                processor.Emit(OpCodes.Ret);

                // Write modified assembly to disk
                hn.Write("HacknetPathfinder.exe");
            }
        }
    }
}
