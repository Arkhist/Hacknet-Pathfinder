using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Hacknet;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Attribute;
using Pathfinder.Exceptions;
using Pathfinder.Game;
using Pathfinder.GameFilesystem;
using Pathfinder.GUI;
using Pathfinder.Internal;
using Pathfinder.ModManager;
using Pathfinder.Util;
using static Pathfinder.Attribute.PatchAttribute;
using Pathfinder.Util.Types;
using static Pathfinder.Event.DrawMainMenuTitlesEvent;
using Pathfinder.Game.OS;

namespace Pathfinder
{
    /// <summary>
    /// Function hooks for the Pathfinder mod system
    /// </summary>
    /// Place all functions to be hooked into Hacknet here
    public static class PathfinderHooks
    {

        [Patch("Hacknet.Program.Main", flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn)]
        public static bool onMain(string[] args)
        {
            CmdArguments arguments = args;
            var log = arguments.GetAllArgumentsWithName("logIgnore");
            foreach (var logArg in log)
                try
                {
                    Logger.RemoveFlag((Logger.LogLevel)Enum.Parse(typeof(Logger.LogLevel), logArg.ToUpper()));
                }
                catch (Exception e)
                {
                    Logger.Error("Could not do {0}, value {1}: ", "logIgnore", logArg, e);
                }
            log = arguments.GetAllArgumentsWithName("log");
            foreach (var logArg in log)
                try
                {
                    Logger.AddFlag((Logger.LogLevel)Enum.Parse(typeof(Logger.LogLevel), logArg.ToUpper()));
                }
                catch (Exception e)
                {
                    Logger.Error("Could not do {0}, value {1}: ", "log", logArg, e);
                }
            var modDirectory = arguments["modDirectory"];
            if (modDirectory != null)
            {
                Logger.Info("Mod Directory redirected to {0}", modDirectory);
                Manager.ModFolderPath = modDirectory;
            }
            var depDirectory = arguments["depDirectory"];
            if (depDirectory != null)
            {
                Logger.Info("Mod Dependency Directory redirected to {0}", depDirectory);
                Manager.DepFolderPath = depDirectory;
            }
            Logger.Verbose("Initializing Pathfinder");
            Pathfinder.Initialize();
            var startUpEvent = new Event.StartUpEvent(args);
            startUpEvent.CallEvent();
            if (startUpEvent.IsCancelled)
                return true;
            return false;
        }

        //  if (this.CanLoadContent)
        //  {
        //	    <HOOK HERE>
        //      PortExploits.populate();
        [Patch("Hacknet.Game1.LoadContent", -1, flags: InjectFlags.PassInvokingInstance)]
        public static void onLoadContent(Game1 self)
        {
            Logger.Verbose("Loading Pathfinder content");
            Manager.LoadModContent();
            var loadContentEvent = new Event.GameLoadContentEvent(self);
            loadContentEvent.CallEvent();
        }

        private static Event.CommandSentEvent commandSentEvent;

        [Patch("Hacknet.ProgramRunner.ExecuteProgram", 13,
            flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
            localsID: new int[] { 1 }
        )]
        public static bool onCommandSent(out bool disconnects, ref bool returnFlag, object osObj, string[] arguments)
        {
            var os = osObj as OS;
            commandSentEvent = new Event.CommandSentEvent(os, arguments)
            {
                Disconnects = true
            };
            var exceptions = commandSentEvent.CallEvent();
            if(exceptions.Count > 0)
                foreach (var pair in exceptions)
                    os.Write("Command Listener Method '{0}' failed with: {1}", pair.Key, pair.Value);
            disconnects = commandSentEvent.Disconnects;
            returnFlag = disconnects;
            if (commandSentEvent.IsCancelled)
            {
                var commandFinishedEvent = new Event.CommandFinishedEvent(commandSentEvent);
                commandFinishedEvent.CallEvent();
                disconnects = commandFinishedEvent.SentEvent.Disconnects;
                returnFlag = disconnects;
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

        public static void onCommandFinished(out bool disconnects, ref bool returnFlag)
        {
            var commandFinishedEvent = new Event.CommandFinishedEvent(commandSentEvent);
            commandSentEvent.PreventCall = true;
            var exceptions = commandFinishedEvent.CallEvent();
            if (exceptions.Count > 0)
                foreach (var pair in exceptions)
                    commandSentEvent.OS.Write("Command End Listener Method '{0}' failed with: {1}", pair.Key, pair.Value);
            disconnects = commandSentEvent.Disconnects;
            returnFlag = disconnects;
        }

        [Patch("Hacknet.OS.Draw", flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
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
                        if (!osStartDrawEvent.IgnoreScanlines) self.drawScanlines();
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

        [Patch("Hacknet.OS.LoadContent", flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn)]
        public static bool onLoadSession(OS self)
        {
            var loadSessionEvent = new Event.OSLoadContentEvent(self);
            loadSessionEvent.CallEvent();
            if (loadSessionEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.OS.LoadContent", flags: InjectFlags.PassInvokingInstance)]
        public static void onPostLoadSession(OS self)
        {
            var postLoadSessionEvent = new Event.OSPostLoadContentEvent(self);
            postLoadSessionEvent.CallEvent();
        }

        [Patch("Hacknet.OS.UnloadContent", flags: InjectFlags.PassInvokingInstance)]
        public static void onUnloadSession(OS self)
        {
            var unloadSessionEvent = new Event.OSUnloadContentEvent(self);
            unloadSessionEvent.CallEvent();
        }

        [Patch("Hacknet.MainMenu.Draw", 120, flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassParametersVal)]
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

        [Patch("Hacknet.MainMenu.drawMainMenuButtons", 248,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals,
            localsID: new int[] { 0, 4 }
        )]
        public static void onMainMenuButtonsDraw(MainMenu self, ref int mainButtonY, ref int secondaryButtonY)
        {
            var drawMainMenuButtonsEvent = new Event.DrawMainMenuButtonsEvent(self, mainButtonY, secondaryButtonY);
            drawMainMenuButtonsEvent.CallEvent();
            mainButtonY = drawMainMenuButtonsEvent.MainButtonY;
            secondaryButtonY = drawMainMenuButtonsEvent.SecondaryButtonY;
        }

        [Patch("Hacknet.OS.loadSaveFile", 30,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.ModifyReturn,
            localsID: new int[] { 0 }
        )]
        public static bool onLoadSaveFile(OS self, ref Stream stream)
        {
            /* TODO: Refactor that null out */
            var loadSaveFileEvent = new Event.OSLoadSaveFileEvent(self, null, stream);
            loadSaveFileEvent.CallEvent();
            if (loadSaveFileEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.OS.writeSaveGame", flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal | InjectFlags.ModifyReturn)]
        public static bool onSaveFile(OS self, string filename)
        {
            var saveFileEvent = new Event.OSSaveFileEvent(self, filename);
            saveFileEvent.CallEvent();
            if (saveFileEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.OS.writeSaveGame", -5,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.PassParametersVal,
            localsID: new int[] { 0 }
        )]
        public static void onSaveWrite(OS self, ref string saveString, string filename)
        {
            var saveWriteEvent = new Event.OSSaveWriteEvent(self, filename, saveString);
            saveWriteEvent.CallEvent();
            saveString = saveWriteEvent.SaveString;
        }

        [Patch("Hacknet.NetworkMap.LoadContent", flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn)]
        public static bool onLoadNetmapContent(NetworkMap self)
        {
            var loadNetmapContentEvent = new Event.NetworkMapLoadContentEvent(self);
            loadNetmapContentEvent.CallEvent();
            if (loadNetmapContentEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : ProgramRunner.AttemptExeProgramExecution
        [Patch("Hacknet.ProgramRunner.AttemptExeProgramExecution", 54,
            flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
            localsID: new int[] { 0, 1, 2, 6 }
        )]
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
            var exceptions = executableExecuteEvent.CallEvent();
            if (exceptions.Count > 0)
                foreach (var pair in exceptions)
                    os.Write("Executable Listener Method '{0}' failed with: {1}", pair.Key, pair.Value);
            result = (int)executableExecuteEvent.Result;
            if (executableExecuteEvent.IsCancelled || result != -1)
                return true;
            return false;
        }

        // Hook location : OS.launchExecutable
        [Patch("Hacknet.OS.launchExecutable", 44,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
            localsID: new int[] { 2 }
        )]
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
            var exceptions = portExecutableExecuteEvent.CallEvent();
            if (exceptions.Count > 0)
                foreach (var pair in exceptions)
                    self.Write("PortHack Listener Method '{0}' failed with: {1}", pair.Key, pair.Value);
            name = portExecutableExecuteEvent.ExecutableName;
            data = portExecutableExecuteEvent.ExecutableData;
            if (portExecutableExecuteEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.Computer.load", flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
        public static bool onLoadSavedComputerStart(out Computer result,
                                                    ref XmlReader reader,
                                                    ref OS os)
        {
            var loadSavedComputerStartEvent = new Event.LoadSavedComputerStartEvent(null, reader, os);
            loadSavedComputerStartEvent.CallEvent();
            result = loadSavedComputerStartEvent.Computer;
            if (loadSavedComputerStartEvent.IsCancelled)
            {
                onLoadSavedComputerEnd(ref result, ref reader, ref os);
                return true;
            }
            return false;
        }

        [Patch("Hacknet.Computer.load", -2,
            flags: InjectFlags.PassParametersRef | InjectFlags.PassLocals,
            localsID: new int[] { 97 }
        )]
        public static void onLoadSavedComputerEnd(ref Computer loadedComputer,
                                                  ref XmlReader reader,
                                                  ref OS os)
        {
            var loadSavedComputerEndEvent = new Event.LoadSavedComputerEndEvent(loadedComputer, reader, os);
            loadSavedComputerEndEvent.CallEvent();
            loadedComputer = loadSavedComputerEndEvent.Computer;
            reader = loadSavedComputerEndEvent.Reader;
        }

        [Patch("Hacknet.ComputerLoader.loadComputer", flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
        public static bool onLoadContentComputerStart(out object result,
                                                      ref string filename,
                                                      ref bool preventAddingToNetmap,
                                                      ref bool preventInitDaemons)
        {
            var loadSavedComputerStartEvent =
                new Event.LoadContentComputerStartEvent(null, filename, preventAddingToNetmap, preventInitDaemons);
            loadSavedComputerStartEvent.CallEvent();
            result = loadSavedComputerStartEvent.Computer;
            filename = loadSavedComputerStartEvent.Filename;
            preventAddingToNetmap = loadSavedComputerStartEvent.PreventNetmapAdd;
            preventInitDaemons = loadSavedComputerStartEvent.PreventDaemonInit;
            if (loadSavedComputerStartEvent.IsCancelled)
            {
                Stream stream = System.IO.File.OpenRead(filename);
                onLoadContentComputerEnd(ref stream, ref result, ref filename, ref preventAddingToNetmap, ref preventInitDaemons);
                return true;
            }
            return false;
        }

        [Patch("Hacknet.ComputerLoader.loadComputer", -2,
            flags: InjectFlags.PassParametersRef | InjectFlags.PassLocals,
            localsID: new int[] { 0, 153 }
        )]
        public static void onLoadContentComputerEnd(ref Stream stream,
                                                    ref object loadedComputer,
                                                    ref string filename,
                                                    ref bool preventAddingToNetmap,
                                                    ref bool preventInitDaemons)
        {
            var loadContentComputerEndEvent =
                new Event.LoadContentComputerEndEvent(loadedComputer as Computer,
                                                      filename,
                                                      preventAddingToNetmap,
                                                      preventInitDaemons,
                                                      stream);
            loadContentComputerEndEvent.CallEvent();
            loadedComputer = loadContentComputerEndEvent.Computer;
        }


        // createdComputer is from the hidden compiler generated value at Hacknet.ComputerLoader/'<>c__DisplayClass4'::c
        // its an irregular addition via injection
        /*public static void onLoadComputer(ref XmlReader reader,
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
        }*/

        static Color defaultTitleColor = new Color(190, 190, 190, 0);
        static SpriteFont defaultTitleFont;

        [Patch("Hacknet.MainMenu.DrawBackgroundAndTitle", 7,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
            localsID: new int[] { 0 }
        )]
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
            var main = new TitleData(mainTitle,
                                         defaultTitleColor,
                                         defaultTitleFont,
                                         (Rect2)dest
                                        );
            var sub = new TitleData(subtitle,
                                       main.Color * 0.5f,
                                       GuiData.smallfont,
                                       new Rect2(520, 178, 0, 0)
                                      );
            var drawMainMenuTitles = new Event.DrawMainMenuTitlesEvent(self, main, sub);
            drawMainMenuTitles.CallEvent();
            if (drawMainMenuTitles.IsCancelled)
                return true;
            main = drawMainMenuTitles.Main;
            sub = drawMainMenuTitles.Sub;
            FlickeringTextEffect.DrawLinedFlickeringText(
                dest = (Rectangle)main.Destination,
                main.Title,
                7f,
                0.55f,
                main.Font,
                null,
                main.Color
            );
            TextItem.doFontLabel(sub.Destination.Vector2f, sub.Title, sub.Font, sub.Color, 600f, 26f);
            Logger.Verbose("Finished Redrawing Main Menu Titles");
            return true;
        }

        [Patch("Hacknet.Game1.UnloadContent", -1, flags: InjectFlags.PassInvokingInstance)]
        public static void onGameUnloadContent(Game1 self)
        {
            var gameUnloadEvent = new Event.GameUnloadEvent(self);
            gameUnloadEvent.CallEvent();
        }

        [Patch("Hacknet.Game1.Update", -5, flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef)]
        public static void onGameUpdate(Game1 self, ref GameTime time)
        {
            var gameUpdateEvent = new Event.GameUpdateEvent(self, time);
            gameUpdateEvent.CallEvent();
        }

        [Patch("Hacknet.DisplayModule.doProbeDisplay", -158,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.PassLocals,
            localsID: new int[] { 0, 1, 10 }
        )]
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

        [Patch("Hacknet.DisplayModule.Update", flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
        public static bool onDisplayModuleUpdate(DisplayModule self, ref float time)
        {
            var displayModuleUpdateEvent = new Event.DisplayModuleUpdateEvent(self, time);
            displayModuleUpdateEvent.CallEvent();
            if (displayModuleUpdateEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.DisplayModule.Draw", flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
        public static bool onDisplayModuleDraw(DisplayModule self, ref float time)
        {
            var displayModuleDrawEvent = new Event.DisplayModuleDrawEvent(self, time);
            displayModuleDrawEvent.CallEvent();
            if (displayModuleDrawEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.Screens.ExtensionsMenuScreen.Draw", 71,
            flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
            localsID: new int[] { 3 }
        )]
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

        [Patch("Hacknet.Screens.ExtensionsMenuScreen.DrawExtensionList", flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
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

        [Patch("Hacknet.OptionsMenu.Draw", 40, flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
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

        [Patch("Hacknet.OptionsMenu.LoadContent", -1, flags: InjectFlags.PassInvokingInstance)]
        public static void onOptionsMenuLoadContent(OptionsMenu self)
        {
            var optionsMenuLoadContentEvent = new Event.OptionsMenuLoadContentEvent(self);
            optionsMenuLoadContentEvent.CallEvent();
        }

        [Patch("Hacknet.OptionsMenu.Update", flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
        public static bool onOptionsMenuUpdate(OptionsMenu self, ref GameTime time, ref bool notFocused, ref bool isCovered)
        {
            var optionsMenuUpdateEvent = new Event.OptionsMenuUpdateEvent(self, time, notFocused, isCovered);
            optionsMenuUpdateEvent.CallEvent();
            if (optionsMenuUpdateEvent.IsCancelled)
                return true;
            return false;
        }

        [Patch("Hacknet.OptionsMenu.apply", flags: InjectFlags.PassInvokingInstance)]
        public static void onOptionsApply(OptionsMenu self)
        {
            var optionsMenuApplyEvent = new Event.OptionsMenuApplyEvent(self);
            optionsMenuApplyEvent.CallEvent();
        }

        [Patch("Hacknet.RunnableConditionalActions.LoadIntoOS",
            flags: InjectFlags.PassParametersVal | InjectFlags.ModifyReturn)]
        public static bool onLoadRunnableActionsIntoOS(string filepath, object OSobj)
        {
            var truePath = LocalizedFileLoader.GetLocalizedFilepath(Utils.GetFileLoadPrefix() + filepath);
            var evt = new Event.ActionsLoadIntoOSEvent(truePath, (OS) OSobj);
            var except = evt.CallEvent();
            if(except.Count > 0)
                throw new EventException("Failed to load conditional actions", except);
            return evt.IsCancelled;
        }


        /* pure bug-fix patch */
        [Patch("Hacknet.SCInstantly.Check", flags:
            InjectFlags.PassParametersVal |  InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn)]
        public static bool onSCInstantlyCheck(SCInstantly self, out bool retVal, object objOS)
        {
            var os = (OS) objOS;
            if (self.needsMissionComplete)
            {
                /* bug-fix here: adds a null check for `os.currentMission` (when player has no mission) */
                if (os.currentMission != null && !os.currentMission.isComplete())
                    retVal = false;
                else
                    retVal = true;
            }
            else retVal = true;
            return true;

        }

        /* philosophical bug-fix patch */
        [Patch("Hacknet.SCOnConnect.Check", 20, flags:
            InjectFlags.PassParametersVal | InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassLocals,
            localsID: new [] {1})]
        public static bool onSCOnConnectCheck(SCOnConnect self, out bool retVal, ref Computer computer, object objOS)
        {
            var os = (OS) objOS;

            if (!string.IsNullOrWhiteSpace(self.requiredFlags))
            {
                if (self.requiredFlags.Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries).Any(flag => !os.Flags.HasFlag(flag)))
                {
                    retVal = false;
                    return true;
                }
            }

            if (self.needsMissionComplete)
            {
                /* if the player doesn't have a mission, is their current mission complete?
                 * current community consensus: "yes". This patch makes the code match that.
                 */
                if (os.currentMission != null && !os.currentMission.isComplete())
                {
                    retVal = false;
                    return true;
                }
            }

            retVal = true;
            if (os.connectedComp != null && os.connectedComp.ip == computer.ip)
                retVal = true;
            else
                retVal = false;

            return true;
        }

        [Patch("Hacknet.ComputerLoader.filter", flags: InjectFlags.PassParametersRef | InjectFlags.ModifyReturn)]
        public static bool onFilterString(out string result, ref string input)
        {
            result = input.HacknetFilter();
            return true;
        }

        [Patch("Hacknet.Utils.AppendToErrorFile", flags: InjectFlags.PassParametersRef)]
        public static void onAppendToErrorFile(ref string text)
        {
            Console.WriteLine("[HACKNET ERROR] " + text);
        }

        [Patch("Hacknet.Terminal.write", flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassParametersRef)]
        public static bool onTerminalWriteAppend(Terminal self, ref string input)
        {
            calledFromSingle = true;
            var terminalWriteEvent = new Event.TerminalWriteEvent(self, input);
            terminalWriteEvent.CallEvent();
            if (terminalWriteEvent.IsCancelled)
            {
                calledFromSingle = false;
                return true;
            }
            input = terminalWriteEvent.Text;
            var terminalWriteAppendEvent = new Event.TerminalWriteAppendEvent(self, input);
            terminalWriteAppendEvent.CallEvent();
            if (terminalWriteAppendEvent.IsCancelled)
                return true;
            input = terminalWriteAppendEvent.Text;
            return false;
        }

        private static bool calledFromSingle = false;
        [Patch("Hacknet.Terminal.writeLine", flags: InjectFlags.PassInvokingInstance | InjectFlags.ModifyReturn | InjectFlags.PassParametersRef)]
        public static bool onTerminalWriteLine(Terminal self, ref string input)
        {
            if(!calledFromSingle)
            {
                var terminalWriteEvent = new Event.TerminalWriteEvent(self, input);
                terminalWriteEvent.CallEvent();
                if (terminalWriteEvent.IsCancelled)
                    return true;
                input = terminalWriteEvent.Text;
            }
            var terminalWriteLineEvent = new Event.TerminalWriteLineEvent(self, input);
            terminalWriteLineEvent.CallEvent();
            if (terminalWriteLineEvent.IsCancelled)
                return true;
            input = terminalWriteLineEvent.Text;
            return false;
        }

        // Prevents double calling of TerminalWriteEvent
        [Patch("Hacknet.Terminal.write", -1)]
        public static void onTerminalWriteAppendEnd()
        {
            calledFromSingle = false;
        }
    }
}
