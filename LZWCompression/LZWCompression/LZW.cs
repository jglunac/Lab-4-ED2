using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LZWCompression
{
    public class LZW : ICompression
    {
        #region Properties
        string Path;
        Dictionary<string, Character> Characters = new Dictionary<string, Character>();
        List<byte> FinalBytes = new List<byte>();
        #endregion

        #region Constructor
        public LZW(string path)
        {
            Path = path;
        }
        #endregion

        #region Compress
        public byte[] Compress()
        {
            GetOriginalCharacters();
            GenerateMeta();
            GetStrings();
            return FinalBytes.ToArray();
        }

        void GetOriginalCharacters()
        {
            using (FileStream fs = File.OpenRead(Path))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int counter = 0;
                    string Cadena;
                    StringBuilder sb = new StringBuilder();
                    int i = 1;
                    while (counter < fs.Length)
                    {
                        Cadena = Convert.ToChar(reader.ReadByte()).ToString();
                        if (!Characters.ContainsKey(Cadena))
                        {
                            Character character = new Character();
                            character.Value = Cadena;
                            character.Key = i;
                            Characters.Add(Cadena, character);
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
                    string Cadena = Convert.ToChar(reader.ReadByte()).ToString();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder resultbuilder = new StringBuilder();
                    int i = Characters.Count + 1;
                    Character AuxCharacter = new Character();
                    string result;
                    while (counter < fs.Length)
                    {
                        sb = new StringBuilder();
                        if (Characters.ContainsKey(Cadena))
                        {
                            counter++;
                            sb.Append(Cadena);
                            Characters.TryGetValue(Cadena, out Character character);
                            AuxCharacter = character;
                            if (counter<fs.Length)
                            {
                                sb.Append(Convert.ToChar(reader.ReadByte()).ToString());
                            }
                            else
                            {
                                string Binary = Convert.ToString(AuxCharacter.Key, 2);
                                Binary.PadLeft(8, '0');
                                byte TargetByte = Convert.ToByte(Binary, 2);
                                FinalBytes.Add(TargetByte);
                            }
                            Cadena = sb.ToString();
                            
                        }
                        else
                        {
                            string Binary = Convert.ToString(AuxCharacter.Key, 2);
                            byte TargetByte = Convert.ToByte(Binary,2);
                            FinalBytes.Add(TargetByte);
                            //resultbuilder.Append(Convert.ToString(AuxCharacter.Key, 2));
                            Character character = new Character();
                            character.Value = Cadena;
                            character.Key = i;
                            Characters.Add(Cadena, character);
                            char LastChar = Convert.ToChar(Cadena.Substring(Cadena.Length - 1, 1));
                            Cadena = LastChar.ToString();
                            i++;
                        }
                        
                    }
                    result = resultbuilder.ToString();
                }
            }
        }

        void GenerateMeta()
        {
            string DiffertentChars = Convert.ToString(Characters.Count, 2);
            DiffertentChars.PadLeft(8, '0');
            byte toMeta = Convert.ToByte(DiffertentChars, 2);
            FinalBytes.Add(toMeta);
            foreach (var item in Characters)
            {
                char baseChar = Convert.ToChar(item.Value.Value);
                byte baseByte = Convert.ToByte(baseChar);
                FinalBytes.Add(baseByte);
            }
        }
        #endregion

        #region Decompress
        public byte[] Decompress()
        {
            return null;
        }



        #endregion
    }
}
