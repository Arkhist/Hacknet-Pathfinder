using System.Collections;
using System.Collections.Generic;
using Pathfinder.GUI;

namespace Pathfinder.Event
{
    public class CommandEvent : OSEvent, IEnumerable<string>
    {
        public string Input { get; }
        public List<string> Arguments { get; set; } = new List<string>();
        public CommandEvent(Hacknet.OS os, string[] args) : base(os)
        {
            Input = string.Join(" ", args);
            Arguments.AddRange(args);
            Arguments.RemoveAll(string.IsNullOrWhiteSpace);
        }

        public string this[int index]
        {
            get
            {
                if (Arguments.Count <= index || index < 0)
                    return "";
                return Arguments[index];
            }
        }

        public IEnumerator<string> GetEnumerator()
            => Arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
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
