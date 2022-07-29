using System.Collections.Generic;

namespace ET
{
    internal class CoroutineLockQueue
    {
        private int coroutineLockType;
        private readonly Dictionary<long, Queue<CoroutineLock>> coroutineLockKeyToQueue = new Dictionary<long, Queue<CoroutineLock>>();
        
        internal CoroutineLockQueue(int coroutineLockType)
        {
            this.coroutineLockType = coroutineLockType;
        }
        
        internal void CoroutineLockDispose(CoroutineLock coroutineLock)
        {
            Queue<CoroutineLock> keyToQueue = coroutineLockKeyToQueue[coroutineLock.Key];
            if (keyToQueue.Count > 0)
            {
                CoroutineLock nextCoroutineLock = keyToQueue.Dequeue();
                nextCoroutineLock.Enable();
                return;
            }
            keyToQueue.Clear();
            CoroutineLockComponent.CoroutineLockQueuePool.Enqueue(keyToQueue);
            coroutineLockKeyToQueue.Remove(coroutineLock.Key);
        }
        
        internal CoroutineLock GetCoroutineLock(long key)
        {
            CoroutineLock coroutineLock;
            if (CoroutineLockComponent.CoroutineLockQueue.Count > 0)
            {
                coroutineLock = CoroutineLockComponent.CoroutineLockQueue.Dequeue();
            }
            else
            {
                coroutineLock = new CoroutineLock();
            }
            coroutineLock.Init(key, this);
            if (!coroutineLockKeyToQueue.ContainsKey(key))
            {
                Queue<CoroutineLock> coroutineLockQueue;
                if (CoroutineLockComponent.CoroutineLockQueuePool.Count > 0)
                {
                    coroutineLockQueue = CoroutineLockComponent.CoroutineLockQueuePool.Dequeue();
                }
                else
                {
                    coroutineLockQueue = new Queue<CoroutineLock>();
                }
                coroutineLockKeyToQueue.Add(key, coroutineLockQueue);
                coroutineLock.Enable();
            }
            else
            {
                coroutineLockKeyToQueue[key].Enqueue(coroutineLock);
            }
            return coroutineLock;
        }

    }
}