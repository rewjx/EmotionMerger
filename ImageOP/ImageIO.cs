using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFormat;
using System.Drawing;
using System.IO;


namespace ImageOP
{
    public class ImageIO
    {

        private static Bitmap TryReadImage(string path, IEnumerable<BaseImageFormat>readers)
        {
            if (readers == null)
                return null;
            if (false == File.Exists(path))
                return null;
            foreach (var r in readers)
            {
                Bitmap img = r.ReadImage(path);
                if (img != null)
                    return img;
            }
            return null;
        }

        /// <summary>
        /// 自动遍历所有实现的图片格式来读取图片
        /// 读取成功，返回bitmap，否则返回null
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadImage(string path)
        {
            IEnumerable<BaseImageFormat> formats = ImageFormatCatalog.Instance.FindAllFormats();
            Bitmap img = TryReadImage(path, formats);
            return img;
        }

        /// <summary>
        /// 查找符合特定tag(通常是后缀名)的图像格式，尝试以该格式打开图像文件
        /// 成功，返回bitmap，否则返回null
        /// </summary>
        /// <param name="path"></param>
        /// <param name="imgFormat"></param>
        /// <returns></returns>
        public static Bitmap ReadImageFilterByTag(string path, string imgFormat)
        {
            IEnumerable<BaseImageFormat> formats = ImageFormatCatalog.Instance.FindSpecialTagFormats(imgFormat);
            Bitmap img = TryReadImage(path, formats);
            return img;
        }

        public static bool SaveImage(Bitmap img, string path, string saveFormat)
        {
            IEnumerable<BaseImageFormat> formats = ImageFormatCatalog.Instance.FindSpecialTagFormats(saveFormat);
            if(formats == null || formats.Count() == 0)
            {
                return false;
            }
            foreach (var writer in formats)
            {
                if(writer.WriteImage(img, path))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
