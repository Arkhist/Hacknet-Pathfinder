using System;
using System.Collections.Generic;
using Pathfinder.GUI;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public class CommandSentEvent : OSEvent
    {
        public bool Disconnects { get; set; }
        public CommandDisplayStateChange StateChange { get; set; } = CommandDisplayStateChange.None;
        public string State
        {
            get
            {
                switch (StateChange)
                {
                    case CommandDisplayStateChange.None:
                    case CommandDisplayStateChange.Daemon:
                        return "";
                    default:
                        return StateChange.ToString().ToLower();
                }
            }
        }
        public List<string> Arguments { get; private set; }
        [Obsolete("Use Arguments")]
        public string[] Args => Arguments.ToArray();
        public CommandSentEvent(Hacknet.OS os, string[] args) : base(os)
        {
            Arguments = new List<string>(args ?? Utility.Array<string>.Empty);
        }
    }
}
