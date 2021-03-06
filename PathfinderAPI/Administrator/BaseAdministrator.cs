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
        
        public virtual void LoadFromXml(ElementInfo info)
        {
            base.ResetsPassword = info.Attributes.GetBool("resetPass");
            base.IsSuper = info.Attributes.GetBool("isSuper");
            
            XMLStorageAttribute.ReadFromElement(info, this);
        }
    }
}
