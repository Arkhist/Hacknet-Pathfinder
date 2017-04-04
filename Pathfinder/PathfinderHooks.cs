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
		public static void onMain(string[] args)
		{
            Pathfinder.Pathfinder.init();
            Pathfinder.Event.StartUpEvent startUpEvent = new Pathfinder.Event.StartUpEvent(args);
            startUpEvent.CallEvent();
            if (startUpEvent.IsCancelled)
                return;
			Hacknet.MainMenu.OSVersion = Hacknet.MainMenu.OSVersion + " Pathfinder v0.1";
		}
	}
}
