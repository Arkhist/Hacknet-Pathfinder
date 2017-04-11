using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Executable
{
    public interface IMainDisplayOverride
    {
        bool IsOverrideActive(Instance instance);
        void DrawMain(Instance instance, Rectangle dest, SpriteBatch sprite);
    }
}
