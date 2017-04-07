using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;

namespace Pathfinder.Gui
{
    static class PathfinderMainMenu
    {
        enum MainMenuState
        {
            GameHandled,
            PathfinderModList
        }

        private static MainMenuState mainMenuState = MainMenuState.GameHandled;

        public static void drawMainMenu(DrawMainMenuEvent pathfinderEvent)
        {
            if (mainMenuState != MainMenuState.PathfinderModList)
                return;
            pathfinderEvent.IsCancelled = true;

            GameScreen baseS = ((GameScreen)pathfinderEvent.MainMenuInstance);

            if (Button.doButton(15, 180, 650, 250, 28, "Return", new Color?(MainMenu.exitButtonColor)))
            {
                mainMenuState = MainMenuState.GameHandled;
            }

            TextItem.doFontLabel(new Vector2(125f, (float)50), "Pathfinder Mod Load Order", GuiData.font, new Color?(Color.White), 3.40282347E+38f, 3.40282347E+38f, false);

            float yPos = 120;
            int index = 0;
            foreach (var modIdentifier in Pathfinder.LoadedModIdentifiers)
            {
                TextItem.doFontLabel(new Vector2(200f, (float)yPos), (++index) + ". " + modIdentifier, GuiData.smallfont, new Color?(Color.White), 3.40282347E+38f, 3.40282347E+38f, false);
                yPos += 30;
            }
        }

        public static void drawPathfinderButtons(DrawMainMenuButtonsEvent pathfinderEvent)
        {
            if (Button.doButton(200, 180, 475, 450, 40, "Pathfinder Mod List", new Color?(MainMenu.buttonColor)))
            {
                mainMenuState = MainMenuState.PathfinderModList;
            }
        }
    }
}
