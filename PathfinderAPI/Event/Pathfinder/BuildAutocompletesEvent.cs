namespace Pathfinder.Event.Pathfinder;

public class BuildAutocompletesEvent : PathfinderEvent
{
    public List<string> Autocompletes { get; set; }

    public BuildAutocompletesEvent(List<string> autocompletes)
    {
        Autocompletes = autocompletes;
    }
}