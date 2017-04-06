using Hacknet;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using System;
using System.Reflection;

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

        public static void drawMainMenu(PathfinderEvent pathfinderEvent)
        {
            DrawMainMenuEvent drawMainMenuEvent = (DrawMainMenuEvent)pathfinderEvent;
            if (mainMenuState != MainMenuState.PathfinderModList)
                return;
            drawMainMenuEvent.IsCancelled = true;

            GameScreen baseS = ((GameScreen)drawMainMenuEvent.MainMenuInstance);

            try
            {
                PostProcessor.begin();
                baseS.ScreenManager.FadeBackBufferToBlack(255);
                GuiData.startDraw();
                Rectangle dest = new Rectangle(0, 0, baseS.ScreenManager.GraphicsDevice.Viewport.Width, baseS.ScreenManager.GraphicsDevice.Viewport.Height);
                Rectangle destinationRectangle = new Rectangle(-20, -20, baseS.ScreenManager.GraphicsDevice.Viewport.Width + 40, baseS.ScreenManager.GraphicsDevice.Viewport.Height + 40);
                Rectangle dest2 = new Rectangle(dest.X + dest.Width / 4, dest.Height / 4, dest.Width / 2, dest.Height / 4);
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, Color.Black);
                if (Settings.DrawHexBackground)
                {
                    HexGridBackground hexBack = (HexGridBackground)drawMainMenuEvent.MainMenuInstance.GetType().GetField("hexBackground", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(drawMainMenuEvent.MainMenuInstance);
                    hexBack.Draw(dest, GuiData.spriteBatch, Color.Transparent, Settings.lighterColorHexBackground ? new Color(20, 20, 20) : new Color(15, 15, 15, 0), HexGridBackground.ColoringAlgorithm.NegaitiveSinWash, 0f);
                }
                TextItem.DrawShadow = false;

                if (Button.doButton(15, 180, 650, 250, 28, "Return", new Color?(MainMenu.exitButtonColor)))
                {
                    mainMenuState = MainMenuState.GameHandled;
                }

                TextItem.doFontLabel(new Vector2(125f, (float)50), "Loaded Pathfinder Mods", GuiData.font, new Color?(Color.White), 3.40282347E+38f, 3.40282347E+38f, false);

                float yPos = 120;
                foreach(string modIdentifier in Pathfinder.LoadedModIdentifiers)
                {
                    TextItem.doFontLabel(new Vector2(200f, (float)yPos), modIdentifier, GuiData.smallfont, new Color?(Color.White), 3.40282347E+38f, 3.40282347E+38f, false);
                    yPos += 30;
                }

                GuiData.endDraw();
                PostProcessor.end();
                baseS.ScreenManager.FadeBackBufferToBlack((int)(255 - baseS.TransitionAlpha));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void drawPathfinderButtons(PathfinderEvent pathfinderEvent)
        {
            DrawMainMenuButtonsEvent drawMainMenuButtonsEvent = (DrawMainMenuButtonsEvent)pathfinderEvent;
            if (Button.doButton(200, 180, 475, 450, 40, "Pathfinder Mod List", new Color?(MainMenu.buttonColor)))
            {
                mainMenuState = MainMenuState.PathfinderModList;
            }
        }
    }
}
