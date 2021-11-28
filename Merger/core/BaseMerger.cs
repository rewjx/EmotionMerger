using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Merger.core
{
    /// <summary>
    /// 所有图像合成类的基类
    /// </summary>
    public abstract class BaseMerger : IMerge
    {
        public abstract string MethodName { get; }

        public abstract Bitmap MergeProcess(ref Bitmap mainImg, ref Bitmap subImg, Tuple<int, int> offset);
        public abstract void PostProcess(ref Bitmap Image);
        public abstract void PreProcessImage(ref Bitmap image);
        public abstract Bitmap ReadImage(string fileName);
        public abstract bool SaveImage(ref Bitmap img, string saveName);

        /// <summary>
        /// 获取图片合成树状依赖
        /// </summary>
        /// <returns></returns>
        public abstract List<TreeNode> GetTreeNodes();

        /// <summary>
        /// 获取单条规则形式的合成依赖
        /// </summary>
        /// <returns></returns>
        public abstract List<PicRuleItem> GetPicturesRules();

        /// <summary>
        /// 直接外部设置图片规则
        /// </summary>
        /// <param name="items"></param>
        public abstract void SetPicturesRules(List<PicRuleItem> items);

        /// <summary>
        /// 直接设置图片分组，并以据分组自动构建图片合成树
        /// </summary>
        /// <param name="groups"></param>
        public abstract void SetPicturesGroups(List<List<string>> groups);

        /// <summary>
        /// 设置偏移计算方式
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public abstract bool SetOffseter(IGetOffset func);
       
        /// <summary>
        /// 获取默认的偏移计算方式
        /// </summary>
        /// <returns></returns>
        public abstract IGetOffset GetDefaultOffseter();

        /// <summary>
        /// 返回偏移计算方式
        /// </summary>
        /// <returns></returns>
        public abstract IGetOffset GetOffseter();

    }


    public class PicRuleItem
    {
        /// <summary>
        /// 合成一张图片所需的全部图片名
        /// </summary>
        public List<string> PicRules { get; set; }

        /// <summary>
        /// 合成后图片保存名字
        /// </summary>
        public string SaveName { get; set; }


        public PicRuleItem() { }

        public PicRuleItem(List<string> picnames, string savename)
        {
            this.PicRules = picnames;
            this.SaveName = savename;
        }
    }

}
