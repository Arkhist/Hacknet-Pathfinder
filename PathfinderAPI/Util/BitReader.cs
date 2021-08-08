using System;
using System.IO;

namespace Pathfinder.Util
{
    public class BitReader : IDisposable
    {
        private BinaryReader Reader;

        private byte currentByte = 0;
        private int offset = 8;
        
        public BitReader(Stream stream)
        {
            Reader = new BinaryReader(stream);
        }

        public byte ReadByte()
        {
            if (offset == 8)
            {
                return Reader.ReadByte();
            }
            return (byte)ReadBitsDepth(8);
        }

        public byte[] ReadBytes(int count)
        {
            byte[] ret = new byte[count];
            for (int i = 0; i < count; i++)
            {
                ret[i] = ReadByte();
            }

            return ret;
        }

        public ushort ReadUInt16()
        {
            ushort ret = 0;
            for (int i = 0; i < 2 ; i++)
            {
                ret |= (ushort)(ReadByte() << i * 8);
            }

            return ret;
        }

        public int ReadBit()
        {
            if (offset == 8)
            {
                currentByte = Reader.ReadByte();
                offset = 0;
            }

            return (currentByte >> offset++) & 1;
        }

        public uint ReadBitsDepth(int bitDepth)
        {
            byte ret = 0;
            for (int i = 0; i < bitDepth; i++)
            {
                ret |= (byte)(ReadBit() << i);
            }

            return ret;
        }

        public void Dispose()
        {
            Reader.Dispose();
        }
    }
}