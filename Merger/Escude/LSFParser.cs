using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Merger.Escude
{
    class LSFParser
    {

        public uint Signature { get { return 0x0046534C; } }

        private int HeaderSize = 28;

        private int ItemSize = 164;


        public string filePath { get; private set; }

        private Dictionary<string, Tuple<int, int>> offsets = null;

        public Dictionary<string, Tuple<int, int>> OffsetInfo
        {
            get
            {
                return offsets;
            }
        }

        private List<List<string>> picGroups = null;

        public List<List<string>> PicGroups
        {
            get
            {
                return picGroups;
            }
        }
        public LSFParser(string path)
        {
            this.filePath = path;
        }

        public bool ParseFile()
        {
            if (File.Exists(this.filePath) == false)
                return false;
            using(FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using(BinaryReader br = new BinaryReader(fs))
                {
                    uint sig = br.ReadUInt32();
                    if (sig != Signature)
                        return false;
                    fs.Seek(10, SeekOrigin.Begin);
                    uint totalCount = br.ReadUInt16();
                    if (HeaderSize + totalCount * ItemSize > fs.Length)
                        return false;
                    offsets = new Dictionary<string, Tuple<int, int>>();
                    picGroups = new List<List<string>>();
                    for(int i=0; i<totalCount; i++)
                    {
                        fs.Seek(HeaderSize + i * ItemSize, SeekOrigin.Begin);
                        if (!ParseItem(br))
                        {
                            offsets = null;
                            picGroups = null;
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool ParseItem(BinaryReader br)
        {
            try
            {
                string name = null;
                byte[] nameBytes = br.ReadBytes(0x80);
                name = Encoding.ASCII.GetString(nameBytes);
                name = name.Substring(0, name.IndexOf('\0'));
                int xoff = br.ReadInt32();
                int yoff = br.ReadInt32();
                br.BaseStream.Seek(16, SeekOrigin.Current);
                byte idx = br.ReadByte();
                if(!string.IsNullOrWhiteSpace(name))
                {
                    offsets[name] = new Tuple<int, int>(xoff, yoff);
                    while(picGroups.Count <= idx)
                    {
                        picGroups.Add(new List<string>());
                    }
                    picGroups[idx].Add(name);
                }
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}
