using System.Collections.Generic;
using System.IO;
using ET;
using UnityEngine;
using BM;
using UnityEngine.UI;

public static class InstallHelper
{
    public static async ETTask InstallApk()
    {
        //创建下载UI
        Transform uiManagerTf = GameObject.Find("UIManager").transform;
        GameObject downLoadUI = Object.Instantiate(Resources.Load<GameObject>("DownLoadUI"), uiManagerTf);
        
        Dictionary<string, bool> updatePackageBundle = new Dictionary<string, bool>()
        {
            {"APK", false},
        };
        UpdateBundleDataInfo updateBundleDataInfo = await AssetComponent.CheckAllBundlePackageUpdate(updatePackageBundle);
        Slider progressSlider = downLoadUI.transform.Find("ProgressSlider").GetComponent<Slider>();
        Text progressText = downLoadUI.transform.Find("ProgressValue/Text").GetComponent<Text>();
        Text speedText = downLoadUI.transform.Find("SpeedValue/Text").GetComponent<Text>();
        Button cancelDownLoad = downLoadUI.transform.Find("Cancel").GetComponent<Button>();
        Button reDownLoad = downLoadUI.transform.Find("ReDown").GetComponent<Button>();
        updateBundleDataInfo.DownLoadFinishCallback += () =>
        {
            Debug.LogError("下载APK完成");
        };
        updateBundleDataInfo.ProgressCallback += p =>
        {
            progressSlider.value = p / 100.0f;
            progressText.text = p.ToString("#0.00") + "%";
        };
        updateBundleDataInfo.DownLoadSpeedCallback += s =>
        {
            speedText.text = (s / 1024.0f).ToString("#0.00") + " kb/s";
        };
        if (updateBundleDataInfo.NeedUpdate)
        {
           //需要更新新的APK
           Debug.LogError("需要更新新的APK " + updateBundleDataInfo.NeedUpdateSize);
           await AssetComponent.DownLoadUpdate(updateBundleDataInfo);
        }
        Object.Destroy(downLoadUI);
        
        return;
        
        string path = Path.Combine(Application.persistentDataPath, "APK", "test.apk");
        //调用Java更新安装接口
        AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.InstallApkHelper");
        androidJavaClass.CallStatic("InstallApk", path);
        
    }
}
