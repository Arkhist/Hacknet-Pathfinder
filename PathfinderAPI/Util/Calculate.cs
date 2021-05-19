using System;
using System.Collections.Generic;

namespace Pathfinder.Util
{
    public static class Calculate
    {
        private delegate T MathFunc<T>(T left, T right);
        private delegate T AbsFunc<T>(T val);
        private delegate T ShiftFunc<T>(T left, int right);

        private static readonly Dictionary<Type, Delegate> AddOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (MathFunc<byte>)((x,y)=>(byte)(x+y))},
            { typeof(sbyte), (MathFunc<sbyte>)((x,y)=>(sbyte)(x+y))},
            { typeof(short), (MathFunc<short>)((x,y)=>(short)(x+y))},
            { typeof(ushort), (MathFunc<ushort>)((x,y)=>(ushort)(x+y))},
            { typeof(int), (MathFunc<int>)((x,y)=>x+y)},
            { typeof(uint), (MathFunc<uint>)((x,y)=>x+y)},
            { typeof(long), (MathFunc<long>)((x,y)=>x+y)},
            { typeof(ulong), (MathFunc<ulong>)((x,y)=>x+y)},
            { typeof(float), (MathFunc<float>)((x,y)=>x+y)},
            { typeof(double), (MathFunc<double>)((x,y)=>x+y)},
            { typeof(decimal), (MathFunc<decimal>)((x,y)=>x+y)}
        };

        private static readonly Dictionary<Type, Delegate> SubOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (MathFunc<byte>)((x,y)=>(byte)(x-y))},
            { typeof(sbyte), (MathFunc<sbyte>)((x,y)=>(sbyte)(x-y))},
            { typeof(short), (MathFunc<short>)((x,y)=>(short)(x-y))},
            { typeof(ushort), (MathFunc<ushort>)((x,y)=>(ushort)(x-y))},
            { typeof(int), (MathFunc<int>)((x,y)=>x-y)},
            { typeof(uint), (MathFunc<uint>)((x,y)=>x-y)},
            { typeof(long), (MathFunc<long>)((x,y)=>x-y)},
            { typeof(ulong), (MathFunc<ulong>)((x,y)=>x-y)},
            { typeof(float), (MathFunc<float>)((x,y)=>x-y)},
            { typeof(double), (MathFunc<double>)((x,y)=>x-y)},
            { typeof(decimal), (MathFunc<decimal>)((x,y)=>x-y)}
        };

        private static readonly Dictionary<Type, Delegate> MulOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (MathFunc<byte>)((x,y)=>(byte)(x*y))},
            { typeof(sbyte), (MathFunc<sbyte>)((x,y)=>(sbyte)(x*y))},
            { typeof(short), (MathFunc<short>)((x,y)=>(short)(x*y))},
            { typeof(ushort), (MathFunc<ushort>)((x,y)=>(ushort)(x*y))},
            { typeof(int), (MathFunc<int>)((x,y)=>x*y)},
            { typeof(uint), (MathFunc<uint>)((x,y)=>x*y)},
            { typeof(long), (MathFunc<long>)((x,y)=>x*y)},
            { typeof(ulong), (MathFunc<ulong>)((x,y)=>x*y)},
            { typeof(float), (MathFunc<float>)((x,y)=>x*y)},
            { typeof(double), (MathFunc<double>)((x,y)=>x*y)},
            { typeof(decimal), (MathFunc<decimal>)((x,y)=>x*y)}
        };

        private static readonly Dictionary<Type, Delegate> DivOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (MathFunc<byte>)((x,y)=>(byte)(x/y))},
            { typeof(sbyte), (MathFunc<sbyte>)((x,y)=>(sbyte)(x/y))},
            { typeof(short), (MathFunc<short>)((x,y)=>(short)(x/y))},
            { typeof(ushort), (MathFunc<ushort>)((x,y)=>(ushort)(x/y))},
            { typeof(int), (MathFunc<int>)((x,y)=>x/y)},
            { typeof(uint), (MathFunc<uint>)((x,y)=>x/y)},
            { typeof(long), (MathFunc<long>)((x,y)=>x/y)},
            { typeof(ulong), (MathFunc<ulong>)((x,y)=>x/y)},
            { typeof(float), (MathFunc<float>)((x,y)=>x/y)},
            { typeof(double), (MathFunc<double>)((x,y)=>x/y)},
            { typeof(decimal), (MathFunc<decimal>)((x,y)=>x/y)}
        };

        private static readonly Dictionary<Type, Delegate> ModOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (MathFunc<byte>)((x,y)=>(byte)(x%y))},
            { typeof(sbyte), (MathFunc<sbyte>)((x,y)=>(sbyte)(x%y))},
            { typeof(short), (MathFunc<short>)((x,y)=>(short)(x%y))},
            { typeof(ushort), (MathFunc<ushort>)((x,y)=>(ushort)(x%y))},
            { typeof(int), (MathFunc<int>)((x,y)=>x%y)},
            { typeof(uint), (MathFunc<uint>)((x,y)=>x%y)},
            { typeof(long), (MathFunc<long>)((x,y)=>x%y)},
            { typeof(ulong), (MathFunc<ulong>)((x,y)=>x%y)},
            { typeof(float), (MathFunc<float>)((x,y)=>x%y)},
            { typeof(double), (MathFunc<double>)((x,y)=>x%y)},
            { typeof(decimal), (MathFunc<decimal>)((x,y)=>x%y)}
        };

        private static readonly Dictionary<Type, Delegate> LSftOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (ShiftFunc<byte>)((x,y)=>(byte)(x<<y))},
            { typeof(sbyte), (ShiftFunc<sbyte>)((x,y)=>(sbyte)(x<<y))},
            { typeof(short), (ShiftFunc<short>)((x,y)=>(short)(x<<y))},
            { typeof(ushort), (ShiftFunc<ushort>)((x,y)=>(ushort)(x<<y))},
            { typeof(int), (ShiftFunc<int>)((x,y)=>x<<y)},
            { typeof(uint), (ShiftFunc<uint>)((x,y)=>x<<y)},
            { typeof(long), (ShiftFunc<long>)((x,y)=>x<<y)},
            { typeof(ulong), (ShiftFunc<ulong>)((x,y)=>x<<y)}
        };

        private static readonly Dictionary<Type, Delegate> RSftOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (ShiftFunc<byte>)((x,y)=>(byte)(x>>y))},
            { typeof(sbyte), (ShiftFunc<sbyte>)((x,y)=>(sbyte)(x<<y))},
            { typeof(short), (ShiftFunc<short>)((x,y)=>(short)(x>>y))},
            { typeof(ushort), (ShiftFunc<ushort>)((x,y)=>(ushort)(x>>y))},
            { typeof(int), (ShiftFunc<int>)((x,y)=>x>>y)},
            { typeof(uint), (ShiftFunc<uint>)((x,y)=>x>>y)},
            { typeof(long), (ShiftFunc<long>)((x,y)=>x>>y)},
            { typeof(ulong), (ShiftFunc<ulong>)((x,y)=>x>>y)}
        };

        private static readonly Dictionary<Type, Delegate> AbsOps = new Dictionary<Type, Delegate>()
        {
            { typeof(byte), (AbsFunc<byte>)(x => x)},
            { typeof(sbyte), (AbsFunc<sbyte>)Math.Abs},
            { typeof(short), (AbsFunc<short>)Math.Abs},
            { typeof(ushort), (AbsFunc<ushort>)(x=>x)},
            { typeof(int), (AbsFunc<int>)Math.Abs},
            { typeof(uint), (AbsFunc<uint>)(x=>x)},
            { typeof(long), (AbsFunc<long>)Math.Abs},
            { typeof(ulong), (AbsFunc<ulong>)(x=>x)},
            { typeof(float), (AbsFunc<float>)Math.Abs},
            { typeof(double), (AbsFunc<double>)Math.Abs},
            { typeof(decimal), (AbsFunc<decimal>)Math.Abs}
        };

        private static T Execute<T>(Dictionary<Type, Delegate> dic, T l, T r)
        {
            if (dic.TryGetValue(typeof(T), out var del))
                return ((MathFunc<T>)del)(l, r);
            return l;
        }

        public static T Add<T>(T l, T r) => Execute(AddOps, l, r);
        public static T Subtract<T>(T l, T r) => Execute(SubOps, l, r);
        public static T Multiply<T>(T l, T r) => Execute(MulOps, l, r);
        public static T Divide<T>(T l, T r) => Execute(DivOps, l, r);
        public static T Modulo<T>(T l, T r) => Execute(ModOps, l, r);
        public static T LeftShift<T>(T l, T r) => Execute(LSftOps, l, r);
        public static T RightShift<T>(T l, T r) => Execute(RSftOps, l, r);
        public static T Min<T>(T l, T r) where T : IComparable<T> => l.CompareTo(r) < 0 ? l : r;
        public static T Max<T>(T l, T r) where T : IComparable<T> => l.CompareTo(r) > 0 ? l : r;
        public static T Abs<T>(T v)
        {
            if (AbsOps.TryGetValue(typeof(T), out var del))
                return ((AbsFunc<T>)del)(v);
            return v;
        }
    }
}