using System.Xml;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Action
{
    public abstract class PathfinderCondition : SerializableCondition
    {
        public virtual string GetSaveStringOverridable()
        {
            return XMLStorageAttribute.WriteToXml(this);
        }

        public virtual void LoadFromXml(XmlReader reader)
        {
            XMLStorageAttribute.ReadFromXml(reader, this);
        }
    }
}
