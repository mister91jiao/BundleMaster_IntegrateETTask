using System;
using System.Collections.Generic;
using System.Threading;

namespace LMTD
{
    internal class LiteThread
    {
        private readonly Thread thread;
        private Action logic = null;
        
        internal LiteThread()
        {
            ThreadFactory.ThreadCount++;
            thread = new Thread(Run);
            thread.Start();
        }

        internal void Action(ILiteThreadAction liteThreadAction)
        {
            logic = liteThreadAction.Logic;
        }

        private void Run()
        {
            while (!ThreadFactory.RecoverKey)
            {
                Thread.Sleep(1);
                if (logic != null)
                {
                    logic();
                    logic = null;
                    //执行完逻辑后自己进池
                    ThreadFactory.ThreadPool.Enqueue(this);
                }
            }
            if (ThreadFactory.ThreadCount == 1)
            {
                ThreadFactory.RecoverKey = false;
            }
            Recovery();
        }

        /// <summary>
        /// 回收这个线程
        /// </summary>
        internal void Recovery()
        {
            ThreadFactory.ThreadCount--;
            thread.Abort();
        }
    }
}