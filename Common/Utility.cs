using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class StringIgnoreCaseCompare : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;
            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }
}
