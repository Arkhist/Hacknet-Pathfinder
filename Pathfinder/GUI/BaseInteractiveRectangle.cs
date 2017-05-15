using System;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gui = Hacknet.Gui;

namespace Pathfinder.GUI
{
    public abstract class BaseInteraction
    {
        protected BaseInteraction(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public virtual bool IsActive => !GuiData.blockingInput && Contains(GuiData.getMousePoint());
        public virtual bool IsReleased => IsActive && (GuiData.mouseLeftUp() || GuiData.mouse.LeftButton == ButtonState.Released);
        public virtual bool IsHeldDown => IsActive && (GuiData.isMouseLeftDown()
                                               && !Gui.Button.DisableIfAnotherIsActive) && !IsReleased;
        public virtual bool JustReleased { get; protected set; }
        public virtual bool WasHeld { get; protected set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool Contains(Point p) => Contains(X, Y, Width, Height, p);

        public static bool Contains(int x, int y, int width, int height, Point p)
        {
            return Convert.ToDouble(x) <= p.X
                          && p.X < Convert.ToDouble(x) + Convert.ToDouble(width)
                          && Convert.ToDouble(y) <= p.Y
                          && p.Y < Convert.ToDouble(y) + Convert.ToDouble(height);
        }

        public abstract bool Draw();
        public abstract void DoDraw();
        public abstract bool HandleInteraction();
    }

    public abstract class BaseInteractiveRectangle : BaseInteraction
    {
        public Action<BaseInteractiveRectangle> DrawFinish { get; set; }

        protected BaseInteractiveRectangle(int x, int y, int width, int height) : base(x, y, width, height) {}

        public override bool HandleInteraction()
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

        public override bool Draw()
        {
            DoDraw();
            var b = HandleInteraction();
            if(DrawFinish != null)
                DrawFinish(this);
            return b;
        }
    }
}
