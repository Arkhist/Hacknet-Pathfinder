using Pathfinder.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder
{
    static class Pathfinder
    {
        static Dictionary<string, PathfinderMod> mods = new Dictionary<string, PathfinderMod>();

        public static void init()
        {

            StartUpEvent.EventListeners += testEventListener;
        }

        public static void testEventListener(PathfinderEvent pathfinderEvent)
        {
            Console.WriteLine("HEY ! LISTEN !");
        }
    }
}
