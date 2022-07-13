using System;

namespace BM
{
    public class DownLoadHandler
    {
        private Action<float> completeCallback;
        public event Action<float> Completed
        {
            add => completeCallback += value;
            remove => this.completeCallback -= value;
        }
    }
}