using System.Collections.Generic;

namespace LMTD
{
    internal static class ThreadFactory
    {
        /// <summary>
        /// 所有线程的数量
        /// </summary>
        internal static uint ThreadCount = 0;
        
        /// <summary>
        /// 所有线程的池
        /// </summary>
        internal static readonly Queue<LiteThread> ThreadPool = new Queue<LiteThread>();

        /// <summary>
        /// 是否开启回收进程，默认不开启
        /// </summary>
        internal static bool RecoverKey
        {
            set
            {
                if (value == false)
                {
                    //说明所有进程都已经被回收
                }
            }
            get => recoverKey;
        }
        private static bool recoverKey = false;
        
        /// <summary>
        /// 执行一个逻辑
        /// </summary>
        public static void ThreadAction(ILiteThreadAction liteThreadAction)
        {
            LiteThread liteThread;
            if (ThreadPool.Count > 0)
            {
                liteThread = ThreadPool.Dequeue();
            }
            else
            {
                liteThread = new LiteThread();
            }
            liteThread.Action(liteThreadAction);
        }
        
    }
}