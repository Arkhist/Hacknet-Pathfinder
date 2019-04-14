using System.Collections.Generic;
using Pathfinder.GUI;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public class CommandEvent : OSEvent
    {
        public List<string> Arguments { get; set; } = new List<string>();
        public List<string> RawArguments { get; set; }
        public CommandEvent(Hacknet.OS os, string[] args) : base(os)
        {
            RawArguments = new List<string>(args ?? Utility.Array<string>.Empty);
            Arguments.AddRange(RawArguments);
            Arguments.RemoveAll(string.IsNullOrWhiteSpace);
        }

        public string this[int index]
        {
            get
            {
                if (Arguments?.Count <= index)
                    return "";
                return Arguments?[index];
            }
        }

        public int ArgCount => Arguments?.Count ?? 0;
    }

    public class CommandSentEvent : CommandEvent
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
        public CommandSentEvent(Hacknet.OS os, string[] args) : base(os, args) { }
    }

    public class CommandFinishedEvent : CommandEvent
    {
        public CommandSentEvent SentEvent { get; }
        public CommandFinishedEvent(CommandSentEvent e) : base(e.OS, e.Arguments.ToArray()) { SentEvent = e; }
    }
}
