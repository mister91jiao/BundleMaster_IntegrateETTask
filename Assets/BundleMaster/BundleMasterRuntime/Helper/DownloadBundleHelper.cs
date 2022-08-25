using System.Collections.Generic;
using UnityEngine.Networking;
using ET;

namespace BM
{
    public static class DownloadBundleHelper
    {
        public static async ETTask<byte[]> DownloadDataByUrl(string url)
        {
            for (int i = 0; i < AssetComponentConfig.ReDownLoadCount; i++)
            {
                byte[] data = await DownloadData(url);
                if (data != null)
                {
                    return data;
                }
            }
            AssetLogHelper.LogError("下载资源失败: " + url);
            return null;
        }
        
        private static async ETTask<byte[]> DownloadData(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                UnityWebRequestAsyncOperation webRequestAsync = webRequest.SendWebRequest();
                ETTask waitDown = ETTask.Create(true);
                webRequestAsync.completed += (asyncOperation) =>
                {
                    waitDown.SetResult();
                };
                await waitDown;
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (!string.IsNullOrEmpty(webRequest.error))
#endif
                {
                    AssetLogHelper.Log("下载Bundle失败 重试\n" + webRequest.error + "\nURL：" + url);
                    return null;
                }
                return webRequest.downloadHandler.data;
            }
        }
        
        public static async ETTask<DownLoadData> DownloadRefDataByUrl(string url)
        {
            DownLoadData downLoadData = DownLoadData.Get();
            for (int i = 0; i < AssetComponentConfig.ReDownLoadCount; i++)
            {
                await DownloadData(url, downLoadData);
                if (downLoadData.Data != null)
                {
                    return downLoadData;
                }
            }
            AssetLogHelper.LogError("下载资源失败: " + url);
            return null;
        }

        private static async ETTask DownloadData(string url, DownLoadData downLoadData)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                UnityWebRequestAsyncOperation webRequestAsync = webRequest.SendWebRequest();
                ETTask waitDown = ETTask.Create(true);
                webRequestAsync.completed += (asyncOperation) =>
                {
                    waitDown.SetResult();
                };
                await waitDown;
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (!string.IsNullOrEmpty(webRequest.error))
#endif
                {
                    AssetLogHelper.Log("下载Bundle失败 重试\n" + webRequest.error + "\nURL：" + url);
                    return;
                }
                downLoadData.Data = webRequest.downloadHandler.data;
                webRequest.downloadHandler.Dispose();
            }
        }
        
        
    }

    public class DownLoadData
    {
        public byte[] Data;

        private static Queue<DownLoadData> _downLoadDataPool = new Queue<DownLoadData>();

        public static DownLoadData Get()
        {
            if (_downLoadDataPool.Count > 0)
            {
                return _downLoadDataPool.Dequeue();
            }
            else
            {
                return new DownLoadData();
            }
        }

        public static void Recovery(DownLoadData downLoadData)
        {
            downLoadData.Data = null;
            _downLoadDataPool.Enqueue(downLoadData);
        }

        public static void ClearPool()
        {
            _downLoadDataPool.Clear();
        }
    }
    
}