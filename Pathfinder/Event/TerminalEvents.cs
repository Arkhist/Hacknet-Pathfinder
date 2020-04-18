using System;
using Hacknet;

namespace Pathfinder.Event
{
    public class TerminalEvent : OSEvent
    {
        public Terminal Terminal { get; private set; }
        public TerminalEvent(Terminal terminal) : base(terminal.os)
        {
            Terminal = terminal;
        }
    }

    public class TerminalWriteEvent : TerminalEvent
    {
        public string Text { get; set; }
        public TerminalWriteEvent(Terminal terminal, string text) : base(terminal)
        {
            Text = text;
        }
    }

    public class TerminalWriteSingleEvent : TerminalEvent
    {
        public string Text { get; set; }
        public TerminalWriteSingleEvent(Terminal terminal, string text) : base(terminal)
        {
            Text = text;
        }
    }

    public class TerminalWriteLineEvent : TerminalEvent
    {
        public string Text { get; set; }
        public TerminalWriteLineEvent(Terminal terminal, string text) : base(terminal)
        {
            Text = text;
        }
    }
}
