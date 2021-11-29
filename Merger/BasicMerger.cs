using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Drawing;
using ImageOP;
using Merger.core;
using System.IO;
using Common;
using ImageFormat;

namespace Merger
{

    /// <summary>
    /// 最基础的0偏移的合成方式
    /// </summary>
    [Export((typeof(BaseMerger)))]
    public class BasicMerger : BaseMerger
    {
        public override string MethodName { get { return MergeMethodName.BasicMethod; } }

        public override string PicPath { get; protected set; }

        public override string SavePath { get; protected set; }

        public override string OffsetPath { get; protected set; }

        public override string InfoPath { get; protected set; }

        public override string SaveFormat { get; protected set; }

        public override string PictureFormat { get; protected set; }

        /// <summary>
        /// 待合成图片的图像文件扩展名
        /// </summary>
        public virtual string PictureExtension { get; protected set; }

        protected IGetOffset offsetCalcer = null;

        /// <summary>
        /// 存储图片合成森林的根节点
        /// </summary>
        protected List<TreeNode> picNodes = null;

        /// <summary>
        /// 存储图片合成规则
        /// </summary>
        protected List<PicRuleItem> picRules = null;

        protected bool IsInitialize { get; set; }

        public BasicMerger()
        {
            IsInitialize = false;
        }

        public override bool SetInitializeParameter(string picpath, string savePath,
            string offsetPath = null, string infoPath = null, string picFormat = null,
            string saveFormat = null)
        {
            this.PicPath = picpath;
            this.SavePath = savePath;
            this.OffsetPath = offsetPath;
            this.InfoPath = infoPath;
            if(picFormat == null)
            {
                picFormat = ImageIO.AutoFindPictureFormat(picpath);
            }
            if(saveFormat == null)
            {
                saveFormat = ImageNames.PNG;
            }
            this.PictureFormat = picFormat;
            this.SaveFormat = saveFormat;
            if (this.PictureFormat == null)
            {
                PictureExtension = null;
                return false;
            }
            PictureExtension = ImageFormatCatalog.Instance.GetExtensionsByTag(PictureFormat);
            IsInitialize = true;
            return true;
        }


        public override Bitmap ReadImage(string name)
        {
            string fullPath = name;
            if (File.Exists(fullPath) == false)
            {
                fullPath = Path.Combine(this.PicPath, name +"." + PictureExtension);
            }
            Bitmap img = ImageIO.ReadImage(fullPath);
            return img;
        }

        public override void PreProcessImage(ref Bitmap img)
        {

        }

        public override Bitmap MergeProcess(ref Bitmap mainImg, ref Bitmap subImg, Tuple<int, int> offset)
        {
            Bitmap rtn = ImageFunc.ImageBlend(mainImg, subImg, new AlphaBlend(), offset.Item1, offset.Item2);
            return rtn;
        }

        public override void PostProcess(ref Bitmap Image)
        {

        }

        public override bool SaveImage(ref Bitmap img, string saveName)
        {
            if (img == null)
                return true;
            string fullPath = Path.Combine(this.SavePath, saveName);
            return ImageIO.SaveImage(img, fullPath, this.SaveFormat);

        }

        public virtual List<TreeNode> BuildTreesFromGroups(List<List<string>> picGroups = null)
        {
            BuildTreeHelper helper = new BuildTreeHelper();
            picNodes =  helper.BuildTrees(picGroups, new HashSet<int>());
            return picNodes;
        }

        public virtual List<PicRuleItem> GetRulesFromTreeNode(List<TreeNode> nodes = null)
        {
            if (nodes == null)
                nodes = this.picNodes;
            if (nodes == null)
                return null;
            List<PicRuleItem> rules = new List<PicRuleItem>();
            for(int i=0; i<nodes.Count; i++)
            {
                string mName = nodes[i].FileName;
                int count = 0;
                foreach (var item in nodes[i].DFSStepTree())
                {
                    item.Reverse();
                    PicRuleItem rule = new PicRuleItem(item, mName + "_" + string.Format("{0:D5}", count));
                    rules.Add(rule);
                    count += 1;
                }
            }
            return rules;
            
        }

        public override List<TreeNode> GetTreeNodes()
        {
            return this.picNodes;
        }

        public override List<PicRuleItem> GetPicturesRules()
        {
            if(this.picRules != null)
            {
                return this.picRules;
            }
            //根据合成树得到合成规则
            if(this.picNodes  != null)
            {
                return GetRulesFromTreeNode();
            }
            return null;
        }

        public override void SetPicturesRules(List<PicRuleItem> items)
        {
            this.picRules = items;
        }

        public override void SetPicturesGroups(List<List<string>> groups)
        {
            if (groups == null || groups.Count == 0)
                return;
            List<TreeNode> newNodes = BuildTreesFromGroups(groups);
            if(newNodes != null)
            {
                picNodes = newNodes;
            }
        }

        public override bool SetOffseter(IGetOffset func)
        {
            this.offsetCalcer = func;
            return true;   
        }

        public override IGetOffset GetDefaultOffseter()
        {
            return new ZeroOffseter();
        }

        public override IGetOffset GetOffseter()
        {
            if (offsetCalcer == null)
                return GetDefaultOffseter();
            return offsetCalcer;
        }
    }



    [Export(typeof(IGetOffset))]
    public class FixedOffseter : IGetOffset
    {
        public virtual string MethodName { get { return OffsetMethodName.FixedOffsetName; } }

        private int xoff = 0;

        private int yoff = 0;

        public FixedOffseter(int x, int y)
        {
            xoff = x;
            yoff = y;
        }

        public virtual Tuple<int, int> GetOffset(string mainPicName, string subPicName)
        {
            return new Tuple<int, int>(xoff, yoff);
        }

    }

    [Export(typeof(IGetOffset))]
    public class ZeroOffseter : IGetOffset
    {
        public virtual string MethodName { get { return OffsetMethodName.ZeroOffsetName; } }


        public virtual Tuple<int, int> GetOffset(string mainPicName, string subPicName)
        {
            return new Tuple<int, int>(0, 0);
        }
    }
}
