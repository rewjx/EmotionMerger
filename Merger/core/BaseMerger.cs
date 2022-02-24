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
        /// <summary>
        /// 方法名
        /// </summary>
        public abstract string MethodName { get; }

        /// <summary>
        /// 待合成图片路径
        /// </summary>
        public abstract string PicPath { get; protected set; }

        /// <summary>
        /// 合成后图片保存路径
        /// </summary>
        public abstract string SavePath { get; protected set; }

        /// <summary>
        /// 包含图片偏移位置信息的文件路径
        /// </summary>
        public abstract string OffsetPath { get; protected set; }

        /// <summary>
        /// 其他所需信息，如cg表等文件路径
        /// </summary>
        public abstract string InfoPath { get; protected set; }

        /// <summary>
        /// 合成后图片保存格式,需唯一标识，对应ImageFormat的tag
        /// </summary>
        public abstract string SaveFormat { get; protected set; }

        /// <summary>
        /// 待合成的图片格式，需唯一标识，对应ImageFormat的tag
        /// </summary>
        public abstract string PictureFormat { get; protected set; }

        public abstract Bitmap MergeProcess(ref Bitmap mainImg, ref Bitmap subImg, Tuple<int, int> offset);
        public abstract void PostProcess(ref Bitmap Image);
        public abstract void PreProcessImage(ref Bitmap image);
        public abstract Bitmap ReadImage(string fileName, bool isFullpath = false);
        public abstract bool SaveImage(ref Bitmap img, string saveName, bool isFullpath = false);

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

        /// <summary>
        ///  设置图片路径，保存路径，包含偏移信息文件的路径，其他信息(如cg表立绘表等文件)的路径，
        ///  以及待读取的图片格式，图片保存格式等参数。
        /// </summary>
        /// <param name="picpath"></param>
        /// <param name="savePath"></param>
        /// <param name="offsetPath"></param>
        /// <param name="infoPath"></param>
        /// <param name="picFormat"></param>
        /// <param name="saveFormat"></param>
        public abstract bool SetInitializeParameter(string picpath, string savePath,
            string offsetPath = null, string infoPath = null, string picFormat = null,
            string saveFormat = null);

    }


    public class PicRuleItem
    {
        /// <summary>
        /// 合成一张图片所需的全部图片名,注意不包括扩展名
        /// </summary>
        public List<string> PicRules { get; set; }

        /// <summary>
        /// 图片组的路径
        /// </summary>
        public string PicPath { get; set; }

        /// <summary>
        /// 合成后图片保存名字
        /// </summary>
        public string SaveName { get; set; }


        public PicRuleItem() { }

        public PicRuleItem(List<string> picnames, string savename, string picpath=null)
        {
            this.PicRules = picnames;
            this.SaveName = savename;
            this.PicPath = picpath;
        }
    }

}
