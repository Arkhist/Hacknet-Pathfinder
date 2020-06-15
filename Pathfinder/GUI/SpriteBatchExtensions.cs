using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using V2 = Microsoft.Xna.Framework.Vector2;
using Pathfinder.Util.Types;

namespace Pathfinder.GUI
{
    public static class SpriteBatchExtensions
    {
        public static void Draw(this SpriteBatch sb,
                                Texture2D tex,
                                Vec2 position,
                                Vec2 dimensions,
                                Color? color,
                                float rotation = 0,
                                SpriteEffects effects = SpriteEffects.None,
                                float layerDepth = 0,
                                Rectangle? destRect = null)
        {
            sb.Draw(tex, (V2)position, destRect, color ?? Color.White, rotation, new V2(0, 0), (V2)dimensions, effects, layerDepth);
        }

        public static void Draw(this SpriteBatch sb,
                                Texture2D tex,
                                Rect2 rect,
                                Color? color,
                                float rotation = 0,
                                SpriteEffects effects = SpriteEffects.None,
                                float layerDepth = 0,
                                Rectangle? destRect = null)
        {
            sb.Draw(tex, rect.Position, rect.Size, color, rotation, effects, layerDepth, destRect);
        }
    }
}
