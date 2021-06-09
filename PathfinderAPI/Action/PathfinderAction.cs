using System.Xml;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Action
{
    public abstract class PathfinderAction : SerializableAction
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
