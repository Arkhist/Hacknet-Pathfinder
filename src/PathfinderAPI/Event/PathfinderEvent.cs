namespace Pathfinder.Event;

public abstract class PathfinderEvent
{
    internal bool _cancelled;
    public bool Cancelled
    {
        get => _cancelled;
        set => _cancelled |= value;
    }
    public bool Thrown { get; internal set; }
}