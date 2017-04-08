using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Event
{
    public class DrawMainMenuEvent : PathfinderEvent
    {
        private Hacknet.MainMenu mainMenuInstance;
        private GameTime gameTime;

        public Hacknet.MainMenu MainMenuInstance
        {
            get
            {
                return mainMenuInstance;
            }
        }

        public GameTime GameTime
        {
            get
            {
                return gameTime;
            }
        }

        public DrawMainMenuEvent(Hacknet.MainMenu mainMenuInstance, GameTime gameTime)
        {
            this.mainMenuInstance = mainMenuInstance;
            this.gameTime = gameTime;
        }
    }

    public class DrawMainMenuButtonsEvent : PathfinderEvent
    {
        private Hacknet.MainMenu mainMenuInstance;

        public Hacknet.MainMenu MainMenuInstance
        {
            get
            {
                return mainMenuInstance;
            }
        }

        public DrawMainMenuButtonsEvent(Hacknet.MainMenu mainMenuInstance)
        {
            this.mainMenuInstance = mainMenuInstance;
        }
    }

    public class DrawMainMenuTitlesEvent : PathfinderEvent
    {
        private Hacknet.MainMenu mainMenuInstance;
        private string mainTitle;
        private string subtitle;

        public Hacknet.MainMenu MainMenuInstance
        {
            get
            {
                return mainMenuInstance;
            }
        }

        public string MainTitle
        {
            get
            {
                return mainTitle;
            }
            set
            {
                mainTitle = value;
            }
        }

        public string Subtitle
        {
            get
            {
                return subtitle;
            }
            set
            {
                subtitle = value;
            }
        }

        public DrawMainMenuTitlesEvent(Hacknet.MainMenu mainMenuInstance, ref string mainTitle, ref string subtitle)
        {
            this.mainMenuInstance = mainMenuInstance;
            this.mainTitle = mainTitle;
            this.subtitle = subtitle;
        }
    }
}
