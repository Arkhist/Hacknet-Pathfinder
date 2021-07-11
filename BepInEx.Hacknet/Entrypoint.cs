using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HN = global::Hacknet;

namespace BepInEx.Hacknet
{
    public class Entrypoint
    {
        public Entrypoint()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBepAssembly;

            Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "dynamicmethod");

            LoadBepInEx.Load();
        }

        public static Assembly ResolveBepAssembly(object sender, ResolveEventArgs args)
        {
            var asmName = new AssemblyName(args.Name);

            foreach (var path in Directory.GetFiles("./BepInEx", $"{asmName.Name}.dll", SearchOption.AllDirectories).Select(x => Path.GetFullPath(x)))
            {
                try
                {
                    return Assembly.LoadFile(path);
                }
                catch {}
            }

            return null;
        }
    }

    internal static class LoadBepInEx
    {
        internal static Assembly HacknetAsm;
        
        internal static Assembly ResolveHacknet(object sender, ResolveEventArgs args)
        {
            if (new AssemblyName(args.Name).Name == "Hacknet")
                return HacknetAsm;
            return null;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Load()
        {
            try
            {
                // Do stuff for BepInEx to recognize where it is
                Paths.SetExecutablePath(Path.Combine(Environment.CurrentDirectory, "Hacknet.exe"));

                Logger.Listeners.Add(new ConsoleLogger());
                HarmonyLib.AccessTools.PropertySetter(typeof(TraceLogSource), nameof(TraceLogSource.IsListening)).Invoke(null, new object[] { true });
                ConsoleManager.Initialize(true);
                Logger.Listeners.Add(new ConsoleLogListener());

                using (var patcher = new Preloader.Core.AssemblyPatcher())
                {
                    patcher.LoadAssemblyDirectories(new [] { Paths.GameRootPath }, new [] { "dll", "exe" });
                    patcher.AddPatchersFromDirectory(Paths.PatcherPluginPath);
                    patcher.PatchAndLoad();

                    HacknetAsm = patcher.LoadedAssemblies["Hacknet.exe"];
                }

                AppDomain.CurrentDomain.AssemblyResolve += ResolveHacknet;
                
                StartHacknet.Start();
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

    internal static class StartHacknet
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Start()
        {
            // Start chainloader for plugins
            var chainloader = new HacknetChainloader();
            chainloader.Initialize();
            chainloader.Execute();

            LoadBepInEx.HacknetAsm.EntryPoint.Invoke(null, new object[] { Environment.GetCommandLineArgs() });
        }
    }
}
