using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Merger.Tmr_Hiro
{
    class TmrGrdParser
    {

        public int Screen_Width { get; private set; }

        public int Screen_Height { get; private set; }

        public int left { get; private set; }

        public int right { get; private set; }

        public int top { get; private set; }

        public int bottom { get; private set; }

        private bool isSucceess = false;

        private string path = null;

        private int fileSize = 0;

        public TmrGrdParser(string path)
        {
            this.path = path;
        }

        public bool ParseFile()
        {
            if (!File.Exists(path))
            {
                isSucceess = false;
                return false;
            }
            byte[] header = null;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                header = new byte[0x10];
                int realSize = fs.Read(header, 0, 0x10);
                fileSize = (int)fs.Length;
                if (realSize < 0x10)
                {
                    isSucceess = false;
                    return false;
                }
               
            }
            using (MemoryStream ms = new MemoryStream(header))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    if (header[0] != 1 && header[0] != 2)
                        isSucceess = false;
                    if (header[1] != 1 && header[1] != 0xA1 && header[1] != 0xA2)
                        isSucceess = false;
                    ms.Seek(2, SeekOrigin.Begin);
                    Screen_Width = br.ReadUInt16();
                    Screen_Height = br.ReadUInt16();
                    ms.Seek(8, SeekOrigin.Begin);
                    left = br.ReadUInt16();
                    right = br.ReadUInt16();
                    top = br.ReadUInt16();
                    bottom = br.ReadInt16();
                }
            }
            isSucceess = true;
            return true;
            
        }
        public Tuple<int,int> GetOffsetFromGRD()
        {
            if (!isSucceess)
                return null;
            return new Tuple<int, int>(left, Screen_Height - bottom);

        }

        public bool IsMainImage()
        {
            if (Screen_Height != 720 || Screen_Width != 1280)
                return true;
            if (Screen_Width == 1280 && Screen_Height == 720 && fileSize < 1600000)
                return false;
            return left == 0 && top == 0 && right == Screen_Width && bottom == Screen_Height;
        }
    }
}
