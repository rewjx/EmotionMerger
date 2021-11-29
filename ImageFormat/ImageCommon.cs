using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel.Composition;
using Common;

namespace ImageFormat
{
    public abstract class ImageCommon:BaseImageFormat
    {
        public override IEnumerable<string> Engines
        {
            get
            {
                return new List<string> { EngineNames.CommonEngine };
            }
        }

        public override Bitmap ReadImage(string path)
        {
            Bitmap pic = null;
            try
            {
                pic = new Bitmap(path);
            }
            catch
            {
                pic = null;
            }
            return pic;
        }
    }

    [Export(typeof(BaseImageFormat))]
    public class ImagePNG : ImageCommon
    {
        public override string tag
        {
            get
            {
                return ImageNames.PNG;
            }
        }

        public override string extension
        {
            get
            {
                return ImageNames.PNG;
            }
        }

        public override bool canWrite { get { return true; } }

        public override bool WriteImage(Bitmap pic, string path)
        {
            if (pic == null)
                return true;
            pic.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            return true;
        }
    }

    [Export(typeof(BaseImageFormat))]
    public class ImageJPG : ImageCommon
    {
        public override string tag
        {
            get
            {
                return ImageNames.JPG;
            }
        }

        public override string extension
        {
            get
            {
                return ImageNames.JPG;
            }
        }

        public override bool canWrite { get { return true; } }

        public override bool WriteImage(Bitmap pic, string path)
        {
            if (pic == null)
                return true;
            pic.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            return true;
        }
    }

    [Export(typeof(BaseImageFormat))]
    public class ImageBMP : ImageCommon
    {
        public override string tag
        {
            get
            {
                return ImageNames.BMP;
            }
        }

        public override string extension
        {
            get
            {
                return ImageNames.BMP;
            }
        }

        public override bool canWrite { get { return true; } }

        public override bool WriteImage(Bitmap pic, string path)
        {
            if (pic == null)
                return true;
            pic.Save(path, System.Drawing.Imaging.ImageFormat.Bmp);
            return true;
        }
    }


}
