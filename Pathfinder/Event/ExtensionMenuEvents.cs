using Hacknet;
using Hacknet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Event
{
    public class ExtensionMenuEvent : PathfinderEvent
    {
        public ExtensionsMenuScreen ExtensionMenuScreen { get; private set; }
        public Rectangle Rectangle { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Vector2 ButtonPosition { get; set; }
        public ExtensionMenuEvent(ExtensionsMenuScreen screen, Rectangle rect, SpriteBatch sprite, Vector2 buttonPos)
        {
            ExtensionMenuScreen = screen;
            Rectangle = rect;
            SpriteBatch = sprite;
            ButtonPosition = buttonPos;
        }
    }

    public class DrawExtensionMenuEvent : ExtensionMenuEvent
    {
        public ScreenManager ScreenManager { get; private set; }
        public DrawExtensionMenuEvent(ExtensionsMenuScreen screen, Rectangle rect, SpriteBatch sprite, ScreenManager manager, Vector2 buttonPos)
            : base(screen, rect, sprite, buttonPos) { ScreenManager = manager; }
    }

    public class DrawExtensionMenuListEvent : ExtensionMenuEvent
    {
        public DrawExtensionMenuListEvent(ExtensionsMenuScreen screen, Vector2 drawpos, Rectangle rect, SpriteBatch sprite)
            : base(screen, rect, sprite, drawpos) {}
    }
}
