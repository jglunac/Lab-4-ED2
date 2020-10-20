using System;

namespace LZWCompression
{
    public interface IComp
    {
        public byte[] Compress();
        public byte[] Decompress();
    }
}
