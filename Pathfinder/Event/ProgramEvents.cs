using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    // Called when Hacknet boots up (Program.Main start)
    public class StartUpEvent : PathfinderEvent
    {
        public List<string> MainArguments { get; private set; }
        public StartUpEvent(string[] args) { MainArguments = new List<string>(args ?? Utility.Array<string>.Empty); }
    }
}
