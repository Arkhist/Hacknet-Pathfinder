using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using Hacknet.Effects;
using Hacknet.Factions;
using Hacknet.Misc;
using Hacknet.Modules.Overlays;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Attribute;
using Pathfinder.Event;
using Pathfinder.GUI;
using Pathfinder.Util;
using Pathfinder.Util.Attribute;
using EMSState = Hacknet.Screens.ExtensionsMenuScreen.EMSState;
using Gui = Hacknet.Gui;
using Rect = Microsoft.Xna.Framework.Rectangle;
using V2 = Microsoft.Xna.Framework.Vector2;
using Pathfinder.Util.Types;

namespace Pathfinder.Internal.GUI
{
    static class ModExtensionsUI
    {
        private static Button returnButton = new Button(-1, -1, 450, 25, "Return to Main Menu", MainMenu.exitButtonColor);
        private static EMSState state = EMSState.Normal;
        private static int tickWaits = -2;
        private static bool buttonsLoaded;

        static ModExtensionsUI()
        {
            returnButton.OnUp += (sender) =>
            {
                Extension.Handler.ActiveInfo = null;
                buttonsLoaded = false;
            };
        }

        private static void LoadButtons(ref V2 pos, ExtensionsMenuScreen ms)
        {
            foreach (var pair in Extension.Handler.ModExtensions)
            {
                pair.Value.Item3.Position = (Vec2)pos;
                pair.Value.Item3.OnUp += (sender) => {
                    Extension.Handler.ActiveInfo = Extension.Handler.ModExtensions[pair.Key].Item1;
                    ms.ExtensionInfoToShow = new PlaceholderExtensionInfo(Extension.Handler.ActiveInfo);
                    Extension.Handler.ActiveExtension = ms.ExtensionInfoToShow;
                    ms.ReportOverride = null;
                    ms.SaveScreen.ProjectName = Extension.Handler.ActiveInfo.Name;
                    SaveFileManager.Init();
                    ms.SaveScreen.ResetForNewAccount();
                };
                pos.Y += 55;
            }
            buttonsLoaded = true;
        }

        private static void DrawModExtensionInfo(SpriteBatch sb, ref Vec2 pos, Rect2 rect, ExtensionsMenuScreen ms)
        {
            if (Extension.Handler.ActiveInfo == null) return;
            sb.DrawString(GuiData.titlefont, Extension.Handler.ActiveInfo.Name.ToUpper(), (V2)pos, Utils.AddativeWhite * 0.66f);
            pos.Y += 80;
            var height = sb.GraphicsDevice.Viewport.Height;
            var num = 256;
            if (height < 900)
                num = 120;
            var dest2 = new Rect2(pos.X, pos.Y, num, num);
            var texture = ms.DefaultModImage;
            if (Extension.Handler.ModExtensions[Extension.Handler.ActiveInfo.Id].Item2 != null)
                texture = Extension.Handler.ModExtensions[Extension.Handler.ActiveInfo.Id].Item2;
            FlickeringTextEffect.DrawFlickeringSpriteFull(sb, (V2)dest2.Position, 0f, (V2)dest2.Size, (V2)dest2.Center, texture, 0, 2, 0.5f, null, Color.White);
            var position = pos + new Vec2(num + 40f, 20f);
            var num2 = rect.Width - (pos.X - rect.X);
            var description = Extension.Handler.ActiveInfo.Description;
            var text = Utils.SuperSmartTwimForWidth(description, (int)num2, GuiData.smallfont);
            sb.DrawString(GuiData.smallfont, text, (V2)position, Utils.AddativeWhite * 0.7f);
            pos = new Vec2(pos.X, pos.Y + num + 10);
            if (ms.IsInPublishScreen)
            {
                sb.DrawString(GuiData.font, "Mod Extensions don't support publishment on the workshop", new V2(300), Utils.AddativeWhite);
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
                    sb.DrawString(GuiData.smallfont, text2, (V2)(pos + Vec2.Right * 460),
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
                        OS.WillLoadSave = true;
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

        private static void DrawUserScreenModExtensionInfo(MainMenu m, SpriteBatch sb, Rect2 rect, ExtensionsMenuScreen ms)
        {
            ms.CreateNewAccountForExtension_UserAndPass = (n, p) =>
            {
                OS.WillLoadSave = false;
                MainMenu.CreateNewAccountForExtensionAndStart(n, p, m.ScreenManager, m, ms);
            };
            ms.LoadAccountForExtension_FileAndUsername = (userFile, username) =>
            {
                m.ExitScreen();
                MainMenu.resetOS();
                OS.WillLoadSave = SaveFileManager.StorageMethods[0].FileExists(userFile);
                var o = new OS
                {
                    SaveGameUserName = userFile,
                    SaveUserAccountName = username
                };
                m.ScreenManager.AddScreen(o);
            };
            ms.SaveScreen.Draw(sb, new Rect2(rect.X, rect.Y + rect.Height / 4, rect.Width, rect.Height * 0.8f).Rectangle);
        }

        internal static void ExtensionMenuListener(DrawExtensionMenuEvent e)
        {
            if (Extension.Handler.ActiveInfo == null && e.ExtensionMenuScreen.ExtensionInfoToShow == null)
            {
                var v = e.ButtonPosition;
                e.IsCancelled = true;
                e.ButtonPosition = e.ExtensionMenuScreen.DrawExtensionList(e.ButtonPosition, e.Rectangle, e.SpriteBatch);
                returnButton.Position = (Vec2)e.ButtonPosition;
                returnButton.Draw();
                if (returnButton.JustReleased)
                    e.ExtensionMenuScreen.ExitExtensionsScreen();
            }
            else if (Extension.Handler.ActiveInfo != null)
            {
                e.IsCancelled = true;
                switch (state)
                {
                    case EMSState.Normal:
                        var pos = (Vec2)e.ButtonPosition;
                        DrawModExtensionInfo(e.SpriteBatch, ref pos, (Rect2)e.Rectangle, e.ExtensionMenuScreen);
                        e.ButtonPosition = (V2)pos;
                        break;
                    default:
                        MainMenu m = null;
                        foreach (var s in e.ScreenManager.GetScreens())
                        {
                            m = s as MainMenu;
                            if (m != null) break;
                        }
                        DrawUserScreenModExtensionInfo(m, e.SpriteBatch, (Rect2)e.Rectangle, e.ExtensionMenuScreen);
                        break;
                }
            }
        }

        internal static void ExtensionListMenuListener(DrawExtensionMenuListEvent e)
        {
            var pos = e.ButtonPosition;
            if (Extension.Handler.ModExtensions.Count > 0)
            {
                if (!buttonsLoaded)
                    LoadButtons(ref pos, e.ExtensionMenuScreen);
                pos.Y += 55;
            }
            e.ButtonPosition = pos;

            if (e.ExtensionMenuScreen.HasLoaded)
                foreach (var pair in Extension.Handler.ModExtensions)
                    pair.Value.Item3.Draw();
        }

        [IgnoreRegister]
        [Event(Priority = -100)]
        internal static void LoadContentForModExtensionListener(OSLoadContentEvent e)
        {
            if (Extension.Handler.ActiveInfo == null)
                return;
            e.IsCancelled = true;
            var os = e.OS;
            if (os.canRunContent)
            {
                // setup for proper game function
                Settings.slowOSStartup = false;
                Settings.initShowsTutorial = false;
                os.initShowsTutorial = false;
                // replacing OS.LoadContent
                os.delayer = new ActionDelayer();
                ComputerLoader.init(os);
                os.content = e.OS.ScreenManager.Game.Content;
                os.username = (os.SaveUserAccountName ?? (Settings.isConventionDemo ? Settings.ConventionLoginName : Environment.UserName));
                os.username = FileSanitiser.purifyStringForDisplay(os.username);
                var compLocation = new V2(0.1f, 0.5f);
                if (os.multiplayer && !os.isServer)
                    compLocation = new V2(0.8f, 0.8f);
                os.ramAvaliable = os.totalRam;
                os.thisComputer = new Computer(os.username + " PC", Utility.GenerateRandomIP(), compLocation, 5, 4, os);
                os.thisComputer.adminIP = os.thisComputer.ip;
                os.thisComputer.idName = "playerComp";
                os.thisComputer.Memory = new MemoryContents();
                os.GibsonIP = Utility.GenerateRandomIP();
                UserDetail value = os.thisComputer.users[0];
                value.known = true;
                os.thisComputer.users[0] = value;
                os.defaultUser = new UserDetail(os.username, "password", 1) { known = true };
                ThemeManager.setThemeOnComputer(os.thisComputer, OSTheme.HacknetBlue);
                if (os.multiplayer)
                {
                    os.thisComputer.addMultiplayerTargetFile();
                    os.sendMessage("newComp #" + os.thisComputer.ip + "#" + compLocation.X +
                                   "#" + compLocation.Y + "#" + 5 + "#" + os.thisComputer.name);
                    os.multiplayerMissionLoaded = false;
                }
                if (!OS.WillLoadSave)
                    People.init();
                os.modules = new List<Module>();
                os.exes = new List<ExeModule>();
                os.shells = new List<ShellExe>();
                os.shellIPs = new List<string>();
                Viewport viewport = os.ScreenManager.GraphicsDevice.Viewport;
                int num2 = 205;
                var num3 = (int)((viewport.Width - RamModule.MODULE_WIDTH - 6) * 0.44420000000000004);
                var num4 = (int)((viewport.Width - RamModule.MODULE_WIDTH - 6) * 0.5558);
                int height = viewport.Height - num2 - OS.TOP_BAR_HEIGHT - 6;
                os.terminal = new Terminal(new Rect(viewport.Width - 2 - num3,
                                                         OS.TOP_BAR_HEIGHT,
                                                         num3,
                                                         viewport.Height - OS.TOP_BAR_HEIGHT - 2), os)
                {
                    name = "TERMINAL"
                };
                os.modules.Add(os.terminal);
                os.netMap = new NetworkMap(new Rect(RamModule.MODULE_WIDTH + 4,
                                                         viewport.Height - num2 - 2,
                                                         num4 - 1,
                                                         num2), os)
                {
                    name = "netMap v1.7"
                };
                os.modules.Add(os.netMap);
                os.display = new DisplayModule(new Rect(RamModule.MODULE_WIDTH + 4,
                                                             OS.TOP_BAR_HEIGHT,
                                                             num4 - 2,
                                                             height), os)
                {
                    name = "DISPLAY"
                };
                os.modules.Add(os.display);
                os.ram = new RamModule(new Rect(2,
                                                     OS.TOP_BAR_HEIGHT,
                                                     RamModule.MODULE_WIDTH,
                                                     os.ramAvaliable + RamModule.contentStartOffset), os)
                {
                    name = "RAM"
                };
                os.modules.Add(os.ram);
                foreach (var m in os.modules.ToArray())
                    m.LoadContent();
                if (os.allFactions == null)
                {
                    os.allFactions = new AllFactions();
                    os.allFactions.init();
                }
                bool flag = false;
                if (!OS.WillLoadSave)
                {
                    os.netMap.nodes.Insert(0, os.thisComputer);
                    os.netMap.visibleNodes.Add(0);
                    MusicManager.loadAsCurrentSong(os.IsDLCConventionDemo ? "Music\\out_run_the_wolves" : "Music\\Revolve");
                    os.LanguageCreatedIn = Settings.ActiveLocale;
                }
                else
                {
                    os.loadSaveFile();
                    flag = true;
                    Settings.initShowsTutorial = false;
                    SaveFixHacks.FixSavesWithTerribleHacks(os);
                }
                var computer = new Computer("JMail Email Server", Utility.GenerateRandomIP(), new V2(0.8f, 0.2f), 6, 1, os)
                {
                    idName = "jmail"
                };
                computer.daemons.Add(new MailServer(computer, "JMail", os));
                MailServer.shouldGenerateJunk = false;
                computer.users.Add(new UserDetail(os.defaultUser.name, "mailpassword", 2));
                computer.initDaemons();
                os.netMap.nodes.Add(computer);
                os.netMap.mailServer = computer;
                os.topBar = new Rect(0, 0, viewport.Width, OS.TOP_BAR_HEIGHT - 1);
                os.crashModule = new CrashModule(new Rect(0, 0, os.ScreenManager.GraphicsDevice.Viewport.Width, os.ScreenManager.GraphicsDevice.Viewport.Height), os);
                os.crashModule.LoadContent();
                Settings.IsInExtensionMode = false; // little hack to prevent intro text module ctor from throwing nullref
                os.introTextModule = new IntroTextModule(new Rect(0, 0, os.ScreenManager.GraphicsDevice.Viewport.Width, os.ScreenManager.GraphicsDevice.Viewport.Height), os)
                {
                    complete = true,
                    text = new string[] { "" }
                };
                os.introTextModule.LoadContent();
                Settings.IsInExtensionMode = true; // hack end
                os.traceTracker = new TraceTracker(os);
                os.IncConnectionOverlay = new IncomingConnectionOverlay(os);
                os.scanLines = os.content.Load<Texture2D>("ScanLines");
                os.cross = os.content.Load<Texture2D>("Cross");
                os.cog = os.content.Load<Texture2D>("Cog");
                os.saveIcon = os.content.Load<Texture2D>("SaveIcon");
                os.beepSound = os.content.Load<SoundEffect>("SFX/beep");
                os.mailicon = new MailIcon(os, new V2(0f, 0f));
                os.mailicon.pos.X = viewport.Width - os.mailicon.getWidth() - 2;
                os.hubServerAlertsIcon = new HubServerAlertsIcon(os.content, "dhs",
                                                                 new string[] { "@channel", "@" + os.defaultUser.name }
                                                                );
                os.hubServerAlertsIcon.Init(os);
                if (os.HasLoadedDLCContent)
                    os.AircraftInfoOverlay = new AircraftInfoOverlay(os);
                SAChangeAlertIcon.UpdateAlertIcon(os);
                os.introTextModule.complete |= (flag || !Settings.slowOSStartup);
                if (!flag)
                    MusicManager.playSong();
                os.inputEnabled = true;
                os.isLoaded = true;
                os.fullscreen = new Rect(0, 0, os.ScreenManager.GraphicsDevice.Viewport.Width, os.ScreenManager.GraphicsDevice.Viewport.Height);
                os.TraceDangerSequence = new TraceDangerSequence(os.content, os.ScreenManager.SpriteBatch, os.fullscreen, os);
                os.endingSequence = new EndingSequenceModule(os.fullscreen, os);
                if (Settings.EnableDLC && DLC1SessionUpgrader.HasDLC1Installed)
                    os.BootAssitanceModule = new BootCrashAssistanceModule(os.fullscreen, os);
                if (Settings.EnableDLC && DLC1SessionUpgrader.HasDLC1Installed && HostileHackerBreakinSequence.IsInBlockingHostileFileState(os))
                {
                    os.rebootThisComputer();
                    os.BootAssitanceModule.ShouldSkipDialogueTypeout = true;
                }
                else
                {
                    if (Settings.EnableDLC && HostileHackerBreakinSequence.IsFirstSuccessfulBootAfterBlockingState(os))
                    {
                        HostileHackerBreakinSequence.ReactToFirstSuccesfulBoot(os);
                        os.rebootThisComputer();
                    }
                    if (!OS.TestingPassOnly)
                        os.runCommand("connect " + os.thisComputer.ip);
                    var folder2 = os.thisComputer.files.root.searchForFolder("sys");
                    if (folder2.searchForFile("Notes_Reopener.bat") != null)
                        os.runCommand("notes");
                }
                if (Settings.EnableDLC && DLC1SessionUpgrader.HasDLC1Installed && os.HasLoadedDLCContent)
                {
                    bool flag4 = false;
                    if (!os.Flags.HasFlag("AircraftInfoOverlayDeactivated"))
                    {
                        flag4 |= os.Flags.HasFlag("AircraftInfoOverlayActivated");
                        if (!flag4)
                        {
                            var c = Programs.getComputer(os, "dair_crash");
                            var folder3 = c.files.root.searchForFolder("FlightSystems");
                            bool flag5 = false;
                            for (int i = 0; i < folder3.files.Count; i++)
                                flag5 |= folder3.files[i].name == "747FlightOps.dll";
                            var aircraftDaemon = (AircraftDaemon)c.getDaemon(typeof(AircraftDaemon));
                            if (!flag5 && !os.Flags.HasFlag("DLC_PlaneResult"))
                                flag4 = true;
                        }
                    }
                    if (flag4)
                    {
                        var c = Programs.getComputer(os, "dair_crash");
                        var aircraftDaemon = (AircraftDaemon)c.getDaemon(typeof(AircraftDaemon));
                        aircraftDaemon.StartReloadFirmware();
                        aircraftDaemon.StartUpdating();
                        os.AircraftInfoOverlay.Activate();
                        os.AircraftInfoOverlay.IsMonitoringDLCEndingCases = true;
                        MissionFunctions.runCommand(0, "playAirlineCrashSongSequence");
                    }
                }
            }
            else if (os.multiplayer)
                os.initializeNetwork();
            os.Flags.AddFlag("FirstAlertComplete");
            os.IsInDLCMode = false;
            os.ShowDLCAlertsIcon = false;

            if (OS.WillLoadSave)
            {
                Stream stream = null;
                if (e.OS.ForceLoadOverrideStream != null && OS.TestingPassOnly)
                    stream = e.OS.ForceLoadOverrideStream;
                else
                    stream = SaveFileManager.GetSaveReadStream(e.OS.SaveGameUserName);
                Extension.Handler.LoadExtension(Extension.Handler.ActiveInfo.Id);
                Extension.Handler.ActiveInfo.OnLoad(e.OS, stream);
            }
            else
            {
                Extension.Handler.CanRegister = true;
                Extension.Handler.ActiveInfo.OnConstruct(e.OS);
                Extension.Handler.CanRegister = false;
            }
        }

        public static void UnloadContentForModExtensionListener(OSUnloadContentEvent e)
        {
            if (Extension.Handler.ActiveInfo != null)
            {
                Extension.Handler.UnloadExtension(Extension.Handler.ActiveInfo.Id);
                Extension.Handler.ActiveInfo.OnUnload(e.OS);
            }
        }
    }
}
