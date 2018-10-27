using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Actions.SerializableAction
{
    public abstract class Interface : Hacknet.SerializableAction
    {
        public abstract void Trigger(Hacknet.OS os);

        public override void Trigger(object os_obj)
        {
            Trigger((Hacknet.OS)os_obj);
        }
    }
}
