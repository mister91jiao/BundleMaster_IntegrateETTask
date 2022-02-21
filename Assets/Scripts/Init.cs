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
        DontDestroyOnLoad(gameObject);
        Initialization().Coroutine();
    }
    void Update()
    {
        AssetComponent.Update();
    }
    private async ETTask Initialization()
    {
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
        LoadHandler<GameObject> loginUIHandler = await AssetComponent.LoadAsync<GameObject>(BPath.Assets_Bundles_LoginUI__prefab);
        GameObject loginUIObj = UnityEngine.Object.Instantiate(loginUIHandler.Asset, uiManagerTf, false);
        loginUIObj.transform.Find("Login").GetComponent<Button>().onClick.AddListener(() =>
        {
            //卸载资源
            GameObject.Destroy(loginUIObj);
            loginUIHandler.UnLoad();
            
            LoadNewScene().Coroutine();
        });

    }

    private async ETTask LoadNewScene()
    {
        LoadSceneHandler loadSceneHandler = await AssetComponent.LoadSceneAsync(BPath.Assets_Scenes_Game__unity);
        //如果需要获取场景加载进度, 用这种加载方式 loadSceneHandler2.GetProgress() , 注意进度不是线性的
        // ETTask loadSceneHandlerTask = AssetComponent.LoadSceneAsync(out LoadSceneHandler loadSceneHandler2, "Assets/Scenes/Game.unity");
        // await loadSceneHandlerTask;
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        operation.completed += asyncOperation =>
        {
            //同步加载资源(加载分包内的资源)
            LoadHandler<GameObject> loadHandler = AssetComponent.Load<GameObject>(BPath.Assets_Bundles_SubBundleAssets_mister91jiao__prefab, "SubBundle");
            GameObject obj = UnityEngine.Object.Instantiate(loadHandler.Asset);
            
            LoadHandler<GameObject> loadHandler1 = AssetComponent.Load<GameObject>(BPath.Assets_Bundles_SubBundleAssets_mister91jiao__prefab);
            GameObject obj1 = UnityEngine.Object.Instantiate(loadHandler1.Asset);
            
            ResetUI().Coroutine();
        };
    }

    private async ETTask ResetUI()
    {
        Transform uiManagerTf = gameObject.transform.Find("UIManager");
        //异步加载资源
        LoadHandler<GameObject> resetUIHandler = await AssetComponent.LoadAsync<GameObject>(BPath.Assets_Bundles_ResetUI__prefab);
        GameObject resetUIObj = UnityEngine.Object.Instantiate(resetUIHandler.Asset, uiManagerTf, false);
        resetUIObj.transform.Find("Reset").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject.Destroy(resetUIObj);
            AssetComponent.UnInitializeAll();
            
            AsyncOperation operation = SceneManager.LoadSceneAsync("Init_2");
            operation.completed += asyncOperation =>
            {
                //重新加载资源
                Initialization().Coroutine();
            };
        });
    }
    
}
