using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageOP
{
    public class AlphaBlend:IBlend
    {
        public int PixelBlend(int bottomValue, int topValue)
        {
            Tuple<byte, byte, byte, byte> bp = ImageFunc.ExtractPixel(bottomValue);
            Tuple<byte, byte, byte, byte> tp = ImageFunc.ExtractPixel(topValue);

            
            float ratio = tp.Item1 / 255.0f;

            //byte ra = (byte)((1 - ratio) * bp.Item1 + ratio * tp.Item1);
            byte rr = (byte)((1 - ratio) * bp.Item2 + ratio * tp.Item2);
            byte rg = (byte)((1 - ratio) * bp.Item3 + ratio * tp.Item3);
            byte rb = (byte)((1 - ratio) * bp.Item4 + ratio * tp.Item4);

            byte ra = Math.Max(bp.Item1, tp.Item1);

            int rtn = ImageFunc.CompressPixel(new Tuple<byte, byte, byte, byte>(ra, rr, rg, rb));
            return rtn;

        }
    }
}
