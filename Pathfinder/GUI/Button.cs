using System;
using Hacknet;
using Hacknet.Gui;
using Hacknet.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pathfinder.GUI
{
    public abstract class BaseButton
    {
        public Rectangle Shape { get; set; }
        public string Text { get; set; }
        public Color Color { get; set; }
        public Texture2D Texture { get; set; } = Hacknet.Utils.white;

        public bool IsActive => !GuiData.blockingInput && Shape.Contains(GuiData.getMousePoint());
        public bool IsReleased => IsActive && (GuiData.mouseLeftUp() || GuiData.mouse.LeftButton == ButtonState.Released);
        public bool IsHeldDown => IsActive && (GuiData.isMouseLeftDown()
                                               && !Hacknet.Gui.Button.DisableIfAnotherIsActive) && !IsReleased;
        public bool JustReleased { get; protected set; }
        public bool WasHeld { get; protected set; }

        protected BaseButton(Rectangle shape, string text, Color? color = null)
        {
            Shape = shape;
            Text = text;
            Color = color.HasValue ? color.Value : GuiData.Default_Trans_Grey_Strong;
        }

        public virtual bool Draw()
        {
            if (WasHeld && IsReleased)
            {
                JustReleased = true;
                WasHeld = false;
            }
            else
                JustReleased = false;
            WasHeld |= IsHeldDown;
            return JustReleased;
        }
    }

    public class Button : BaseButton
    {
        public bool ForceNoColorTag { get; set; }
        public bool OnlyOutline { get; set; }
        public bool SmallButton { get; set; }

        public Button(Rectangle shape, string text, Color? color = null) : base(shape, text, color) {}
        public Button(Point position, Point size, string text, Color? color = null)
            : this(new Rectangle(position.X, position.Y, size.X, size.Y), text, color) {}
        public Button(int x, int y, int width, int height, string text, Color? color = null)
            : this(new Rectangle(x, y, width, height), text, color) {}

        public override bool Draw()
        {
            this.DoDraw();
            return base.Draw();
        }

        public virtual void DoDraw()
        {
            int num = (!ForceNoColorTag && Shape.Width > 65) ? 13 : 0;
            Rectangle rect = Shape;
            if (Texture.Equals(Utils.white))
            {
                if (!OnlyOutline)
                {
                    GuiData.spriteBatch.Draw(Utils.white, rect, (IsActive) ? ((IsHeldDown) ? GuiData.Default_Trans_Grey_Dark : GuiData.Default_Trans_Grey_Bright) : GuiData.Default_Trans_Grey);
                    rect.Width = num;
                    GuiData.spriteBatch.Draw(Utils.white, rect, Color);
                }
                RenderedRectangle.doRectangleOutline(Shape.X,
                                                     Shape.Y,
                                                     Shape.Width,
                                                     Shape.Height,
                                                     1,
                                                     OnlyOutline ?
                                                     Color : new Color?(GuiData.Default_Trans_Grey_Solid));
            }
            else
                GuiData.spriteBatch.Draw(Texture, rect, (IsActive) ? ((IsHeldDown) ? GuiData.Default_Unselected_Color : GuiData.Default_Lit_Backing_Color) : Color);
            SpriteFont spriteFont = SmallButton ? GuiData.detailfont : GuiData.tinyfont;
            Vector2 scale = spriteFont.MeasureString(Text);
            float num2 = LocaleActivator.ActiveLocaleIsCJK() ? 4f : 0f;
            float y2 = scale.Y;
            if (scale.X > Shape.Width - 4)
                scale.X = (Shape.Width - (4 + num + 5)) / scale.X;
            else
                scale.X = 1f;
            if (scale.Y > Shape.Height + num2 - 0f)
                scale.Y = (Shape.Height + num2 - 0f) / scale.Y;
            else
                scale.Y = 1f;
            scale.X = Math.Min(scale.X, scale.Y);
            scale.Y = scale.X;
            if (Utils.FloatEquals(1f, scale.Y))
                scale = Vector2.One;
            num += 4;
            float num3 = y2 * scale.Y;
            float num4 = Shape.Y + Shape.Height / 2f - num3 / 2f + 1f - num2 * scale.Y / 2f;
            GuiData.spriteBatch.DrawString(spriteFont, Text, new Vector2((int)(Shape.X + 2 + num + 0.5), (int)(num4 + 0.5)), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.5f);
        }
    }
}
