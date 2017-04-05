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

    // Called after Hacknet loads the Game Object (actual game)
    class LoadContentEvent : PathfinderEvent
    {
        private Hacknet.Game1 gameInstance;

        public Hacknet.Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public LoadContentEvent(Hacknet.Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
        }
    }

    class CommandSentEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;
        private string[] args;
        private bool disconnectFlag = false;

        public bool Disconnects
        {
            get
            {
                return disconnectFlag;
            }
            set
            {
                disconnectFlag = value;
            }
        }

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
                return osInstance;
            }
        }

        public CommandSentEvent(Hacknet.OS osInstance, string[] args)
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
