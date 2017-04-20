using System;
using Hacknet;

namespace Pathfinder.Event
{
    public class GameEvent : PathfinderEvent
    {
        public Game1 GameInstance { get; private set; }
        public GameEvent(Game1 ins)
        {
            GameInstance = ins;
        }
    }

    // Called after Hacknet loads the Game Object (actual game)
    public class GameLoadContentEvent : GameEvent
    {
        public GameLoadContentEvent(Game1 ins) : base(ins) {}
    }

    public class GameExitEvent : GameEvent
    {
        public GameExitEvent(Game1 ins) : base(ins) {}
    }
}
