using System;
using System.Collections.Generic;

namespace Pathfinder.Extension
{
    public abstract class Info
    {
        public string Id { get; internal set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string LogoPath { get; }
        public abstract bool AllowSaves { get; }
        public abstract void Construct(Hacknet.OS os);
    }
}
