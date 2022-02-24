using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Merger.core
{
    public class BuildRuleHelper
    {
        /// <summary>
        /// 以组合的方式获取多个图片组的合成规则
        /// </summary>
        /// <param name="picGroups">图片全路径或单独文件名</param>
        /// <param name="ignoreGIdx"></param>
        /// <returns></returns>
        public virtual List<PicRuleItem> BuildRulesCombination(List<List<string>> picGroups, 
            HashSet<int> ignoreGIdx, int startIdx=0)
        {
            List<PicRuleItem> rtn = new List<PicRuleItem>();
            int count = 0;
            List<string> curItems = null;
            BuildRulesCombinationRecursive(picGroups, ignoreGIdx, startIdx, ref curItems, ref count, ref rtn);
            return rtn;

        }

        protected  virtual void BuildRulesCombinationRecursive(List<List<string>> picGroups, 
            HashSet<int> ignoreGIdx, int curIdx,
            ref List<string>curItems, ref int count,  ref List<PicRuleItem>rtn)
        {
            for (int i = 0; i < picGroups[curIdx].Count; i++)
            {
                if (curItems == null)
                {
                    curItems = new List<string>();
                    string path = Path.GetDirectoryName(picGroups[curIdx][i]);

                    string pureName = Path.GetFileNameWithoutExtension(picGroups[curIdx][i]);
                    throw new NotImplementedException("not implement");

                }
            }

        }

 
    }
}
