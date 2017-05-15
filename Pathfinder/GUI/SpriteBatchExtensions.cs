using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.GUI
{
    public static class SpriteBatchExtensions
    {
        public static void Draw(this SpriteBatch sb,
                                Texture2D tex,
                                Vector2 position,
                                Vector2 dimensions,
                                Color? color,
                                float rotation = 0,
                                SpriteEffects effects = SpriteEffects.None,
                                float layerDepth = 0,
                                Rectangle? destRect = null)
        {
            sb.Draw(tex, position, destRect, color ?? Color.White, rotation, new Vector2(0), dimensions, effects, layerDepth);
        }
    }
}
