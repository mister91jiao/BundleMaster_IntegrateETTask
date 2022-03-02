using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace BM
{
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public class BundleMasterWindow : EditorWindow
    {
        private static BundleMasterWindow _instance = null;

        /// <summary>
        /// 运行时配置文件
        /// </summary>
        private static BundleMasterRuntimeConfig _bundleMasterRuntimeConfig = null;

        private static bool _runtimeConfigLoad = false;
        
        private static int _w;
        private static int _h;
        
        /// <summary>
        /// 分包配置文件资源目录
        /// </summary>
        public static string AssetLoadTablePath = "Assets/Editor/BundleMasterEditor/BuildSettings/AssetLoadTable.asset";
        private static AssetLoadTable _assetLoadTable = null;

        /// <summary>
        /// 选中查看的分包信息
        /// </summary>
        private static AssetsLoadSetting _selectAssetsLoadSetting = null;
        
        
        [MenuItem("Tools/BuildAsset/打开配置界面")]
        public static void Init()
        {
            Debug.LogError("打开界面");
            if (_instance == null)
            {
                _instance = (BundleMasterWindow)EditorWindow.GetWindow(typeof(BundleMasterWindow), true, "BundleMasterEditor", true);
                _w = Screen.width / 2;
                _h = Screen.height / 2;
                _instance.position = new Rect(_w / 2, _h / 2, _w, _h);
                _instance.maxSize = new Vector2(_w, _h);
                _instance.minSize = new Vector2(_w, _h);
                //加载配置文件
                _bundleMasterRuntimeConfig = AssetDatabase.LoadAssetAtPath<BundleMasterRuntimeConfig>(AssetComponentConfig.RuntimeConfigPath);
                _runtimeConfigLoad = false;
                if (_bundleMasterRuntimeConfig != null)
                {
                    _runtimeConfigLoad = true;
                }
            }
        }
        
        Vector2 scrollScenePos = Vector2.zero;
        Vector2 scrollPos = Vector2.zero;
        
        public void OnGUI()
        {
            if (!_runtimeConfigLoad)
            {
                GUILayout.BeginArea(new Rect(_w / 4, _h / 8, _w / 2, _h / 4));
                if (GUILayout.Button("创建运行时配置文件", GUILayout.Width(_w / 2), GUILayout.Height(_h / 4), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                {
                    _bundleMasterRuntimeConfig = ScriptableObject.CreateInstance<BundleMasterRuntimeConfig>();
                    _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Develop;
                    _bundleMasterRuntimeConfig.BundleServerUrl = "";
                    _bundleMasterRuntimeConfig.MaxDownLoadCount = 8;
                    _bundleMasterRuntimeConfig.ReDownLoadCount = 3;
                    if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources")))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources"));
                    }
                    AssetDatabase.CreateAsset(_bundleMasterRuntimeConfig, AssetComponentConfig.RuntimeConfigPath);
                    AssetDatabase.Refresh();
                    _runtimeConfigLoad = true;
                }
                GUILayout.EndArea();
                return;
            }

            bool needFlush = false;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("当前资源加载模式: \t" + _bundleMasterRuntimeConfig.AssetLoadMode, GUILayout.Width(_w / 4), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(false));
            if (GUILayout.Button("开发模式", GUILayout.Width(_w / 6), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(true)))
            {
                _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Develop;
            }
            if (GUILayout.Button("本地模式", GUILayout.Width(_w / 6), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(true)))
            {
                _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Local;
            }
            if (GUILayout.Button("构建模式", GUILayout.Width(_w / 6), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(true)))
            {
                _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Build;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(_h / 32);
            GUILayout.BeginHorizontal(GUILayout.Height(_h / 16), GUILayout.ExpandHeight(false));
            GUILayout.Label("资源服务器地址: ", GUILayout.Width(_w / 10), GUILayout.ExpandWidth(false));
            string bundleServerUrl = _bundleMasterRuntimeConfig.BundleServerUrl;
            bundleServerUrl = EditorGUILayout.TextField(bundleServerUrl, GUILayout.MinWidth(_w / 5), GUILayout.ExpandWidth(true));
            if (!string.Equals(_bundleMasterRuntimeConfig.BundleServerUrl, bundleServerUrl, StringComparison.Ordinal))
            {
                _bundleMasterRuntimeConfig.BundleServerUrl = bundleServerUrl;
                needFlush = true;
            }
            GUILayout.Label("最大同时下载资源数: ", GUILayout.Width(_w / 8), GUILayout.ExpandWidth(false));
            int maxDownLoadCount = _bundleMasterRuntimeConfig.MaxDownLoadCount;
            maxDownLoadCount = EditorGUILayout.IntField(maxDownLoadCount, GUILayout.Width(_w / 16), GUILayout.ExpandWidth(false));
            if (_bundleMasterRuntimeConfig.MaxDownLoadCount != maxDownLoadCount)
            {
                _bundleMasterRuntimeConfig.MaxDownLoadCount = maxDownLoadCount;
                needFlush = true;
            }
            GUILayout.Label("下载失败重试数: ", GUILayout.Width(_w / 10), GUILayout.ExpandWidth(false));
            int reDownLoadCount = _bundleMasterRuntimeConfig.ReDownLoadCount;
            reDownLoadCount = EditorGUILayout.IntField(reDownLoadCount, GUILayout.Width(_w / 16), GUILayout.ExpandWidth(false));
            if (_bundleMasterRuntimeConfig.ReDownLoadCount != reDownLoadCount)
            {
                _bundleMasterRuntimeConfig.ReDownLoadCount = reDownLoadCount;
                needFlush = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("分包配置总索引文件: ", GUILayout.Width(_w / 8), GUILayout.ExpandWidth(false));
            _assetLoadTable =  (AssetLoadTable)EditorGUILayout.ObjectField(_assetLoadTable, typeof(AssetLoadTable), true, GUILayout.Width(_w / 3), GUILayout.ExpandWidth(false));
            bool noTable = _assetLoadTable == null;
            if (GUILayout.Button("创建分包配置总索引文件", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(true)))
            {
                bool create = EditorUtility.DisplayDialog("警告", "已经存在一个或多个分包配置总索引文件了, 创建会覆盖路径下同名文件, 确定要继续创建吗? ", "确定创建", "取消创建");
                Debug.LogError(create);
                needFlush = true;
            }
            EditorGUI.BeginDisabledGroup(noTable);
            GUI.color = new Color(0.654902F, 0.9921569F, 0.2784314F);
            if (GUILayout.Button("添加一个分包配置", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(true)))
            {
                needFlush = true;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("--- <入口场景> ----------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.BeginArea(new Rect(_w / 1.5f, _h / 3, 400, 200));
            GUI.color = new Color(0.9921569F, 0.7960784F, 0.509804F);
            GUILayout.Label("初始场景是不需要打进AssetBundle里的, 这\n里填的初始场景会自动放入 Build Settings 中\n的 Scenes In Build 里。");
            GUI.color = Color.white;
            GUILayout.EndArea();
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(noTable);
            if (GUILayout.Button("增加一个入口场景", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(false)))
            {
                needFlush = true;
            }
            if (GUILayout.Button("清空所有入口场景", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(false)))
            {
                needFlush = true;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            scrollScenePos = EditorGUILayout.BeginScrollView(scrollScenePos, false, false, GUILayout.Height(_h / 10), GUILayout.ExpandHeight(true));
            if (!noTable)
            {
                for (int i = 0; i < _assetLoadTable.InitScene.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    SceneAsset sceneAsset = _assetLoadTable.InitScene[i];
                    if (sceneAsset != null)
                    {
                        SceneAsset asset = (SceneAsset)EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false, GUILayout.Width(_w / 3),GUILayout.ExpandHeight(false));
                        if (asset == null || asset != sceneAsset)
                        {
                            _assetLoadTable.InitScene[i] = asset;
                            needFlush = true;
                        }
                    }
                    else
                    {
                        SceneAsset asset = (SceneAsset)EditorGUILayout.ObjectField(null, typeof(SceneAsset), false, GUILayout.Width(_w / 3),GUILayout.ExpandHeight(false));
                        if (asset != null)
                        {
                            _assetLoadTable.InitScene[i] = asset;
                            needFlush = true;
                        }
                    }
                    if (GUILayout.Button("将此场景从入口场景中移除", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(false)))
                    {
                
                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("--- <所有分包配置文件> ----------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            //处理单个分包
            GUILayout.BeginHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(_h / 6), GUILayout.ExpandHeight(true));
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(AssetsLoadSetting)}"))
            {
                AssetsLoadSetting loadSetting = AssetDatabase.LoadAssetAtPath<AssetsLoadSetting>(AssetDatabase.GUIDToAssetPath(guid));
                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(loadSetting, typeof(AssetsLoadSetting), false, GUILayout.Width(_w / 3),GUILayout.ExpandHeight(false));
                EditorGUI.EndDisabledGroup();
                GUILayout.Label("是否启用当前分包配置 ", GUILayout.Width(_w / 7),GUILayout.ExpandHeight(false));
                if (!noTable)
                {
                    bool enable = _assetLoadTable.AssetsLoadSettings.Contains(loadSetting);
                    bool enableChange = EditorGUILayout.Toggle(enable);
                    if (enable != enableChange)
                    {
                        if (enableChange)
                        {
                            _assetLoadTable.AssetsLoadSettings.Add(loadSetting);
                        }
                        else
                        {
                            _assetLoadTable.AssetsLoadSettings.Remove(loadSetting);
                        }
                        needFlush = true;
                    }
                }
                if (GUILayout.Button("选择查看此分包配置信息", GUILayout.Width(_w / 4), GUILayout.ExpandWidth(false)))
                {
                    _selectAssetsLoadSetting = loadSetting;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.Label("--- <选中的分包配置信息> ----------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal();
            GUILayout.Label("当前选择的分包配置信息文件: ", GUILayout.ExpandWidth(false));
            _selectAssetsLoadSetting = (AssetsLoadSetting)EditorGUILayout.ObjectField(_selectAssetsLoadSetting, typeof(AssetsLoadSetting), true, GUILayout.Width(_w / 3),GUILayout.ExpandHeight(false));
            GUILayout.Label("\t\t\t\t\t\t", GUILayout.ExpandWidth(false));
            GUI.color = new Color(0.9921569F, 0.2745098F, 0.282353F);
            if (GUILayout.Button("删除当前选择的分包配置", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(false)))
            {
                
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
            if (needFlush)
            {
                
            }
        }

        public void OnDestroy()
        {
            _instance = null;
        }
    }
}