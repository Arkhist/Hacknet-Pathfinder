using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Hacknet;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.GameFilesystem;
using Pathfinder.GUI;
using Pathfinder.ModManager;
using Pathfinder.Util;
using MainTitleData = Pathfinder.Event.DrawMainMenuTitlesEvent.TitleData<int>;
using SubTitleData = Pathfinder.Event.DrawMainMenuTitlesEvent.TitleData<float>;

namespace Pathfinder
{
    /// <summary>
    /// Function hooks for the Pathfinder mod system
    /// </summary>
    /// Place all functions to be hooked into Hacknet here
    public static class PathfinderHooks
    {
        public static bool onMain(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    if (args[i] == "-logIgnore" && args.Length > i + 1)
                        Logger.RemoveFlag((Logger.LogLevel)Enum.Parse(typeof(Logger.LogLevel), args[i + 1].ToUpper()));
                    if (args[i] == "-log" && args.Length > i + 1)
                        Logger.AddFlag((Logger.LogLevel)Enum.Parse(typeof(Logger.LogLevel), args[i + 1].ToUpper()));
                }
                catch (Exception e)
                {
                    Logger.Error("Could not do {0}, value {1}: ", args[i], args.Length > i + 1 ? args[i + 1] : "null", e);
                }
            }
            Logger.Verbose("Initializing Pathfinder");
            Pathfinder.Initialize();
            var startUpEvent = new Event.StartUpEvent(args);
            startUpEvent.CallEvent();
            if (startUpEvent.IsCancelled)
                return true;
            return false;
        }

        //  Hook location : Game1.LoadContent()
        //  if (this.CanLoadContent)
        //  {
        //	    <HOOK HERE>
        //      PortExploits.populate();
        public static void onLoadContent(Game1 self)
        {
            Logger.Verbose("Loading Pathfinder content");
            Manager.LoadModContent();
            var loadContentEvent = new Event.GameLoadContentEvent(self);
            loadContentEvent.CallEvent();
        }

        // Hook location : ProgramRunner.ExecuteProgram()
        public static bool onCommandSent(out bool disconnects, ref bool returnFlag, object osObj, string[] arguments)
        {
            var os = osObj as OS;
            var commandSentEvent = new Event.CommandSentEvent(os, arguments)
            {
                Disconnects = true
            };
            commandSentEvent.CallEvent();
            disconnects = commandSentEvent.Disconnects;
            returnFlag = disconnects;
            if (commandSentEvent.IsCancelled)
            {
                if (os.commandInvalid)
                    os.commandInvalid = false;
                else if (commandSentEvent.StateChange != CommandDisplayStateChange.None)
                {
                    var state = commandSentEvent.State;
                    os.display.command = state;
                    os.display.commandArgs =
                        (state + " " + string.Join(" ", commandSentEvent.Arguments.Skip(1).ToArray())).Split(' ');
                    os.display.typeChanged();
                }
                return true;
            }
            disconnects = false;
            return false;
        }

        public static bool onOSDraw(OS self, ref GameTime time)
        {
            var osStartDrawEvent = new Event.OSStartDrawEvent(self, time);
            Event.OSEndDrawEvent osEndDrawEvent = null;
            try
            {
                if (self.lastGameTime == null) self.lastGameTime = time;
                switch (osStartDrawEvent.DrawType)
                {
                    case Event.OSDrawEvent.Type.Standard:
                        PostProcessor.begin();
                        GuiData.startDraw();
                        osStartDrawEvent.CallEvent();
                        if (osStartDrawEvent.IsCancelled) break;
                        try
                        {
                            if (!self.TraceDangerSequence.PreventOSRendering)
                            {
                                self.drawBackground();
                                if (self.terminalOnlyMode) self.terminal.Draw((float)time.ElapsedGameTime.TotalSeconds);
                                else self.drawModules(time);
                                SFX.Draw(GuiData.spriteBatch);
                            }
                            if (self.TraceDangerSequence.IsActive) self.TraceDangerSequence.Draw();
                        }
                        catch (Exception ex)
                        {

                            self.drawErrorCount++;
                            if (self.drawErrorCount < 5)
                                Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex) + "\r\n\r\n");
                        }
                        break;
                    case Event.OSDrawEvent.Type.BootingSequence:
                        osStartDrawEvent.IgnoreScanlines = true;
                        goto case Event.OSDrawEvent.Type.EndingSequence;
                    case Event.OSDrawEvent.Type.BootAssistance:
                    case Event.OSDrawEvent.Type.EndingSequence:
                        PostProcessor.begin();
                        osStartDrawEvent.CallEvent();
                        if (osStartDrawEvent.IsCancelled) break;
                        self.ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        switch (osStartDrawEvent.DrawType)
                        {
                            case Event.OSDrawEvent.Type.BootAssistance:
                                self.BootAssitanceModule.Draw((float)time.ElapsedGameTime.TotalSeconds);
                                break;
                            case Event.OSDrawEvent.Type.BootingSequence:
                                if (self.thisComputer.disabled)
                                {
                                    self.RequestRemovalOfAllPopups();
                                    if (self.TraceDangerSequence.IsActive)
                                        self.TraceDangerSequence.CancelTraceDangerSequence();
                                    self.crashModule.Draw((float)time.ElapsedGameTime.TotalSeconds);
                                }
                                else self.introTextModule.Draw((float)time.ElapsedGameTime.TotalSeconds);
                                break;
                            case Event.OSDrawEvent.Type.EndingSequence:
                                self.endingSequence.Draw((float)time.ElapsedGameTime.TotalSeconds);
                                break;

                        }
                        if(!osStartDrawEvent.IgnoreScanlines) self.drawScanlines();
                        break;
                    case Event.OSDrawEvent.Type.Loading:
                        GuiData.startDraw();
                        osStartDrawEvent.CallEvent();
                        if (osStartDrawEvent.IsCancelled) break;
                        TextItem.doSmallLabel(new Vector2(0f, 700f), LocaleTerms.Loc("Loading..."), null);
                        break;
                    case Event.OSDrawEvent.Type.Custom:
                        osStartDrawEvent.CallEvent();
                        if (osStartDrawEvent.IsCancelled) return true;
                        break;
                }
                osEndDrawEvent = new Event.OSEndDrawEvent(self, time, osStartDrawEvent.DrawType);
                switch (osEndDrawEvent.DrawType)
                {
                    case Event.OSDrawEvent.Type.Standard:
                        GuiData.endDraw();
                        PostProcessor.end();
                        if (!osStartDrawEvent.IgnorePostFXDraw)
                        {
                            GuiData.startDraw();
                            if (self.postFXDrawActions != null)
                            {
                                self.postFXDrawActions.Invoke();
                                self.postFXDrawActions = null;
                            }
                            if (!osStartDrawEvent.IgnoreScanlines) self.drawScanlines();
                            GuiData.endDraw();
                        }
                        break;
                    case Event.OSDrawEvent.Type.BootAssistance:
                    case Event.OSDrawEvent.Type.BootingSequence:
                    case Event.OSDrawEvent.Type.EndingSequence:
                        self.ScreenManager.SpriteBatch.End();
                        PostProcessor.end();
                        break;
                    case Event.OSDrawEvent.Type.Loading:
                        GuiData.endDraw();
                        break;
                    default:
                        osEndDrawEvent.CallEvent();
                        break;
                }
            }
            catch (Exception ex)
            {
                osEndDrawEvent = new Event.OSEndDrawEvent(self, time, Event.OSDrawEvent.Type.Error);
                osEndDrawEvent.CallEvent();
                if (osEndDrawEvent.IsCancelled)
                    return true;
                self.drawErrorCount++;
                if (self.drawErrorCount >= 3) self.handleDrawError();
                else Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex));
            }
            return true;
        }

        // Hook location : OS.LoadContent()
        public static bool onLoadSession(OS self)
        {
            var loadSessionEvent = new Event.OSLoadContentEvent(self);
            loadSessionEvent.CallEvent();
            if (loadSessionEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : end of OS.LoadContent()
        public static void onPostLoadSession(OS self)
        {
            var postLoadSessionEvent = new Event.OSPostLoadContentEvent(self);
            postLoadSessionEvent.CallEvent();
        }

        public static void onUnloadSession(OS self)
        {
            var unloadSessionEvent = new Event.OSUnloadContentEvent(self);
            unloadSessionEvent.CallEvent();
        }

        // Hook location : MainMenu.Draw()
        public static bool onMainMenuDraw(MainMenu self, GameTime gameTime)
        {
            var drawMainMenuEvent = new Event.DrawMainMenuEvent(self, gameTime);
            drawMainMenuEvent.CallEvent();
            if (drawMainMenuEvent.IsCancelled)
            {
                GuiData.endDraw();
                PostProcessor.end();
                self.ScreenManager.FadeBackBufferToBlack(255 - self.TransitionAlpha);
                return true;
            }
            return false;
        }

        // Hook location : MainMenu.drawMainMenuButtons()
        public static void onMainMenuButtonsDraw(MainMenu self, ref int mainButtonY, ref int secondaryButtonY)
        {
            var drawMainMenuButtonsEvent = new Event.DrawMainMenuButtonsEvent(self, mainButtonY, secondaryButtonY);
            drawMainMenuButtonsEvent.CallEvent();
            mainButtonY = drawMainMenuButtonsEvent.MainButtonY;
            secondaryButtonY = drawMainMenuButtonsEvent.SecondaryButtonY;
        }

        // Hook location : OS.loadSaveFile()
        public static bool onLoadSaveFile(OS self, ref Stream stream, ref XmlReader xmlReader)
        {
            var loadSaveFileEvent = new Event.OSLoadSaveFileEvent(self, xmlReader, stream);
            loadSaveFileEvent.CallEvent();
            if (loadSaveFileEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : OS.writeSaveGame()
        public static bool onSaveFile(OS self, string filename)
        {
            var saveFileEvent = new Event.OSSaveFileEvent(self, filename);
            saveFileEvent.CallEvent();
            if (saveFileEvent.IsCancelled)
                return true;
            return false;
        }

        public static void onSaveWrite(OS self, ref string saveString, string filename)
        {
            var saveWriteEvent = new Event.OSSaveWriteEvent(self, filename, saveString);
            saveWriteEvent.CallEvent();
            saveString = saveWriteEvent.SaveString;
        }

        // Hook location : NetworkMap.LoadContent()
        public static bool onLoadNetmapContent(NetworkMap self)
        {
            var loadNetmapContentEvent = new Event.NetworkMapLoadContentEvent(self);
            loadNetmapContentEvent.CallEvent();
            if (loadNetmapContentEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : ProgramRunner.AttemptExeProgramExecution
        public static bool onExecutableExecute(out int result,
                                               ref Computer com,
                                               ref Folder fol,
                                               ref int finde,
                                               ref string exeFileData,
                                               ref OS os,
                                               ref string[] args)
        {
            GameFilesystem.File f = null;
            if (finde >= 0)
                f = os.thisComputer.GetFilesystem().Directory.FindDirectory(fol.name).GetFile(finde);

            var executableExecuteEvent =
                new Event.ExecutableExecuteEvent(com, fol, finde, f, os, args);
            executableExecuteEvent.CallEvent();
            result = (int)executableExecuteEvent.Result;
            if (executableExecuteEvent.IsCancelled || result != -1)
                return true;
            return false;
        }

        // Hook location : OS.launchExecutable
        public static bool onPortExecutableExecute(OS self,
                                                     ref Rectangle dest,
                                                     ref string name,
                                                     ref string data,
                                                     ref int port,
                                                     ref string[] args,
                                                     ref string originalName)
        {
            var portExecutableExecuteEvent =
                new Event.ExecutablePortExecuteEvent(self, dest, name, data, port, args);
            portExecutableExecuteEvent.CallEvent();
            name = portExecutableExecuteEvent.ExecutableName;
            data = portExecutableExecuteEvent.ExecutableData;
            if (portExecutableExecuteEvent.IsCancelled)
                return true;
            return false;
        }

        // createdComputer is from the hidden compiler generated value at Hacknet.ComputerLoader/'<>c__DisplayClass4'::c
        // its an irregular addition via injection
        public static void onLoadComputer(ref XmlReader reader,
                                          string filename,
                                          bool preventAddingToNetmap,
                                          bool preventInitDaemons,
                                          Computer createdComputer)
        {
            var loadComputerEvent = new Event.LoadComputerXmlReadEvent(createdComputer,
                                                                reader,
                                                                filename,
                                                                preventAddingToNetmap,
                                                                preventInitDaemons, 
                                                                false);
            loadComputerEvent.CallEvent();
        }

        public static void onLoadSaveComputer(ref Computer c,
                                              ref XmlReader reader,
                                              ref OS os)
        {
            var loadComputerEvent = new Event.LoadComputerXmlReadEvent(c,
                                                                reader,
                                                                "",
                                                                false,
                                                                false,
                                                                true);
            loadComputerEvent.CallEvent();
        }

        static Color defaultTitleColor = new Color(190, 190, 190, 0);
        static SpriteFont defaultTitleFont;

        public static bool onDrawMainMenuTitles(MainMenu self, out bool result, ref Rectangle dest)
        {
            if (defaultTitleFont == null) defaultTitleFont = self.ScreenManager.Game.Content.Load<SpriteFont>("Kremlin");
            Logger.Verbose("Redrawing Main Menu Titles");
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var mainTitle = "HACKNET";
            var subtitle = "OS"
                + (DLC1SessionUpgrader.HasDLC1Installed ? "+Labyrinths " : " ")
                + MainMenu.OSVersion + " Pathfinder " + version;
            result = true;
            if (Settings.IsExpireLocked)
            {
                TimeSpan timeSpan = Settings.ExpireTime - DateTime.Now;
                string text3;
                if (timeSpan.TotalSeconds < 1.0)
                {
                    text3 = LocaleTerms.Loc("TEST BUILD EXPIRED - EXECUTION DISABLED");
                    result = false;
                }
                else
                {
                    text3 = "Test Build : Expires in " + timeSpan.ToString();
                }
                TextItem.doFontLabel(new Vector2(180f, 105f), text3, GuiData.smallfont, Color.Red * 0.8f, 600f, 26f, false);
            }
            var main = new MainTitleData(mainTitle,
                                         defaultTitleColor,
                                         defaultTitleFont,
                                         dest
                                        );
            var sub = new SubTitleData(subtitle,
                                       main.Color * 0.5f,
                                       GuiData.smallfont,
                                       new Vector4(520, 178, 0, 0)
                                      );
            var drawMainMenuTitles = new Event.DrawMainMenuTitlesEvent(self, main, sub);
            drawMainMenuTitles.CallEvent();
            if (drawMainMenuTitles.IsCancelled)
                return true;
            main = drawMainMenuTitles.Main;
            sub = drawMainMenuTitles.Sub;
            FlickeringTextEffect.DrawLinedFlickeringText(
                dest = main.Destination,
                main.Title,
                7f,
                0.55f,
                main.Font,
                null,
                main.Color
            );
            TextItem.doFontLabel(sub.Destination, sub.Title, sub.Font, sub.Color, 600f, 26f);
            Logger.Verbose("Finished Redrawing Main Menu Titles");
            return true;
        }

        public static void onGameUnloadContent(Game1 self)
        {
            var gameUnloadEvent = new Event.GameUnloadEvent(self);
            gameUnloadEvent.CallEvent();
        }

        public static void onGameUpdate(Game1 self, ref GameTime time)
        {
            var gameUpdateEvent = new Event.GameUpdateEvent(self, time);
            gameUpdateEvent.CallEvent();
        }

        public static void onPortNameDraw(DisplayModule self,
                                            ref Rectangle rect,
                                            ref Computer computer,
                                            ref Vector2 lockPos)
        {
            var leftMeasure = Vector2.Zero;
            if (Port.Instance.compToInst.ContainsKey(computer)) foreach (var i in Port.Instance.compToInst[computer])
                {
                    rect.Y = self.y + 4;
                    lockPos.Y = rect.Y + 4;
                    self.spriteBatch.Draw(Utils.white, rect, i.Unlocked ? self.os.unlockedColor : self.os.lockedColor);
                    self.spriteBatch.Draw(i.Unlocked ? self.openLockSprite : self.lockSprite, lockPos, Color.White);
                    var portLeft = "Port#: " + i.Port.PortDisplay;
                    leftMeasure = GuiData.font.MeasureString(portLeft);
                    self.spriteBatch.DrawString(GuiData.font, portLeft, new Vector2(self.x, self.y + 3), Color.White);
                    var portRight = " - " + i.Port.PortName;
                    var rightMeasure = GuiData.smallfont.MeasureString(portRight);
                    var width = rect.Width - leftMeasure.X - 50f;
                    var single1 = Math.Min(1f, width / rightMeasure.X);
                    self.spriteBatch.DrawString(GuiData.smallfont, portRight, new Vector2(self.x + leftMeasure.X, self.y + 4),
                                                Color.White, 0f, Vector2.Zero, single1, SpriteEffects.None, 0.8f);
                    self.y += 45;
                }
        }

        public static bool onDisplayModuleUpdate(DisplayModule self, ref float time)
        {
            var displayModuleUpdateEvent = new Event.DisplayModuleUpdateEvent(self, time);
            displayModuleUpdateEvent.CallEvent();
            if (displayModuleUpdateEvent.IsCancelled)
                return true;
            return false;
        }

        public static bool onDisplayModuleDraw(DisplayModule self, ref float time)
        {
            var displayModuleDrawEvent = new Event.DisplayModuleDrawEvent(self, time);
            displayModuleDrawEvent.CallEvent();
            if (displayModuleDrawEvent.IsCancelled)
                return true;
            return false;
        }

        public static bool onExtensionsMenuScreenDraw(Hacknet.Screens.ExtensionsMenuScreen self,
                                                      ref Vector2 buttonPos,
                                                      ref Rectangle dest,
                                                      ref SpriteBatch sb,
                                                      ref ScreenManager manager)
        {
            var drawExtensionMenuEvent = new Event.DrawExtensionMenuEvent(self, dest, sb, manager, buttonPos);
            drawExtensionMenuEvent.CallEvent();
            if (drawExtensionMenuEvent.IsCancelled)
                return true;
            return false;
        }

        public static bool onExtensionsMenuListDraw(Hacknet.Screens.ExtensionsMenuScreen self,
                                                    out Vector2 result,
                                                    ref Vector2 drawPos,
                                                    ref Rectangle dest,
                                                    ref SpriteBatch sb)
        {
            var drawExtensionMenuListEvent = new Event.DrawExtensionMenuListEvent(self, drawPos, dest, sb);
            drawExtensionMenuListEvent.CallEvent();
            drawPos = drawExtensionMenuListEvent.ButtonPosition;
            result = drawPos;
            if (drawExtensionMenuListEvent.IsCancelled)
                return true;
            return false;
        }

        public static bool onOptionsMenuDraw(OptionsMenu self, ref GameTime time)
        {
            var optionsMenuDrawEvent = new Event.OptionsMenuDrawEvent(self, time);
            optionsMenuDrawEvent.CallEvent();
            if (optionsMenuDrawEvent.IsCancelled)
            {
                GuiData.endDraw();
                PostProcessor.end();
                return true;
            }
            return false;
        }

        public static void onOptionsMenuLoadContent(OptionsMenu self)
        {
            var optionsMenuLoadContentEvent = new Event.OptionsMenuLoadContentEvent(self);
            optionsMenuLoadContentEvent.CallEvent();
        }

        public static bool onOptionsMenuUpdate(OptionsMenu self, ref GameTime time, ref bool notFocused, ref bool isCovered)
        {
            var optionsMenuUpdateEvent = new Event.OptionsMenuUpdateEvent(self, time, notFocused, isCovered);
            optionsMenuUpdateEvent.CallEvent();
            if (optionsMenuUpdateEvent.IsCancelled)
                return true;
            return false;
        }

        public static void onOptionsApply(OptionsMenu self)
        {
            var optionsMenuApplyEvent = new Event.OptionsMenuApplyEvent(self);
            optionsMenuApplyEvent.CallEvent();
        }


        public static void onAddSerializableConditions(ref Dictionary<string, Func<XmlReader, SerializableCondition>> dict)
        {
            // HACKNET BUG FIX : DoesNotHaveFlags not in dictionary
            dict.Add("DoesNotHaveFlags", new Func<XmlReader, SerializableCondition>(SCDoesNotHaveFlags.DeserializeFromReader));

            var deserializers = Actions.SerializableCondition.ConditionHandler.GetDeserializers();

            foreach(KeyValuePair<string, Actions.SerializableCondition.ConditionHandler.Deserializer> pair in deserializers)
                dict.Add(pair.Key, new Func<XmlReader, SerializableCondition>(pair.Value));
        }

        public static void onAddSerializableActions(ref Dictionary<string, Func<XmlReader, SerializableAction>> dict)
        {
            var deserializers = Actions.SerializableAction.ActionHandler.GetDeserializers();

            foreach (KeyValuePair<string, Actions.SerializableAction.ActionHandler.Deserializer> pair in deserializers)
                dict.Add(pair.Key, new Func<XmlReader, SerializableAction>(pair.Value));
        }
    }
}
