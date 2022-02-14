using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BM
{
    /// <summary>
    /// 用于配置所有分包的构建信息
    /// </summary>
    public class AssetLoadTable : ScriptableObject
    {
        [Header("构建路径")]
        [Tooltip("构建的资源的所在路径（Assets同级目录下的路径）")] public string BundlePath = "Bundles";

        /// <summary>
        /// 返回打包路径
        /// </summary>
        public string BuildBundlePath
        {
            get
            {
                var path = $"{Application.dataPath}/../{BundlePath}";
                DirectoryInfo info;
                if (!Directory.Exists(path))
                {
                    info = Directory.CreateDirectory(path);
                }
                else
                {
                    info = new DirectoryInfo(path);
                }

                return info.FullName;
            }
        }
        
        [Header("所有分包配置信息")]
        [Tooltip("每一个分包的配置信息")]
        public List<AssetsLoadSetting> AssetsLoadSettings = new List<AssetsLoadSetting>();
    }
}