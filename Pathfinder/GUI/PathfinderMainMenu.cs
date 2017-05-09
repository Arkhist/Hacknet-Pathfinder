using Hacknet;
using Gui = Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using System.Collections.Generic;
using System;

namespace Pathfinder.GUI
{
    static class PathfinderMainMenu
    {
        enum MainMenuState
        {
            GameHandled,
            PathfinderModList
        }

        private static Button modListButton = new Button(180, 600, 450, 40, "Pathfinder Mod List", MainMenu.buttonColor)
        {
            DrawFinish = (r) => { if (r.JustReleased) mainMenuState = MainMenuState.PathfinderModList; }
        };
        private static Button returnButton = new Button(180, 650, 250, 28, "Return", MainMenu.exitButtonColor)
        {
            DrawFinish = (r) => { if (r.JustReleased) mainMenuState = MainMenuState.GameHandled; }
        };

        private static Dictionary<string, Button> unloadButtons = new Dictionary<string, Button>();
        private static List<string> disabledModIds = new List<string>();
        private static Dictionary<string, Button> reloadButtons = new Dictionary<string, Button>();
        private static bool buttonsPreped = false;

        private static MainMenuState mainMenuState = MainMenuState.GameHandled;

        private static void PrepareButtons(DrawMainMenuEvent e)
        {
            if (buttonsPreped)
                return;
            var ids = Pathfinder.LoadedModIdentifiers;
            unloadButtons.Clear();
            foreach (var id in ids)
            {
                unloadButtons[id] = new Button(-1, -1, 100, 30, "Unload")
                {
                    DrawFinish = r => { if (r.JustReleased) { Pathfinder.UnloadMod(Pathfinder.GetMod(id)); disabledModIds.Add(id); } }
                };
                reloadButtons[id] = new GUI.Button(-1, -1, 100, 30, "Load")
                {
                    DrawFinish = r =>
                    {
                        if (r.JustReleased)
                        {
                            try
                            {
                                Pathfinder.LoadMod(Pathfinder.GetMod(id).GetType().Assembly.Location, true);
                            }
                            catch (Exception) { }
                            disabledModIds.Remove(id);
                        }
                    }
                };
            }
            buttonsPreped = true;
        }

        public static void DrawMainMenu(DrawMainMenuEvent e)
        {
            if (mainMenuState != MainMenuState.PathfinderModList)
                return;
            e.IsCancelled = true;

            PrepareButtons(e);

            GameScreen baseS = e.MainMenu;

            returnButton.Draw();

            Gui.TextItem.doFontLabel(new Vector2(125f, 50), "Pathfinder Mod Load Order", GuiData.font, Color.White);

            float yPos = 120;
            int index = 0;
            Button b;
            foreach (var id in Pathfinder.LoadedModIdentifiers)
            {
                b = unloadButtons[id];
                b.X = 500;
                b.Y = (int)yPos;
                b.Draw();
                Gui.TextItem.doFontLabel(new Vector2(200f, yPos), (++index) + ". " + id, GuiData.smallfont, Color.White);
                yPos += 30;
            }

            if (disabledModIds.Count < 1) return;

            Gui.TextItem.doFontLabel(new Vector2(200, yPos), "Disabled Mods", GuiData.font, Color.White);
            yPos += 50;

            foreach (var id in disabledModIds)
            {
                b = reloadButtons[id];
                b.X = 500;
                b.Y = (int)yPos;
                b.Draw();
                Gui.TextItem.doFontLabel(new Vector2(200f, yPos), (++index) + ". " + id, GuiData.smallfont, Color.White);
                yPos += 30;
            }
        }

        public static void DrawPathfinderButtons(DrawMainMenuButtonsEvent e) => modListButton.Draw();
    }
}
