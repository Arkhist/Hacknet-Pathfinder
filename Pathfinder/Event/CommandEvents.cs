using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public class CommandSentEvent : OSEvent
    {
        public bool Disconnects { get; set; }
        public bool HandleReturn { get; set; }
        public string StateChange { get; set; } = "";
        public List<string> Arguments { get; private set; }
        [Obsolete("Use Arguments")]
        public string[] Args => Arguments.ToArray();
        public CommandSentEvent(Hacknet.OS os, string[] args) : base(os)
        {
            Arguments = new List<string>(args ?? Utility.Array<string>.Empty);
        }
    }
}
