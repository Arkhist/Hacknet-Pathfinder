using System;
using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public class Instance : IEquatable<Type>, IEquatable<Instance>
    {
        internal static Dictionary<Hacknet.Computer, List<Instance>> compToInst =
            new Dictionary<Hacknet.Computer, List<Instance>>();

        internal Computer activeComputer;

        public Type Port { get; private set; }
        public bool Unlocked { get; set; }
        public bool IsRemapped { get; private set; }
        public int MappedPort
        {
            get
            {
                int remap = 0;
                activeComputer?.PortRemapping.TryGetValue(Port.PortDisplay, out remap);
                return remap != 0 ? remap : Port.PortDisplay;
            }
            set
            {
                if (activeComputer == null) return;
                if (value == 0)
                {
                    activeComputer.PortRemapping.Remove(Port.PortDisplay);
                    IsRemapped = false;
                }
                else
                {
                    activeComputer.PortRemapping.Add(Port.PortDisplay, value);
                    IsRemapped = true;
                }
            }
        }

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
            if (c.PortRemapping.ContainsKey(ins.Port.PortDisplay) && !compToInst[c].Contains(ins) && replace || sameDisplay == null)
            {
                if (replace)
                {
                    c.ports.Remove(ExeInfoManager.GetExecutableInfo(ins.Port.PortDisplay).PortNumber);
                    c.PortRemapping.Remove(ins.Port.PortDisplay);
                }
                else return false;
            }
            if (!compToInst[c].Contains(ins) && sameDisplay == null)
            {
                compToInst[c].Add(ins);
                return true;
            }
            if (sameDisplay != null && replace)
            {
                compToInst[c].Remove(sameDisplay);
                compToInst[c].Add(ins);
                ins.activeComputer = c;
                return true;
            }
            return false;
        }

        public static bool RemoveFrom(Instance ins, Hacknet.Computer c)
        {
            if (!compToInst.ContainsKey(c) || !compToInst[c].Contains(ins))
                return false;
            c.PortRemapping.Remove(ins.Port.PortDisplay);
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
