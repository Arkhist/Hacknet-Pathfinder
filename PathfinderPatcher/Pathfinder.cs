using System;
using System.IO;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Inject;

namespace PathfinderPatcher
{
    // This will be the modloader.
    public static class PatcherProgram
    {
        internal static void Main(string[] args)
        {
            Console.WriteLine(String.Join("|", args));
            string pathfinderDir = "", exeDir = "";
            int index = 0;
            foreach (var arg in args)
            {
                if (arg.Equals("-pathfinderDir"))
                    pathfinderDir = args[index + 1] + '\\';
                if (arg.Equals("-exeDir"))
                    exeDir = args[index + 1] + '\\';
                index++;
            }

            // Opens Hacknet.exe and HacknetPathfinder.dll's Assembly
            var ad = LoadAssembly(exeDir + "Hacknet.exe");
            var pathfinder = LoadAssembly(pathfinderDir + "Pathfinder.dll");

            ad.AddAssemblyAttribute<InternalsVisibleToAttribute>("Pathfinder");
            ad.RemoveInternals();

            ad.EntryPoint.InjectWith(
                pathfinder.MainModule.GetType("Pathfinder.PathfinderHooks")
                                     .GetMethod("onMain"),
                flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn);

            ad.Write("HacknetPathfinder.exe");
        }

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
            foreach (TypeDefinition type in ad.MainModule.Types)
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

        internal static AssemblyDefinition LoadAssembly(string fileName, ReaderParameters parameters = null)
        {
            parameters = parameters ?? new ReaderParameters(ReadingMode.Deferred);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName is null/empty");
            }
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
    }
}
