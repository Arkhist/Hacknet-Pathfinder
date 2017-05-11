using System;
using Hacknet;
using Hacknet.Effects;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Event;
using Pathfinder.GUI;
using Pathfinder.Util;
using EMSState = Hacknet.Screens.ExtensionsMenuScreen.EMSState;
using Gui = Hacknet.Gui;

namespace Pathfinder.Internal.GUI
{
    static class ModExtensionsUI
    {
        private static Button returnButton = new Button(-1, -1, 450, 25, "Return to Main Menu", MainMenu.exitButtonColor)
        {
            DrawFinish = r => { Extension.Handler.ActiveInfo = null; buttonsLoaded = false; }
        };
        private static EMSState state;
        private static int tickWaits = -2;
        private static bool buttonsLoaded;
        private static bool modHandled;

        private static void LoadButtons(ref Vector2 pos, ExtensionsMenuScreen ms)
        {
            foreach (var pair in Extension.Handler.idToButton)
            {
                pair.Value.X = (int)pos.X;
                pair.Value.Y = (int)pos.Y;
                pair.Value.DrawFinish = r =>
                {
                    Extension.Handler.ActiveInfo = Extension.Handler.idToInfo[pair.Key];
                    ms.ReportOverride = null;
                    ms.SaveScreen.ProjectName = Extension.Handler.ActiveInfo.Name;
                    SaveFileManager.Init();
                    ms.SaveScreen.ResetForNewAccount();
                    modHandled = true;
                };
                pos = new Vector2(pos.X, pos.Y + 55);
            }
            buttonsLoaded = true;
        }

        private static void DrawModExtensionInfo(SpriteBatch sb, ref Vector2 pos, Rectangle rect, ExtensionsMenuScreen ms)
        {
            if (Extension.Handler.ActiveInfo == null) return;
            sb.DrawString(GuiData.titlefont, Extension.Handler.ActiveInfo.Name.ToUpper(), pos, Utils.AddativeWhite * 0.66f);
            pos.Y += 80;
            var height = sb.GraphicsDevice.Viewport.Height;
            var num = 256;
            if (height < 900)
                num = 120;
            var dest2 = new Rectangle((int)pos.X, (int)pos.Y, num, num);
            var texture = ms.DefaultModImage;
            if (Extension.Handler.idToLogo[Extension.Handler.ActiveInfo.Id] != null)
                texture = Extension.Handler.idToLogo[Extension.Handler.ActiveInfo.Id];
            FlickeringTextEffect.DrawFlickeringSprite(sb, dest2, texture, 2f, 0.5f, null, Color.White);
            var position = pos + new Vector2(num + 40f, 20f);
            var num2 = rect.Width - (pos.X - rect.X);
            var description = Extension.Handler.ActiveInfo.Description;
            var text = Utils.SuperSmartTwimForWidth(description, (int)num2, GuiData.smallfont);
            sb.DrawString(GuiData.smallfont, text, position, Utils.AddativeWhite * 0.7f);
            pos = new Vector2(pos.X, pos.Y + num + 10);
            if (ms.IsInPublishScreen)
            {
                sb.DrawString(GuiData.font, "Mod Extensions don't support publishment on the workshop", new Vector2(300), Utils.AddativeWhite);
                if (tickWaits < -1)
                    tickWaits = 10000;
                else if (tickWaits > -1)
                    --tickWaits;
                else
                {
                    ms.IsInPublishScreen = false;
                    tickWaits = -2;
                }
            }
            else
            {
                if (ms.ReportOverride != null)
                {
                    var text2 = Utils.SuperSmartTwimForWidth(ms.ReportOverride, 800, GuiData.smallfont);
                    sb.DrawString(GuiData.smallfont, text2, pos + new Vector2(460f, 0f),
                                  (ms.ReportOverride.Length > 250) ? Utils.AddativeRed : Utils.AddativeWhite);
                }
                int num3 = 40;
                int num4 = 5;
                int num5 = Extension.Handler.ActiveInfo.AllowSaves ? 4 : 2;
                int num6 = height - (int)pos.Y - 55;
                num3 = Math.Min(num3, (num6 - num5 * num4) / num5);
                if (Gui.Button.doButton(7900010, (int)pos.X, (int)pos.Y, 450, num3, "New " + Extension.Handler.ActiveInfo.Name + " Account",
                                    MainMenu.buttonColor))
                {
                    state = EMSState.GetUsername;
                    ms.SaveScreen.ResetForNewAccount();
                }
                pos.Y += num3 + num4;
                if (Extension.Handler.ActiveInfo.AllowSaves)
                {
                    bool flag = !string.IsNullOrWhiteSpace(SaveFileManager.LastLoggedInUser.FileUsername);
                    if (Gui.Button.doButton(7900019, (int)pos.X, (int)pos.Y, 450, num3,
                                        flag ? ("Continue Account : " + SaveFileManager.LastLoggedInUser.Username) : " - No Accounts - ",
                                        flag ? MainMenu.buttonColor : Color.Black))
                    {
                        Hacknet.OS.WillLoadSave = true;
                        if (ms.LoadAccountForExtension_FileAndUsername != null)
                            ms.LoadAccountForExtension_FileAndUsername.Invoke(SaveFileManager.LastLoggedInUser.FileUsername,
                                                                              SaveFileManager.LastLoggedInUser.Username);
                    }
                    pos.Y += num3 + num4;
                    if (Gui.Button.doButton(7900020, (int)pos.X, (int)pos.Y, 450, num3, "Login...", flag ? MainMenu.buttonColor : Color.Black))
                    {
                        state = EMSState.ShowAccounts;
                        ms.SaveScreen.ResetForLogin();
                    }
                    pos.Y += num3 + num4;
                }
                if (Gui.Button.doButton(7900030,
                                    (int)pos.X,
                                    (int)pos.Y,
                                    450,
                                    num3,
                                    "Run Verification Tests",
                                    MainMenu.buttonColor))
                    Logger.Info("Extension verification tests can not be performed on mod extensions");
                pos.Y += num3 + num4;
                if (Settings.AllowExtensionPublish && PlatformAPISettings.Running)
                {
                    ms.IsInPublishScreen |= Gui.Button.doButton(7900031, (int)pos.X, (int)pos.Y, 450, num3, "Steam Workshop Publishing",
                                                            MainMenu.buttonColor);
                    pos.Y += num3 + num4;
                }
                if (Gui.Button.doButton(7900040, (int)pos.X, (int)pos.Y, 450, 25, "Back to Extension List", MainMenu.exitButtonColor))
                {
                    ms.ExtensionInfoToShow = null;
                    Extension.Handler.ActiveInfo = null;
                }
                pos.Y += 30;
            }
        }

        private static void DrawUserScreenModExtensionInfo(SpriteBatch sb, Rectangle rect, ExtensionsMenuScreen ms) =>
            ms.SaveScreen.Draw(sb, new Rectangle(rect.X, rect.Y + rect.Height / 4, rect.Width, (int)(rect.Height * 0.8f)));

        internal static void ExtensionMenuListener(DrawExtensionMenuEvent e)
        {
            if (Extension.Handler.ActiveInfo == null && e.ExtensionMenuScreen.ExtensionInfoToShow == null)
            {
                modHandled = false;
                var v = e.ButtonPosition;
                e.IsCancelled = true;
                e.ButtonPosition = e.ExtensionMenuScreen.DrawExtensionList(e.ButtonPosition, e.Rectangle, e.SpriteBatch);
                returnButton.X = (int)e.ButtonPosition.X;
                returnButton.Y = (int)e.ButtonPosition.Y;
                if (returnButton.Draw())
                    e.ExtensionMenuScreen.ExitExtensionsScreen();
            }
            else if (modHandled)
            {
                e.IsCancelled = true;
                switch (state)
                {
                    case EMSState.Normal:
                        var pos = e.ButtonPosition;
                        DrawModExtensionInfo(e.SpriteBatch, ref pos, e.Rectangle, e.ExtensionMenuScreen);
                        e.ButtonPosition = pos;
                        break;
                    default:
                        DrawUserScreenModExtensionInfo(e.SpriteBatch, e.Rectangle, e.ExtensionMenuScreen);
                        break;
                }
            }
        }

        internal static void ExtensionListMenuListener(DrawExtensionMenuListEvent e)
        {
            var pos = e.ButtonPosition;
            if (Extension.Handler.idToButton.Count > 0 && !buttonsLoaded)
                LoadButtons(ref pos, e.ExtensionMenuScreen);
            e.ButtonPosition = pos;

            if (e.ExtensionMenuScreen.HasLoaded)
                foreach (var pair in Extension.Handler.idToButton)
                    pair.Value.Draw();
        }

        internal static void PostLoadForModExtensionsListener(OSPostLoadContenEvent e) => Extension.Handler.ActiveInfo?.Construct(e.OS);
    }
}
