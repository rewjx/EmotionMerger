using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merger.Escude;
using Merger.core;
using Merger.core.TreeScheduler;
using Merger;
using System.Threading;
using ImageOP;
using System.Drawing;
using System.Diagnostics;
using Common;
using Merger.Tmr_Hiro;

namespace ConsoleApp
{
    class Program
    {

        static void PrintHelpMessage()
        {
            Console.WriteLine("进行CG或立绘图片的合成");
            Console.WriteLine("使用方法:");
            Console.WriteLine("*.exe -i -s -m -p[optional] -t[optional]");
            Console.WriteLine("参数-i: 待合成的图片的文件夹路径");
            Console.WriteLine("参数-s: 合成后图片保存路径");
            Console.WriteLine("参数-m: 图片合成方法(不同引擎往往需要采用不同的合成方式，请正确选择合成方法)");
            Console.WriteLine("参数-p(可选,与-m参数有关): 除图片外，图片合成所需的必要文件路径(如偏移信息文件,合成表文件)");
            Console.WriteLine("参数-t(可选): 使用的线程数量)");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("帮助相关:");
            Console.WriteLine("参数-lm: 查看支持的引擎或方法名，用于-m参数");
        }

        static void PrintMethodNames()
        {
            List<string> names = MergerCatalog.Instance.GetAllMethodName();
            foreach (var item in names)
            {
                Console.WriteLine(item);
            }
        }
        static bool Process(CmdParser parser)
        {
            bool isPrintMethod = parser.Has("-lm");
            if(isPrintMethod)
            {
                PrintMethodNames();
                return true;
            }
                
            bool hasPicPath = parser.Has("-i");
            if (hasPicPath == false)
                return false;
            bool hasSavePath = parser.Has("-s");
            if (hasSavePath == false)
                return false;
            bool hasMethodOp = parser.Has("-m");
            if (hasMethodOp == false)
                return false;
            string picPath = parser["-i"].First;
            string savePath = parser["-s"].First;
            string methodName = parser["-m"].First;
            string offsetPath = null;
            if(parser.Has("-p"))
            {
                offsetPath = parser["-p"].First;
            }

            int threadNum = 5;
            if(parser.Has("-t"))
            {
                if(!int.TryParse(parser["-t"].First, out threadNum))
                {
                    threadNum = 5;
                }
            }

            //1610张图片，3.5GB，3线程节能大约100s，5线程平衡大约50s，平衡模式5线程基本占满cpu，因此10线程速度基本不变
            // BaseMerger merger = MergerCatalog.Instance.FindSpecialNameMergers(MergeMethodName.TmrHiroMethod);

            // EscudeMerger merger = new EscudeMerger(picPath, offsetPath, savePath);

            BaseMerger merger = MergerCatalog.Instance.FindSpecialNameMergers(methodName);
            merger.SetInitializeParameter(picPath, savePath, offsetPath);
            List<TreeNode> nodes = merger.GetTreeNodes();
            CancellationTokenSource cs = new CancellationTokenSource();
            TreeScheduler pro = new TreeScheduler(nodes, (IMerge)merger, merger.GetDefaultOffseter(),
                new Progress<int>(), cs.Token, maxTaskCount:threadNum);
            Console.WriteLine("total picture count: " + pro.GetProbTotalCount());
            Stopwatch s = new Stopwatch();
            s.Start();
            pro.Run().Wait();
            s.Stop();
            Console.WriteLine("used " + s.ElapsedMilliseconds.ToString() + "ms");
            return true;
        }

        static void Main(string[] args)
        {
            CmdParser parser = CmdParser.Parse(args);
            if(!Process(parser))
            {
                PrintHelpMessage();
                Console.WriteLine("\n\n");
                Console.WriteLine("按任意键退出");
                Console.ReadKey();
            }
        }
    }
}
