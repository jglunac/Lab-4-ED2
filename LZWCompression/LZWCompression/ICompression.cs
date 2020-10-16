using System;

namespace LZWCompression
{
    public interface ICompression
    {
        public byte[] Compress();
        public byte[] Decompress();
    }
}
