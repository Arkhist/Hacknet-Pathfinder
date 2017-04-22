using System.Reflection;
using Hacknet;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using Hacknet.Effects;
using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;

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
            Pathfinder.init();
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
            Pathfinder.LoadModContent();
            var loadContentEvent = new Event.GameLoadContentEvent(self);
            loadContentEvent.CallEvent();
        }

        // Hook location : ProgramRunner.ExecuteProgram()
        public static bool onCommandSent(out bool disconnects, object osObj, string[] arguments)
        {
            var os = osObj as Hacknet.OS;
            var commandSentEvent = new Event.CommandSentEvent(os, arguments);
            commandSentEvent.CallEvent();
            if (commandSentEvent.IsCancelled)
            {
                disconnects = commandSentEvent.Disconnects;
                return true;
            }
            disconnects = commandSentEvent.Disconnects;
            return false;
        }

        // Hook location : OS.LoadContent()
        public static bool onLoadSession(Hacknet.OS self)
        {
            var loadSessionEvent = new Event.OSLoadContentEvent(self);
            loadSessionEvent.CallEvent();
            if (loadSessionEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : end of OS.LoadContent()
        public static void onPostLoadSession(Hacknet.OS self)
        {
            var postLoadSessionEvent = new Event.OSPostLoadContenEvent(self);
            postLoadSessionEvent.CallEvent();
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
        public static void onMainMenuButtonsDraw(MainMenu self)
        {
            var drawMainMenuButtonsEvent = new Event.DrawMainMenuButtonsEvent(self);
            drawMainMenuButtonsEvent.CallEvent();
        }

        // Hook location : OS.loadSaveFile()
        public static bool onLoadSaveFile(Hacknet.OS self, ref Stream stream, ref XmlReader xmlReader)
        {
            var loadSaveFileEvent = new Event.OSLoadSaveFileEvent(self, xmlReader, stream);
            loadSaveFileEvent.CallEvent();
            if (loadSaveFileEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : OS.writeSaveGame()
        public static bool onSaveFile(Hacknet.OS self, string filename)
        {
            var saveFileEvent = new Event.OSSaveFileEvent(self, filename);
            saveFileEvent.CallEvent();
            if (saveFileEvent.IsCancelled)
            {
                return true;
            }
            return false;
        }

        public static void onSaveWrite(Hacknet.OS self, ref string saveString, string filename)
        {
            var saveWriteEvent = new Event.OSSaveWriteEvent(self, filename, saveString);
            saveWriteEvent.CallEvent();
            saveString = saveWriteEvent.SaveString;
        }

        // Hook location : NetworkMap.LoadContent()
        public static bool onLoadNetmapContent(Hacknet.NetworkMap self)
        {
            var loadNetmapContentEvent = new Event.NetworkMapLoadContentEvent(self);
            loadNetmapContentEvent.CallEvent();
            if (loadNetmapContentEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : OS.launchExecutable
        public static bool onExecutableExecute(out int result,
                                               ref Hacknet.Computer com,
                                               ref Folder fol,
                                               ref int finde,
                                               ref string exeFileData,
                                               ref Hacknet.OS os,
                                               ref string[] args)
        {
            var executableExecuteEvent =
                new Event.ExecutableExecuteEvent(com, fol, finde, finde >= 0 ? fol.files[finde] : null, os, args);
            executableExecuteEvent.CallEvent();
            result = (int) executableExecuteEvent.Result;
            if (executableExecuteEvent.IsCancelled || result != -1)
                return true;
            return false;
        }

        public static bool onPortExecutableExecute(Hacknet.OS self,
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

        public static void onLoadComputer(ref Hacknet.Computer createdComputer,
                                          ref XmlReader reader,
                                          string filename,
                                          bool preventAddingToNetmap,
                                          bool preventInitDaemons)
        {
            var loadComputerEvent = new Event.LoadComputerXmlReadEvent(createdComputer,
                                                                reader,
                                                                filename,
                                                                preventAddingToNetmap,
                                                                preventInitDaemons);
            loadComputerEvent.CallEvent();
        }

        static FieldInfo titleFontField = typeof(MainMenu).GetField("titleFont",
                                                                            BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo titleColorField = typeof(MainMenu).GetField("titleColor",
                                                                            BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool onDrawMainMenuTitles(MainMenu self, out bool result, ref Rectangle dest)
        {
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
                if (timeSpan.TotalSeconds< 1.0)
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
            var drawMainMenuTitles = new Event.DrawMainMenuTitlesEvent(self, mainTitle, subtitle);
            drawMainMenuTitles.CallEvent();
            if (drawMainMenuTitles.IsCancelled)
                return true;
            mainTitle = drawMainMenuTitles.MainTitle;
            subtitle = drawMainMenuTitles.Subtitle;

            var c = (Color)titleColorField.GetValue(self);
            FlickeringTextEffect.DrawLinedFlickeringText (
                dest,
                mainTitle,
                7f,
                0.55f,
                titleFontField.GetValue(self) as SpriteFont,
                null,
                c,
                2
            );
            TextItem.doFontLabel (new Vector2(520f, 178f), subtitle, GuiData.smallfont, c * 0.5f, 600f, 26f, false);
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
    }
}
