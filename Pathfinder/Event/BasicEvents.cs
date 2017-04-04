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

    // Called after Hacknet creates the Game instance (Program.Main)
    class GameStartEvent : PathfinderEvent
    {
        private Hacknet.Game1 gameInstance;

        public Hacknet.Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public GameStartEvent(Hacknet.Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
        }
    }

    // Called after Hacknet loads the OS object (actual game)
    class LoadContentEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public LoadContentEvent(Hacknet.OS osInstance)
        {
            this.osInstance = osInstance;
        }
    }

    class TerminalMessageEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;
        private string[] args;

        public string[] Args
        {
            get
            {
                return args;
            }
        }

        public Hacknet.OS OsInstance
        {
            get
            {
                return OsInstance;
            }
        }

        public TerminalMessageEvent(Hacknet.OS osInstance, string[] args)
        {
            this.osInstance = osInstance;
            this.args = args;
        }
    }

    class GameExitEvent : PathfinderEvent
    {
        private Hacknet.Game1 gameInstance;

        public Hacknet.Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public GameExitEvent(Hacknet.Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
        }
    }
}
