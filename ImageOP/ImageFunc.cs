using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageOP
{
    public class ImageFunc
    {
        public static Bitmap DeepCopyImage(Bitmap img)
        {
            if (img == null)
                return null;
            Bitmap copy = img.Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);
            return copy;
        }

        public static Bitmap GetSubImage(Bitmap img, Rectangle rect)
        {
            if (img == null)
                return null;
            Bitmap copy = img.Clone(rect, img.PixelFormat);
            return copy;
        }

        /// <summary>
        /// 把PixelFormat转化为PixelFormat.Format32bppArgb
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap TransformTo32ARGB(Bitmap img)
        {
            if (img == null)
                return null;
            Bitmap rtn = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            using(Graphics g=  Graphics.FromImage(rtn))
            {
                g.PageUnit = GraphicsUnit.Pixel;
                g.DrawImageUnscaled(img, 0, 0);
            }
            return rtn;
        }

        /// <summary>
        /// 从int数据中提取出byte类型的a,r,g,b
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Tuple<byte, byte, byte, byte> ExtractPixel(int v)
        {
            byte b = (byte)(v & 0xFF);
            byte g = (byte)((v >> 8) & 0xFF);
            byte r = (byte)((v >> 16) & 0xFF);
            byte a = (byte)((v >> 24) & 0xFF);
            return new Tuple<byte, byte, byte, byte>(a, r, g, b);
        }

        public static int CompressPixel(Tuple<byte,byte,byte, byte> argb)
        {
            int rtn = 0;
            rtn = (rtn) | argb.Item1;
            rtn = (rtn << 8) | argb.Item2;
            rtn = (rtn << 8) | argb.Item3;
            rtn = (rtn << 8) | argb.Item4;
            return rtn;
        }

        /// <summary>
        /// 对重合部分混合，且限定图像为mainImg大小，超出边界部分被忽略
        /// </summary>
        /// <param name="mainImg"></param>
        /// <param name="subImg"></param>
        /// <param name="blendFunc"></param>
        /// <param name="xoffset"></param>
        /// <param name="yoffset"></param>
        /// <returns></returns>
        public static Bitmap ImageBlend(Bitmap mainImg, Bitmap subImg, IBlend blendFunc, int xoffset, int yoffset)
        {
            if(mainImg == null)
            {
                return DeepCopyImage(subImg);
            }
            //subImg和mainImg完全无重合
            if(subImg == null || xoffset >= mainImg.Width || yoffset >= mainImg.Height ||
                (xoffset + subImg.Width) <= 0 || (yoffset + subImg.Height) <= 0)
            {
                return DeepCopyImage(mainImg);
            }
            Bitmap rtn = DeepCopyImage(mainImg);
            BitmapData rtnData = rtn.LockBits(new Rectangle(0, 0, rtn.Width, rtn.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData subData = subImg.LockBits(new Rectangle(0, 0, subImg.Width, subImg.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int mainsx = Math.Max(0, xoffset);
            int mainsy = Math.Max(0, yoffset);
            int subsx =  Math.Max(0, -xoffset);
            int subsy =  Math.Max(0, -yoffset);
            int validWidth = Math.Min(mainImg.Width - mainsx, subImg.Width - subsx);
            int validHeight = Math.Min(mainImg.Height - mainsy, subImg.Height - subsy);

            unsafe
            {
                int* rtnPtr = (int*)(rtnData.Scan0);
                int* subPtr = (int*)(subData.Scan0);

                //移动到重合部分的左上角
                rtnPtr += (rtnData.Stride * mainsy + mainsx * 4) / 4;
                subPtr += (subData.Stride * subsy + subsx * 4) / 4;
                for(int y=0; y<validHeight; y++)
                {
                    for(int x=0; x<validWidth; x++)
                    {
                        int mainValue = *rtnPtr;
                        int subValue = *subPtr;
                        int blendValue = blendFunc.PixelBlend(mainValue, subValue);
                        *rtnPtr = blendValue;
                        rtnPtr += 1;
                        subPtr += 1;
                    }
                    rtnPtr += (rtnData.Stride - validWidth * 4) / 4;
                    subPtr += (subData.Stride - validWidth * 4) / 4;
                }
               
            }
            rtn.UnlockBits(rtnData);
            subImg.UnlockBits(subData);
            return rtn;
        }

        /// <summary>
        /// 对重合部分混合，且若超出mainImg大小，则扩展图像大小，最终会完全包含两个图像
        /// </summary>
        /// <param name="mainImg"></param>
        /// <param name="subImg"></param>
        /// <param name="blendFunc"></param>
        /// <param name="xoffset"></param>
        /// <param name="yoffset"></param>
        /// <returns></returns>
        public static Bitmap ImageBlendExtend(Bitmap mainImg, Bitmap subImg, IBlend blendFunc, int xoffset, int yoffset)
        {
            if (mainImg == null)
            {
                return DeepCopyImage(subImg);
            }
            if (subImg == null)
            {
                return DeepCopyImage(mainImg);
            }
            throw new NotImplementedException();
        }
    }


}
