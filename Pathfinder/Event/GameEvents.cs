using System;
using Hacknet;

namespace Pathfinder.Event
{
    public class GameEvent : PathfinderEvent
    {
        public Game1 Game { get; private set; }
        [Obsolete("Use Game")]
        public Game1 GameInstance => Game;
        public GameEvent(Game1 ins) { Game = ins; }
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
