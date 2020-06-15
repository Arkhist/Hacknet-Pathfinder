using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Attribute;
using Pathfinder.Event;
using Pathfinder.GUI;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Pathfinder.Util.Types;
using Gui = Hacknet.Gui;
using V2 = Microsoft.Xna.Framework.Vector2;

namespace Pathfinder.Internal.GUI
{
    static class ModList
    {
        private static bool GameHandled { get; set; } = true;
        private static bool ButtonsWerePrepared { get; set; }

        private static Button modListButton = new Button(180, 600, 450, 50, "Pathfinder Mod List", MainMenu.buttonColor)
        {
            SelectedColor = new Color(124, 137, 149),
            UpInputCallback = (sender) => GameHandled = false
        };
        private static Button returnButton = new Button(180, 650, 250, 28, "Return", MainMenu.exitButtonColor)
        {
            SelectedColor = Color.Gray,
            UpInputCallback = (sender) => GameHandled = true
        };

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
                    UpInputCallback = (sender) => Manager.MarkForUnload(mod)
                };
                LoadButtons[id] = new Button(-1, -1, 100, 30, "Load")
                {
                    UpInputCallback = (sender) => Manager.MarkForLoad(mod)
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
            returnButton.Draw();
            Gui.TextItem.doFontLabel(new V2(125f, 50), "Pathfinder Mod Load Order", GuiData.font, Color.White);

            var yPos = 120f;
            var index = 0;
            string title = null;
            foreach (var id in Manager.LoadedModIds)
            {
                title = Manager.GetLoadedMod(id).GetType().GetFirstAttribute<ModInfoAttribute>()?.PublicTitle;
                Gui.TextItem.doFontLabel(new V2(200f, yPos),
                                         (++index) + ". " + (title != null ? title + " | " : "") + id,
                                         GuiData.smallfont,
                                         Color.White);
                UnloadButtons[id].Position = new Vec2(500, yPos);
                UnloadButtons[id].Draw();
                yPos += 30;
            }
            Manager.UnloadMarkedMods();
            if (Manager.UnloadedModIds.Count < 1)
                return;
            Gui.TextItem.doFontLabel(new V2(200, yPos), "Disabled Mods", GuiData.font, Color.White);
            yPos += 50;
            index = 0;
            foreach (var id in Manager.UnloadedModIds)
            {
                Gui.TextItem.doFontLabel(new V2(200f, yPos), (++index) + ". " + id, GuiData.smallfont, Color.White);
                LoadButtons[id].Position = new Vec2(500, yPos);
                LoadButtons[id].Draw();
                yPos += 30;
            }
            Manager.LoadMarkedMods();
        }

        public static void DrawModListButton(DrawMainMenuButtonsEvent e)
        {
            modListButton.Rect.Y = e.MainButtonY;
            e.SecondaryButtonY = e.MainButtonY += 65;
            modListButton.Draw();
        }
    }
}
