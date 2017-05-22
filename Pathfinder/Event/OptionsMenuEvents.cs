using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Event
{
    public class OptionsMenuEvent : PathfinderEvent
    {
        public OptionsMenu OptionsMenu { get; }
        public OptionsMenuEvent(OptionsMenu menu) { OptionsMenu = menu; }
    }

    public class OptionsMenuDrawEvent : OptionsMenuEvent
    {
        public GameTime GameTime { get; }
        public OptionsMenuDrawEvent(OptionsMenu menu, GameTime time) : base(menu) {GameTime = time; }
    }

    public class OptionsMenuLoadContentEvent : OptionsMenuEvent
    {
        public OptionsMenuLoadContentEvent(OptionsMenu menu) : base(menu) {}
    }

    public class OptionsMenuUpdateEvent : OptionsMenuEvent
    {
        public GameTime GameTime { get; }
        public bool ScreenNotFocused { get; }
        public bool ScreenIsCovered { get; }
        public OptionsMenuUpdateEvent(OptionsMenu menu, GameTime time, bool notFocused, bool isCovered) : base(menu)
        {
            GameTime = time;
            ScreenNotFocused = notFocused;
            ScreenIsCovered = isCovered;
        }
    }

    public class OptionsMenuApplyEvent : OptionsMenuEvent
    {
        public OptionsMenuApplyEvent(OptionsMenu menu) : base(menu) { }
    }
}
