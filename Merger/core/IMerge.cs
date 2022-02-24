using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Merger.core
{
    public interface IMerge
    {

        string MethodName { get; }

        /// <summary>
        /// 指定读取图像文件的方法
        /// </summary>
        /// <param name="fileName">不包含路径及扩展名的纯名字</param>
        /// <returns></returns>
        Bitmap ReadImage(string fileName, bool isFullpath = false);

        /// <summary>
        /// 读取文件后，执行合成前，进行图像的预处理操作
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        void PreProcessImage(ref Bitmap image);

        /// <summary>
        /// 两张图进行合成
        /// </summary>
        /// <param name="mainImg"></param>
        /// <param name="subImg"></param>
        /// <returns></returns>
        Bitmap MergeProcess(ref Bitmap mainImg, ref Bitmap subImg, Tuple<int,int> offset);

        /// <summary>
        /// 合成后，后处理操作
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        void PostProcess(ref Bitmap Image);

        /// <summary>
        /// 保存图片操作
        /// </summary>
        /// <param name="img"></param>
        /// <param name="saveName"></param>
        /// <returns></returns>
        bool SaveImage(ref Bitmap img, string saveName, bool isFullpath = false);

    }
}
