using System.Reflection;
using Hacknet;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using Hacknet.Effects;
using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Graphics;

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
            Pathfinder.LoadModContent();
            var loadContentEvent = new Event.LoadContentEvent(self);
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
            var loadSessionEvent = new Event.LoadSessionEvent(self);
            loadSessionEvent.CallEvent();
            if (loadSessionEvent.IsCancelled)
                return true;
            return false;
        }


        // Hook location : end of OS.LoadContent()
        public static void onPostLoadSession(Hacknet.OS self)
        {
            var postLoadSessionEvent = new Event.PostLoadSessionEvent(self);
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
            var loadSaveFileEvent = new Event.LoadSaveFileEvent(self, xmlReader, stream);
            loadSaveFileEvent.CallEvent();
            if (loadSaveFileEvent.IsCancelled)
            {
                return true;
            }
            return false;
        }

        // Hook location : OS.writeSaveGame()
        public static bool onSaveFile(Hacknet.OS self, string filename)
        {
            var saveFileEvent = new Event.SaveFileEvent(self, filename);
            saveFileEvent.CallEvent();
            if (saveFileEvent.IsCancelled)
            {
                return true;
            }
            return false;
        }

        public static void onSaveWrite(Hacknet.OS self, ref string saveString, string filename)
        {
            var saveWriteEvent = new Event.SaveWriteEvent(self, filename, saveString);
            saveWriteEvent.CallEvent();
            saveString = saveWriteEvent.SaveString;
        }

        // Hook location : NetworkMap.LoadContent()
        public static bool onLoadNetmapContent(NetworkMap self)
        {
            var loadNetmapContentEvent = new Event.LoadNetmapContentEvent(self);
            loadNetmapContentEvent.CallEvent();
            if (loadNetmapContentEvent.IsCancelled)
                return true;
            return false;
        }

        // Hook location : OS.launchExecutable
        public static bool onExecutableExecute(out int result,
                                               ref Hacknet.Computer computer,
                                               ref Folder exeFolder,
                                               ref int fileIndex,
                                               ref string exeFileData,
                                               ref Hacknet.OS os,
                                               ref string[] argArray)
        {
            var executableExecuteEvent = new Event.ExecutableExecuteEvent(computer,
                                                                          exeFolder,
                                                                          fileIndex,
                                                                          fileIndex >= 0 ? exeFolder.files[fileIndex] : null,
                                                                          os,
                                                                          argArray);
            executableExecuteEvent.CallEvent();
            result = (int) executableExecuteEvent.Result;
            if (executableExecuteEvent.IsCancelled || result != -1)
                return true;
            return false;
        }

        public static bool onPortExecutableExecute(Hacknet.OS self,
                                                     ref Rectangle location,
                                                     ref string exeName,
                                                     ref string exeFileData,
                                                     ref int targetPort,
                                                     ref string[] argArray,
                                                     ref string originalName)
        {
            var portExecutableExecuteEvent = new Event.PortExecutableExecuteEvent(self,
                                                                                    location,
                                                                                    exeName,
                                                                                    exeFileData,
                                                                                    targetPort,
                                                                                    argArray);
            portExecutableExecuteEvent.CallEvent();
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
                TextItem.doFontLabel(new Vector2(180f, 105f), text3, GuiData.smallfont, new Color?(Color.Red* 0.8f), 600f, 26f, false);
            }
            var drawMainMenuTitles = new Event.DrawMainMenuTitlesEvent(self, ref mainTitle, ref subtitle);
            drawMainMenuTitles.CallEvent();
            if (drawMainMenuTitles.IsCancelled)
                return true;

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
            TextItem.doFontLabel (new Vector2(520f, 178f), subtitle, GuiData.smallfont, new Color? (c* 0.5f), 600f, 26f, false);
            return true;
        }
    }
}
