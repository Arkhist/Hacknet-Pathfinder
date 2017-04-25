using System;
using System.Collections.Generic;

namespace Pathfinder.Port
{
    public class Instance : IEquatable<PortType>, IEquatable<Instance>
    {
        internal static Dictionary<Hacknet.Computer, List<Instance>> compToInst =
            new Dictionary<Hacknet.Computer, List<Instance>>();

        public PortType Port { get; private set; }
        public bool Unlocked { get; set; }

        public Instance(PortType port, bool unlocked = false)
        {
            if (port == null)
                throw new ArgumentNullException(nameof(port));
            Port = port;
            Unlocked = unlocked;
        }

        public static bool AssignTo(Instance ins, Hacknet.Computer c)
        {
            if (!compToInst.ContainsKey(c))
                compToInst.Add(c, new List<Instance>());
            if (!compToInst[c].Contains(ins))
            {
                compToInst[c].Add(ins);
                return true;
            }
            return false;
        }

        public static bool RemoveFrom(Instance ins, Hacknet.Computer c)
        {
            if (!compToInst.ContainsKey(c) || !compToInst[c].Contains(ins))
                return false;
            return compToInst[c].Remove(ins);
        }

        public static Instance GetInstanceIn(Hacknet.Computer c, PortType type)
        {
            Instance r = null;
            if (compToInst.ContainsKey(c))
                r = compToInst[c].Find(i => i.Port == type);
            return r;
        }

        public bool AssignTo(Hacknet.Computer c) => AssignTo(this, c);
        public bool RemoveFrom(Hacknet.Computer c) => RemoveFrom(this, c);

        public bool Equals(PortType other) => other.Equals(this);
        public bool Equals(Instance other) => Port.Equals(other) && Unlocked == other.Unlocked;
    }
}
