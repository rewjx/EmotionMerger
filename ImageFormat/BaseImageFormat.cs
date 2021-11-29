using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageFormat
{
    public abstract class BaseImageFormat
    {
        /// <summary>
        /// 图像格式tag，保证唯一，大多时候与扩展名一致
        /// </summary>
        public abstract string tag { get;  }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public abstract string extension { get; }

        /// <summary>
        /// which engines often use this image format
        /// </summary>
        public abstract IEnumerable<string> Engines { get; }

        /// <summary>
        /// whether support write in the image format
        /// </summary>
        public abstract bool canWrite { get; }

        /// <summary>
        /// if read failed, return null; else return bitmap image
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract Bitmap ReadImage(string path);

        public abstract bool WriteImage(Bitmap pic, string path);
    }
}
