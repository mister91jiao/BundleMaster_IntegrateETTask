using System;
using System.Collections.Generic;
using UnityEngine;
using ET;
using BM;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Init : MonoBehaviour
{
    private void Awake()
    {
        System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            AssetLogHelper.LogError(e.ExceptionObject.ToString());
        };
        ETTask.ExceptionHandler += AssetLogHelper.LogError;
    }

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
        //重新配置热更路径(开发方便用, 打包移动端需要注释注释)
        AssetComponentConfig.HotfixPath = Application.dataPath + "/../HotfixBundles/";
        
        AssetComponentConfig.DefaultBundlePackageName = "AllBundle";
        Dictionary<string, bool> updatePackageBundle = new Dictionary<string, bool>()
        {
            {AssetComponentConfig.DefaultBundlePackageName, false},
            {"SubBundle", false},
            {"OriginFile", false},
        };
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
        //加载图集
        await AssetComponent.LoadAsync(out LoadHandler atlasHandler, BPath.Assets_Bundles_Atlas_UIAtlas__spriteatlasv2);
        //异步加载资源
        GameObject loginUIAsset = await AssetComponent.LoadAsync<GameObject>(out LoadHandler loginUIHandler, BPath.Assets_Bundles_LoginUI__prefab);
        GameObject loginUIObj = UnityEngine.Object.Instantiate(loginUIAsset, uiManagerTf, false);
        
        // GameObject subUI = await AssetComponent.LoadAsync<GameObject>(out LoadHandler usbUIHandler, "Assets/Bundles/SubBundleAssets/SubUI_Copy.prefab");
        // GameObject subUIObj = UnityEngine.Object.Instantiate(subUI, loginUIObj.transform, false);
        
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
            //GameObject gameObjectAsset = AssetComponent.Load<GameObject>(BPath.Assets_Bundles_SubBundleAssets_mister91jiao__prefab, "SubBundle");
            BundleRuntimeInfo bundleRuntimeInfo = AssetComponent.GetBundleRuntimeInfo("SubBundle");
            GameObject gameObjectAsset = bundleRuntimeInfo.Load<GameObject>(BPath.Assets_Bundles_SubBundleAssets_mister91jiao__prefab);
            GameObject obj = UnityEngine.Object.Instantiate(gameObjectAsset);
            // GameObject gameObjectAsset1 = AssetComponent.Load<GameObject>(BPath.Assets_Bundles_SubBundleAssets_mister91jiao__prefab);
            // GameObject obj1 = UnityEngine.Object.Instantiate(gameObjectAsset1);
            AssetComponent.LoadAsync<GameObject>(out LoadHandler handler, BPath.Assets_Bundles_SubBundleAssets_mister91jiao__prefab).Coroutine();
            handler.Completed += loadHandler =>
            {
                UnityEngine.Object.Instantiate(loadHandler.Asset);
                ResetUI().Coroutine();
            };
            LoadGroupTest().Coroutine();
        };
    }

    private async ETTask LoadGroupTest()
    {
        Texture zfnp = await AssetComponent.LoadAsync<Texture>(out LoadHandler handler, BPath.Assets_Bundles_GroupBundle_zfnp__jpg);
        //Debug.LogError(zfnp.height);
        handler.UnLoad();
    }
    
    private async ETTask ResetUI()
    {
        Transform uiManagerTf = gameObject.transform.Find("UIManager");
        //异步加载资源
        UnityEngine.Object resetUIAsset = await AssetComponent.LoadAsync(BPath.Assets_Bundles_ResetUI__prefab);
        GameObject resetUIObj = UnityEngine.Object.Instantiate(resetUIAsset as GameObject, uiManagerTf, false);
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
