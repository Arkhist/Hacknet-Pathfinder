using System.Xml;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Administrator
{
    public abstract class BaseAdministrator : Hacknet.Administrator
    {
        protected Computer computer;
        protected OS opSystem;

        public BaseAdministrator(Computer computer, OS opSystem) : base()
        {
            this.computer = computer;
            this.opSystem = opSystem;
        }
    }
}
