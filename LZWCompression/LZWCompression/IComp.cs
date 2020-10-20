using System;

namespace LZWCompression
{
    public interface IComp
    {
        public byte[] Compress(string path, string FileName, int bSize);
        public byte[] Decompress(string path, int buffer);
    }
}
