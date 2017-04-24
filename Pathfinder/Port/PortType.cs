using System;

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

        public bool Equals(PortType other) => other != null && PortName == other.PortName && PortDisplay == other.PortDisplay && PortId == other.PortId;
        public bool Equals(Instance other) => Equals(other?.Port);
        public override bool Equals(Object obj)
        {
            var pt = obj as PortType;
            if (pt != null)
                return this == pt;
            var ins = obj as Instance;
            if (ins != null)
                return this.Equals(ins);
            return false;
        }

        public override int GetHashCode() => PortName.GetHashCode() ^ PortDisplay.GetHashCode();
        public override string ToString()
        {
            return string.Format("{0} {1}", PortName, PortDisplay);
        }

        public static bool operator ==(PortType left, PortType right) => left.Equals(right);
        public static bool operator !=(PortType left, PortType right) => !(left != right);

        public static PortType GetById(string portId) => Handler.GetPort(portId);
    }
}
