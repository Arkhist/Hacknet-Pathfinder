using System.IO;

namespace Pathfinder.Util
{
    public class LZWBinaryReader
    {
        public BinaryReader Reader { get; }

        public LZWBinaryReader(BinaryReader reader)
        {
            Reader = reader;
        }

        public byte[] ReadBytes(int length)
        {
            return null;
        }
    }
}
