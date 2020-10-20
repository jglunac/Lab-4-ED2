using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LZWCompression;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\brazi\Desktop\hola.txt";
            string path2 = @"C:\Users\brazi\Desktop\comprimidowe.txt";
            LZW compression = new LZW(path);
            byte[] FileData = compression.Compress();

            string res = Encoding.ASCII.GetString(FileData);
            for (int i = 0; i < res.Length; i++)
            {
                char aux = res[i];
            }   
        }
    }
}
