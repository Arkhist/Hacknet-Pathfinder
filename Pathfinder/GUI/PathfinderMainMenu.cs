using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;

namespace Pathfinder.GUI
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

            GameScreen baseS = pathfinderEvent.MainMenuInstance;

            if (Button.doButton(15, 180, 650, 250, 28, "Return", MainMenu.exitButtonColor))
            {
                mainMenuState = MainMenuState.GameHandled;
            }

            TextItem.doFontLabel(new Vector2(125f, 50), "Pathfinder Mod Load Order", GuiData.font, Color.White, 3.40282347E+38f, 3.40282347E+38f, false);

            float yPos = 120;
            int index = 0;
            foreach (var modIdentifier in Pathfinder.LoadedModIdentifiers)
            {
                TextItem.doFontLabel(new Vector2(200f, yPos), (++index) + ". " + modIdentifier, GuiData.smallfont, Color.White, 3.40282347E+38f, 3.40282347E+38f, false);
                yPos += 30;
            }
        }

        public static void drawPathfinderButtons(DrawMainMenuButtonsEvent pathfinderEvent)
        {
            if (Button.doButton(200, 180, 600, 450, 40, "Pathfinder Mod List", MainMenu.buttonColor))
            {
                mainMenuState = MainMenuState.PathfinderModList;
            }
        }
    }
}
