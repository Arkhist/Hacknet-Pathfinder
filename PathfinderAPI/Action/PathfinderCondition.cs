using System.Xml;
using System.Xml.Linq;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Action
{
    public abstract class PathfinderCondition : SerializableCondition
    {
        public virtual XElement GetSaveElement()
        {
            return XMLStorageAttribute.WriteToElement(this);
        }

        public virtual void LoadFromXml(ElementInfo info)
        {
            XMLStorageAttribute.ReadFromElement(info, this);
        }
    }
}
