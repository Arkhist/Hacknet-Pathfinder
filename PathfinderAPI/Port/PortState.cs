using System;
using Hacknet;

namespace Pathfinder.Port
{
    public class PortState
    {
        public Computer Computer { get; internal set; }
        public PortRecord Record { get; }
        private string _DisplayName;
        public string DisplayName
        {
            get => _DisplayName ?? Record.DefaultDisplayName;
            set => _DisplayName = value;
        }
        private int _PortNumber = -1;
        public int PortNumber
        {
            get => _PortNumber > -1 ? _PortNumber : Record.DefaultPortNumber;
            set => _PortNumber = value;
        }
        private bool _Cracked;
        public bool CrackedSilent
        {
            get => _Cracked;
            set => _Cracked = value;
        }
        /// <summary>
        /// When set, expects ip to be the Player's Computer
        /// </summary>
        /// <value></value>
        public bool Cracked
        {
            get => _Cracked;
            set => SetPort(value, Computer.os.thisComputer.ip);
        }
        public void SetPort(bool toOpen, string ipFrom)
        {
            if(toOpen && !Cracked)
                Computer.openPort(Record.Protocol, ipFrom);
            else if(!toOpen && Cracked)
                Computer.closePort(Record.Protocol, ipFrom);
        }

        public PortState(Computer comp, PortRecord record, bool cracked = false)
        {
            Computer = comp;
            Record = record;
            CrackedSilent = cracked;
        }
        public PortState(Computer comp, PortRecord record, string displayName, int portNumber = -1, bool cracked = false) : this(comp, record, cracked)
        {
            DisplayName = displayName;
            PortNumber = portNumber;
        }
        public PortState(Computer comp, string protocol, bool cracked = false)
        {
            Computer = comp;
            Record = PortManager.GetPortRecordFromProtocol(protocol);
            CrackedSilent = cracked;
        }
        public PortState(Computer comp, string protocol, string displayName, int portNumber = -1, bool cracked = false) : this(comp, protocol, cracked)
        {
            DisplayName = displayName;
            PortNumber = portNumber;
        }

        [Obsolete("Avoid PortData")]
        public static explicit operator PortData(PortState state)
            => new PortData(state.Record.Protocol, state.Record.OriginalPortNumber, state.PortNumber, state.DisplayName)
            {
                Cracked = state.Cracked
            };

        [Obsolete("Avoid PortData")]
        public static explicit operator PortState(PortData data)
            => new PortState(null, (PortRecord)data, data.Cracked);
    }
}