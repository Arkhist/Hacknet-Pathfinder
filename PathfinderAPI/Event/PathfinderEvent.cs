﻿namespace Pathfinder.Event;

public abstract class PathfinderEvent
{
    internal bool cancelled = false;
    public bool Cancelled { 
        get
        {
            return cancelled;
        }
        set
        {
            cancelled |= value;
        }
    }
    public bool Thrown { get; internal set; } = false;
}