using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Event
{
    public class GameEvent : PathfinderEvent
    {
        public Game1 Game { get; private set; }
        public GameEvent(Game1 ins) { Game = ins; }
    }

    // Called after Hacknet loads the Game Object (actual game)
    public class GameLoadContentEvent : GameEvent
    {
        public GameLoadContentEvent(Game1 ins) : base(ins) {}
    }

    public class GameUpdateEvent : GameEvent
    {
        public GameTime Time { get; private set; }
        public GameUpdateEvent(Game1 ins, GameTime time) : base(ins) { Time = time; }
    }

    public class GameUnloadEvent : GameEvent
    {
        public GameUnloadEvent(Game1 ins) : base(ins) {}
    }
}
