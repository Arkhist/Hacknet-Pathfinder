using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.GUI.ModOptions
{
    public interface AbstractOptions
    {
        void LoadContent(OptionsMenu menu);
        void Draw(OptionsMenu menu, GameTime time);
        void Update(OptionsMenu menu, GameTime time, bool notFocused, bool covered);
        void Apply(OptionsMenu menu);
    }
}
