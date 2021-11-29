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

        /// <summary>
        /// 传入无扩展名的path，将会根据saveFormat自动补全文件扩展名
        /// </summary>
        /// <param name="img"></param>
        /// <param name="path"></param>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        public static bool SaveImage(Bitmap img, string path, string saveFormat)
        {
            IEnumerable<BaseImageFormat> formats = ImageFormatCatalog.Instance.FindSpecialTagFormats(saveFormat);
            if(formats == null || formats.Count() == 0)
            {
                return false;
            }
            foreach (var writer in formats)
            {
                string fullPath = path + "." + writer.extension;
                if(writer.WriteImage(img, fullPath))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据图片自动获取能打开该图片的图片格式tag
        /// 若都不能打开返回null
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetImageFormat(string imgPath, string extension=null)
        {
            IEnumerable<BaseImageFormat> formats = null;
            if(extension != null)
            {
                formats = ImageFormatCatalog.Instance.FindSpecialExtensionFormats(extension);
            }
            else
            {
                formats = ImageFormatCatalog.Instance.FindAllFormats();
            }
            if (formats == null)
                return null;
            foreach (var f in formats)
            {
                Bitmap pic = f.ReadImage(imgPath);
                if(pic != null)
                {
                    return f.tag;
                }

            }
            return null;
        }

        /// <summary>
        /// 自动识别某图片文件的tag(format)
        /// 或某文件夹下最多种类的图片的tag
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AutoFindPictureFormat(string path)
        {
            //是文件，则直接返回该文件对应的图片tag
            if (File.Exists(path))
            {
                string ext = Path.GetExtension(path);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    ext = null;
                }
                return ImageIO.GetImageFormat(path, ext);
            }
            //是文件夹，则先统计该文件夹下最多数目的图像扩展名，然后获取该扩展名对应的tag
            HashSet<string> allExts = new HashSet<string>();
            foreach (string item in ImageFormatCatalog.Instance.GetAllExtensions())
            {
                allExts.Add(item.ToLower());
            }
            Dictionary<string, int> counts = new Dictionary<string, int>();
            Dictionary<string, string> filelog = new Dictionary<string, string>();
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (FileInfo item in dir.GetFiles())
                {
                    string ext = item.Extension.ToLower();
                    if (allExts.Contains(ext))
                    {
                        if(counts.ContainsKey(ext))
                        {
                            counts[ext] += 1;
                        }
                        else
                        {
                            counts[ext] = 1;
                            filelog[ext] = item.FullName;
                        }
                    }
                }
                int maxCount = counts.Values.Max();
                var maxExts = counts.Where(item => item.Value == maxCount).ToList();
                foreach (var item in maxExts)
                {
                    string rtn = GetImageFormat(filelog[item.Key], item.Key);
                    if (rtn != null)
                        return rtn;

                }
            }
            return null;
        }
    }
}
