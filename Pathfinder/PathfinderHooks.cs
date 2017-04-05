namespace Pathfinder
{
    /// <summary>
    /// Function hooks for the Pathfinder mod system
    /// </summary>
    /// Place all functions to be hooked into Hacknet here
    public static class PathfinderHooks
    {
        public static bool onMain(string[] args)
        {
            Pathfinder.init();
            var startUpEvent = new Event.StartUpEvent(args);
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
            Pathfinder.LoadModContent();
            var loadContentEvent = new Event.LoadContentEvent(self);
            loadContentEvent.CallEvent();
        }

        // Hook location : ProgramRunner.ExecuteProgram()
        public static bool onCommandSent(out bool disconnects, object osObj, string[] arguments)
        {
            var os = osObj as Hacknet.OS;
            var commandSentEvent = new Event.CommandSentEvent(os, arguments);
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
