using System.Collections.Generic;
using UnityEngine;
using ET;
using BM;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Init : MonoBehaviour
{
    void Start()
    {
        Initialization().Coroutine();
    }
    
    void Update()
    {
        AssetComponent.Update();
    }

    private async ETTask Initialization()
    {
        DontDestroyOnLoad(gameObject);
        await CheckHotfix();
        await InitUI();
    }
    
    private async ETTask CheckHotfix()
    {
        //重新配置热更路径
        AssetComponentConfig.HotfixPath = Application.dataPath + "/../HotfixBundles/";
        
        AssetComponentConfig.DefaultBundlePackageName = "AllBundle";
        List<string> updatePackageBundle = new List<string>(){AssetComponentConfig.DefaultBundlePackageName, "SubBundle"};
        UpdateBundleDataInfo updateBundleDataInfo = await AssetComponent.CheckAllBundlePackageUpdate(updatePackageBundle);
        if (updateBundleDataInfo.NeedUpdate)
        {
            Debug.LogError("需要更新, 大小: " + updateBundleDataInfo.NeedUpdateSize);
            await AssetComponent.DownLoadUpdate(updateBundleDataInfo);
        }
        await AssetComponent.Initialize(AssetComponentConfig.DefaultBundlePackageName);
        await AssetComponent.Initialize("SubBundle");
    }

    private async ETTask InitUI()
    {
        Transform uiManagerTf = gameObject.transform.Find("UIManager");
        //异步加载资源
        LoadHandler<GameObject> loginUIHandler = await AssetComponent.LoadAsync<GameObject>("Assets/Bundles/LoginUI.prefab");
        GameObject loginUIObj = UnityEngine.Object.Instantiate(loginUIHandler.Asset, uiManagerTf, false);
        loginUIObj.transform.Find("Login").GetComponent<Button>().onClick.AddListener(() =>
        {
            //卸载资源
            GameObject.Destroy(loginUIObj);
            loginUIHandler.UnLoad();
            
            loadNewScene().Coroutine();
        });

    }

    private async ETTask loadNewScene()
    {
        LoadSceneHandler loadSceneHandler = await AssetComponent.LoadSceneAsync("Assets/Scenes/Game.unity");
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        operation.completed += asyncOperation =>
        {
            //同步加载资源(加载分包内的资源)
            LoadHandler<GameObject> loadHandler = AssetComponent.Load<GameObject>("Assets/Bundles/SubBundleAssets/mister91jiao.prefab", "SubBundle");
            GameObject obj = UnityEngine.Object.Instantiate(loadHandler.Asset);
        };
    }
}
