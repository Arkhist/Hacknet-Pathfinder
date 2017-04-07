using System.Reflection;
using Hacknet;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;

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
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Hacknet.MainMenu.OSVersion = Hacknet.MainMenu.OSVersion + " Pathfinder " + version;
            return false;
        }

        //  Hook location : Game1.LoadContent()
        //  if (this.CanLoadContent)
        //  {
        //	    <HOOK HERE>
        //      PortExploits.populate();
        public static void onLoadContent(Hacknet.Game1 self)
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
        public static bool onMainMenuDraw(Hacknet.MainMenu self, GameTime gameTime)
        {
            var drawMainMenuEvent = new Event.DrawMainMenuEvent(self, gameTime);
            drawMainMenuEvent.CallEvent();
            if (drawMainMenuEvent.IsCancelled)
            {
                GuiData.endDraw();
                PostProcessor.end();
                self.ScreenManager.FadeBackBufferToBlack((int)(255 - self.TransitionAlpha));
                return true;
            }
            return false;
        }

        // Hook location : MainMenu.drawMainMenuButtons()
        public static void onMainMenuButtonsDraw(Hacknet.MainMenu self)
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

        // Hook location : NetworkMap.LoadContent()
        public static bool onLoadNetmapContent(Hacknet.NetworkMap self)
        {
            var loadNetmapContentEvent = new Event.LoadNetmapContentEvent(self);
            loadNetmapContentEvent.CallEvent();
            if (loadNetmapContentEvent.IsCancelled)
                return true;
            return false;
        }

        public static bool onExecutableExecute(Hacknet.OS self, ref Rectangle location, ref string exeName, ref string exeFileData, ref int targetPort, ref string[] allParams, ref string originalName)
        {
            var executableExecuteEvent = new Event.ExecutableExecuteEvent(self, location, exeName, exeFileData, targetPort, allParams);
            executableExecuteEvent.CallEvent();
            if (executableExecuteEvent.IsCancelled)
                return true;
            return false;
        }
    }
}
