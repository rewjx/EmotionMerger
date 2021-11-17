using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ImageOP;
using System.IO;
using Common;
using System.ComponentModel.Composition;

namespace Merger.core
{
    /// <summary>
    /// 最基础的0偏移的合成方式
    /// </summary>
   [Export((typeof(IMerge)))]
    public class BaseMerger:IMerge
    {
        public virtual string MethodName { get { return MergeMethodName.BasicMethod; } }

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

        protected List<TreeNode> taskNodes;

        public BaseMerger(string picpath, string savePath, string saveFormat=ImageNames.PNG)
        {
            this.picPath = picpath;
            this.savePath = savePath;
            this.saveFormat = saveFormat;

        }

        public virtual Bitmap ReadImage(string name)
        {
            string fullPath = Path.Combine(this.picPath, name);
            Bitmap img = ImageIO.ReadImage(fullPath);
            return img;
        }

        public virtual void PreProcessImage(ref Bitmap img)
        {

        }

        public virtual Bitmap MergeProcess(ref Bitmap mainImg, ref Bitmap subImg, Tuple<int, int> offset)
        {
            Bitmap rtn = ImageFunc.ImageBlend(mainImg, subImg, new AlphaBlend(), offset.Item1, offset.Item2);
            return rtn;
        }

        public virtual void PostProcess(ref Bitmap Image)
        {

        }

        public virtual bool SaveImage(ref Bitmap img, string saveName)
        {
            if (img == null)
                return true;
            string fullName = saveName + "." + saveFormat;
            string fullPath = Path.Combine(savePath, fullName);
            return ImageIO.SaveImage(img, fullPath, saveFormat);

        }

        /// <summary>
        /// 获取该方式默认的偏移坐标计算方式
        /// </summary>
        /// <returns></returns>
        public virtual IGetOffset GetDefaultOffseter()
        {
            return new ZeroOffseter();
        }

        public virtual List<TreeNode> BuildTrees(List<List<string>> picGroups = null)
        {
            BuildTreeHelper helper = new BuildTreeHelper();
            return helper.BuildTrees(picGroups, new HashSet<int>());
        }

        public virtual List<TreeNode> GetTreeNodes()
        {
            return this.taskNodes;
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
