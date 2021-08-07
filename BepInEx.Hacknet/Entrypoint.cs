using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HN = global::Hacknet;

namespace BepInEx.Hacknet
{
    public static class Entrypoint
    {
        public static void Bootstrap()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBepAssembly;
            if (Type.GetType("Mono.Runtime") != null)
                AppDomain.CurrentDomain.AssemblyResolve += ResolveGACAssembly;

            Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "dynamicmethod");

            LoadBepInEx.Load();
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

            foreach (var path in Directory
                .GetFiles($"/usr/lib/mono/gac/{asmName.Name}", $"{asmName.Name}.dll", SearchOption.AllDirectories)
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
    }

    internal static class LoadBepInEx
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Load()
        {
            try
            {
                // Do stuff for BepInEx to recognize where it is
                Paths.SetExecutablePath(typeof(HN.Program).Assembly.GetName().Name);

                Logger.Listeners.Add(new ConsoleLogger());
                HarmonyLib.AccessTools.PropertySetter(typeof(TraceLogSource), nameof(TraceLogSource.IsListening)).Invoke(null, new object[] { true });
                ConsoleManager.Initialize(true);

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
}
