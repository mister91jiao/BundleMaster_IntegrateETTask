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
                coroutineLockKeyToQueue.Add(key, new Queue<CoroutineLock>());
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