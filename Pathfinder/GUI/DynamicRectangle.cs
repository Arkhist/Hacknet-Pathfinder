using System;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.GUI
{
    public struct MoveLine
    {
        public float Max { get; set; }
        public float Min { get; set; }
        public MoveLine(float max = 0, float min = 0)
        {
            Max = max;
            Min = min;
        }

        public static explicit operator Vector2(MoveLine line) => new Vector2(line.Min, line.Max);
        public static explicit operator MoveLine(Vector2 vec) => new MoveLine(vec.Y, vec.X);
    }

    public abstract class BaseDynamicRectangle : BaseInteractiveRectangle
    {
        public Color SelectedColor { get; set; } = GuiData.Default_Selected_Color;
        public Color DeselectedColor { get; set; } = GuiData.Default_Unselected_Color;
        public float SelectableBorder { get; set; }
        public MoveLine? XBound { get; set; } = new MoveLine(3.40282347E+38f, -3.40282347E+38f);
        public MoveLine? YBound { get; set; } = new MoveLine(3.40282347E+38f, -3.40282347E+38f);

        public Vector2 MovedPosition { get; protected set; }

        protected BaseDynamicRectangle(int x, int y, int width, int height) : base(x, y, width, height) {}

        public bool InBorder => Contains((int)(X + Math.Max(0, SelectableBorder)), (int)(Y + Math.Max(0, SelectableBorder)),
                                         (int)(SelectableBorder < 0 ? 0 : Width - 2f * SelectableBorder),
                                         (int)(SelectableBorder < 0 ? 0 : Height - 2f * SelectableBorder),
                                         GuiData.getMousePoint());
    }

    public class DynamicRectangle : BaseDynamicRectangle
    {
        protected Vector2 OriginalClickPosition;
        protected Vector2 ClickPositionOffset;

        public bool IsDragging { get; set; }

        public DynamicRectangle(int x, int y, int width, int height) : base(x, y, width, height) { }
        public DynamicRectangle(int x,
                                int y,
                                int width,
                                int height,
                                Color? selected,
                                Color? deselected = null,
                                float selectableBorder = -1,
                                bool xMove = true,
                                bool yMove = true) : this(x, y, width, height)
        {
            if (selected.HasValue)
                SelectedColor = selected.Value;
            if (deselected.HasValue)
                DeselectedColor = deselected.Value;
            SelectableBorder = selectableBorder;
            if (!xMove)
                XBound = null;
            if (!yMove)
                YBound = null;
        }
        public DynamicRectangle(int x,
                                int y,
                                int width,
                                int height,
                                Color? selected,
                                Color? deselected,
                                float selectableBorder,
                                MoveLine? xbound,
                                MoveLine? ybound = null)
            : this(x, y, width, height, selected, deselected, selectableBorder, xbound.HasValue, ybound.HasValue)
        {
            XBound = xbound;
            YBound = ybound;
        }

        public override bool IsActive => Contains(GuiData.getMousePoint());

        public override bool Draw()
        {
            IsDragging = false;
            if (IsActive && !InBorder)
            {
                if (!IsHeldDown)
                {
                    if (GuiData.mouseWasPressed())
                    {
                        OriginalClickPosition = GuiData.getMousePos();
                        ClickPositionOffset = new Vector2(OriginalClickPosition.X - X, OriginalClickPosition.Y - Y);
                    }
                }
            }
            if(IsHeldDown)
            {
                float mvx = MovedPosition.X, mvy = MovedPosition.Y;
                if (XBound.HasValue)
                {
                    mvx = GuiData.mouse.X - X - ClickPositionOffset.X;
                    mvx = Math.Min(Math.Max(X + mvx, XBound.Value.Min), XBound.Value.Max) - X;
                }
                if (YBound.HasValue)
                {
                    mvy = GuiData.mouse.Y - Y - ClickPositionOffset.Y;
                    mvy = Math.Min(Math.Max(Y + mvy, YBound.Value.Min), YBound.Value.Max) - Y;
                }
                MovedPosition = new Vector2(mvx, mvy);
                IsDragging = true;
            }
            GuiData.blockingInput |= IsActive || IsHeldDown;
            base.Draw();
            return IsDragging;
        }

        public override void DoDraw()
        {
            GuiData.spriteBatch.Draw(Utils.white, MovedPosition, new Vector2(Width, Height), (IsHeldDown) ? SelectedColor : DeselectedColor);
        }
    }
}
