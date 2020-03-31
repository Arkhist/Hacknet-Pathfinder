using System;
using Hacknet;
using Hacknet.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;
using Pathfinder.Util.Types;
using Gui = Hacknet.Gui;
using V2 = Microsoft.Xna.Framework.Vector2;

namespace Pathfinder.GUI
{
    public abstract class BaseButton : RectangleControlBase
    {
        public string Text { get; set; }
        public virtual Texture2D Texture { get; set; }

        public Color FocusColor { get; set; } = GuiData.Default_Trans_Grey_Bright;
        public Color PressColor { get; set; } = GuiData.Default_Trans_Grey_Dark;

        protected BaseButton(float x, float y, float width, float height, string text, Color? color = null) : base(x, y, width, height)
        {
            Text = text;
            Modulate = color ?? GuiData.Default_Trans_Grey;
        }
    }

    public class Button : BaseButton
    {
        public bool SyncColorAndTexture { get; set; } = true;

        public sealed override Texture2D Texture
        {
            get => base.Texture;
            set
            {
                if (SyncColorAndTexture)
                {
                    if (value == null)
                    {
                        PressColor = GuiData.Default_Trans_Grey_Dark;
                        FocusColor = GuiData.Default_Trans_Grey_Bright;
                        Modulate = GuiData.Default_Trans_Grey;
                    }
                    else
                    {
                        PressColor = GuiData.Default_Unselected_Color;
                        FocusColor = GuiData.Default_Lit_Backing_Color;
                        Modulate = GuiData.Default_Trans_Grey_Strong;
                    }
                }
                base.Texture = value;
            }
        }

        public bool ForceNoColorTag { get; set; }
        public bool OnlyOutline { get; set; }
        public bool SmallButton
        {
            get => TextFont == GuiData.detailfont;
            set
            {
                if (value) TextFont = GuiData.detailfont;
                else TextFont = GuiData.tinyfont;
            }
        }

        public Color OutlineColor { get; set; } = GuiData.Default_Trans_Grey_Solid;
        public Color SelectedColor { get; set; } = GuiData.Default_Trans_Grey_Strong;
        public SpriteFont TextFont { get; set; } = GuiData.tinyfont;

        public Button(Rectangle shape, string text, Color? color = null)
            : this(shape.X, shape.Y, shape.Width, shape.Height, text, color) {}
        public Button(Point position, Point size, string text, Color? color = null)
            : this(new Rectangle(position.X, position.Y, size.X, size.Y), text, color) {}
        public Button(float x, float y, float width, float height, string text, Color? color = null)
            : base(x, y, width, height, text, color) { Texture = null; }

        public override void CallDraw()
        {
            var num = (!ForceNoColorTag && Size.X > 65) ? 13 : 0;
            var rect = Rect.Abs();
            if (Texture == null)
            {
                if (!OnlyOutline)
                {
                    Batch.Draw(Utils.white, rect,
                        IsFocused ? (
                            IsPressed
                            ? PressColor
                            : FocusColor)
                            : Modulate
                        );
                    rect.Width = num;
                    Batch.Draw(Utils.white, rect, SelectedColor);
                }
                DrawOutline(1, OnlyOutline ? SelectedColor : OutlineColor);
            }
            else
                Batch.Draw(Texture, rect,
                    IsFocused ? (
                        IsPressed
                        ? PressColor
                        : FocusColor)
                        : Modulate
                    );
            var scale = TextFont.MeasureString(Text);
            var num2 = LocaleActivator.ActiveLocaleIsCJK() ? 4f : 0f;
            var y2 = scale.Y;
            if (scale.X > Rect.Width - 4)
                scale.X = (Rect.Width - (4 + num + 5)) / scale.X;
            else
                scale.X = 1f;
            if (scale.Y > Rect.Height + num2 - 0f)
                scale.Y = (Rect.Height + num2 - 0f) / scale.Y;
            else
                scale.Y = 1f;
            scale.X = Math.Min(scale.X, scale.Y);
            scale.Y = scale.X;
            if (Utils.FloatEquals(1f, scale.Y))
                scale = V2.One;
            num += 4;
            var num3 = y2 * scale.Y;
            var num4 = Rect.Y + Rect.Height / 2f - num3 / 2f + 1f - num2 * scale.Y / 2f;
            Batch.DrawString(TextFont,
                Text,
                new V2((int)(Rect.X + 2 + num + 0.5), (int)(num4 + 0.5)),
                Color.White,
                0f,
#pragma warning disable RECS0030 // Suggests using the class declaring a static function when calling it
                V2.Zero,
#pragma warning restore RECS0030 // Suggests using the class declaring a static function when calling it
                scale,
                SpriteEffects.None,
                0.5f);
        }
    }
}
