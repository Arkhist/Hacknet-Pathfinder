using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
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
                // Fix a dumb harmonyx bug caused by yours truly that *could* be fixed by updating harmony but master branch of harmonyx
                // depends on latest monomod release that has *another* dumb bug (that one wasnt me)
                // so instead we workaround until we die
                new Harmony("ILManipulatorUnpatchFix").Patch(
                    AccessTools.Method(typeof(PatchInfo), nameof(PatchInfo.RemovePatch)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(LoadBepInEx), nameof(FixIlmanipulatorUnpatching)))
                );

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

        internal static void FixIlmanipulatorUnpatching(PatchInfo __instance, MethodInfo patch)
        {
            __instance.ilmanipulators = __instance.ilmanipulators.Where(p => p.PatchMethod != patch).ToArray();
        }
    }
}
