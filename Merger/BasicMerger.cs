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


namespace Merger
{

    /// <summary>
    /// 最基础的0偏移的合成方式
    /// </summary>
    [Export((typeof(BaseMerger)))]
    public class BasicMerger : BaseMerger
    {
        public override string MethodName { get { return MergeMethodName.BasicMethod; } }

        protected IGetOffset offsetCalcer = null;

        /// <summary>
        /// 待合成图片的文件夹路径
        /// </summary>
        protected string picPath;

        /// <summary>
        /// 图像格式
        /// </summary>
        protected string picFormat;

        /// <summary>
        /// 合成后保存路径
        /// </summary>
        protected string savePath;

        /// <summary>
        /// 合成后保存图片格式
        /// </summary>
        protected string saveFormat;

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

        public BasicMerger(string picpath, string savePath, string saveFormat = ImageNames.PNG)
        {
            SetInitializeParameter(picpath, savePath, saveFormat);
        }

        public virtual void  SetInitializeParameter(string picpath, string savePath,
            string saveFormat = ImageNames.PNG, string offsetPath=null)
        {
            this.picPath = picpath;
            this.savePath = savePath;
            this.saveFormat = saveFormat;
            IsInitialize = true;
        }

        public override Bitmap ReadImage(string name)
        {
            string fullPath = Path.Combine(this.picPath, name);
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
            string fullName = saveName + "." + saveFormat;
            string fullPath = Path.Combine(savePath, fullName);
            return ImageIO.SaveImage(img, fullPath, saveFormat);

        }

        public virtual List<TreeNode> BuildTrees(List<List<string>> picGroups = null)
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
            List<TreeNode> newNodes = BuildTrees(groups);
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
