using System.Collections;
using System.Collections.Generic;
using System.IO;
using ET;
using UnityEngine;
using BM;

public static class InstallHelper
{
    public static async ETTask InstallApk()
    {
        Dictionary<string, bool> updatePackageBundle = new Dictionary<string, bool>()
        {
            {"APK", false},
        };
        UpdateBundleDataInfo updateBundleDataInfo = await AssetComponent.CheckAllBundlePackageUpdate(updatePackageBundle);
        if (updateBundleDataInfo.NeedUpdate)
        {
           //需要更新新的APK
           Debug.LogError("需要更新新的APK " + updateBundleDataInfo.NeedUpdateSize);
           await AssetComponent.DownLoadUpdate(updateBundleDataInfo);
           Debug.LogError("下载APK完成");
        }
        
        return;
        
        string path = Path.Combine(Application.persistentDataPath, "APK", "test.apk");
        //调用Java更新安装接口
        AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.InstallApkHelper");
        androidJavaClass.CallStatic("InstallApk", path);
        
    }
}
