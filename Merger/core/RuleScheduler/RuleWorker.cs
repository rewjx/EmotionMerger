using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ImageOP;
using System.IO;
using System.Drawing;

namespace Merger.core.RuleScheduler
{
    class RuleWorker
    {
        /// <summary>
        /// rulesQueue中取出一条规则开始合成
        /// </summary>
        private ConcurrentQueue<PicRuleItem> rulesQueue = null;

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

        public RuleWorker(ConcurrentQueue<PicRuleItem> q, IProgress<int> progress, CancellationToken token,
            IMerge merger, IGetOffset calc)
        {
            this.rulesQueue = q;
            this.progress = progress;
            this.cancelToken = token;
            this.merger = merger;
            this.calc = calc;
        }

        public async Task ExecuteMain()
        {
            while(true)
            {
                if(cancelToken.IsCancellationRequested)
                {
                    break;
                }
                PicRuleItem rule;
                if (!rulesQueue.TryDequeue(out rule))
                {
                    break;
                }
                if(rule.PicRules == null || rule.PicRules.Count == 0)
                {
                    continue;
                }
                bool isFullPath = rule.PicPath != null;
                string curPath = null;
                if (isFullPath)
                {
                    curPath = Path.Combine(rule.PicPath, rule.PicRules[0]);
                }
                else
                {
                    curPath = rule.PicRules[0];
                }
                Bitmap parentImg = await Task.Run(() => merger.ReadImage(curPath, isFullPath));
                merger.PreProcessImage(ref parentImg);
                for (int i=1; i<rule.PicRules.Count; i++)
                {
                    if (isFullPath)
                    {
                        curPath = Path.Combine(rule.PicPath, rule.PicRules[i]);
                    }
                    else
                    {
                        curPath = rule.PicRules[i];
                    }
                    Bitmap subImg = await Task.Run(() => merger.ReadImage(curPath, isFullPath));
                    merger.PreProcessImage(ref subImg);
                    Tuple<int, int> offset = calc.GetOffset(rule.PicRules[0], rule.PicRules[i]);
                    Bitmap mImg = merger.MergeProcess(ref parentImg, ref subImg, offset);
                    merger.PostProcess(ref mImg);
                    if(subImg != null)
                    {
                        subImg.Dispose();
                    }
                    if(parentImg != null)
                    {
                        parentImg.Dispose();
                    }
                    parentImg = mImg;
                }
                bool isSuccess = await Task.Run(() => merger.SaveImage(ref parentImg, rule.SaveName));
                if(parentImg != null)
                {
                    parentImg.Dispose();
                }
                if (false == isSuccess)
                {
                    throw new Exception("图片保存失败");
                }
                progress.Report(1);
            }
        }


    }
}
