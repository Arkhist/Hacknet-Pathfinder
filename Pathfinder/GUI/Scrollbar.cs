using System;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Input;
using Pathfinder.Util.Types;

namespace Pathfinder.GUI
{
    public abstract class ScrollbarBase : RectangleControlBase
    {
        public float ContentSize { get; set; }
        public float CurrentScroll { get; set; }
        public bool Vertical { get; set; }

        protected ScrollbarBase(float x, float y, float width, float height, float contentSize, float scroll, bool vertical)
            : base(x, y, width, height)
        {
            ContentSize = contentSize;
            CurrentScroll = scroll;
            Vertical = vertical;
        }
    }

    public class Scrollbar : ScrollbarBase
    {
        public bool DrawUnderBar { get; set; }

        protected RectangleDynamic Draggable { get; set; }

        public Scrollbar(Rectangle shape, float contentSize, float scroll = 0, bool vertical = true)
            : this(shape.X, shape.Y, shape.Width, shape.Height, contentSize, scroll, vertical) { }
        public Scrollbar(Point position, Point size, float contentSize, float scroll = 0, bool vertical = true)
            : this(position.X, position.Y, size.X, size.Y, contentSize, scroll, vertical) { }
        public Scrollbar(float x, float y, float width, float height, float contentSize, float scroll = 0, bool vertical = true)
            : base(x, y, width, height, contentSize, scroll, vertical)
        {
            Draggable = new RectangleDynamic(Rect.X, Rect.Y, Rect.Width, Rect.Height, Color.White, Color.Gray, Rect.Width, false, false);
        }

        public override void Input()
        {
            base.Input();

            var drawSize = ContentSize;
            if ((Vertical ? Rect.Height : Rect.Width) > drawSize)
                drawSize = Vertical ? Rect.Height : Rect.Width;

            var diff = ContentSize - drawSize;
            var drawQuo = drawSize / ContentSize * drawSize;
            var scroll = CurrentScroll / diff;

            Draggable.Rect.X = Vertical ? Rect.X : Rect.X + (scroll * (drawSize - drawQuo));
            Draggable.Rect.Y = !Vertical ? Rect.Y : Rect.Y + (scroll * (drawSize - drawQuo));
            Draggable.Rect.Width = Vertical ? Rect.Width : drawQuo;
            Draggable.Rect.Height = !Vertical ? Rect.Height : drawQuo;
            Draggable.SelectableBorder = Vertical ? Rect.Width : Rect.Height;
            Draggable.XBound = null;
            Draggable.YBound = null;
            if (Vertical)
                Draggable.YBound = new Vec2(Rect.Y + drawSize - drawQuo, Rect.Y);
            else
                Draggable.XBound = new Vec2(Rect.X + drawSize - drawQuo, Rect.X);

            CurrentScroll = Vertical ? Draggable.MovedPosition.Y : Draggable.MovedPosition.X;
            if (Math.Abs(CurrentScroll) > 0.1f)
                CurrentScroll = ((scroll * (drawSize - drawQuo)) + CurrentScroll) / (drawQuo - drawQuo) * diff;
        }

        public override void CallDraw()
        {
            if (DrawUnderBar || IsFocused)
                Batch.Draw(Utils.white, Rect, Color.Gray * 0.1f);

            Draggable.Draw();
        }
    }
}
