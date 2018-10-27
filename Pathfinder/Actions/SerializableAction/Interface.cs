using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Actions.SerializableAction
{
    public abstract class Interface : Hacknet.SerializableAction
    {
        public abstract void Trigger(Hacknet.OS os);
    }
}
