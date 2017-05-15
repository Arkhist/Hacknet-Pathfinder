using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gui = Hacknet.Gui;

namespace Pathfinder.GUI
{
    public abstract class BaseScrollPanel : BaseInteractiveRectangle
    {
        protected List<BaseInteraction> drawList = new List<BaseInteraction>();

        public Scrollbar Scrollbar { get; }
        public Color Color { get; set; }
        public Texture2D Texture { get; set; } = Utils.white;
        public Action<BaseScrollPanel> ExtraDraw { get; set; }

        protected BaseScrollPanel(int x, int y, int width, int height, int barCenterOffset, bool vertical, Color? color)
            : base(x, y, width, height)
        {
            Color = color ?? GuiData.Default_Dark_Background_Color;
            Scrollbar = new Scrollbar(vertical ? x + barCenterOffset + width / 2 : x,
                                      !vertical ? y + barCenterOffset + height / 2 : y,
                                      width,
                                      height,
                                      -1,
                                      0,
                                      vertical);
        }
        public void Add(BaseInteraction interactive) => drawList.Add(interactive);
        public void Remove(BaseInteraction interactive) => drawList.Remove(interactive);
    }

    public class ScrollPanel : BaseScrollPanel
    {
        public bool OnlyOutline { get; set; }

        public ScrollPanel(Rectangle shape, int barCenterOffset = 0, bool vertical = true, Color? color = null)
            : base(shape.X, shape.Y, shape.Width, shape.Height, barCenterOffset, vertical, color) {}
        public ScrollPanel(Point position, Point size, string text, int barCenterOffset = 0, bool vertical = true, Color? color = null)
            : this(new Rectangle(position.X, position.Y, size.X, size.Y), barCenterOffset, vertical, color) {}
        public ScrollPanel(int x, int y, int width, int height, int barCenterOffset = 0, bool vertical = true, Color? color = null)
            : base(x, y, width, height, barCenterOffset, vertical, color) {}

        public override void DoDraw()
        {
            Rectangle rect = new Rectangle(X, Y, Width, Height);
            if (Texture.Equals(Utils.white))
            {
                if (!OnlyOutline)
                {
                    GuiData.spriteBatch.Draw(Utils.white, rect, GuiData.Default_Trans_Grey);
                    rect.Width = Width > 65 ? 13 : 0;
                    GuiData.spriteBatch.Draw(Utils.white, rect, Color);
                }
                Gui.RenderedRectangle.doRectangleOutline(X, Y, Width, Height, 1,
                                                         OnlyOutline ? Color : GuiData.Default_Trans_Grey_Solid);
            }
            else
                GuiData.spriteBatch.Draw(Texture, rect, Color);

            foreach (var interact in drawList)
            {
                if (Scrollbar.Vertical)
                    Scrollbar.ContentSize = Math.Max(interact.Y + interact.Height, Scrollbar.ContentSize);
                else
                    Scrollbar.ContentSize = Math.Max(interact.X + interact.Width, Scrollbar.ContentSize);
            }

            Scrollbar.Draw();

            GuiData.spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            GuiData.spriteBatch.GraphicsDevice.ScissorRectangle = rect;
            foreach (var interact in drawList)
            {
                var scroll = (int)Scrollbar.CurrentScroll;
                if (Scrollbar.Vertical)
                    interact.Y -= scroll;
                else
                    interact.X -= scroll;
                interact.Draw();
                if (Scrollbar.Vertical)
                    interact.Y += scroll;
                else
                    interact.X += scroll;

            }
            ExtraDraw(this);
            GuiData.spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
        }
    }
}
