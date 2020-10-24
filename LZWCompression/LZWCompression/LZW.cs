using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using HuffmanCompression;
namespace LZWCompression
{
    public class LZW : ICompression
    {
        #region Properties
        string Path;
        //El diccionario puede ser <string, int>?
        Dictionary<string, int> Characters = new Dictionary<string, int>();
        Dictionary<int, string> DecompressedCharacters = new Dictionary<int, string>();
        List<byte> FinalBytes = new List<byte>();
        int IDBits = 0;
        Queue<int> IDqueue = new Queue<int>();
        int DifferentCharacters;
        int bSize;
        public string Name;
        #endregion
        public LZW(string path)
        {
            Path = path;
        }

        #region Compress
        public byte[] Compress(string path, string FileName, int bSize)
        {
            FinalBytes.Clear();
            Characters.Clear();
            Name = FileName;
            AddMetaName();
            IDqueue = new Queue<int>();
            GetOriginalCharacters();
            GenerateMeta();
            GetStrings();
            AddIDQueue();
            return FinalBytes.ToArray();
        }
        void AddIDQueue()
        {
            int count = IDqueue.Count;
            string BinaryByte = "";
            string binary = "";
            for (int i = 0; i < count; i++)
            {
                binary = (Convert.ToString(IDqueue.Dequeue(), 2)).PadLeft(IDBits, '0');
                int missingBits = 8 - BinaryByte.Length;
                if (missingBits <= binary.Length)
                {
                    BinaryByte += binary.Substring(0, missingBits);
                    binary = binary.Remove(0, missingBits);
                }
                else BinaryByte += binary;
                
                while (BinaryByte.Length>=8)
                {
                    FinalBytes.Add(Convert.ToByte(BinaryByte.Substring(0,8), 2));
                    
                    if(BinaryByte.Length == 8)
                    {
                        BinaryByte = binary;
                        binary = "";
                        
                    }
                    else BinaryByte = BinaryByte.Remove(0, 8);

                }
                
            }
            if (BinaryByte != "")
            {
                BinaryByte = BinaryByte.PadRight(8, '0');
                FinalBytes.Add(Convert.ToByte(BinaryByte, 2));
            }
        }

        void GetOriginalCharacters()
        {
            using (FileStream fs = File.OpenRead(Path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int counter = 0;
                    string Cadena;
                    
                    int i = 1;
                    while (counter < fs.Length)
                    {
                        Cadena = Convert.ToChar(reader.ReadByte()).ToString();
                        if (!Characters.ContainsKey(Cadena))
                        {
                            //Character character = new Character();
                            //character.Value = Cadena;
                            //character.Key = i;
                            Characters.Add(Cadena, i);
                            i++;
                        }
                        
                        counter++;
                    }
                }
            }
        }

        void GetStrings()
        {
            using (FileStream fs = File.OpenRead(Path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int counter = 0;
                    //string CurrentString = Convert.ToChar(reader.ReadByte()).ToString();
                    StringBuilder CurrentString = new StringBuilder();
                    CurrentString.Append(Convert.ToChar(reader.ReadByte()).ToString());
                    //StringBuilder resultbuilder = new StringBuilder();
                    int i = Characters.Count + 1;
                    int ExistentStringID = 0;
                    //Character AuxCharacter = new Character();
                    //string result;
                    while (counter < fs.Length)
                    {
                        //CurrentString = new StringBuilder();
                        if (Characters.ContainsKey(CurrentString.ToString()))
                        {
                            counter++;
                            //CurrentString.Append(CurrentString);
                            Characters.TryGetValue(CurrentString.ToString(), out ExistentStringID);
                            
                            //AuxCharacter = character;
                            if (counter<fs.Length)
                            {
                                CurrentString.Append(Convert.ToChar(reader.ReadByte()).ToString());
                            }
                            else
                            {
                                string Binary = Convert.ToString(ExistentStringID, 2);
                                //Binary.PadLeft(8, '0');
                                //byte TargetByte = Convert.ToByte(Binary, 2);

                                if (Binary.Length > IDBits)
                                {
                                    IDBits = Binary.Length;
                                }
                                IDqueue.Enqueue(ExistentStringID);

                            }
                            //CurrentString = CurrentString.ToString();

                        }
                        else
                        {
                            string Binary = Convert.ToString(ExistentStringID, 2);
                            //byte TargetByte = Convert.ToByte(Binary,2);
                            //FinalBytes.Add(TargetByte);
                            if (Binary.Length > IDBits)
                            {
                                IDBits = Binary.Length;
                            }
                            IDqueue.Enqueue(ExistentStringID);
                            //resultbuilder.Append(Convert.ToString(AuxCharacter.Key, 2));
                            //Character character = new Character();
                            //character.Value = CurrentString;
                            //character.Key = i;
                            Characters.Add(CurrentString.ToString(), i);
                            string LastChar = CurrentString.ToString(CurrentString.Length - 1, 1);
                            CurrentString.Clear();
                            CurrentString.Append(LastChar);
                            i++;
                        }
                        
                    }
                    FinalBytes.Insert(Name.Length+1, Convert.ToByte(IDBits));
                    //result = resultbuilder.ToString();
                }
            }
        }

        void GenerateMeta()
        {
            //string DifferentChars = Convert.ToString(Characters.Count, 2);
            //DifferentChars.PadLeft(8, '0');
            //byte toMeta = Convert.ToByte(DifferentChars, 2);
            //FinalBytes.Add(toMeta);
           
            FinalBytes.Add(Convert.ToByte(Characters.Count));
            foreach (var item in Characters)
            {
                //char baseChar = Convert.ToChar(item.Value.Value);
                //byte baseByte = Convert.ToByte(baseChar);
                FinalBytes.Add((byte)Convert.ToChar(item.Key));
            }
        }

        void AddMetaName()
        {
            for (int i = 0; i < Name.Length; i++)
            {
                FinalBytes.Add((byte)Name[i]);
            }
            FinalBytes.Add(10);
        }
        #endregion

        #region Decompress
        public byte[] Decompress(string path, int buffer)
        {
            bSize = buffer;
            DecompressedCharacters.Clear();
            FinalBytes.Clear();
            IDqueue.Clear();

            int ContinuePoint = GetDifferentCharacters(path, GetMetaName(path));
            FillIDQueue(ContinuePoint, path);
            GenerateTable(ContinuePoint, path);
            return FinalBytes.ToArray();
        }
        int GetMetaName(string path)
        {
            int ReadingPoint = 0;
            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    
                    Name = "";
                    char nameChar;
                    do
                    {
                      nameChar = Convert.ToChar(reader.ReadByte());
                        if (nameChar != 10)
                        {
                            Name += nameChar.ToString();
                        }
                        ReadingPoint++;
                    } while (nameChar != 10);
                }
            }
            return ReadingPoint;
        }
        int GetDifferentCharacters(string path, int toReturn)
        {
            int toReturnPoint = toReturn;
            using (FileStream fs = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    reader.ReadBytes(toReturn);
                    IDBits = Convert.ToInt32(reader.ReadByte());
                    toReturnPoint++;
                    DifferentCharacters = Convert.ToInt32(reader.ReadByte());
                    toReturnPoint++;
                    for (int i = 0; i < DifferentCharacters; i++)
                    {
                        DecompressedCharacters.Add(i + 1, Convert.ToChar(reader.ReadByte()).ToString());
                        toReturnPoint++;
                    }
                }
            }
            return toReturnPoint;
        }
        void GenerateTable(int fromHere, string path)
        {
            int counter = DifferentCharacters+1;
            string PrevString="";
            string ActualString = "";
            string PrevPlusActualFirst="";
            while (IDqueue.Count >0)
            {
                int prueba = IDqueue.Peek();
                if (DecompressedCharacters.ContainsKey(IDqueue.Peek()))
                {
                    DecompressedCharacters.TryGetValue(IDqueue.Dequeue(), out ActualString);
                    if (PrevString != "")
                    {
                        PrevPlusActualFirst = PrevString + ActualString.Substring(0, 1);
                        if (!DecompressedCharacters.ContainsValue(PrevPlusActualFirst))
                        {
                            DecompressedCharacters.Add(counter, PrevPlusActualFirst);
                            counter++;
                        }
                    }   
                }
                else
                {
                    ActualString = PrevString + PrevString[0];
                    IDqueue.Dequeue();
                    if (!DecompressedCharacters.ContainsValue(ActualString))
                    {
                        DecompressedCharacters.Add(counter, ActualString);
                        counter++;
                    }
                }
                PrevString = ActualString;
                //Hasta aquí están iguales en texto y en diccionario
                for (int i = 0; i < ActualString.Length; i++)
                {
                    FinalBytes.Add((byte)ActualString[i]);
                }
                //Aquí ya hay más texto en la descompresión
            }
        }
        void FillIDQueue(int fromHere, string path)
        {
            StringBuilder Binary = new StringBuilder();
            List<byte> Bytes = new List<byte>();
            byte[] ByteArray;
            using (FileStream fs = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int counter = 0;
                    reader.ReadBytes(fromHere);
                    while (counter < fs.Length)
                    {
                        ByteArray = reader.ReadBytes(bSize);
                        for (int i = 0; i < ByteArray.Length; i++)
                        {
                            Binary.Append(Convert.ToString(Convert.ToInt32(ByteArray[i]), 2).PadLeft(8,'0'));
                            while (Binary.Length >= IDBits)
                            {
                                int ToIDqueue = Convert.ToInt32(Binary.ToString(0, IDBits), 2);
                                if (ToIDqueue > 0)
                                {
                                    IDqueue.Enqueue(ToIDqueue);
                                }
                                Binary.Remove(0, IDBits);
                            }
                        }
                        counter += bSize;
                    }
                }
            }
        }
        #endregion
        #region Json
        public void UpdateCompressions(string path, string name, string route, double originalSize, double CompressedSize)
        {
            double compressionFactor, compressionRatio, reductionPercentage;

            compressionRatio = CompressedSize / originalSize;
            compressionFactor = originalSize / CompressedSize;
            reductionPercentage = compressionRatio * 100;

            LZWCompression compression = new LZWCompression();
            compression.OriginalName = name;
            compression.CompressedFilePath = route;
            compression.CompressionRatio = compressionRatio;
            compression.CompressionFactor = compressionFactor;
            compression.ReductionPercentage = reductionPercentage;

            path += @"\CompressedFiles.json";

            List<LZWCompression> PreviousFile = new List<LZWCompression>();
            if (File.Exists(path))
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    string file;
                    MemoryStream memory = new MemoryStream();
                    fs.CopyTo(memory);
                    file = Encoding.ASCII.GetString(memory.ToArray());
                    if (file != "")
                    {
                        PreviousFile = DeserializeCompression(file);
                    }
                }
            }
            PreviousFile.Add(compression);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(WriteJson(PreviousFile));
            }

        }
        public string WriteJson(List<LZWCompression> list)
        {
            return JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        public static List<LZWCompression> DeserializeCompression(string content)
        {
            return JsonSerializer.Deserialize<List<LZWCompression>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        #endregion
    }
}
