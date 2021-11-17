using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

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

        public IEnumerable<BaseImageFormat> FindSpecialTagFormats(string tag)
        {
            if (imgFormats == null)
                return null;
            return imgFormats.Where(x => x.tag == tag).ToList();
        }

        public IEnumerable<BaseImageFormat> FindSpecialEngineFormats(string engineName)
        {
            if (imgFormats == null)
                return null;
            return imgFormats.Where(x => x.Engines.Contains(engineName)).ToList();
        }


    }
}
