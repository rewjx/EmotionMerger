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
        /// file suffix
        /// </summary>
        public abstract string tag { get;  }

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
