using System.Xml.Linq;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Daemon;

public abstract class BaseDaemon : Hacknet.Daemon
{
    public BaseDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem)
    {
        this.name = Identifier;
    }

    public virtual string Identifier => this.GetType().Name;

    /// <summary>
    /// DO NOT USE! This is a stubbed version of the base game method and is never saved by Pathfinder
    /// </summary>
    /// <returns>Returns null always</returns>
    public sealed override string getSaveString() => null;

    public virtual XElement GetSaveElement()
    {
        return XMLStorageAttribute.WriteToElement(this);
    }

    public virtual void LoadFromXml(ElementInfo info)
    {
        XMLStorageAttribute.ReadFromElement(info, this);
    }
}