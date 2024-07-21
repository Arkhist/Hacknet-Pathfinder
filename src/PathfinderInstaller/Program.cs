using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace PathfinderInstaller;

internal static class Program
{
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("Trimming", "IL2111")]
    private static async Task<int> Main(string[] args)
    {
        var pathIndex = Array.IndexOf(args, "--hacknet-path");
        var path = pathIndex == -1 ? SteamUtils.FindInstallDirectory("365450") : args[pathIndex + 1];
        
        if (path is null || !Directory.Exists(path))
        {
            Console.WriteLine("Please supply the path to the Hacknet folder manually with --hacknet-path");
            return 1;
        }

        var hacknetPath = Path.Combine(path, "Hacknet.exe");
        
        var hn = AssemblyDefinition.FromFile(hacknetPath);
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
                new CilInstruction(CilOpCodes.Call, importer.ImportMethod(typeof(Path).GetMethod("GetFullPath", [typeof(string)])!)),
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
        
        newHn.Write(Path.Combine(path, "Hacknet.dll"));

        if (args.AsSpan().Contains("--patch-only"))
        {
            return 0;
        }
        
        // now to get... everything else
        using var client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new ProductInfoHeaderValue("PathfinderInstaller", "6.0.0")
                }
            }
        };

        var releases = await client.GetFromJsonAsync("https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases", GitHubSerializerContext.Default.GitHubReleaseArray);

        Console.Write("Allow prerelease version? (y/n) ");
        var AllowPrerelease = Console.ReadLine() switch
        {
            "y" => true,
            "n" => false,
            _ => throw new ArgumentException("Not y or n")
        };
        
        var release = releases!.First(r => AllowPrerelease || !r.Prerelease);

        if (!release.TagName.StartsWith("v6."))
        {
            Console.WriteLine($"Cannot install {release.TagName} with this installer");
        }
        
        Console.WriteLine($"Installing Pathfinder {release.TagName} from {release.HtmlUrl}");

        var pfAsset = release.Assets.First(a => a.Name == "Pathfinder.Release.zip");

        var pathfinderResponse = await client.GetAsync(pfAsset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);

        var pfZipStream = await pathfinderResponse.Content.ReadAsStreamAsync();

        var zipFileName = Path.GetTempFileName();
        var zipStream = File.Open(zipFileName, FileMode.Truncate, FileAccess.ReadWrite);

        var length = pathfinderResponse.Content.Headers.ContentLength!.Value;

        void Progress(long total)
        {
            var progress = (int)((double)total / length * 25d);

            Console.CursorLeft = 0;
            Console.Write("Download progress: [");
            Console.Write(new string('#', progress));
            Console.Write(new string('-', 25 - progress));
            Console.Write(']');
        }

        Progress(0);
        
        CopyToWithProgress(pfZipStream, zipStream, Progress);
        Console.WriteLine();
        
        await pfZipStream.DisposeAsync();
        pathfinderResponse.Dispose();

        zipStream.Seek(0, SeekOrigin.Begin);

        var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
        
        zip.ExtractToDirectory(path, true);
        
        await zipStream.DisposeAsync();
        
        File.Delete(zipFileName);
        
        return 0;
    }

    private static void CopyToWithProgress(Stream from, Stream to, Action<long> progress)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(8192);

        long total = 0;
        int amount;
        while ((amount = from.Read(buffer, 0, buffer.Length)) != 0)
        {
            to.Write(buffer, 0, amount);
            progress(total += amount);
        }

        ArrayPool<byte>.Shared.Return(buffer);
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
        if (original.Module!.ManagedEntryPoint == original)
        {
            cloned.Module!.ManagedEntryPoint = cloned;
        }
        else if (cloned.Name == "InitPlatformAPI")
        {
            var inst = cloned.CilMethodBody!.Instructions.Single(i => i is { OpCode.Code: CilCode.Call, Operand: IMethodDefOrRef { Name.Value: "InitSafe" } });
            var call = (IMethodDefOrRef)inst.Operand!;
            inst.Operand = new MemberReference(call.DeclaringType, "Init", call.Signature);
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
