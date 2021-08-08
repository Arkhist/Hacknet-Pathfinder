using System;
using System.IO;
using System.Linq;

namespace Pathfinder.Util
{
    public class SBAwareBinaryReader : BinaryReader
    {
        public enum Endianess
        {
            LITTLE,
            BIG
        }

        public Endianess Endian;

        public SBAwareBinaryReader(Stream stream, Endianess endian) : base(stream)
        {
            Endian = endian;
        }

        public override short ReadInt16()
        {
            if (Endian == Endianess.LITTLE)
            {
                return base.ReadInt16();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt16(ReadBytes(2).Reverse().ToArray(), 0);
            }

            return BitConverter.ToInt16(ReadBytes(2), 0);
        }
        
        public override ushort ReadUInt16()
        {
            if (Endian == Endianess.LITTLE)
            {
                return base.ReadUInt16();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(ReadBytes(2).Reverse().ToArray(), 0);
            }

            return BitConverter.ToUInt16(ReadBytes(2), 0);
        }
        
        public override int ReadInt32()
        {
            if (Endian == Endianess.LITTLE)
            {
                return base.ReadInt32();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(ReadBytes(4).Reverse().ToArray(), 0);
            }

            return BitConverter.ToInt32(ReadBytes(4), 0);
        }
        
        public override uint ReadUInt32()
        {
            if (Endian == Endianess.LITTLE)
            {
                return base.ReadUInt32();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(ReadBytes(4).Reverse().ToArray(), 0);
            }

            return BitConverter.ToUInt32(ReadBytes(4), 0);
        }

        public override long ReadInt64()
        {
            if (Endian == Endianess.LITTLE)
            {
                return base.ReadInt64();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt64(ReadBytes(8).Reverse().ToArray(), 0);
            }

            return BitConverter.ToInt64(ReadBytes(8), 0);
        }

        public override ulong ReadUInt64()
        {
            if (Endian == Endianess.LITTLE)
            {
                return base.ReadUInt64();
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt64(ReadBytes(8).Reverse().ToArray(), 0);
            }

            return BitConverter.ToUInt64(ReadBytes(8), 0);
        }
    }
}