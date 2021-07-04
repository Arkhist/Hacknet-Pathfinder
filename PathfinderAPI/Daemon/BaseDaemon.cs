using System.Xml;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Daemon
{
    public abstract class BaseDaemon : Hacknet.Daemon
    {
        public BaseDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem)
        {
            this.name = Identifier;
        }

        public virtual string Identifier => this.GetType().Name;

        public override string getSaveString()
        {
            return XMLStorageAttribute.WriteToXml(this);
        }

        public virtual void LoadFromXml(ElementInfo info)
        {
            XMLStorageAttribute.ReadFromElement(info, this);
        }
    }
}
