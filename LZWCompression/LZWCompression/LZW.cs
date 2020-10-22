﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HuffmanCompression;
namespace LZWCompression
{
    public class LZW : IComp
    {
        #region Properties
        string Path;
        //El diccionario puede ser <string, int>?
        Dictionary<string, int> Characters = new Dictionary<string, int>();
        Dictionary<int, string> DecompressedCharacters = new Dictionary<int, string>();
        List<byte> FinalBytes = new List<byte>();
        int IDBits = 0;
        Queue<int> IDqueue;
        int DifferentCharacters;
        int bSize;
        #endregion

        #region Constructor
        public LZW(string path)
        {
            Path = path;
        }
        #endregion

        #region Compress
        public byte[] Compress(string path, string FileName, int bSize)
        {
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
                if (BinaryByte.Length == 8)
                {
                    FinalBytes.Add(Convert.ToByte(BinaryByte, 2));
                    BinaryByte = binary;
                }
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
                    FinalBytes.Insert(0, Convert.ToByte(IDBits));
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
        #endregion

        #region Decompress
        public byte[] Decompress(string path, int buffer)
        {
            bSize = buffer;
            int ContinuePoint = GetDifferentCharacters(path);
            FillIDQueue(ContinuePoint, path);
            GenerateTable(ContinuePoint, path);
            return FinalBytes.ToArray();
        }

        int GetDifferentCharacters(string path)
        {
            int toReturnPoint = 0;
            using (FileStream fs = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    IDBits = Convert.ToInt32(reader.ReadByte());
                    toReturnPoint++;
                    DifferentCharacters = Convert.ToInt32(reader.ReadByte());
                    toReturnPoint++;
                    for (int i = 0; i < DifferentCharacters; i++)
                    {
                        DecompressedCharacters.Add(i + 1, Convert.ToString(reader.ReadByte()));
                        toReturnPoint++;
                    }
                }
            }
            return toReturnPoint;
        }
        void GenerateTable(int fromHere, string path)
        {
            int counter = 0;
            string PrevCadena="";
            string ActualCadena = "";
            string PrevActual="";
            using (FileStream fs = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    counter += fromHere;
                    reader.ReadBytes(fromHere);
                    while (counter < fs.Length)
                    {
                        int TargetString = IDqueue.Peek();
                        IDqueue.Dequeue();
                        if (DecompressedCharacters.ContainsKey(TargetString))
                        {
                            DecompressedCharacters.TryGetValue(TargetString, out ActualCadena);
                            FinalBytes.Add(Convert.ToByte(ActualCadena));
                            PrevActual = PrevActual + ActualCadena.Substring(0, 1);
                            DecompressedCharacters.Add(DecompressedCharacters.Count + 1, PrevActual);
                        }
                        PrevCadena = ActualCadena;
                    }
                }
            }
        }

        void FillIDQueue(int fromHere, string path)
        {
            StringBuilder Binary = new StringBuilder();
            List<byte> Bytes = new List<byte>();
            byte[] aux;
            using (FileStream fs = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int counter = 0;
                    reader.ReadBytes(fromHere);
                    while (counter < fs.Length)
                    {
                        aux = reader.ReadBytes(bSize);
                        foreach (var item in aux)
                        {
                            Bytes.Add(item);
                        }

                        for (int i = 0; i < Bytes.Count; i++)
                        {
                            if (Binary.Length < IDBits)
                            {
                                Binary.Append(Convert.ToString(Convert.ToInt32(Bytes[i]), 2));
                            }
                            else
                            {
                                IDqueue.Enqueue(Convert.ToInt32(Binary.ToString().Substring(0, IDBits)));
                                string temprString = Binary.ToString().Substring(IDBits);
                                Binary = new StringBuilder();
                                Binary.Append(temprString);
                            }
                        }

                        counter += bSize;
                    }

                }
            }
        }
        #endregion
    }
}
