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

        //  Hook location : Game1.LoadContent()
        //  if (this.CanLoadContent)
        //  {
        //	    <HOOK HERE>
        //      PortExploits.populate();
        public static void onLoadContent(Hacknet.Game1 self)
        {
            var loadContentEvent = new Pathfinder.Event.LoadContentEvent(self);
            loadContentEvent.CallEvent();
        }

        // Hook location : ProgramRunner.ExecuteProgram()
        public static bool onCommandSent(Hacknet.OS os, string[] arguments, out bool disconnects)
        {
            var commandSentEvent = new Pathfinder.Event.CommandSentEvent(os, arguments);
            commandSentEvent.CallEvent();
            if (commandSentEvent.IsCancelled)
            {
                disconnects = commandSentEvent.Disconnects;
                return true;
            }
            disconnects = commandSentEvent.Disconnects;
            return false;
        }
	}
}
