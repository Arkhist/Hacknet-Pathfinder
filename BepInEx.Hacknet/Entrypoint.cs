using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

namespace BepInEx.Hacknet
{
    public static class Entrypoint
    {
        
        public static void Bootstrap()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBepAssembly;

            Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "dynamicmethod");

            LoadBepInEx.Load();
        }

        public static Assembly ResolveBepAssembly(object sender, ResolveEventArgs args)
        {
            var asmName = new AssemblyName(args.Name);

            var possiblePath = Path.GetFullPath($"./BepInEx/core/{asmName.Name}.dll");

            if (!File.Exists(possiblePath))
                return null;

            try
            {
                return Assembly.LoadFile(possiblePath);
            }
            catch
            {
                return null;
            }
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
                Paths.SetExecutablePath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                Logger.Listeners.Add(new ConsoleLogger());
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
