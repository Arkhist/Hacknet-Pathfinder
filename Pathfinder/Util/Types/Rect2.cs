using System;
using Microsoft.Xna.Framework;

namespace Pathfinder.Util.Types
{
    public class Rect2 : Vec4, IEquatable<Rect2>
    {
        public Vec2 Position
        {
            get => new Vec2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vec2 Size
        {
            get => new Vec2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Vec2 End
        {
            get => new Vec2(EndX, EndY);
            set
            {
                EndX = value.X;
                EndY = value.Y;
            }
        }

        public Vec2 Center => new Vec2(CenterX, CenterY);

        public float Width
        {
            get => Z;
            set => Z = value;
        }

        public float Height
        {
            get => W;
            set => W = value;
        }

        public float EndX
        {
            get => X + Width;
            set => Width = value - X;
        }

        public float EndY
        {
            get => Y + Height;
            set => Height = value - Y;
        }

        public float CenterX => X + Width / 2;
        public float CenterY => Y + Height / 2;

        public float Area => Width * Height;
        public bool HasArea => Width > 0 && Height > 0;

        public Rect2(float x = 0, float y = 0, float width = 0, float height = 0) : base(x, y, width, height) { }
        public Rect2(Vec2 position, Vec2 size = default) : base(position.X, position.Y, size.X, size.Y) { }
        public Rect2(float x, float y = 0, Vec2 size = default) : base(x, y, size.X, size.Y) { }
        public Rect2(Vec4 v4) : base(v4.X, v4.Y, v4.Z, v4.W) { }

        public bool Intersects(Rect2 rect)
            => X.CompareTo(rect.EndX) < 0 &&
                EndX.CompareTo(rect.X) > 0 &&
                Y.CompareTo(rect.EndY) < 0 &&
                EndY.CompareTo(rect.Y) > 0;

        public float DistanceTo(Point p) => DistanceTo((Vec2)p);
        public float DistanceTo(Vector2 p) => DistanceTo((Vec2)p);

        public float DistanceTo(Vec2 p)
        {
            float dist = 0;
            var inside = true;
            if (p.X < X)
            {
                dist = X - p.X;
                inside = false;
            }
            if (p.Y < Y)
            {
                var d = Y - p.Y;
                dist = inside ? d : Math.Min(dist, d);
                inside = false;
            }
            if (p.X >= EndX)
            {
                var d = p.X - EndX;
                dist = inside ? d : Math.Min(dist, d);
                inside = false;
            }
            if (p.Y >= EndY)
            {
                var d = p.Y - EndY;
                dist = inside ? d : Math.Min(dist, d);
                inside = false;
            }

            if (inside) return 0;
            return dist;
        }

        public bool Encloses(Rect2 rect)
            => rect.X.CompareTo(X) >= 0 && rect.Y.CompareTo(Y) >= 0 &&
                EndX.CompareTo(X) <= 0 && EndY.CompareTo(Y) <= 0;

        public bool HasPoint(Point p) => HasPoint((Vec2)p);
        public bool HasPoint(Vector2 p) => HasPoint((Vec2)p);

        public bool HasPoint(Vec2 p)
            => p.X.CompareTo(X) >= 0 &&
                p.Y.CompareTo(Y) >= 0 &&
                p.X.CompareTo(EndX) < 0 &&
                p.Y.CompareTo(EndY) < 0;

        public Rect2 GrowMargins(float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            if(left != 0)
                X -= left;
            if(top != 0)
                Y -= top;
            if (left != 0 && right != 0)
                Width += left + right;
            if (top != 0 && bottom != 0)
                Height += top + bottom;
            return this;
        }

        public Rect2 Grow(float grow) => GrowMargins(grow, grow, grow, grow);

        public new Rect2 Abs()
            => new Rect2(X + Math.Min(Width, 0), Y + Math.Min(Height, 0), Size.Abs());

        public Rect2 ExpandTo(Point p) => ExpandTo((Vec2)p);
        public Rect2 ExpandTo(Vector2 p) => ExpandTo((Vec2)p);

        public Rect2 ExpandTo(Vec2 p)
        {
            if (p.X.CompareTo(X) < 0)
                X = p.X;
            if (p.Y.CompareTo(Y) < 0)
                Y = p.Y;
            if (p.X.CompareTo(EndX) > 0)
                EndX = p.X;
            if (p.Y.CompareTo(EndY) > 0)
                EndY = p.Y;
            return this;
        }

        public Rect2 Expand(Vec2 p) => New.ExpandTo(p);
        public Rect2 Expand(Point p) => New.ExpandTo(p);
        public Rect2 Expand(Vector2 p) => New.ExpandTo(p);

        public Rect2 Clip(Rect2 rect)
        {
            var newRect = new Rect2(rect);
            if (!Intersects(rect)) return new Rect2();
            newRect.X = Math.Max(newRect.X, X);
            newRect.Y = Math.Max(newRect.Y, Y);
            newRect.EndX = Math.Min(newRect.EndX, EndX);
            newRect.EndY = Math.Min(newRect.EndY, EndY);
            return newRect;
        }

        public Rect2 Merge(Rect2 rect)
        {
            var newRect = new Rect2(rect);
            newRect.X = Math.Min(newRect.X, X);
            newRect.Y = Math.Min(newRect.Y, Y);
            newRect.EndX = Math.Max(newRect.EndX, EndX);
            newRect.EndY = Math.Max(newRect.EndY, EndY);
            return newRect;
        }

        public Rect2 New => new Rect2(this);

        public override bool Equals(object obj) => Equals(obj as Rect2);

        public override int GetHashCode() 
            => unchecked(-1930765514 * -1521134295) + Position.GetHashCode() * -1521134295 + Size.GetHashCode();

        public bool Equals(Rect2 other) => other != null && base.Equals(other);

        public static bool operator ==(Rect2 l, Rect2 r) => l.Equals(r);
        public static bool operator !=(Rect2 l, Rect2 r) => !l.Equals(r);

        public static explicit operator Rectangle(Rect2 r) => r.Rectangle;
        public static explicit operator Rect2(Rectangle r) => new Rect2(r.X, r.Y, r.Width, r.Height);
        public static explicit operator Vector4(Rect2 r) => r.Vector4f;
        public static explicit operator Rect2(Vector4 r) => new Rect2(r.X, r.Y, r.Z, r.W);
    }
}
