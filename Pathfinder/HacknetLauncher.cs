using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HacknetPathfinder.Pathfinder
{
    static class HacknetLauncher
    {
        public static void runHacknet(string[] args)
        {
            try
            {
                Type.GetType("Hacknet.Program,Hacknet").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { args });
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
