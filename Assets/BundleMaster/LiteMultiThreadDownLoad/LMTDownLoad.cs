using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LMTD
{
    public class LMTDownLoad : ILiteThreadAction, IDisposable
    {
        private static Queue<LMTDownLoad> _lmtDownLoadQueue = new Queue<LMTDownLoad>(); 

        /// <summary>
        /// 创建一个下载器
        /// </summary>
        public static LMTDownLoad Create(string url, string filePath)
        {
            LMTDownLoad lmtDownLoad;
            if (_lmtDownLoadQueue.Count > 0)
            {
                lmtDownLoad = _lmtDownLoadQueue.Dequeue();
            }
            else
            {
                lmtDownLoad = new LMTDownLoad();
            }
            lmtDownLoad.url = url;
            lmtDownLoad.filePath = filePath;
            return lmtDownLoad;
        }

        /// <summary>
        /// 下载地址
        /// </summary>
        private string url = null;
        
        /// <summary>
        /// 文件存储路径
        /// </summary>
        private string filePath = null;
        
        private void DownLoad()
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //获取文件流
            using Stream receiveStream = httpWebResponse.GetResponseStream();
            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            //创建一块存储的大小
            byte[] blockBytes = new byte[1024];
            Debug.Assert(receiveStream != null, nameof(receiveStream) + "获取远程文件流为空\n[" + url + "]\n");
            int blockSize = receiveStream.Read(blockBytes, 0, blockBytes.Length);
             while (blockSize > 0)
             {
                 //循环写入读取数据
                 fileStream.Write(blockBytes, 0, blockSize);
                 blockSize = receiveStream.Read(blockBytes, 0, blockBytes.Length);
             }
             fileStream.Close();
             receiveStream.Close();
             httpWebResponse.Close();
            //循环结束下载完成
        }
        
        public void Logic()
        {
            DownLoad();
        }

        public void Dispose()
        {
            url = null;
            filePath = null;
            _lmtDownLoadQueue.Enqueue(this);
        }
        
    }
}