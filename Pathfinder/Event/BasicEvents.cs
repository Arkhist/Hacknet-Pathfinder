using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Event
{
    // Called when Hacknet boots up (Program.Main start)
    class StartUpEvent : PathfinderEvent
    {
        private string[] mainArgs;

        public string[] MainArgs
        {
            get
            {
                return mainArgs;
            }
        }

        public StartUpEvent(string[] args)
        {
            mainArgs = args;
        }

    }
}
