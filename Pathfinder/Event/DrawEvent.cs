using Microsoft.Xna.Framework;

namespace Pathfinder.Event
{
    public class DrawMainMenuEvent : PathfinderEvent
    {
        public Hacknet.MainMenu MainMenuInstance
        {
            get; private set;
        }

        public GameTime GameTime
        {
            get; private set;
        }

        public DrawMainMenuEvent(Hacknet.MainMenu mainMenuInstance, GameTime gameTime)
        {
            MainMenuInstance = mainMenuInstance;
            GameTime = gameTime;
        }
    }

    public class DrawMainMenuButtonsEvent : PathfinderEvent
    {
        public Hacknet.MainMenu MainMenuInstance
        {
            get; private set;
        }

        public DrawMainMenuButtonsEvent(Hacknet.MainMenu mainMenuInstance)
        {
            MainMenuInstance = mainMenuInstance;
        }
    }

    public class DrawMainMenuTitlesEvent : PathfinderEvent
    {
        public Hacknet.MainMenu MainMenuInstance
        {
            get; set;
        }

        public string MainTitle
        {
            get; set;
        }

        public string Subtitle
        {
            get; set;
        }

        public DrawMainMenuTitlesEvent(Hacknet.MainMenu mainMenuInstance, ref string mainTitle, ref string subtitle)
        {
            MainMenuInstance = mainMenuInstance;
            MainTitle = mainTitle;
            Subtitle = subtitle;
        }
    }
}
