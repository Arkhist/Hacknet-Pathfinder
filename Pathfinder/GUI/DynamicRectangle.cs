using System;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Input;
using Pathfinder.Util.Types;

namespace Pathfinder.GUI
{

    public abstract class RectangleDynamicBase : RectangleControlBase
    {
        public Color SelectedColor { get; set; } = GuiData.Default_Selected_Color;
        public Color DeselectedColor { get; set; } = GuiData.Default_Unselected_Color;
        public float SelectableBorder { get; set; } = -1;
        public Vec2 XBound { get; set; } = new Vec2(3.40282347E+38f, -3.40282347E+38f);
        public Vec2 YBound { get; set; } = new Vec2(3.40282347E+38f, -3.40282347E+38f);

        public Vec2 MovedPosition { get; protected set; }

        protected RectangleDynamicBase(float x, float y, float width, float height) : base(x, y, width, height) {}

        public bool IsInBorder(Vec2 v) =>
            Rect.New.GrowMargins(SelectableBorder,
                SelectableBorder,
                SelectableBorder < 0 ? 0 : Rect.Width - 2f * SelectableBorder,
                SelectableBorder < 0 ? 0 : Rect.Height - 2f * SelectableBorder)
                .HasPoint(v);
    }

    public class RectangleDynamic : RectangleDynamicBase
    {
        protected Vec2 OriginalClickPosition;
        protected Vec2 ClickPositionOffset;

        public bool IsDragging { get; set; }
        public bool InBorder { get; protected set; }

        public RectangleDynamic(float x, float y, float width, float height) : base(x, y, width, height) { }
        public RectangleDynamic(float x,
                                float y,
                                float width,
                                float height,
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
        public RectangleDynamic(float x,
                                float y,
                                float width,
                                float height,
                                Color? selected,
                                Color? deselected,
                                float selectableBorder,
                                Vec2 xbound,
                                Vec2 ybound = null)
            : this(x, y, width, height, selected, deselected, selectableBorder, xbound != null, ybound != null)
        {
            XBound = xbound;
            YBound = ybound;
        }

        public override void Input()
        {
            base.Input();
            IsDragging = false;
            InBorder = false;
            if (IsFocused)
            {
                var mousePos = (Vec2)GuiData.getMousePos();
                InBorder = IsInBorder(mousePos);
                if(!InBorder && !IsPressed && GuiData.isMouseLeftDown())
                {
                    OriginalClickPosition = mousePos - Rect.Position;
                    ClickPositionOffset = OriginalClickPosition - Rect.Position;
                }
                if(IsPressed)
                {
                    var movePos = new Vec2(MovedPosition);
                    if(XBound != null)
                    {
                        movePos.X = mousePos.X - Rect.X - OriginalClickPosition.X;
                        movePos.X = Math.Min(Math.Max(Rect.Y + movePos.X, XBound.Y), XBound.X) - Rect.X;
                    }
                    if(YBound != null)
                    {
                        movePos.Y = mousePos.Y - Rect.Y - OriginalClickPosition.Y;
                        movePos.Y = Math.Min(Math.Max(Rect.Y + movePos.Y, YBound.Y), YBound.X) - Rect.Y;
                    }
                    MovedPosition = movePos;
                    IsDragging = true;
                }
                GuiData.blockingInput |= IsFocused || IsPressed;
            }
        }

        public override void CallDraw()
        {
            Batch.Draw(Utils.white, MovedPosition, new Vec2(Rect.Width, Rect.Height), IsPressed ? SelectedColor : DeselectedColor);
        }
    }
}
