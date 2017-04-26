using Hacknet;
using Gui = Hacknet.Gui;
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

        private static Button modListButton = new Button(180, 600, 450, 40, "Pathfinder Mod List", MainMenu.buttonColor);
        private static Button returnButton = new Button(180, 650, 250, 28, "Return", MainMenu.exitButtonColor);

        private static MainMenuState mainMenuState = MainMenuState.GameHandled;

        public static void drawMainMenu(DrawMainMenuEvent e)
        {
            if (mainMenuState != MainMenuState.PathfinderModList)
                return;
            e.IsCancelled = true;

            GameScreen baseS = e.MainMenu;

            if (returnButton.Draw())
                mainMenuState = MainMenuState.GameHandled;

            Gui.TextItem.doFontLabel(new Vector2(125f, 50), "Pathfinder Mod Load Order", GuiData.font, Color.White, 3.40282347E+38f, 3.40282347E+38f, false);

            float yPos = 120;
            int index = 0;
            foreach (var modIdentifier in Pathfinder.LoadedModIdentifiers)
            {
                Gui.TextItem.doFontLabel(new Vector2(200f, yPos), (++index) + ". " + modIdentifier, GuiData.smallfont, Color.White, 3.40282347E+38f, 3.40282347E+38f, false);
                yPos += 30;
            }
        }

        public static void drawPathfinderButtons(DrawMainMenuButtonsEvent e)
        {
            if (modListButton.Draw())
                mainMenuState = MainMenuState.PathfinderModList;
        }
    }
}
