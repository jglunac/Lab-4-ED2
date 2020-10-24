using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LZWCompression;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\joseg\Desktop\Pruebas LZW\Archivos\cuento.txt";
            string path2 = @"C:\Users\joseg\Desktop\Pruebas LZW\Descompresiones\cuento.txt";
            LZW compression = new LZW(path);
            byte[] byte1;
            byte[] byte2;
            using (FileStream fs = File.OpenRead(path2))
            {
                using (FileStream fs2 = File.OpenRead(path))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        using (BinaryReader reader2 = new BinaryReader(fs2))
                        {
                            int counter = 0;
                            while (counter<fs.Length)
                            {
                                byte1 = reader.ReadBytes(100);
                                byte2 = reader2.ReadBytes(100);
                                counter += 100;
                                Console.WriteLine(byte1.SequenceEqual(byte2));
                            }
                        }
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
