using Hacknet.Mission;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Mission
{
    public abstract class PathfinderGoal : MisisonGoal
    {
        public virtual void LoadFromXML(ElementInfo info)
        {
            XMLStorageAttribute.ReadFromElement(info, this);
        }
    }
}
