using System.Reflection;
using Hacknet;
using Microsoft.Xna.Framework;

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
    }
}
