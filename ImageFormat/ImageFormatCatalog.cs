using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using Common;

namespace ImageFormat
{

    public class ImageFormatCatalog
    {
        private static Lazy<ImageFormatCatalog> _instance =
            new Lazy<ImageFormatCatalog>(() => new ImageFormatCatalog());


        public static ImageFormatCatalog Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        [ImportMany(typeof(BaseImageFormat))]
        private IEnumerable<BaseImageFormat> imgFormats = null;

        private ImageFormatCatalog()
        {
            imgFormats = null;
            var catalog = new AggregateCatalog();
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            catalog.Catalogs.Add(new DirectoryCatalog(location));
            using(var container = new CompositionContainer(catalog))
            {
                container.ComposeParts(this);
            }
        }

        public IEnumerable<BaseImageFormat> FindAllFormats()
        {
            return imgFormats;
        }

       /// <summary>
       /// 查找特定tag的图片格式(图片tag保证唯一)
       /// </summary>
       /// <param name="tag"></param>
       /// <returns></returns>
        public IEnumerable<BaseImageFormat> FindSpecialTagFormats(string tag)
        {
            if (imgFormats == null)
                return null;
            return imgFormats.Where(x => x.tag.Equals(tag, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// 获取所有图片扩展名
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllExtensions()
        {
            if (imgFormats == null)
                return null;
            return imgFormats.Select(x => x.extension).ToList();
        }

        /// <summary>
        /// 查找tag对应图片格式的扩展名
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string GetExtensionsByTag(string tag)
        {
            IEnumerable<BaseImageFormat> formats = FindSpecialTagFormats(tag);
            if (formats == null)
                return null;
            List<BaseImageFormat> fs = formats.ToList();
            if (fs.Count == 0)
                return null;
            return fs[0].extension;
        }

        /// <summary>
        /// 查找特定扩展名的图片格式
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public IEnumerable<BaseImageFormat> FindSpecialExtensionFormats(string extension)
        {
            if (imgFormats == null)
                return null;
            return imgFormats.Where(x => x.extension.Equals(extension, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// 查找特定引擎的图片格式
        /// </summary>
        /// <param name="engineName"></param>
        /// <returns></returns>
        public IEnumerable<BaseImageFormat> FindSpecialEngineFormats(string engineName)
        {
            if (imgFormats == null)
                return null;
            return imgFormats.Where(x => 
            x.Engines.Contains(engineName,new StringIgnoreCaseCompare())).ToList();
        }




    }
}
