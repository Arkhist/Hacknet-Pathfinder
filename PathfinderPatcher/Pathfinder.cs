using System;
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
			// Opens Hacknet.exe's Assembly
			AssemblyDefinition ad = AssemblyDefinition.ReadAssembly("Hacknet.exe");
			ad.AddAssemblyAttribute<InternalsVisibleToAttribute>("HacknetPathfinder");
			ad.RemoveInternals();

			var pathfinder = AssemblyDefinition.ReadAssembly("HacknetPathfinder.dll");
			ad.EntryPoint.InjectWith(
				pathfinder.MainModule.GetType("HacknetPathfinder.PathfinderHooks")
			                         .GetMethod("onMain"), 
				flags: InjectFlags.PassParametersVal);

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
    }
}
