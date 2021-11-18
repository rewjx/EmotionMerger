using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace Merger.core.TreeScheduler
{
    /*
    /// 优点：
    /// 树状结构能够充分利用共同路径来提高性能。
    /// 如合成a->b->c, a->b->d两张图片，组织成树后，a->b这一步无需重复合成。
    /// 如果采用单独的每一张图片进行合成的方式，a->b这一步需要重复合成多次。
    /// 缺点：
    /// 1. 并行化处理较麻烦
    /// 2. 该方式很难处理某些图片组可忽略的情况
    /// a. 若某节点的子叶子节点都可忽略，则会重复生成图片，带来性能损失
    /// b. 若到叶子节点再保存图片，则还需保存合成路径，才能辨别为不同图片，给与不同命名。除非利用随机数或时间等命名。
    /// 且只在立绘中会出现某些图片组可有可无(如脸上红晕，眼镜等装饰品)，cg中一般不会出现
    /// 因此暂不支持这种图片组的自动忽略，采用树状结构，将会忽视canIgnore属性。
    */

    /// <summary>
    /// 图片合成树的执行
    /// 会无视canIgnore属性,凡是树中节点都会参与合成
    /// </summary>
    public sealed class TreeScheduler
    {

        private static object enQueueLocker = new object();

        private TaskQueue tasksQueue;

        private List<TreeNode> taskRoots;

        private int maxTaskCount = 5;

        private int curTaskIdx = 0;

        private IMerge merger;

        private IGetOffset offsetCalcer;

        private IProgress<int> progress;

        private CancellationToken cancelToken;

        private bool isRunning = false;

        public delegate void RootTaskEmptyHandler();

        public event RootTaskEmptyHandler rootTaskEmptyEvent;

        public TreeScheduler(List<TreeNode> taskForests, IMerge merger, IGetOffset calc,
            IProgress<int>progress,  CancellationToken token,
            int maxTaskCount=5, int maxQueueLen=10)
        {
            if (maxTaskCount <= 0)
                maxTaskCount = Environment.ProcessorCount;
            maxTaskCount = Math.Min(maxTaskCount, Environment.ProcessorCount);
            maxQueueLen = Math.Max(maxQueueLen, maxTaskCount + 2);
            this.tasksQueue = new TaskQueue(maxQueueLen);
            taskRoots = taskForests;
            this.maxTaskCount = maxTaskCount;
            this.merger = merger;
            this.progress = progress;
            this.offsetCalcer = calc;
            this.cancelToken = token;
            curTaskIdx = 0;
            isRunning = false;
            tasksQueue.TaskQueueEmptyEvent += new TaskQueue.TaskQueueEmptyHandler(AddRootTask2Queue);
        }

        /// <summary>
        /// 获取总共有多少个叶子节点(总图片数量)
        /// </summary>
        /// <returns></returns>
        public int GetProbTotalCount()
        {
            if (this.taskRoots == null)
                return 0;
            int count = 0;
            foreach (TreeNode node in taskRoots)
            {
                foreach (var item in node.DFSStepTree(false))
                {
                    count += 1;
                }
            }
            return count;

        }

        public void AddRootTask2Queue()
        {
            lock(enQueueLocker)
            {
                if (curTaskIdx >= taskRoots.Count)
                    return;
                while (curTaskIdx < taskRoots.Count)
                {
                    TreeNode node = taskRoots[curTaskIdx];
                    node.SetParentImg(null);
                    if (!tasksQueue.Enqueue(node))
                    {
                        break;
                    }
                    curTaskIdx += 1;
                }
                if (curTaskIdx >= taskRoots.Count)
                {
                    rootTaskEmptyEvent();
                }
            }
        }

        public async Task Run()
        {
            if (isRunning)
                return;
            if (taskRoots == null || taskRoots.Count == 0)
                return;
            isRunning = true;
            TreeWorker[] workers = new TreeWorker[maxTaskCount];
            Task[] tasks = new Task[maxTaskCount];
            for (int i = 0; i < maxTaskCount; i++)
            {
                TreeWorker one = new TreeWorker(tasksQueue, progress, cancelToken, merger, offsetCalcer);
                rootTaskEmptyEvent += new RootTaskEmptyHandler(one.SetNoMoreTopTask);
                workers[i] = one;
            }
            curTaskIdx = 0;
            AddRootTask2Queue();
            for (int i=0; i<maxTaskCount; i++)
            {
                tasks[i] = Task.Factory.StartNew(workers[i].ExecuteMain, TaskCreationOptions.LongRunning);
            }
            Task.WaitAll(tasks);
            isRunning = false;
        }


    }
}
