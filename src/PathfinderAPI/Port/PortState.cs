using Hacknet;

namespace Pathfinder.Port;

public class PortState
{
    public Computer Computer { get; internal set; }
    public PortRecord Record { get; }
    private string _DisplayName;
    public string DisplayName
    {
        get => _DisplayName;
        set => _DisplayName = value ?? Record.DefaultDisplayName;
    }
    private int _PortNumber;
    public int PortNumber
    {
        get => _PortNumber;
        set => _PortNumber = value  > -1 ? value : Record.DefaultPortNumber;
    }
    public bool Cracked { get; set; }
    public void SetCracked(bool cracked, string ipFrom)
    {
        if(cracked && !Cracked)
            Computer.openPort(Record.Protocol, ipFrom);
        else if(!cracked && Cracked)
            Computer.closePort(Record.Protocol, ipFrom);
    }

    public PortState(Computer comp, PortRecord record, bool cracked) : this(comp, record, null, cracked: cracked)
    {
    }
    public PortState(Computer comp, PortRecord record, string displayName = null, int portNumber = -1, bool cracked = false)
    {
        Computer = comp;
        Record = record;
        Cracked = cracked;
        DisplayName = displayName;
        PortNumber = portNumber;
    }
    public PortState(Computer comp, string protocol, bool cracked) : this(comp, protocol, null, cracked: cracked)
    {
    }
    public PortState(Computer comp, string protocol, string displayName = null, int portNumber = -1, bool cracked = false)
    {
        Computer = comp;
        Record = PortManager.GetPortRecordFromProtocol(protocol);
        Cracked = cracked;
        DisplayName = displayName;
        PortNumber = portNumber;
    }

    public PortState Clone(Computer comp = null)
    {
        return Record.CreateState(comp, _DisplayName, _PortNumber, Cracked);
    }

    public bool Remove()
    {
        return Computer.RemovePort(Record);
    }
}