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
}
