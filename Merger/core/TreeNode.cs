using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Merger.core
{
    public class TreeNode:IDisposable
    {
        /// <summary>
        /// 节点文件名
        /// </summary>
        public string FileName { private set; get; }

        /// <summary>
        /// 该节点图像保存时的文件名
        /// </summary>
        public string SaveName { set; get; }

        /// <summary>
        /// 该文件是否是必须的
        /// </summary>
        public bool canIgnore { private set; get; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<TreeNode> childs;

        /// <summary>
        /// 到父节点为止的图像
        /// </summary>
        public Bitmap parentImg;

        /// <summary>
        /// 主图片文件名
        /// </summary>
        public string mainImgName { get; private set; }

        public TreeNode(string name, bool canIgnore=false)
        {
            this.FileName = name;
            this.canIgnore = canIgnore;
            this.childs = null;
        }

        public TreeNode(string name, List<TreeNode>childs, bool canIgnore=false)
        {
            this.FileName = name;
            this.canIgnore = canIgnore;
            this.childs = childs;
        }

        public void AddChild(TreeNode node)
        {
            if(this.childs == null)
            {
                this.childs = new List<TreeNode>();
            }
            if(!IsContainChild(node.FileName))
            {
                this.childs.Add(node);
            }
        }

        public void AddChilds(IEnumerable<TreeNode> nodes)
        {
            foreach (var item in nodes)
            {
                AddChild(item);
            }
        }

        public void ClearChilds()
        {
            if(this.childs != null)
            {
                this.childs.Clear();
            }
        }

        public bool IsContainChild(string childName)
        {
            if (this.childs == null)
                return false;
            return this.childs.Any(x => x.FileName == childName);
        }

        public void SetParentImg(Bitmap img)
        {
            if(parentImg != null)
            {
                parentImg.Dispose();
            }
            parentImg = img;
            disposed = false;
        }

        public void SetMainImgName(string name)
        {
            this.mainImgName = name;
        }

        /// <summary>
        /// 深度优先的方式进行树的遍历，返回到叶子节点路径的迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable<List<string>> DFSStepTree(bool useIgnore=true)
        {
            //叶子节点
            if (this.childs == null || this.childs.Count == 0)
            {
                if(canIgnore && useIgnore)
                {
                    yield return new List<string>();
                }
                yield return new List<string> { this.FileName };
            }
            else
            {
                foreach (TreeNode c in this.childs)
                {
                    foreach (var path in c.DFSStepTree(useIgnore))
                    {
                        if (canIgnore && useIgnore)
                        {
                            yield return path;
                        }
                        List<string> one = new List<string>(path);
                        one.Add(FileName);
                        yield return one;
                    }
                }
            }
        }

        #region IDisposable Region

        bool disposed = false;

        ~TreeNode()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    if(parentImg != null)
                    {
                        parentImg.Dispose();
                        parentImg = null;
                    }
                }
                disposed = true;
            }
        }

        #endregion

    }

}
