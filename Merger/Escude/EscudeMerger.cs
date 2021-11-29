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

namespace Merger.Escude
{
    [Export((typeof(BaseMerger)))]
    public class EscudeMerger: BasicMerger
    {
        public override string MethodName { get { return MergeMethodName.EscudeMethod; } }

        private Dictionary<string, Tuple<int, int>> offsets;

        public EscudeMerger()
        {

        }

        public override bool SetInitializeParameter(string picpath, string savePath,
            string offsetPath = null, string infoPath = null, string picFormat = null,
            string saveFormat = null)
        {
            bool rtn = base.SetInitializeParameter(picpath, savePath, offsetPath, infoPath, picFormat, saveFormat);
            if(string.IsNullOrWhiteSpace(offsetPath))
            {
                this.OffsetPath = picpath;
            }
            return rtn;
        }

        private HashSet<string> GetAllFileName()
        {
            HashSet<string> fileNames = new HashSet<string>();
            DirectoryInfo dir = new DirectoryInfo(this.PicPath);
            foreach (FileInfo item in dir.GetFiles())
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(item.Name));
            }
            return fileNames;
        }


        public override List<TreeNode> BuildTreesFromGroups(List<List<string>> picGroups = null)
        {
            DirectoryInfo dir = new DirectoryInfo(OffsetPath);
            List<TreeNode> nodes = new List<TreeNode>();
            offsets = new Dictionary<string, Tuple<int, int>>();
            HashSet<string> allFileNames = GetAllFileName();
            foreach (FileInfo item in dir.GetFiles("*.lsf"))
            {
                LSFParser parser = new LSFParser(item.FullName);
                if(!parser.ParseFile())
                {
                    continue;
                }
                List<List<string>> validPics = new List<List<string>>();
                //把路径中不存在的图片名从list中移除
                for(int i=0; i<parser.PicGroups.Count; i++)
                {
                    List<string> smallg = new List<string>();
                    for(int k=0; k<parser.PicGroups[i].Count; k++)
                    {
                        if (allFileNames.Contains(parser.PicGroups[i][k]))
                        {
                            smallg.Add(parser.PicGroups[i][k]);
                            offsets.Add(parser.PicGroups[i][k], parser.OffsetInfo[parser.PicGroups[i][k]]);
                        }
                    }
                    if(smallg.Count > 0)
                    {
                        validPics.Add(smallg);
                    }
                }
                BuildTreeHelper helper = new BuildTreeHelper();
                List<TreeNode> parts = helper.BuildTrees(validPics, new HashSet<int>());
                if (parts != null)
                    nodes.AddRange(parts);
            }
            picNodes = nodes;
            return picNodes;
        }

        public override IGetOffset GetDefaultOffseter()
        {
            return new EscudeOffseter(this.offsets);
        }


    }

    [Export(typeof(IGetOffset))]
    public class EscudeOffseter : IGetOffset 
    {
        public virtual string MethodName { get { return OffsetMethodName.EscudeOffsetName; } }

        private Dictionary<string, Tuple<int, int>> offsetCache = null;

        public EscudeOffseter(string filepath)
        {

        }

        public EscudeOffseter(Dictionary<string, Tuple<int, int>> offsets)
        {
            this.offsetCache = offsets;
        }



        public Tuple<int, int> GetOffset(string mainPicName, string subPicName)
        {
            if(false == offsetCache.ContainsKey(mainPicName) || false == offsetCache.ContainsKey(subPicName))
            {
                throw new ArgumentException("未找到对应文件的索引");
            }
            int xoff = offsetCache[subPicName].Item1 - offsetCache[mainPicName].Item1;
            int yoff = offsetCache[subPicName].Item2 - offsetCache[mainPicName].Item2;
            return new Tuple<int, int>(xoff, yoff);
        }

    }

}
