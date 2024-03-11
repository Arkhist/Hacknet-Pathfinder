using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace PathfinderPatcher;

internal static class Program
{
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("Trimming", "IL2111")]
    private static void Main(string[] args)
    {
        var hn = AssemblyDefinition.FromFile("Hacknet.exe");
        var m = hn.ManifestModule!;

        var newHn = new AssemblyDefinition("Hacknet", new Version(1, 0, 0, 1));
        newHn.Modules.Add(new ModuleDefinition("Hacknet"));
        var newM = newHn.ManifestModule!;

        var cloner = new MemberCloner(newM)
            .AddListener(new PublicizingListener())
            .AddListener(new InjectTypeClonerListener(newM));

        foreach (var type in m.TopLevelTypes)
            cloner.Include(type);

        cloner.Clone();

        var importer = newM.DefaultImporter;

        var cctor = newM.GetOrCreateModuleConstructor();
        cctor.CilMethodBody = new CilMethodBody(cctor)
        {
            Instructions =
            {
                new CilInstruction(CilOpCodes.Ldstr, "./BepInEx/core/BepInEx.Hacknet.dll"),
                new CilInstruction(CilOpCodes.Call, importer.ImportMethod(typeof(System.IO.Path).GetMethod("GetFullPath", [typeof(string)])!)),
                new CilInstruction(CilOpCodes.Call, importer.ImportMethod(typeof(Assembly).GetMethod("LoadFile", [typeof(string)])!)),
                new CilInstruction(CilOpCodes.Ldstr, "BepInEx.Hacknet.Entrypoint"),
                new CilInstruction(CilOpCodes.Callvirt, importer.ImportMethod(typeof(Assembly).GetMethod("GetType", [typeof(string)])!)),
                new CilInstruction(CilOpCodes.Ldstr, "Bootstrap"),
                new CilInstruction(CilOpCodes.Callvirt, importer.ImportMethod(typeof(Type).GetMethod("GetMethod", [typeof(string)])!)),
                new CilInstruction(CilOpCodes.Ldnull),
                new CilInstruction(CilOpCodes.Ldnull),
                new CilInstruction(CilOpCodes.Callvirt, importer.ImportMethod(typeof(MethodBase).GetMethod("Invoke", [typeof(object), typeof(object[])])!)),
                new CilInstruction(CilOpCodes.Pop),
                new CilInstruction(CilOpCodes.Ret)
            }
        };
        
        newHn.Write("Hacknet.dll");

        Console.WriteLine("Successfully wrote Hacknet.dll to disk");
    }
}

internal sealed class PublicizingListener : IMemberClonerListener
{
    public void OnClonedType(TypeDefinition original, TypeDefinition cloned)
    {
        if (cloned.IsNested)
            cloned.IsNestedPublic = true;
        else
            cloned.IsPublic = true;
    }
    public void OnClonedMethod(MethodDefinition original, MethodDefinition cloned)
    {
        cloned.IsPublic = true;
        if (original.Module.ManagedEntryPoint == original)
        {
            cloned.Module.ManagedEntryPoint = cloned;
        }
    }
    public void OnClonedField(FieldDefinition original, FieldDefinition cloned)
    {
        cloned.IsPublic = true;
    }
    public void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned) { }
    public void OnClonedProperty(PropertyDefinition original, PropertyDefinition cloned) { }
    public void OnClonedEvent(EventDefinition original, EventDefinition cloned) { }
}
