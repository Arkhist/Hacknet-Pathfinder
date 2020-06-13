using System;
using System.Linq;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Input;
using Pathfinder.Util.Types;
using Gui = Hacknet.Gui;

namespace Pathfinder.GUI
{
    public delegate void ControlCallback(IControl sender);

    public interface IControl
    {
        Vec2 Position { set; get; }
        float Rotation { get; set; }
        bool Visible { get; set; }
        Color Modulate { get; set; }
        void Draw();
        void Input();
        void AcceptInput();
        void EndDraw();
    }

    public abstract class ControlBase : IControl
    {
        public static SpriteBatch Batch => GuiData.spriteBatch;

        public bool IsBlocking { get; set; }

        public virtual Vec2 Position { set; get; }
        public float Rotation { get; set; }
        public bool Visible { get; set; } = true;
        public Color Modulate { get; set; } = Color.White;

        public abstract void Draw();
        public virtual void Input() { }

        public static bool AcceptedInput { get; protected set; }
        public void AcceptInput() => AcceptedInput = true;
        public static bool EndedDraw { get; protected set; }
        public void EndDraw() => EndedDraw = true;
    }

    public abstract class BasicRectangleBase : ControlBase
    {
        public Rect2 Rect { get; set; }
        public override Vec2 Position
        {
            get => Rect.Position;
            set => Rect.Position = value;
        }
        public Vec2 Size
        {
            get => Rect.Size;
            set => Rect.Size = value;
        }

        protected BasicRectangleBase(Rect2 rect)
        {
            Rect = rect;
            Modulate = GuiData.Default_Backing_Color;
        }

        public bool Contains(Vec2 point) => Rect.HasPoint(point);
        public bool Contains(Point point) => Rect.HasPoint(point);
        public bool Contains(Vector2 point) => Rect.HasPoint(point);
        public bool Contains(double x, double y) => Rect.HasPoint(new Vec2(x, y));

        public override void Input()
        {
            if (GuiData.blockingInput || AcceptedInput)
            {
                AcceptedInput = false;
                return;
            }
            GuiData.blockingInput = IsBlocking && Contains(GuiData.getMousePos());
        }
        public override void Draw() => Input();

        public void DrawOutline(float thickness, Color? modulate = null)
        {
            if (modulate == null) modulate = Modulate;
            var rect = Rect.Abs();
            var tmp = rect.New;
            tmp.Width = thickness;
            tmp.Height = rect.Height;
            Batch.Draw(Utils.white, tmp, modulate);
            tmp.X += thickness;
            tmp.Width = Rect.Width - 2 * thickness;
            tmp.Height = thickness;
            Batch.Draw(Utils.white, tmp, modulate);
            tmp.Y += rect.Height - thickness;
            Batch.Draw(Utils.white, tmp, modulate);
            tmp.X = rect.X + rect.Width - thickness;
            tmp.Width = thickness;
            tmp.Y = rect.Y;
            tmp.Height = rect.Height;
            Batch.Draw(Utils.white, tmp, modulate);
        }
    }

    public class BasicRectangle : BasicRectangleBase
    {

        public BasicRectangle(Rect2 rect) : base(rect) { }
        public BasicRectangle(float x, float y, float width, float height) : this(new Rect2(x, y, width, height)) { }
        public BasicRectangle(Vec2 position, Vec2 size) : this(position.X, position.Y, size.X, size.Y) { }

        public override void Draw()
        {
            if (Visible) { base.Draw(); Batch.Draw(Utils.white, Rect.Abs(), Modulate); }
        }
    }

    public abstract class RectangleControlBase : BasicRectangleBase
    {
        public event ControlCallback OnDraw;
        public event ControlCallback OnDown;
        public event ControlCallback OnUp;
        public event ControlCallback OnPressed;
        public event ControlCallback OnInput;

        protected RectangleControlBase(Rect2 rect) : base(rect) { }
        protected RectangleControlBase(float x, float y, float width, float height) : this(new Rect2(x, y, width, height)) { }
        protected RectangleControlBase(Vec2 position, Vec2 size) : this(position.X, position.Y, size.X, size.Y) { }

        public virtual bool IsFocused { get; protected set; }
        public virtual bool IsReleased
        {
            get => !IsPressed;
            protected set => IsPressed = !value;
        }
        public virtual bool IsPressed { get; protected set; }

        public virtual bool JustPressed { get; protected set; }
        public virtual bool JustReleased { get; protected set; }

        public ControlCallback DrawCallback
        {
            get => OnDraw;
            set => OnDraw += value;
        }
        public ControlCallback DownInputCallback
        {
            get => OnDown;
            set => OnDown += value;
        }
        public ControlCallback UpInputCallback
        {
            get => OnUp;
            set => OnUp += value;
        }
        public ControlCallback PressedInputCallback
        {
            get => OnPressed;
            set => OnPressed += value;
        }
        public ControlCallback InputCallback
        {
            get => OnInput;
            set => OnInput += value;
        }

        public virtual void CallDraw() { }

        public override void Draw()
        {
            base.Draw();
            if (Visible)
            {
                OnDraw?.Invoke(this);
                if(EndedDraw)
                {
                    EndedDraw = false;
                    return;
                }
                CallDraw();
            }
        }

        public override void Input()
        {
            base.Input();
            IsFocused = false;
            JustReleased = false;
            JustPressed = false;
            if(AcceptedInput)
            {
                AcceptedInput = false;
                return;
            }
            if (!GuiData.blockingInput && Contains(GuiData.getMousePos()))
            {
                IsFocused = !Gui.Button.DisableIfAnotherIsActive;

                var pressed = GuiData.isMouseLeftDown();
                if (IsPressed && !pressed) JustReleased = true;
                else JustPressed |= !IsPressed && pressed;

                IsPressed = pressed;
            }
            else if (IsPressed) IsPressed = IsFocused;
            OnInput?.Invoke(this);
            if (AcceptedInput)
            {
                AcceptedInput = false;
                return;
            }
            if ((JustPressed || JustReleased) && IsFocused)
            {
                if (JustPressed) OnDown?.Invoke(this);
                else OnUp?.Invoke(this);
                OnPressed?.Invoke(this);
            }
        }

    }
}
