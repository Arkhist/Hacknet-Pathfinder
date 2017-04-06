#pragma warning disable CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant

namespace Pathfinder.Event
{
    // Called when Hacknet boots up (Program.Main start)
    public class StartUpEvent : PathfinderEvent
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
    public class LoadContentEvent : PathfinderEvent
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

    public class CommandSentEvent : PathfinderEvent
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

    public class LoadSessionEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public LoadSessionEvent(Hacknet.OS osInstance)
        {
            this.osInstance = osInstance;
        }
    }

    public class PostLoadSessionEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public PostLoadSessionEvent(Hacknet.OS osInstance)
        {
            this.osInstance = osInstance;
        }
    }


    public class GameExitEvent : PathfinderEvent
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
