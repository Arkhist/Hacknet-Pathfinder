using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PathfinderPatcher
{
    public static class Entrypoint
    {
        public static void Bootstrap()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveBepAssembly;

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
            BepInEx.Hacknet.Loader.Begin();
        }
    }
}
