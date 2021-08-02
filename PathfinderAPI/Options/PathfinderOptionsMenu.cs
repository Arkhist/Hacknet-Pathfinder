using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Hacknet;
using BepInEx.Logging;
using Hacknet;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Gui;
using Hacknet.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pathfinder.Event;
using Pathfinder.Event.Options;
using Pathfinder.GUI;

namespace Pathfinder.Options
{
    [HarmonyPatch]
    internal static class PathfinderOptionsMenu
    {
        private static bool isInPathfinderMenu = false;
        private static string currentTabName = null;

        private static PFButton ReturnButton = new PFButton(10, 10, 220, 54, "Back to Options", Color.Yellow);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.Draw))]
        internal static bool Draw(ref OptionsMenu __instance, GameTime gameTime)
        {
            if (!isInPathfinderMenu) 
                return true;
        
            PostProcessor.begin();
			GuiData.startDraw();
			PatternDrawer.draw(new Rectangle(0, 0, __instance.ScreenManager.GraphicsDevice.Viewport.Width, __instance.ScreenManager.GraphicsDevice.Viewport.Height), 0.5f, Color.Black, new Color(2, 2, 2), GuiData.spriteBatch);
            
            if (ReturnButton.Do())
            {
                currentTabName = null;
                isInPathfinderMenu = false;
                GuiData.endDraw();
                PostProcessor.end();
                var saveEvent = new CustomOptionsSaveEvent();
                EventManager<CustomOptionsSaveEvent>.InvokeAll(saveEvent);
                return false;
            }

            var tabs = OptionsManager.Tabs;
            
            int tabX = 10;

            foreach (var tab in tabs.Values)
            {
                if (currentTabName == null)
                    currentTabName = tab.Name;
                var active = currentTabName == tab.Name;
                // Display tab button
                if (Button.doButton(tab.ButtonID, tabX, 70, 128, 20, tab.Name, active ? Color.Green : Color.Gray))
                {
                    currentTabName = tab.Name;
                    break;
                }
                tabX += 128 + 10;

                if (currentTabName != tab.Name)
                    continue;

                // Display options
                int optX = 80, optY = 110;
                foreach (var option in tab.Options)
                {
                    option.Draw(optX, optY);
                    optY += 10 + option.SizeY;
                }
            }

            GuiData.endDraw();
			PostProcessor.end();
            return false;
        }

        private static PFButton EnterButton = new PFButton(240, 10, 220, 54, "Pathfinder Options", Color.Yellow);

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.Draw))]
        internal static void BeforeEndDrawOptions(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            c.GotoNext(MoveType.AfterLabel, x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(GuiData), nameof(GuiData.endDraw))));

            c.EmitDelegate<System.Action>(() =>
            {
                if (EnterButton.Do())
                {
                    isInPathfinderMenu = true;
                }
            });
        }
    }
}
