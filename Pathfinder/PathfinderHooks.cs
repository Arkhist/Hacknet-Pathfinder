using System;
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
			Hacknet.MainMenu.OSVersion = Hacknet.MainMenu.OSVersion + " Pathfinder v0.1";
		}
	}
}
