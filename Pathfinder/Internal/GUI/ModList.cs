using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.GUI;
using Pathfinder.ModManager;
using Pathfinder.ModManager.Attribute;
using Pathfinder.Util;
using Gui = Hacknet.Gui;

namespace Pathfinder.Internal.GUI
{
    static class ModList
    {
        private static bool GameHandled { get; set; } = true;
        private static bool ButtonsWerePrepared { get; set; }

        private static Button modListButton = new Button(180, 600, 450, 40, "Pathfinder Mod List", MainMenu.buttonColor)
        { DrawFinish = (r) => GameHandled &= !r.JustReleased };

        private static Button returnButton = new Button(180, 650, 250, 28, "Return", MainMenu.exitButtonColor)
        { DrawFinish = (r) => GameHandled |= r.JustReleased };

        private static Dictionary<string, Button> LoadButtons { get; } = new Dictionary<string, Button>();
        private static Dictionary<string, Button> UnloadButtons { get; } = new Dictionary<string, Button>();

        private static void PrepareButtons()
        {
            if (ButtonsWerePrepared)
                return;
            var modIds = Pathfinder.LoadedModIdentifiers;
            modIds.AddRange(Pathfinder.UnloadedModIdentifiers);
            UnloadButtons.Clear();
            LoadButtons.Clear();
            foreach (var id in modIds)
            {
                var mod = Manager.GetLoadedMod(id);
                var loc = mod.GetType().Assembly.Location;
                UnloadButtons[id] = new Button(-1, -1, 100, 30, "Unload")
                {
                    DrawFinish = r => { if (r.JustReleased) Manager.UnloadMod(mod); }
                };
                LoadButtons[id] = new Button(-1, -1, 100, 30, "Load")
                {
                    DrawFinish = r =>
                    {
                        if (r.JustReleased)
                        {
                            var modType = mod.GetType();
                            try
                            {
                                var loadedMod = Manager.LoadMod(modType);
                                Manager.CurrentMod = loadedMod;
                                loadedMod?.LoadContent();
                                Manager.CurrentMod = null;
                            }
                            catch (Exception e)
                            {
                                Logger.Error("Mod '{0}' of file '{1}' failed to load:\n\t{2}",
                                             modType.FullName,
                                             Path.GetFileName(modType.Assembly.Location),
                                             e);
                            }
                        }
                    }
                };
            }
            ButtonsWerePrepared = true;
        }

        public static void DrawModList(DrawMainMenuEvent e)
        {
            if (GameHandled)
                return;
            e.IsCancelled = true;

            PrepareButtons();
            GameScreen baseS = e.MainMenu;
            returnButton.Draw();
            Gui.TextItem.doFontLabel(new Vector2(125f, 50), "Pathfinder Mod Load Order", GuiData.font, Color.White);

            var yPos = 120f;
            var index = 0;
            string title = null;
            foreach (var id in Pathfinder.LoadedModIdentifiers)
            {
                UnloadButtons[id].Position = new Vector2(500, yPos);
                UnloadButtons[id].Draw();
                title = Manager.GetLoadedMod(id).GetType().GetFirstAttribute<TitleAttribute>()?.Title;
                Gui.TextItem.doFontLabel(new Vector2(200f, yPos),
                                         (++index) + ". " + (title != null ? title + " | " : "") + id,
                                         GuiData.smallfont,
                                         Color.White);
                yPos += 30;
            }

            if (Pathfinder.UnloadedModIdentifiers.Count < 1)
                return;
            Gui.TextItem.doFontLabel(new Vector2(200, yPos), "Disabled Mods", GuiData.font, Color.White);
            yPos += 50;
            index = 0;
            foreach (var id in Pathfinder.UnloadedModIdentifiers)
            {
                LoadButtons[id].Position = new Vector2(500, yPos);
                LoadButtons[id].Draw();
                Gui.TextItem.doFontLabel(new Vector2(200f, yPos), (++index) + ". " + id, GuiData.smallfont, Color.White);
                yPos += 30;
            }
        }

        public static void DrawModListButton(DrawMainMenuButtonsEvent e) => modListButton.Draw();
    }
}
