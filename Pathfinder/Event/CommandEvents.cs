using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public class CommandSentEvent : PathfinderEvent
    {
        public bool Disconnects
        {
            get; set;
        }

        public List<string> Arguments
        {
            get; private set;
        }

        [Obsolete("Use Arguments")]
        public string[] Args
        {
            get
            {
                return Arguments.ToArray();
            }
        }

        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public CommandSentEvent(Hacknet.OS osInstance, string[] args)
        {
            OsInstance = osInstance;
            Arguments = new List<string>(args ?? Utility.Array<string>.Empty);
            Disconnects = false;
        }
    }
}
