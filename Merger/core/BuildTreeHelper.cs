using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merger.core
{
    public class BuildTreeHelper
    {


        /// <summary>
        /// 根据分组建立合成树
        /// </summary>
        /// <param name="picGroups">不同索引的List为不同组，索引为0的组代表该组合成时在最下方，索引越大，代表对应组越靠后合成</param>
        /// <param name="ignoreGIdx">存储可忽略的图片组在picGroups中的下标</param>
        /// <param name="idx"></param>
        /// <param name="mainName"></param>
        /// <returns></returns>
        public virtual List<TreeNode> BuildTrees(List<List<string>> picGroups, HashSet<int>ignoreGIdx, int idx=0, string mainName=null)
        {
            int saveCount = 0;
            List<TreeNode> rtn = BuildTreesRecursive(picGroups, ignoreGIdx, ref saveCount, idx, mainName);
            return rtn;
        }

        protected virtual List<TreeNode> BuildTreesRecursive(List<List<string>> picGroups, HashSet<int> ignoreGIdx,
            ref int saveCount, int idx = 0, string mainName = null)
        {
            if (picGroups == null || idx >= picGroups.Count)
                return null;
            List<TreeNode> roots = new List<TreeNode>();
            bool canIgnore = false;
            if (ignoreGIdx != null && ignoreGIdx.Contains(idx))
            {
                canIgnore = true;
            }
            for (int i = 0; i < picGroups[idx].Count; i++)
            {
                TreeNode node = new TreeNode(picGroups[idx][i], canIgnore);
                if (idx == 0 || mainName == null)
                {
                    node.SetMainImgName(picGroups[idx][i]);
                    mainName = picGroups[idx][i];
                }
                else
                {
                    node.SetMainImgName(mainName);
                }
                if (idx == 0)
                {
                    saveCount = 0;
                }
                List<TreeNode> subs = BuildTreesRecursive(picGroups, ignoreGIdx, ref saveCount, idx + 1, mainName);
                if (subs != null)
                {
                    node.AddChilds(subs);
                }
                if (node.childs == null || node.childs.Count == 0)
                {
                    node.SaveName = mainName + "_" + string.Format("{0:D5}", saveCount);
                    saveCount += 1;
                }
                roots.Add(node);
            }
            return roots;
        }
    }
}
