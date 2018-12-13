using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Port
{
    public class Instance : IEquatable<Type>, IEquatable<Instance>
    {
        internal static Dictionary<Hacknet.Computer, List<Instance>> compToInst =
            new Dictionary<Hacknet.Computer, List<Instance>>();

        public Type Port { get; private set; }
        public bool Unlocked { get; set; }

        public Instance(Type port, bool unlocked = false)
        {
            if (port == null)
                throw new ArgumentNullException(nameof(port));
            Port = port;
            Unlocked = unlocked;
        }

        public static bool AssignTo(Instance ins, Hacknet.Computer c, bool replace = false)
        {
            if (!compToInst.ContainsKey(c))
                compToInst.Add(c, new List<Instance>());
            var sameDisplay = compToInst[c].FirstOrDefault((i) => i.Port.PortDisplay == ins.Port.PortDisplay);
            if (!compToInst[c].Contains(ins) && sameDisplay == null)
            {
                compToInst[c].Add(ins);
                return true;
            }
            if (sameDisplay != null && replace)
            {
                compToInst[c].Remove(sameDisplay);
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

        public static Instance GetInstanceIn(Hacknet.Computer c, Type type)
        {
            Instance r = null;
            if (compToInst.ContainsKey(c))
                r = compToInst[c].Find(i => i.Port == type);
            return r;
        }

        public bool AssignTo(Hacknet.Computer c, bool replace = false) => AssignTo(this, c , replace);
        public bool RemoveFrom(Hacknet.Computer c) => RemoveFrom(this, c);

        public bool Equals(Type other) => other.Equals(this);
        public bool Equals(Instance other) => Port.Equals(other) && Unlocked == other.Unlocked;
    }
}
