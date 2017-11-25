using System;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.GUI
{
    public abstract class BaseScrollbar : BaseInteractiveRectangle<int>
    {
        public int ContentSize { get; set; }
        public float CurrentScroll { get; set; }
        public bool Vertical { get; set; }

        protected BaseScrollbar(int x, int y, int width, int height, int contentSize, float scroll, bool vertical)
            : base(x, y, width, height)
        {
            ContentSize = contentSize;
            CurrentScroll = scroll;
            Vertical = vertical;
        }

        public virtual float DrawScroll()
        {
            Draw();
            return CurrentScroll;
        }
    }

    public class Scrollbar : BaseScrollbar
    {
        public bool DrawUnderBar { get; set; }

        protected DynamicRectangle Draggable { get; set; }

        public Scrollbar(Rectangle shape, int contentSize, float scroll = 0, bool vertical = true)
            : this(shape.X, shape.Y, shape.Width, shape.Height, contentSize, scroll, vertical) { }
        public Scrollbar(Point position, Point size, int contentSize, float scroll = 0, bool vertical = true)
            : this(position.X, position.Y, size.X, size.Y, contentSize, scroll, vertical) { }
        public Scrollbar(int x, int y, int width, int height, int contentSize, float scroll = 0, bool vertical = true)
            : base(x, y, width, height, contentSize, scroll, vertical)
        {
            Draggable = new DynamicRectangle(X, Y, Width, Height, Color.White, Color.Gray, Width, false, false);
        }

        public override void DoDraw()
        {
            var drawSize = ContentSize;
            if ((Vertical ? Height : Width) > drawSize)
                drawSize = Vertical ? Height : Width;
            if (DrawUnderBar || IsActive)
                GuiData.spriteBatch.Draw(Utils.white, new Rectangle(X, Y, Width, Height), Color.Gray * 0.1f);

            float diff = ContentSize - drawSize;
            float drawQuo = drawSize / ContentSize * drawSize;
            float scroll = CurrentScroll / diff;

            Draggable.X = Vertical ? X : X + (scroll * (drawSize - drawQuo));
            Draggable.Y = !Vertical ? Y : Y + (scroll * (drawSize - drawQuo));
            Draggable.Width = Vertical ? Width : drawQuo;
            Draggable.Height = !Vertical ? Height : drawQuo;
            Draggable.SelectableBorder = Vertical ? Width : Height;
            Draggable.XBound = null;
            Draggable.YBound = null;
            if (Vertical)
                Draggable.YBound = new MoveLine((Y + drawSize) - drawQuo, Y);
            else
                Draggable.XBound = new MoveLine((X + drawSize) - drawQuo, X);

            Draggable.Draw();

            CurrentScroll = Vertical ? Draggable.MovedPosition.Y : Draggable.MovedPosition.X;
            if (Math.Abs(CurrentScroll) > 0.1f)
                CurrentScroll = ((scroll * (drawSize - drawQuo)) + CurrentScroll) / (drawQuo - drawQuo) * diff;
        }
    }
}
