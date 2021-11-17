using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Merger.core
{
    public class TaskQueue
    {
        private static object locker = new object();

        private Queue<TreeNode> _queue = null;

        public int maxQueueLen { get; private set; }

        public delegate void TaskQueueEmptyHandler();

        public event TaskQueueEmptyHandler TaskQueueEmptyEvent;

        public int Count
        {
            get
            {
                lock(locker)
                {
                    if (_queue == null)
                        return 0;
                    return _queue.Count;
                }
            }
        }

        public TaskQueue(int maxQueueLen=10)
        {
            this.maxQueueLen = maxQueueLen;
            _queue = new Queue<TreeNode>();
        }

        public bool Enqueue(TreeNode node)
        {
            if (node == null)
                return false;
            lock(locker)
            {
                if (_queue == null || _queue.Count >= maxQueueLen)
                    return false;
                _queue.Enqueue(node);
                return true;
            }

        }


        public TreeNode Dequeue()
        {
            TreeNode item = null;
            int leftCount = -1;
            lock(locker)
            {
                if (_queue == null)
                    return null;
                if(_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                }
                leftCount = _queue.Count;
            }
            if(leftCount == 0)
            {
                //该事件可能重复触发
                TaskQueueEmptyEvent();
            }
            return item;

        }






        
    }
}
