using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Merger.AdvHD
{

    public class PnaInfoStruct
    {
        public int unk1;
        public int idx;
        public int offset_x;
        public int offset_y;
        public int width;
        public int height;
        public int unk2;
        public int unk3;
        public int unk4;
        public int rawSize;

        public int InnerIndex;

        /// <summary>
        /// 判断该结构体是否有实际对应数据
        /// </summary>
        /// <returns></returns>
        public bool IsValidStruct()
        {
            return rawSize > 0;
        }

        public bool IsMainImg()
        {
            if (height >= 1000)
                return true;
            return false;
        }

        public override string ToString()
        {
            return InnerIndex.ToString() + "\t" + offset_x.ToString() + "\t" + offset_y.ToString();
        }
    }
    public class PnaParser
    {
        public List<PnaInfoStruct> pnaOffsets { get; private set; }

        public string FilePath { get; private set; }

        private int entryInfoSize = 40;

        public PnaParser(string path)
        {
            this.FilePath = path;
            pnaOffsets = null;
        }

        public PnaInfoStruct ReadOneStruct(BinaryReader br)
        {
            PnaInfoStruct data = new PnaInfoStruct();
            data.unk1 = br.ReadInt32();
            data.idx = br.ReadInt32();
            data.offset_x = br.ReadInt32();
            data.offset_y = br.ReadInt32();
            data.width = br.ReadInt32();
            data.height = br.ReadInt32();
            data.unk2 = br.ReadInt32();
            data.unk3 = br.ReadInt32();
            data.unk4 = br.ReadInt32();
            data.rawSize = br.ReadInt32();
            return data;
        }

        public bool ParseFile()
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] header = br.ReadBytes(20);
                    if (header.Length < 20)
                        return false;
                    int entryCount = 0;
                    using (MemoryStream ms = new MemoryStream(header))
                    {
                        using (BinaryReader hbr = new BinaryReader(ms))
                        {
                            ms.Seek(16, SeekOrigin.Begin);
                            entryCount = hbr.ReadInt32();
                        }
                    }
                    int infoSize = entryCount * entryInfoSize;
                    byte[] pinfo = br.ReadBytes(infoSize);
                    if (pinfo.Length < infoSize)
                        return false;
                    pnaOffsets = new List<PnaInfoStruct>();
                    //pnaOffsets = new List<Tuple<int, int>>();
                    int curIdx = 0;
                    using (MemoryStream ms = new MemoryStream(pinfo))
                    {
                        using (BinaryReader fbr = new BinaryReader(ms))
                        {
                            while (curIdx < entryCount)
                            {
                                PnaInfoStruct data = ReadOneStruct(fbr);
                                data.InnerIndex = curIdx;
                                if (data.IsValidStruct())
                                {
                                    pnaOffsets.Add(data);
                                    //pnaOffsets.Add(new Tuple<int, int>(data.offset_x, data.offset_y));
                                }
                                curIdx += 1;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }

}
