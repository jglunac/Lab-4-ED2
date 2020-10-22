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
            string path = @"C:\Users\joseg\Desktop\hola.txt";
            string path2 = @"C:\Users\joseg\Desktop\comprimidowe.txt";
            LZW compression = new LZW(path);
            byte[] FileData = compression.Compress(path,"hola", 100);

            //string res = Encoding.ASCII.GetString(FileData);
            //for (int i = 0; i < res.Length; i++)
            //{
            //    char aux = res[i];
            //}   
            byte[] DeCompressed = compression.Decompress()
            using (FileStream fs = File.Create(path2))
            {
                fs.Write(FileData);
            }
        }
    }
}
