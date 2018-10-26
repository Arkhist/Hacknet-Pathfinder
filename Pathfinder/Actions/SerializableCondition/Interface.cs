using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hacknet;

namespace Pathfinder.Actions.SerializableCondition
{
    public abstract class Interface : Hacknet.SerializableCondition
    {
        public virtual bool Check(OS os) { return true; }
    }
}
