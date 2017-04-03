using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HacknetPathfinder.Pathfinder
{
    static class Events
    {
        public delegate void GameEventHandler();
        public static event GameEventHandler GameStartUpEvent;
        public static event GameEventHandler GameStartedEvent;
        public static event GameEventHandler GameExitingEvent;
        public static event GameEventHandler GameExitedEvent;


        static void CallGameStartUpEvent()
        {
            if (GameStartUpEvent != null)
                GameStartUpEvent();
        }

        static void CallGameStartedEvent()
        {
            if (GameStartedEvent != null)
                GameStartedEvent();
        }

        static void CallGameExitingEvent()
        {
            if (GameExitingEvent != null)
                GameExitingEvent();
        }

        static void CallGameExitedEvent()
        {
            if (GameExitedEvent != null)
                GameExitedEvent();
        }
    }
}
