using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merger.core
{
    public interface IGetOffset
    {
        string MethodName { get; }
        /// <summary>
        /// 指定两张图片名字，计算sub图像左上角相对main图像左上角的偏移
        /// 如果mainPic左上角为(x,y)，返回结果offset为(ox,oy)，则subPic左上角位于(x+ox, y+oy)
        /// </summary>
        /// <param name="mainPicName"></param>
        /// <param name="subPicName"></param>
        /// <returns></returns>
        Tuple<int, int> GetOffset(string mainPicName, string subPicName);
    }
}
