using System;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.GUI.ModOptions
{
    public abstract class AbstractOptions
    {
        public OptionsMenu OptionsMenu { get; internal set; }
        public abstract void LoadContent();
        public abstract void Draw(GameTime time);
        public abstract void Update(GameTime time, bool notFocused, bool covered);
        public abstract void Apply();
    }
}
