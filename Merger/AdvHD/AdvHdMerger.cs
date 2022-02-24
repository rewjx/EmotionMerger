using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using ImageOP;
using Merger.core;
using System.IO;
using System.Drawing;
using System.ComponentModel.Composition;

namespace Merger.AdvHD
{
    [Export((typeof(BaseMerger)))]
    public class AdvHdMerger:BasicMerger
    {
        public override string MethodName { get { return MergeMethodName.AdvHDMethod; } }

        private Dictionary<string, Tuple<int, int>> offsets;

        public override bool SetInitializeParameter(string picpath, string savePath,
            string offsetPath = null, string infoPath = null, string picFormat = null,
            string saveFormat = null)
        {
            bool rtn = base.SetInitializeParameter(picpath, savePath, offsetPath, infoPath, picFormat, saveFormat);
            if (string.IsNullOrWhiteSpace(offsetPath))
            {
                this.OffsetPath = picpath;
            }
            PictureFormat = ImageNames.PNG;
            PictureExtension = ImageNames.PNG;
            return rtn;
        }

        public override Bitmap ReadImage(string name, bool isFullpath=false)
        {
            string realPath = name;
            if (!isFullpath)
            {
                string[] strs = name.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                realPath = Path.Combine(PicPath, strs[0]);
                realPath = Path.Combine(realPath, name);
            }
            realPath = realPath + "." + PictureExtension;
            Bitmap img = ImageIO.ReadImage(realPath);
            return img;
        }

        public void AddInList(ref List<List<string>> gs, string item, int idx)
        {
            while (gs.Count <= idx)
            {
                gs.Add(new List<string>());
            }
            gs[idx].Add(item);
        }

        public List<List<string>> ReadOneDirectoryForMerge(string dirName)
        {
            string fullDir = Path.Combine(PicPath, dirName);
            string offsetFile = Path.Combine(fullDir, "_cht.bin");
            PnaParser parser = new PnaParser(offsetFile);
            if (!parser.ParseFile())
                return null;
            List<List<string>> rtn = new List<List<string>>();
            for(int i=0; i< parser.pnaOffsets.Count; i++)
            {
                PnaInfoStruct info = parser.pnaOffsets[i];
                string useName = dirName + "#" + string.Format("{0:D3}", info.InnerIndex);
                string filename = Path.Combine(fullDir, useName + ".png");
                if (!File.Exists(filename))
                    continue;
                if(info.IsMainImg())
                {
                    AddInList(ref rtn, useName, 0);
                }
                else
                {
                    AddInList(ref rtn, useName, 1);
                }
                offsets[useName] = new Tuple<int, int>(info.offset_x, info.offset_y);
            }
            return rtn;

        }

        public override List<TreeNode> BuildTreesFromGroups(List<List<string>> picGroups = null)
        {
            offsets = new Dictionary<string, Tuple<int, int>>();
            DirectoryInfo dir = new DirectoryInfo(PicPath);
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (DirectoryInfo item in dir.GetDirectories())
            {
                List<List<string>> pics = ReadOneDirectoryForMerge(item.Name);
                BuildTreeHelper helper = new BuildTreeHelper();
                List<TreeNode> parts = helper.BuildTrees(pics, new HashSet<int>());
                if (parts != null)
                {
                    nodes.AddRange(parts);
                }
            }
            this.picNodes = nodes;
            return picNodes;
        }

        public override IGetOffset GetDefaultOffseter()
        {
            return new AdvHDOffseter(offsets);
        }
    }


    [Export(typeof(IGetOffset))]
    public class AdvHDOffseter : IGetOffset
    {
        public virtual string MethodName { get { return OffsetMethodName.AdvHdOffsetName; } }

        private Dictionary<string, Tuple<int, int>> offsetCache;

        public AdvHDOffseter(Dictionary<string, Tuple<int, int>> cache = null)
        {
            this.offsetCache = cache;
        }

        public Tuple<int, int> GetOffset(string mainPicName, string subPicName)
        {
            if (false == offsetCache.ContainsKey(mainPicName) || false == offsetCache.ContainsKey(subPicName))
            {
                throw new ArgumentException("未找到对应文件的索引");
            }
            int xoff = offsetCache[subPicName].Item1 - offsetCache[mainPicName].Item1;
            int yoff = offsetCache[subPicName].Item2 - offsetCache[mainPicName].Item2;
            return new Tuple<int, int>(xoff, yoff);
        }
    }
}
