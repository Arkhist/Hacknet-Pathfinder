using System.Xml;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Action
{
    public abstract class PathfinderCondition : SerializableCondition
    {
        public virtual string GetSaveStringOverridable()
        {
            return XMLStorageAttribute.WriteToXml(this);
        }

        public virtual void LoadFromXml(ElementInfo info)
        {
            XMLStorageAttribute.ReadFromElement(info, this);
        }
    }
}
