using Rectangle = Microsoft.Xna.Framework.Rectangle;
using V2 = Microsoft.Xna.Framework.Vector2;
using V3 = Microsoft.Xna.Framework.Vector3;
using V4 = Microsoft.Xna.Framework.Vector4;

namespace Pathfinder.Util
{
    public class Vector1<T>
    {
        public T X { get; set; }
        public Vector1(T x) { X = x; }
        public static T2 Convert<T2>(T c)
        {
            T2 result;
            ConvertAny.TryConvert(c, out result);
            return result;
        }
        public static T Convert<T2>(T2 c)
        {
            T result;
            ConvertAny.TryConvert(c, out result);
            return result;
        }
    }

    public class Vector2<T> : Vector1<T>
    {
        public T Y { get; set; }
        public Vector2(T x, T y) : base(x) { Y = y; }
        public V2 Vector2f => this;
        public static implicit operator V2(Vector2<T> v) =>
            new V2(Convert<float>(v.X), Convert<float>(v.Y));
        public static implicit operator Vector2<T>(V2 v) =>
            new Vector2<T>(Convert(v.X), Convert(v.Y));
    }

    public class Vector3<T> : Vector2<T>
    {
        public T Z { get; set; }
        public Vector3(T x, T y, T z) : base(x, y) { Z = z; }
        public V3 Vector3f => this;
        public static implicit operator V3(Vector3<T> v) =>
            new V3(Convert<float>(v.X), Convert<float>(v.Y), Convert<float>(v.Z));
        public static implicit operator Vector3<T>(V3 v) =>
            new Vector3<T>(Convert(v.X), Convert(v.Y), Convert(v.Z));
    }

    public class Vector4<T> : Vector3<T>
    {
        public T W { get; set; }
        public Vector4(T x, T y, T z, T w) : base(x, y, z) { W = w; }
        public V4 Vector4f => this;
        public Rectangle Rectangle => this;
        public static implicit operator V4(Vector4<T> v) =>
            new V4(Convert<float>(v.X), Convert<float>(v.Y), Convert<float>(v.Z), Convert<float>(v.W));
        public static implicit operator Vector4<T>(V4 v) =>
            new Vector4<T>(Convert(v.X), Convert(v.Y), Convert(v.Z), Convert(v.W));
        public static implicit operator Rectangle(Vector4<T> v) =>
            new Rectangle(Convert<int>(v.X), Convert<int>(v.Y), Convert<int>(v.Z), Convert<int>(v.W));
        public static implicit operator Vector4<T>(Rectangle v) =>
            new Vector4<T>(Convert(v.X), Convert(v.Y), Convert(v.Width), Convert(v.Height));
    }
}
