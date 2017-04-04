using System;
using System.Reflection;

namespace HacknetPathfinder
{
    /// <summary>
    /// Function hooks for the Pathfinder mod system
    /// </summary>
    /// Place all functions to be hooked into Hacknet here
    public static class PathfinderHooks
    {
        public static bool onMain(string[] args)
        {
            Pathfinder.Pathfinder.init();
            var startUpEvent = new Pathfinder.Event.StartUpEvent(args);
            startUpEvent.CallEvent();
            if (startUpEvent.IsCancelled)
                return true;
            Hacknet.MainMenu.OSVersion = Hacknet.MainMenu.OSVersion + " Pathfinder v0.1";
            return false;
        }
    }
}
