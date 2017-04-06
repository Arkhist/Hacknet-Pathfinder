using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Event
{
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
