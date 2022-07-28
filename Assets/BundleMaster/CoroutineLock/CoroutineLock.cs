using System;
using UnityEngine;

namespace ET
{
    public class CoroutineLock : IDisposable
    {
        private bool isDispose = false;
        internal long Key;
        private CoroutineLockQueue _coroutineLockQueue;
        private ETTask waitTask;
        
        internal void Init(long key, CoroutineLockQueue coroutineLockQueue)
        {
            isDispose = false;
            this.Key = key;
            this._coroutineLockQueue = coroutineLockQueue;
            waitTask = ETTask.Create(true);
        }

        internal void Enable()
        {
            waitTask.SetResult();
        }

        internal ETTask Wait()
        {
            return waitTask;
        }
        
        
        public void Dispose()
        {
            if (isDispose)
            {
                Debug.LogError("协程锁重复释放");
                return;
            }
            waitTask = null;
            isDispose = true;
            _coroutineLockQueue.CoroutineLockDispose(this);
            _coroutineLockQueue = null;
            CoroutineLockComponent.CoroutineLockQueue.Enqueue(this);
            
        }
    }
}