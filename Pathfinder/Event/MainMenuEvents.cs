using System;
using Microsoft.Xna.Framework;

namespace Pathfinder.Event
{
    public class MainMenuEvent : PathfinderEvent
    {
        public Hacknet.MainMenu MainMenu { get; private set; }
        [Obsolete("Use MainMenu")]
        public Hacknet.MainMenu MainMenuInstance => MainMenu;
        public MainMenuEvent(Hacknet.MainMenu mainMenu)
        {
            MainMenu = mainMenu;
        }
    }

    public class DrawMainMenuEvent : MainMenuEvent
    {
        public GameTime GameTime { get; private set; }
        public DrawMainMenuEvent(Hacknet.MainMenu mainMenu, GameTime gameTime) : base(mainMenu) { GameTime = gameTime; }
    }

    public class DrawMainMenuButtonsEvent : MainMenuEvent
    {
        public DrawMainMenuButtonsEvent(Hacknet.MainMenu mainMenu) : base(mainMenu) {}
    }

    public class DrawMainMenuTitlesEvent : MainMenuEvent
    {
        public string MainTitle { get; set; }
        public string Subtitle { get; set; }
        public DrawMainMenuTitlesEvent(Hacknet.MainMenu mainMenu, string mainTitle, string subtitle) : base (mainMenu)
        {
            MainTitle = mainTitle;
            Subtitle = subtitle;
        }
    }
}
