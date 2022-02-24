using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace Merger.core.RuleScheduler
{
    public sealed class RuleScheduler
    {
        private ConcurrentQueue<PicRuleItem> ruleQueue;

        private IMerge merger;

        private IGetOffset offsetCalcer;

        private IProgress<int> progress;

        private int maxTaskCount = 5;

        private CancellationToken cancelToken;

        private bool isRunning = false;

        public RuleScheduler(List<PicRuleItem> taskRules, IMerge merger, IGetOffset calc,
            IProgress<int> progress, CancellationToken token,
            int maxTaskCount = 5)
        {
            if (maxTaskCount <= 0)
                maxTaskCount = Environment.ProcessorCount;
            maxTaskCount = Math.Min(maxTaskCount, Environment.ProcessorCount);
            this.maxTaskCount = maxTaskCount;
            ruleQueue = new ConcurrentQueue<PicRuleItem>();
            if(taskRules != null)
            {
                for(int i=0; i<taskRules.Count; i++)
                {
                    ruleQueue.Enqueue(taskRules[i]);
                }
            }
            this.merger = merger;
            this.offsetCalcer = calc;
            this.progress = progress;
            this.cancelToken = token;
            isRunning = false;
        }

        public async Task Run()
        {
            if (isRunning || ruleQueue == null || ruleQueue.Count == 0)
            {
                return;
            }
            isRunning = true;
            RuleWorker[] workers = new RuleWorker[this.maxTaskCount];
            Task[] tasks = new Task[this.maxTaskCount];
            for(int i=0; i<this.maxTaskCount; i++)
            {
                workers[i] = new RuleWorker(ruleQueue, progress, cancelToken, merger, offsetCalcer);
                tasks[i] = Task.Factory.StartNew(workers[i].ExecuteMain, TaskCreationOptions.LongRunning);
            }
            Task.WaitAll(tasks);
            isRunning = false;

        }
    }
    
}