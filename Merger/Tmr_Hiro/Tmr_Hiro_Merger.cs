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

namespace Merger.Tmr_Hiro
{
    [Export(typeof(BaseMerger))]
    public class Tmr_Hiro_Merger:BasicMerger
    {


        public override string MethodName { get { return MergeMethodName.TmrHiroMethod; } }

        /// <summary>
        /// 包含偏移信息的文件路径
        /// </summary>
        private string OffsetFilePath;

        private Dictionary<string, Tuple<int, int>> offsets;

        public Tmr_Hiro_Merger()
        {
            IsInitialize = false;
        }

        public override void SetInitializeParameter(string picpath, string savePath,
            string saveFormat = "PNG", string offsetPath = null)
        {
            base.SetInitializeParameter(picpath, savePath, saveFormat, offsetPath);
            if (string.IsNullOrWhiteSpace(offsetPath))
            {
                this.OffsetFilePath = picpath;
            }
            else
            {
                this.OffsetFilePath = offsetPath;
            }
            picFormat = "png";
        }

        public override Bitmap ReadImage(string name)
        {
            string fullPath = Path.Combine(picPath, name + "." + picFormat);
            return ImageIO.ReadImage(fullPath);
        }

        public void AddInList(ref List<List<string>> gs, string item, int idx)
        {
            while(gs.Count <= idx)
            {
                gs.Add(new List<string>());
            }
            gs[idx].Add(item);
        }

        public Dictionary<string, List<List<string>>> ReadPicturesForBuild()
        {
            DirectoryInfo dir = new DirectoryInfo(picPath);
            offsets = new Dictionary<string, Tuple<int, int>>();
            FileInfo[] files = dir.GetFiles("*.png");
            List<FileInfo> sortFiles = files.OrderBy(x => x.Name).ToList();
            Dictionary<string, List<List<string>>> picGroups = new Dictionary<string, List<List<string>>>();
            List<List<string>> oneGroup = null;
            int lastGID = -1;
            string lastMainName = null;
           
            foreach (FileInfo file in sortFiles)
            {
                string pureName = Path.GetFileNameWithoutExtension(file.Name);
                string grdName = Path.Combine(OffsetFilePath, pureName + ".grd");
                string[] parts = pureName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                int gid = int.Parse(parts[0]);
                int innerIdx = int.Parse(parts[3]);
                TmrGrdParser parser = new TmrGrdParser(grdName);
                bool success = parser.ParseFile();
                if (!success)
                {
                    continue;
                }
                if(gid  != lastGID || oneGroup == null || parser.IsMainImage())
                {
                    if(oneGroup != null)
                    {
                        picGroups[lastMainName] = oneGroup;
                    }
                    oneGroup = new List<List<string>>();
                    lastGID = gid;
                }
                if(parser.IsMainImage())
                {
                   // Console.WriteLine("ismain: " + pureName);
                    AddInList(ref oneGroup, pureName, 0);
                    lastMainName = pureName;
                }
                else
                {
                    //Console.WriteLine("not main: " + pureName);
                    AddInList(ref oneGroup, pureName, 1);
                }

                offsets[pureName] = parser.GetOffsetFromGRD();
            }
            if(oneGroup != null)
            {
                picGroups[lastMainName] = oneGroup;
            }
            return picGroups;
        }

        public override List<TreeNode> BuildTrees(List<List<string>> picGroups = null)
        {
            if (picGroups != null)
                return base.BuildTrees(picGroups);
            Dictionary<string, List<List<string>>> cgGroups = ReadPicturesForBuild();
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (var k in cgGroups.Keys)
            {
                BuildTreeHelper helper = new BuildTreeHelper();
                List<TreeNode> parts = helper.BuildTrees(cgGroups[k], new HashSet<int>());
                if(parts != null)
                {
                    nodes.AddRange(parts);
                }
            }
            this.picNodes = nodes;
            return picNodes;
        }

        public override IGetOffset GetDefaultOffseter()
        {
            return new TmrHiroOffseter(offsets);
        }
    }

    [Export(typeof(IGetOffset))]
    public class TmrHiroOffseter : IGetOffset
    {
        public virtual string MethodName { get { return OffsetMethodName.TmrHiroOffsetName; } }

        private Dictionary<string, Tuple<int, int>> offsetCache;

        public TmrHiroOffseter(Dictionary<string, Tuple<int,int>> cache=null)
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
