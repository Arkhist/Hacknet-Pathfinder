using System;
using BepInEx.Logging;

namespace BepInEx.Hacknet
{
    public static class Loader
    {
        public static void Begin()
        {
            try
            {
                // Do stuff for BepInEx to recognize where it is
                Paths.SetExecutablePath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                Logger.Listeners.Add(new ConsoleLogListener());

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
