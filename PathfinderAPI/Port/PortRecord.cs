using System;
using Hacknet;

namespace Pathfinder.Port
{
    public struct PortRecord : IEquatable<PortRecord>
    {
        public string Protocol { get; }
        public int OriginalPortNumber { get; }
        public string DefaultDisplayName { get; }
        public int DefaultPortNumber { get; private set; }
        public bool IsValid => Protocol != null;
        public PortRecord(string protocol, string defDisplayName, int defPortNumber)
        {
            Protocol = protocol;
            DefaultDisplayName = defDisplayName;
            DefaultPortNumber = defPortNumber;
            OriginalPortNumber = defPortNumber;
        }

        internal PortRecord(string protocol, string defDisplayName, int defPortNumber, int originalPortNumber) : this(protocol, defDisplayName, defPortNumber)
        {
            OriginalPortNumber = originalPortNumber;
        }

        public PortState CreateState(Computer computer, string displayName = null, int portNumber = -1, bool cracked = false)
        {
            if(!IsValid) return null;
            return new PortState(computer, this, displayName, portNumber, cracked);
        }

        public bool Equals(PortRecord other)
        {
            return Protocol == other.Protocol && OriginalPortNumber == other.OriginalPortNumber;
        }
        public static bool operator ==(PortRecord first, PortRecord second)
        {
            return first.Equals(second);
        }
        public static bool operator !=(PortRecord first, PortRecord second)
        {
            return !(first == second);
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((PortRecord)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Protocol != null ? Protocol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OriginalPortNumber;
                return hashCode;
            }
        }

        [Obsolete("Avoid PortData")]
        public static explicit operator PortRecord(PortData data)
            => new PortRecord(data.Protocol, data.DisplayName, data.Port, data.OriginalPort);

        [Obsolete("Avoid PortData")]
        public static explicit operator PortData(PortRecord record)
            => new PortData(record.Protocol, record.OriginalPortNumber, record.DefaultPortNumber, record.DefaultDisplayName);
    }
}