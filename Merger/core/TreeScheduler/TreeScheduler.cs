using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace Merger.core.TreeScheduler
{
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
                foreach (var item in node.DFSStepTree())
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
