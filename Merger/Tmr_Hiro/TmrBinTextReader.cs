using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Merger.Tmr_Hiro
{
    internal class TmrBinTextReader
    {
        public string FilePath { get; set; }

        public List<string> stringTexts { get; private set; }

        public Encoding encoding { get; private set; }

        public TmrBinTextReader(string path, Encoding encoding=null)
        {
            this.FilePath = path;
            stringTexts = null;
            if(encoding == null)
            {
                this.encoding = Encoding.GetEncoding(932);
            }
            else
            {
                this.encoding = encoding;
            }
        }
        public bool ParseFile()
        {
            try
            {
                int curPos = 0;
                stringTexts = new List<string>();
                using (FileStream fs = new FileStream(FilePath, FileMode.Open))
                {
                    int readLen = Math.Min(4096 * 50, (int)fs.Length - curPos);
                    byte[] data = new byte[readLen];
                    fs.Seek(curPos, SeekOrigin.Begin);
                    fs.Read(data, 0, data.Length);
                    using(MemoryStream ms = new MemoryStream(data))
                    {
                        using(BinaryReader br = new BinaryReader(ms))
                        {
                            while (ms.Position < ms.Length)
                            {
                                if (ms.Length - ms.Position < 2)
                                    break;
                                ushort strlen = br.ReadUInt16();
                                if (ms.Length - ms.Position < strlen)
                                    break;
                                byte[] strdata = br.ReadBytes(strlen);
                                string str = this.encoding.GetString(strdata);
                                stringTexts.Add(str);
                                curPos += (2 + strlen);
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {
                stringTexts = null;
                return false;
            }
        }

    }
}
