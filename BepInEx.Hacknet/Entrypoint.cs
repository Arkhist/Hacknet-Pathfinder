using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using HN = global::Hacknet;

namespace BepInEx.Hacknet;

public static class Entrypoint
{
    public static void Bootstrap()
    {
        WriteDiagnosticHeader();

        AppDomain.CurrentDomain.AssemblyResolve += ResolveBepAssembly;
        if (Type.GetType("Mono.Runtime") != null)
            AppDomain.CurrentDomain.AssemblyResolve += ResolveGACAssembly;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveRenamedAssembly;

        Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "dynamicmethod");

        LoadBepInEx.Load();
    }

    private static void WriteDiagnosticHeader()
    {
        string Center(string s, int r)
        {
            int x = r   - s.Length;
            int l = x/2 + s.Length;
            return s.PadLeft(l).PadRight(r);
        }
        void WriteInBox(params string[] lines)
        {
            int l = lines.Max(s => s.Length);

            Console.WriteLine("#" + new string('=', l+2) + "#");
            foreach (string line in lines)
                Console.WriteLine("| " + Center(line,l) + " |");
            Console.WriteLine("#" + new string('=', l+2) + "#");
        }
        bool hasDLC  = File.Exists("Content/DLC/DLCFaction.xml");
        bool isSteam = typeof(HN.PlatformAPI.Storage.SteamCloudStorageMethod).GetField("deserialized") != null;
        WriteInBox
        (
            $"Hacknet {(hasDLC ? "+Labyrinths " : "")}{(isSteam ? "Steam" : "Non-Steam")} {HN.MainMenu.OSVersion}",
            $"{Environment.OSVersion.Platform} ({SDL2.SDL.SDL_GetPlatform()})",
            $"Chainloader {HacknetChainloader.VERSION}"
        );
    }

    public static Assembly ResolveBepAssembly(object sender, ResolveEventArgs args)
    {
        var asmName = new AssemblyName(args.Name);

        foreach (var path in Directory
                     .GetFiles("./BepInEx", $"{asmName.Name}.dll", SearchOption.AllDirectories)
                     .Select(Path.GetFullPath))
        {
            try
            {
                return Assembly.LoadFile(path);
            }
            catch {}
        }

        return null;
    }

    public static Assembly ResolveGACAssembly(object sender, ResolveEventArgs args)
    {
        var asmName = new AssemblyName(args.Name);

        try
        {
            foreach (var path in Directory
                         .GetFiles($"/usr/lib/mono/gac/{asmName.Name}", $"{asmName.Name}.dll", SearchOption.AllDirectories)
                         .Select(Path.GetFullPath))
            {
                try
                {
                    return Assembly.LoadFile(path);
                }
                catch { }
            }
        }
        catch { }

        return null;
    }

    public static Assembly ResolveRenamedAssembly(object sender, ResolveEventArgs args)
    {
        var asmName = new AssemblyName(args.Name);

        if (ChainloaderFix.Remaps.TryGetValue(asmName.Name, out Assembly ret))
            return ret;

        return null;
    }
}

internal static class LoadBepInEx
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Load()
    {
        try
        {
            Paths.SetExecutablePath(typeof(HN.Program).Assembly.GetName().Name);

            Logger.Listeners.Add(new ConsoleLogger());
            AccessTools.PropertySetter(typeof(TraceLogSource), nameof(TraceLogSource.IsListening)).Invoke(null, [true]);
            ConsoleManager.Initialize(true, true);

            // Start chainloader for plugins
            var chainloader = new HacknetChainloader();
            chainloader.Initialize();
            chainloader.Execute();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fatal loading exception:");
            Console.WriteLine(ex);
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}