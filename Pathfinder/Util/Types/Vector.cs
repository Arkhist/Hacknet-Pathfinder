using System;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using V2 = Microsoft.Xna.Framework.Vector2;
using V3 = Microsoft.Xna.Framework.Vector3;
using V4 = Microsoft.Xna.Framework.Vector4;

namespace Pathfinder.Util.Types
{
    public class Vec1 : IEquatable<Vec1>
    {
        public float X { get; set; }
        public Vec1(double x = default) { X = (float)x; }
        public void Deconstruct(out float x) => x = X;
        public Vec1 Abs() => new Vec1(Math.Abs(X));

        public override bool Equals(object obj) => Equals(obj as Vec1);

        public bool Equals(Vec1 other)
            => other != null && X.CompareTo(other.X) == 0 && GetHashCode() == other.GetHashCode();

        public override int GetHashCode() => -1830369473 + X.GetHashCode();

        public static bool operator ==(Vec1 l, Vec1 r) => l.Equals(r);
        public static bool operator !=(Vec1 l, Vec1 r) => !l.Equals(r);
        public static Vec1 operator +(Vec1 l, Vec1 r) => new Vec1(l.X + r.X);
        public static Vec1 operator -(Vec1 l, Vec1 r) => new Vec1(l.X - r.X);
        public static Vec1 operator *(Vec1 l, Vec1 r) => new Vec1(l.X * r.X);
        public static Vec1 operator /(Vec1 l, Vec1 r) => new Vec1(l.X / r.X);
        public static Vec1 operator %(Vec1 l, Vec1 r) => new Vec1(l.X % r.X);

        public static Vec1 Zero => new Vec1(0);
        public static Vec1 One => new Vec1(1);
    }

    public class Vec2 : Vec1, IEquatable<Vec2>
    {
        public float Y { get; set; }
        public Vec2(double x = 0, double y = 0) : base(x) { Y = (float)y; }
        public Vec2(Vec2 v) : this(v.X, v.Y) { }
        public V2 Vector2f => (V2)this;
        public Point Point => (Point)this;
        public static explicit operator V2(Vec2 v) => new V2(v.X, v.Y);
        public static explicit operator Vec2(V2 v) => new Vec2(v.X, v.Y);
        public static explicit operator Point(Vec2 v) => new Point((int)v.X, (int)v.Y);
        public static explicit operator Vec2(Point v) => new Vec2(v.X, v.Y);

        public static bool operator ==(Vec2 l, Vec2 r) => l.Equals(r);
        public static bool operator !=(Vec2 l, Vec2 r) => !l.Equals(r);

        public void Deconstruct(out float x, out float y)
        {
            Deconstruct(out x);
            y = Y;
        }
        public new Vec2 Abs() => new Vec2(Math.Abs(X), Math.Abs(Y));

        public override bool Equals(object obj) => Equals(obj as Vec2);

        public bool Equals(Vec2 other)
            => other != null && base.Equals(other) && Y.CompareTo(other.Y) == 0;

        public override int GetHashCode() =>
            unchecked(20836138 * -1521134295) + base.GetHashCode()
                * -1521134295 + Y.GetHashCode();

        public static Vec2 operator +(Vec2 l, Vec2 r) => new Vec2(l.X + r.X, l.Y + r.Y);
        public static Vec2 operator -(Vec2 l, Vec2 r) => new Vec2(l.X - r.X, l.Y - r.Y);
        public static Vec2 operator *(Vec2 l, Vec2 r) => new Vec2(l.X * r.X, l.Y * r.Y);
        public static Vec2 operator /(Vec2 l, Vec2 r) => new Vec2(l.X / r.X, l.Y / r.Y);
        public static Vec2 operator %(Vec2 l, Vec2 r) => new Vec2(l.X % r.X, l.Y % r.Y);

        public static Vec2 operator *(Vec2 l, long r) => new Vec2(l.X * r, l.Y * r);
        public static Vec2 operator *(Vec2 l, double r) => new Vec2((float)(l.X * r), (float)(l.Y * r));
        public static Vec2 operator /(Vec2 l, long r) => new Vec2(l.X / r, l.Y / r);
        public static Vec2 operator /(Vec2 l, double r) => new Vec2((float)(l.X / r), (float)(l.Y / r));

        public static Vec2 operator *(long r, Vec2 l) => new Vec2(l.X * r, l.Y * r);
        public static Vec2 operator *(double r, Vec2 l) => new Vec2((float)(l.X * r), (float)(l.Y * r));
        public static Vec2 operator /(long r, Vec2 l) => new Vec2(r / l.X, r / l.Y);
        public static Vec2 operator /(double r, Vec2 l) => new Vec2((float)(r / l.X), (float)(r / l.Y));

        public new static Vec2 Zero => new Vec2();
        public new static Vec2 One => new Vec2(1, 1);
        public static Vec2 Infinite => new Vec2(float.PositiveInfinity, float.PositiveInfinity);
        public static Vec2 Left => new Vec2(-1, 0);
        public static Vec2 Right => new Vec2(1, 0);
        public static Vec2 Up => new Vec2(0, -1);
        public static Vec2 Down => new Vec2(0, 1);

    }

    public class Vec3 : Vec2, IEquatable<Vec3>
    {
        public float Z { get; set; }
        public Vec3(double x = default, double y = default, double z = default) : base(x, y) { Z = (float)z; }
        public V3 Vector3f => (V3)this;
        public static explicit operator V3(Vec3 v) => new V3(v.X, v.Y, v.Z);
        public static explicit operator Vec3(V3 v) => new Vec3(v.X, v.Y, v.Z);

        public static bool operator ==(Vec3 vector1, Vec3 vector2) => vector1.Equals(vector2);
        public static bool operator !=(Vec3 vector1, Vec3 vector2) => !vector1.Equals(vector2);

        public void Deconstruct(out float x, out float y, out float z)
        {
            Deconstruct(out x, out y);
            z = Z;
        }

        public new Vec3 Abs() => new Vec3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));

        public override bool Equals(object obj) => Equals(obj as Vec3);

        public bool Equals(Vec3 other)
            => other != null &&
                   base.Equals(other) && Z.CompareTo(other.Z) == 0;

        public override int GetHashCode()
            => unchecked(71168995 * -1521134295) + base.GetHashCode()
                * -1521134295 + Z.GetHashCode();

        public static Vec3 operator +(Vec3 l, Vec3 r) => new Vec3(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        public static Vec3 operator -(Vec3 l, Vec3 r) => new Vec3(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        public static Vec3 operator *(Vec3 l, Vec3 r) => new Vec3(l.X * r.X, l.Y * r.Y, l.Z * r.Z);
        public static Vec3 operator /(Vec3 l, Vec3 r) => new Vec3(l.X / r.X, l.Y / r.Y, l.Z / r.Z);
        public static Vec3 operator %(Vec3 l, Vec3 r) => new Vec3(l.X % r.X, l.Y % r.Y, l.Z % r.Z);

        public static Vec3 operator *(Vec3 l, long r) => new Vec3(l.X * r, l.Y * r, l.Z * r);
        public static Vec3 operator *(Vec3 l, double r) => new Vec3((float)(l.X * r), (float)(l.Y * r), (float)(l.Z * r));
        public static Vec3 operator /(Vec3 l, long r) => new Vec3(l.X / r, l.Y / r, l.Z / r);
        public static Vec3 operator /(Vec3 l, double r) => new Vec3((float)(l.X / r), (float)(l.Y / r), (float)(l.Z / r));
    }

    public class Vec4 : Vec3, IEquatable<Vec4>
    {
        public float W { get; set; }
        public Vec4(float x = default, float y = default, float z = default, float w = default) : base(x, y, z) { W = w; }
        public V4 Vector4f => (V4)this;
        public Rectangle Rectangle => (Rectangle)this;
        public static explicit operator V4(Vec4 v) => new V4(v.X, v.Y, v.Z, v.W);
        public static explicit operator Vec4(V4 v) => new Vec4(v.X, v.Y, v.Z, v.W);
        public static explicit operator Rectangle(Vec4 v)
            => new Rectangle((int)Math.Round(v.X), (int)Math.Round(v.Y), (int)Math.Round(v.Z), (int)Math.Round(v.W));
        public static explicit operator Vec4(Rectangle v) => new Vec4(v.X, v.Y, v.Width, v.Height);

        public static bool operator ==(Vec4 vector1, Vec4 vector2) => vector1.Equals(vector2);
        public static bool operator !=(Vec4 vector1, Vec4 vector2) => vector1.Equals(vector2);

        public void Deconstruct(out float x, out float y, out float z, out float w)
        {
            Deconstruct(out x, out y, out z);
            w = W;
        }

        public new Vec4 Abs() => new Vec4(Calculate.Abs(X), Calculate.Abs(Y), Calculate.Abs(Z), Calculate.Abs(W));

        public override bool Equals(object obj) => Equals(obj as Vec4);

        public bool Equals(Vec4 other)
            => other != null &&
                   base.Equals(other) &&
                   W.CompareTo(other.W) == 0;

        public override int GetHashCode()
            => unchecked(-146940052 * -1521134295) + base.GetHashCode()
                * -1521134295 + W.GetHashCode();

        public static Vec4 operator +(Vec4 l, Vec4 r) => new Vec4(l.X + r.X, l.Y + r.Y, l.Z + r.Z, l.W + r.W);
        public static Vec4 operator -(Vec4 l, Vec4 r) => new Vec4(l.X - r.X, l.Y - r.Y, l.Z - r.Z, l.W - r.W);
        public static Vec4 operator *(Vec4 l, Vec4 r) => new Vec4(l.X * r.X, l.Y * r.Y, l.Z * r.Z, l.W * r.W);
        public static Vec4 operator /(Vec4 l, Vec4 r) => new Vec4(l.X / r.X, l.Y / r.Y, l.Z / r.Z, l.W / r.W);
        public static Vec4 operator %(Vec4 l, Vec4 r) => new Vec4(l.X % r.X, l.Y % r.Y, l.Z % r.Z, l.W % r.W);

        public static Vec4 operator *(Vec4 l, long r) => new Vec4(l.X * r, l.Y * r, l.Z * r, l.W * r);
        public static Vec4 operator *(Vec4 l, double r) => new Vec4((float)(l.X * r), (float)(l.Y * r), (float)(l.Z * r), (float)(l.W * r));
        public static Vec4 operator /(Vec4 l, long r) => new Vec4(l.X / r, l.Y / r, l.Z / r, l.W / r);
        public static Vec4 operator /(Vec4 l, double r) => new Vec4((float)(l.X / r), (float)(l.Y / r), (float)(l.Z / r), (float)(l.W / r));
    }
}
