using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageOP
{
    public interface IBlend
    {
        /// <summary>
        /// 像素点混合
        /// </summary>
        /// <param name="bottomValue">底层像素值</param>
        /// <param name="topValue">上层像素值</param>
        /// <returns></returns>
        int PixelBlend(int bottomValue, int topValue);
    }
}
