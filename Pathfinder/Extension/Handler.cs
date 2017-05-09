using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using Hacknet.Effects;
using Hacknet.Gui;
using Hacknet.PlatformAPI.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Event;
using Pathfinder.Util;
using EMSState = Hacknet.Screens.ExtensionsMenuScreen.EMSState;

namespace Pathfinder.Extension
{
    public static class Handler
    {
        public static Info ActiveInfo { get; private set; }

        internal static Dictionary<string, Info> idToInfo = new Dictionary<string, Info>();
        private static Dictionary<string, Texture2D> idToLogo = new Dictionary<string, Texture2D>();
        private static Dictionary<string, GUI.Button> idToButton = new Dictionary<string, GUI.Button>();

        private static GUI.Button returnButton = new GUI.Button(-1, -1, 450, 25, "Return to Main Menu", MainMenu.exitButtonColor)
        {
            DrawFinish = r => { ActiveInfo = null; buttonsLoaded = false; }
        };
        private static EMSState state;
        private static int tickWaits = -2;
        private static bool buttonsLoaded;
        private static bool modHandled;

        public static string RegisterExtension(string id, Info extensionInfo)
        {
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to register extension {1} with id {2}",
                           Utility.GetPreviousStackFrameIdentity(),
                           extensionInfo.GetType().FullName,
                           id);
            if (idToInfo.ContainsKey(id))
                return null;

            extensionInfo.Id = id;
            idToInfo.Add(id, extensionInfo);
            Texture2D t = null;
            if (File.Exists(extensionInfo.LogoPath))
                using (var fs = File.OpenRead(extensionInfo.LogoPath))
                    t = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, fs);
            idToLogo.Add(id, t);
            idToButton.Add(id, new GUI.Button(-1, -1, 450, 50, extensionInfo.Name, Color.White));
            return id;
        }

        internal static bool UnregisterExtension(string id)
        {
            id = Utility.GetId(id);
            if (!idToInfo.ContainsKey(id))
                return true;

            var info = idToInfo[id];
            info.Id = null;
            idToLogo.Remove(id);
            idToButton.Remove(id);
            return idToInfo.Remove(id);
        }

        private static void LoadButtons(DrawExtensionMenuListEvent e)
        {
            foreach (var pair in idToButton)
            {
                pair.Value.X = (int)e.ButtonPosition.X;
                pair.Value.Y = (int)e.ButtonPosition.Y;
                pair.Value.DrawFinish = r =>
                {
                    ActiveInfo = idToInfo[pair.Key];
                    e.ExtensionMenuScreen.ReportOverride = null;
                    e.ExtensionMenuScreen.SaveScreen.ProjectName = ActiveInfo.Name;
                    SaveFileManager.Init();
                    e.ExtensionMenuScreen.SaveScreen.ResetForNewAccount();
                    modHandled = true;
                };
                e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + 55);
            }
            buttonsLoaded = true;
        }

        private static void DrawModExtensionInfo(DrawExtensionMenuEvent e)
        {
            if (ActiveInfo == null) return;
            e.SpriteBatch.DrawString(GuiData.titlefont, ActiveInfo.Name.ToUpper(), e.ButtonPosition, Utils.AddativeWhite * 0.66f);
            e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + 80);
            var height = e.SpriteBatch.GraphicsDevice.Viewport.Height;
            var num = 256;
            if (height < 900)
                num = 120;
            var dest2 = new Rectangle((int)e.ButtonPosition.X, (int)e.ButtonPosition.Y, num, num);
            var texture = e.ExtensionMenuScreen.DefaultModImage;
            if (idToLogo[ActiveInfo.Id] != null)
                texture = idToLogo[ActiveInfo.Id];
            FlickeringTextEffect.DrawFlickeringSprite(e.SpriteBatch, dest2, texture, 2f, 0.5f, null, Color.White);
            var position = e.ButtonPosition + new Vector2(num + 40f, 20f);
            var num2 = e.Rectangle.Width - (e.ButtonPosition.X - e.Rectangle.X);
            var description = ActiveInfo.Description;
            var text = Utils.SuperSmartTwimForWidth(description, (int)num2, GuiData.smallfont);
            e.SpriteBatch.DrawString(GuiData.smallfont, text, position, Utils.AddativeWhite * 0.7f);
            e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + num + 10);
            if (e.ExtensionMenuScreen.IsInPublishScreen)
            {
                e.SpriteBatch.DrawString(GuiData.font, "Mod Extensions don't support publishment on the workshop", new Vector2(300), Utils.AddativeWhite);
                if (tickWaits < -1)
                    tickWaits = 10000;
                else if (tickWaits > -1)
                    --tickWaits;
                else
                {
                    e.ExtensionMenuScreen.IsInPublishScreen = false;
                    tickWaits = -2;
                }
            }
            else
            {
                if (e.ExtensionMenuScreen.ReportOverride != null)
                {
                    var text2 = Utils.SuperSmartTwimForWidth(e.ExtensionMenuScreen.ReportOverride, 800, GuiData.smallfont);
                    e.SpriteBatch.DrawString(GuiData.smallfont, text2,
                                             e.ButtonPosition + new Vector2(460f, 0f),
                                             (e.ExtensionMenuScreen.ReportOverride.Length > 250) ?
                                             Utils.AddativeRed : Utils.AddativeWhite);
                }
                int num3 = 40;
                int num4 = 5;
                int num5 = ActiveInfo.AllowSaves ? 4 : 2;
                int num6 = height - (int)e.ButtonPosition.Y - 55;
                num3 = Math.Min(num3, (num6 - num5 * num4) / num5);
                if (Button.doButton(7900010,
                                    (int)e.ButtonPosition.X,
                                    (int)e.ButtonPosition.Y,
                                    450,
                                    num3,
                                    "New " + ActiveInfo.Name + " Account",
                                    MainMenu.buttonColor))
                {
                    state = EMSState.GetUsername;
                    e.ExtensionMenuScreen.SaveScreen.ResetForNewAccount();
                }
                e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + num3 + num4);
                if (ActiveInfo.AllowSaves)
                {
                    bool flag = !string.IsNullOrWhiteSpace(SaveFileManager.LastLoggedInUser.FileUsername);
                    if (Button.doButton(7900019,
                                        (int)e.ButtonPosition.X,
                                        (int)e.ButtonPosition.Y,
                                        450,
                                        num3,
                                        flag ? ("Continue Account : " + SaveFileManager.LastLoggedInUser.Username) : " - No Accounts - ",
                                        flag ? MainMenu.buttonColor : Color.Black))
                    {
                        Hacknet.OS.WillLoadSave = true;
                        if (e.ExtensionMenuScreen.LoadAccountForExtension_FileAndUsername != null)
                            e.ExtensionMenuScreen.LoadAccountForExtension_FileAndUsername.Invoke(SaveFileManager.LastLoggedInUser.FileUsername, SaveFileManager.LastLoggedInUser.Username);
                    }
                    e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + num3 + num4);
                    if (Button.doButton(7900020,
                                        (int)e.ButtonPosition.X,
                                        (int)e.ButtonPosition.Y,
                                        450,
                                        num3,
                                        "Login...",
                                        flag ? MainMenu.buttonColor : Color.Black))
                    {
                        state = EMSState.ShowAccounts;
                        e.ExtensionMenuScreen.SaveScreen.ResetForLogin();
                    }
                    e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + num3 + num4);
                }
                if (Button.doButton(7900030, 
                                    (int)e.ButtonPosition.X, 
                                    (int)e.ButtonPosition.Y, 
                                    450, 
                                    num3,
                                    "Run Verification Tests",
                                    MainMenu.buttonColor))
                    Logger.Info("Extension verification tests can not be performed on mod extensions");
                e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + num3 + num4);
                if (Settings.AllowExtensionPublish && PlatformAPISettings.Running)
                {
                    e.ExtensionMenuScreen.IsInPublishScreen |= Button.doButton(7900031,
                                                                               (int)e.ButtonPosition.X,
                                                                               (int)e.ButtonPosition.Y,
                                                                               450, num3, 
                                                                               "Steam Workshop Publishing",
                                                                               MainMenu.buttonColor);
                    e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + num3 + num4);
                }
                if (Button.doButton(7900040, 
                                    (int)e.ButtonPosition.X, 
                                    (int)e.ButtonPosition.Y, 
                                    450, 
                                    25, 
                                    "Back to Extension List",
                                    MainMenu.exitButtonColor))
                {
                    e.ExtensionMenuScreen.ExtensionInfoToShow = null;
                    ActiveInfo = null;
                }
                e.ButtonPosition = new Vector2(e.ButtonPosition.X, e.ButtonPosition.Y + 30);
            }
        }

        private static void DrawUserScreenModExtensionInfo(DrawExtensionMenuEvent e)
        {
            e.ExtensionMenuScreen.SaveScreen.Draw(e.SpriteBatch,
                                                  new Rectangle(e.Rectangle.X,
                                                                 e.Rectangle.Y + e.Rectangle.Height / 4,
                                                                 e.Rectangle.Width,
                                                                 (int)(e.Rectangle.Height * 0.8f)));
        }

        internal static void ExtensionMenuListener(DrawExtensionMenuEvent e)
        {
            if (ActiveInfo == null && e.ExtensionMenuScreen.ExtensionInfoToShow == null)
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
                        DrawModExtensionInfo(e);
                        break;
                    default:
                        DrawUserScreenModExtensionInfo(e);
                        break;
                }
            }
        }

        internal static void ExtensionListMenuListener(DrawExtensionMenuListEvent e)
        {
            if (idToButton.Count > 0 && !buttonsLoaded)
                LoadButtons(e);

            if (e.ExtensionMenuScreen.HasLoaded)
                foreach (var pair in idToButton)
                    pair.Value.Draw();
        }

        internal static void PostLoadForModExtensionsListener(OSPostLoadContenEvent e)
        {
            ActiveInfo?.Construct(e.OS);
        }
    }
}
