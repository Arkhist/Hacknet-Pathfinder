using System;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pathfinder.Util;
using Gui = Hacknet.Gui;

namespace Pathfinder.GUI
{
    public abstract class BaseInteraction<T>
    {
        protected BaseInteraction(T x, T y, T width, T height)
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
        public T X { get; set; }
        public T Y { get; set; }
        public T Width { get; set; }
        public T Height { get; set; }

        public Vector2<T> Position
        {
            get { return new Vector2<T>(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public Vector2<T> Size
        {
            get { return new Vector2<T>(Width, Height); }
            set { Width = value.X; Height = value.Y; }
        }

        public Vector4<T> Rectangle
        {
            get { return new Vector4<T>(X, Y, Width, Height); }
            set { X = value.X; Y = value.Y; Width = value.Z; Height = value.W; }
        }

        public abstract bool HandleInteraction();

        public bool Contains(Point p) => Contains(X, Y, Width, Height, p);

        public static bool Contains(T x, T y, T width, T height, Point p)
        {
            return Convert.ToDouble(x) <= p.X
                          && p.X < Convert.ToDouble(x) + Convert.ToDouble(width)
                          && Convert.ToDouble(y) <= p.Y
                          && p.Y < Convert.ToDouble(y) + Convert.ToDouble(height);
        }
    }

    public abstract class BaseInteractiveRectangle<T> : BaseInteraction<T>
    {
        public Action<BaseInteractiveRectangle<T>> DrawFinish { get; set; }

        protected BaseInteractiveRectangle(T x, T y, T width, T height) : base(x, y, width, height) {}

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

        public virtual bool Draw()
        {
            DoDraw();
            var b = HandleInteraction();
            if(DrawFinish != null)
                DrawFinish(this);
            return b;
        }

        public abstract void DoDraw();
    }
}
