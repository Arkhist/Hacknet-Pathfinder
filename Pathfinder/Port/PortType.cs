using System;
using Pathfinder.Util;

namespace Pathfinder.Port
{
    public class PortType : IEquatable<PortType>, IEquatable<Instance>
    {
        public string PortName { get; private set; }
        public uint PortDisplay { get; private set; }
        public string PortId { get; internal set; }

        public PortType(string portName, uint portDisplay)
        {
            if (portName == null)
                throw new ArgumentNullException(nameof(portName));
            PortName = portName;
            PortDisplay = portDisplay;
        }

        public bool AssignTo(Hacknet.Computer c, bool unlocked = false)
        {
            var i = GetWithin(c);
            if(i == null)
                return new Instance(this, unlocked).AssignTo(c);
            i.Unlocked = unlocked;
            return false;
        }

        public bool RemoveFrom(Hacknet.Computer c)
        {
            var i = GetWithin(c);
            if (i != null)
                return i.RemoveFrom(c);
            return false;
        }

        public Instance GetWithin(Hacknet.Computer c) => Instance.GetInstanceIn(c, this);
        public bool Equals(PortType other) => other != null
                                && PortName == other.PortName
                                && PortDisplay == other.PortDisplay
                                && PortId == other.PortId;
        public bool Equals(Instance other) => Equals(other?.Port);
        public override string ToString() => string.Format("{0} {1}", PortDisplay, PortName);
        public static PortType GetById(string portId)
        {
            portId = Utility.GetId(portId);
            return Handler.GetPort(portId);
        }
    }
}
