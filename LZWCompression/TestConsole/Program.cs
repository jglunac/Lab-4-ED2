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
            //string path = @"C:\Users\brazi\Desktop\ESTRUCTURA DE DATOS II\easy test.txt";
            string path = @"C:\Users\brazi\Desktop\hola.txt";
            string path2 = @"C:\Users\brazi\Desktop\comprimidowe.txt";
            string path3 = @"C:\Users\brazi\Desktop\descomprimidowe.txt";
            LZW compression = new LZW(path);
            byte[] FileData = compression.Compress(path,"hola", 100);  
            using (FileStream fs = File.Create(path2))
            {
                fs.Write(FileData);
            }
            byte[] Decompressed = compression.Decompress(path2, 50);
            using (FileStream fs = File.Create(path3))
            {
                fs.Write(Decompressed);
            }
        }
    }
}
