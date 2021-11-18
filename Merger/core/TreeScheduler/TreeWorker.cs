using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;
using ImageOP;

namespace Merger.core.TreeScheduler
{
    class TreeWorker
    {
        /// <summary>
        /// tasksQueue中取出任务节点开始执行
        /// </summary>
        private TaskQueue tasksQueue = null;

        /// <summary>
        /// 进行进度报告的接口
        /// </summary>
        private IProgress<int> progress = null;

        /// <summary>
        /// 取消令牌
        /// </summary>
        private CancellationToken cancelToken;

        /// <summary>
        /// 具体合成方法
        /// </summary>
        private IMerge merger;

        /// <summary>
        /// 获得偏移的方法
        /// </summary>
        private IGetOffset calc;

        /// <summary>
        /// 是否请求了把子任务添加到队列中
        /// </summary>
        private bool requestEnqueue = false;

        /// <summary>
        /// 是否能够结束工作
        /// </summary>
        private bool canFinish = false;

        /// <summary>
        /// 当任务队列为空时，最多尝试次数。如果canFinish为true且达到最多尝试次数，则任务将会停止。
        /// </summary>
        private int maxRetryTimes = 3;

        public TreeWorker(TaskQueue q, IProgress<int>progress, CancellationToken token,
            IMerge merger, IGetOffset calc)
        {
            this.tasksQueue = q;
            this.progress = progress;
            this.cancelToken = token;
            this.merger = merger;
            this.calc = calc;
            AddTaskQueueEmptyHandler();
        }

        ~TreeWorker()
        {
            RemoveTaskQueueEmptyHandler();
        }

        private void AddTaskQueueEmptyHandler()
        {
            if (this.tasksQueue == null)
                return;
            RemoveTaskQueueEmptyHandler();
            this.tasksQueue.TaskQueueEmptyEvent += new TaskQueue.TaskQueueEmptyHandler(SetEnqueueFlag);
        }

        private void RemoveTaskQueueEmptyHandler()
        {
            if (this.tasksQueue == null)
                return;
            this.tasksQueue.TaskQueueEmptyEvent -= new TaskQueue.TaskQueueEmptyHandler(SetEnqueueFlag);
        }

        public void SetEnqueueFlag()
        {
            requestEnqueue = true;
        }

        public void SetNoMoreTopTask()
        {
            //无最顶层节点任务时，当前节点的工作若已经完成，则可结束
            canFinish = true;
        }

        public async Task ExecuteMain()
        {
            int continuousTimes = 0;
            while (true)
            {
                //取消任务，任务停止执行
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }
                TreeNode node = tasksQueue.Dequeue();
                if (node != null)
                {
                    continuousTimes = 0;
                    ExecuteNode(node).Wait();
                    //await ExecuteNode(node);
                    node.Dispose();
                }
                else
                {
                    continuousTimes += 1;
                    if (canFinish && continuousTimes > maxRetryTimes)
                    {
                        break;
                    }
                    Thread.Sleep(300);
                }

            }
            
        }

        public async Task ExecuteNode(TreeNode node)
        {
            //无视该属性，取消注释也会因为图片命名问题造成同名文件覆盖，不考虑修复该问题
            //if(node.canIgnore)
            //{
            //    Pass2ChildNodes(node, node.parentImg).Wait();
            //}
            Bitmap img = await DoMerge(node);
            try
            {
                Pass2ChildNodes(node, img).Wait();
            }
            finally
            {
                if(img != null)
                {
                    img.Dispose();
                }
            }
        }

        //是否异步读写图片对性能基本无影响

        private async Task Pass2ChildNodes(TreeNode node, Bitmap img)
        {
            //叶子节点，代表一张图片合成完毕，保存图片
            if (node.childs == null || node.childs.Count == 0)
            {
                bool isSuccess = await Task.Run(() => merger.SaveImage(ref img, node.SaveName));
                if (false == isSuccess)
                {
                    throw new Exception("图片保存失败");
                }
                progress.Report(1);
            }
            //非叶子节点，还需继续合成
            else
            {
                int childIdx = 0;
                while (childIdx < node.childs.Count)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    //如果请求了往队列中添加任务，则把该节点的部分子任务加到队列中
                    if (requestEnqueue)
                    {
                        while (childIdx < node.childs.Count - 1)
                        {
                            Bitmap copyImg = ImageFunc.DeepCopyImage(img);
                            TreeNode sonNode = node.childs[childIdx];
                            sonNode.SetParentImg(copyImg);
                            if (!tasksQueue.Enqueue(sonNode))
                            {
                                break;
                            }
                            childIdx += 1;
                        }
                        requestEnqueue = false;
                        continue;
                    }
                    node.childs[childIdx].SetParentImg(img);
                    ExecuteNode(node.childs[childIdx]).Wait();
                    childIdx += 1;
                }
            }

        }

        public async Task<Bitmap> DoMerge(TreeNode node)
        {
            Task<Bitmap> readTask = Task.Run(()=> merger.ReadImage(node.FileName));
            Bitmap subImg = await readTask;
            merger.PreProcessImage(ref subImg);
            Tuple<int, int> offset = calc.GetOffset(node.mainImgName, node.FileName);    
            Bitmap result = merger.MergeProcess(ref node.parentImg, ref subImg, offset);
            merger.PostProcess(ref result);
            if(subImg != null)
                subImg.Dispose();
            return result;
            
        }

    }
}
