using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BM
{
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public partial class BundleMasterWindow : EditorWindow
    {
        private static BundleMasterWindow _instance = null;

        /// <summary>
        /// 自动刷新配置界面
        /// </summary>
        private static bool _autoFlush = true;
        
        /// <summary>
        /// 需要刷新
        /// </summary>
        private bool needFlush = false;
        
        /// <summary>
        /// 运行时配置文件
        /// </summary>
        private static BundleMasterRuntimeConfig _bundleMasterRuntimeConfig = null;

        private static bool _runtimeConfigLoad = false;
        
        private static int _w = 960;
        private static int _h = 540;
        
        /// <summary>
        /// 运行时配置文件的路径
        /// </summary>
        public static string RuntimeConfigPath = "Assets/Resources/BMConfig.asset";
        
        /// <summary>
        /// 分包文件资源索引配置
        /// </summary>
        public static string AssetLoadTablePath = "Assets/Editor/BundleMasterEditor/BuildSettings/AssetLoadTable.asset";

        /// <summary>
        /// 分包配置信息
        /// </summary>
        public static string AssetsLoadSettingPath = "Assets/Editor/BundleMasterEditor/BuildSettings/AssetsLoadSetting";
        
        /// <summary>
        /// 原生资源包配置信息
        /// </summary>
        public static string AssetsOriginSettingPath = "Assets/Editor/BundleMasterEditor/BuildSettings/AssetsOriginSetting";
        
        private static AssetLoadTable _assetLoadTable = null;

        /// <summary>
        /// 选中查看的分包信息
        /// </summary>
        private static AssetsSetting _selectAssetsSetting = null;
        
        /// <summary>
        /// 是否查看子页面
        /// </summary>
        private static bool _viewSub = false;
        
        [MenuItem("Tools/BuildAsset/打开配置界面")]
        public static void Init()
        {
            Open(true);
        }
        
        Vector2 scrollScenePos = Vector2.zero;
        Vector2 scrollPos = Vector2.zero;
        Vector2 scrollBundleScenePos = Vector2.zero;
        Vector2 scrollPathPos = Vector2.zero;
        
        private static void Open(bool focus)
        {
            if (_instance != null)
            {
                return;
            }
            _viewSub = false;
            _instance = (BundleMasterWindow)EditorWindow.GetWindow(typeof(BundleMasterWindow), true, "BundleMasterEditor", focus);
            //_instance.position = new Rect(_w / 2, _h / 2, _w, _h);
            _instance.maxSize = new Vector2(_w, _h);
            _instance.minSize = new Vector2(_w, _h);
            //加载配置文件
            _bundleMasterRuntimeConfig = AssetDatabase.LoadAssetAtPath<BundleMasterRuntimeConfig>(RuntimeConfigPath);
            _runtimeConfigLoad = false;
            if (_bundleMasterRuntimeConfig != null)
            {
                _runtimeConfigLoad = true;
            }
        }
        public void OnGUI()
        {
            Open(false);
            if (!_runtimeConfigLoad)
            {
                GUILayout.BeginArea(new Rect(_w / 4, _h / 8, _w / 2, _h / 4));
                if (GUILayout.Button("创建运行时配置文件", GUILayout.Width(_w / 2), GUILayout.Height(_h / 4), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                {
                    _bundleMasterRuntimeConfig = ScriptableObject.CreateInstance<BundleMasterRuntimeConfig>();
                    _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Develop;
                    _bundleMasterRuntimeConfig.MaxDownLoadCount = 8;
                    _bundleMasterRuntimeConfig.ReDownLoadCount = 3;
                    if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources")))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources"));
                    }
                    AssetDatabase.CreateAsset(_bundleMasterRuntimeConfig, RuntimeConfigPath);
                    AssetDatabase.Refresh();
                    _runtimeConfigLoad = true;
                }
                GUILayout.EndArea();
                return;
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("当前资源加载模式: \t" + _bundleMasterRuntimeConfig.AssetLoadMode, GUILayout.Width(_w / 4), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(false));
            if (GUILayout.Button("开发模式", GUILayout.Width(_w / 6), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(true)))
            {
                _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Develop;
                DevelopSceneChange.CheckSceneChange(_bundleMasterRuntimeConfig.AssetLoadMode);
                needFlush = true;
            }
            if (GUILayout.Button("本地模式", GUILayout.Width(_w / 6), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(true)))
            {
                _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Local;
                DevelopSceneChange.CheckSceneChange(_bundleMasterRuntimeConfig.AssetLoadMode);
                needFlush = true;
            }
            if (GUILayout.Button("构建模式", GUILayout.Width(_w / 6), GUILayout.Height(_h / 8), GUILayout.ExpandWidth(true)))
            {
                _bundleMasterRuntimeConfig.AssetLoadMode = AssetLoadMode.Build;
                DevelopSceneChange.CheckSceneChange(_bundleMasterRuntimeConfig.AssetLoadMode);
                needFlush = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
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
            _autoFlush = GUILayout.Toggle(_autoFlush, "是否自动应用更新配置界面数据");
            if (!_autoFlush)
            {
                if (GUILayout.Button("应用更新"))
                {
                    needFlush = false;
                    Flush();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("--- <构建AssetBundle配置> ----------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("分包配置总索引文件: ", GUILayout.Width(_w / 8), GUILayout.ExpandWidth(false));
            _assetLoadTable =  (AssetLoadTable)EditorGUILayout.ObjectField(_assetLoadTable, typeof(AssetLoadTable), true, GUILayout.Width(_w / 3), GUILayout.ExpandWidth(false));
            bool noTable = _assetLoadTable == null;
            if (GUILayout.Button("查找或创建分包配置索引文件", GUILayout.Width(_w / 5.5f), GUILayout.ExpandWidth(true)))
            {
                _assetLoadTable = AssetDatabase.LoadAssetAtPath<AssetLoadTable>(AssetLoadTablePath);
                if (_assetLoadTable == null)
                {
                    _assetLoadTable = ScriptableObject.CreateInstance<AssetLoadTable>();
                    AssetDatabase.CreateAsset(_assetLoadTable, BundleMasterWindow.AssetLoadTablePath);
                    needFlush = true;
                }
            }
            EditorGUI.BeginDisabledGroup(noTable);
            GUI.color = new Color(0.654902F, 0.9921569F, 0.2784314F);
            if (GUILayout.Button("添加一个分包配置", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(true)))
            {
                int index = 0;
                while (true)
                {
                    AssetsLoadSetting assetLoadTable = AssetDatabase.LoadAssetAtPath<AssetsLoadSetting>(AssetsLoadSettingPath + "_" + index + ".asset");
                    if (assetLoadTable == null)
                    {
                        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AssetsLoadSetting>(), AssetsLoadSettingPath + "_" + index + ".asset");
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
                needFlush = true;
            }
            GUI.color = Color.white;
            GUI.color = new Color(0.9921569F, 0.7960784F, 0.509804F);
            if (GUILayout.Button("添加一个原生资源包配置", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(true)))
            {
                int index = 0;
                while (true)
                {
                    AssetsOriginSetting assetsOriginSetting = AssetDatabase.LoadAssetAtPath<AssetsOriginSetting>(AssetsOriginSettingPath + "_" + index + ".asset");
                    if (assetsOriginSetting == null)
                    {
                        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AssetsOriginSetting>(), AssetsOriginSettingPath + "_" + index + ".asset");
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
                needFlush = true;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            if (!_viewSub)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("--- <入口场景> ----------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                GUILayout.BeginArea(new Rect(_w / 1.5f, 175, 400, 200));
                GUI.color = new Color(0.9921569F, 0.7960784F, 0.509804F);
                GUILayout.Label("初始场景是不需要打进AssetBundle里的, 这\n里填的初始场景会自动放入 Build Settings 中\n的 Scenes In Build 里。");
                GUI.color = Color.white;
                GUILayout.EndArea();
                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(noTable);
                if (GUILayout.Button("增加一个入口场景", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(false)))
                {
                    _assetLoadTable.InitScene.Add(null);
                    needFlush = true;
                }
                if (GUILayout.Button("清空所有入口场景", GUILayout.Width(_w / 6), GUILayout.ExpandWidth(false)))
                {
                    _assetLoadTable.InitScene.Clear();
                    needFlush = true;
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                scrollScenePos = EditorGUILayout.BeginScrollView(scrollScenePos, false, false, GUILayout.Height(_h / 6), GUILayout.ExpandHeight(true));
                if (!noTable)
                {
                    HashSet<int> needRemoveScene = new HashSet<int>();
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
                            needRemoveScene.Add(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                    if (needRemoveScene.Count > 0)
                    {
                        List<SceneAsset> changeSceneList = new List<SceneAsset>();
                        for (int i = 0; i < _assetLoadTable.InitScene.Count; i++)
                        {
                            if (!needRemoveScene.Contains(i))
                            {
                                changeSceneList.Add(_assetLoadTable.InitScene[i]);
                            }
                        }
                        _assetLoadTable.InitScene = changeSceneList;
                        needFlush = true;
                    }
                    
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("--- <配置索引索引文件信息> --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                EditorGUI.BeginDisabledGroup(noTable);
                GUILayout.BeginHorizontal();
                GUILayout.Label("相对构建路径文件夹名称[BundlePath]: ", GUILayout.Width(_w / 4.5f), GUILayout.ExpandWidth(false));
                string bundlePath = "";
                if (!noTable)
                {
                    bundlePath = _assetLoadTable.BundlePath;
                }
                bundlePath = EditorGUILayout.TextField(bundlePath, GUILayout.Width(_w / 8), GUILayout.ExpandWidth(false));
                if (!noTable)
                {
                    if (!string.Equals(_assetLoadTable.BundlePath, bundlePath, StringComparison.Ordinal))
                    {
                        _assetLoadTable.BundlePath = bundlePath;
                        needFlush = true;
                    }
                }
                GUILayout.Label("是否启用绝对路径: ", GUILayout.Width(_w / 9), GUILayout.ExpandWidth(false));
                bool enableRelativePath = false;
                if (!noTable)
                {
                    enableRelativePath = _assetLoadTable.EnableRelativePath;
                }
                bool enableRelativePathChange = EditorGUILayout.Toggle(enableRelativePath, GUILayout.Width(_w / 80), GUILayout.ExpandWidth(false));
                if (!noTable)
                {
                    if (enableRelativePath != enableRelativePathChange)
                    {
                        _assetLoadTable.EnableRelativePath = enableRelativePathChange;
                        needFlush = true;
                    }
                }
                GUILayout.Label("绝对路径: ", GUILayout.Width(_w / 16), GUILayout.ExpandWidth(false));
                string relativePath = "";
                if (!noTable)
                {
                    relativePath = _assetLoadTable.RelativePath;
                }
                relativePath = EditorGUILayout.TextField(relativePath, GUILayout.Width(_w / 2.5f), GUILayout.ExpandWidth(false));
                if (!noTable)
                {
                    if (!string.Equals(_assetLoadTable.RelativePath, relativePath, StringComparison.Ordinal))
                    {
                        _assetLoadTable.RelativePath = relativePath;
                        needFlush = true;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(_h / 64);
                GUILayout.BeginHorizontal();
                GUILayout.Label("加密文件夹名称: ", GUILayout.Width(_w / 10), GUILayout.ExpandWidth(false));
                string encryptPathFolder = "";
                if (!noTable)
                {
                    encryptPathFolder = _assetLoadTable.EncryptPathFolder;
                }
                encryptPathFolder = EditorGUILayout.TextField(encryptPathFolder, GUILayout.Width(_w / 8), GUILayout.ExpandWidth(false));
                if (!noTable)
                {
                    if (!string.Equals(_assetLoadTable.EncryptPathFolder, encryptPathFolder, StringComparison.Ordinal))
                    {
                        _assetLoadTable.EncryptPathFolder = encryptPathFolder;
                        needFlush = true;
                    }
                }
                GUILayout.Label("是否生成路径代码: ", GUILayout.Width(_w / 9), GUILayout.ExpandWidth(false));
                bool generatePathCode = false;
                if (!noTable)
                {
                    generatePathCode = _assetLoadTable.GeneratePathCode;
                }
                bool generatePathCodeChange = EditorGUILayout.Toggle(generatePathCode, GUILayout.Width(_w / 80), GUILayout.ExpandWidth(false));
                if (!noTable)
                {
                    if (generatePathCode != generatePathCodeChange)
                    {
                        _assetLoadTable.GeneratePathCode = generatePathCodeChange;
                        needFlush = true;
                    }
                }
                GUILayout.Label("Assets下代码生成路径: ", GUILayout.Width(_w / 7), GUILayout.ExpandWidth(false));
                string generateCodeScriptPath = "";
                if (!noTable)
                {
                    generateCodeScriptPath = _assetLoadTable.GenerateCodeScriptPath;
                }
                generateCodeScriptPath = EditorGUILayout.TextField(generateCodeScriptPath, GUILayout.Width(_w / 4), GUILayout.ExpandWidth(false));
                if (!noTable)
                {
                    if (!string.Equals(_assetLoadTable.GenerateCodeScriptPath, generateCodeScriptPath, StringComparison.Ordinal))
                    {
                        _assetLoadTable.GenerateCodeScriptPath = generateCodeScriptPath;
                        needFlush = true;
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                GUILayout.BeginHorizontal();
                GUILayout.Label("--- <所有分包配置文件> --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                //处理单个分包
                GUILayout.BeginHorizontal();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.ExpandHeight(true));
                foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(AssetsSetting)}"))
                {
                    AssetsSetting loadSetting = AssetDatabase.LoadAssetAtPath<AssetsSetting>(AssetDatabase.GUIDToAssetPath(guid));
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(loadSetting, typeof(AssetsLoadSetting), false, GUILayout.Width(_w / 3),GUILayout.ExpandHeight(false));
                    EditorGUI.EndDisabledGroup();
                    GUILayout.Label("是否启用当前分包配置 ", GUILayout.Width(_w / 7),GUILayout.ExpandHeight(false));
                    if (!noTable)
                    {
                        bool enable = _assetLoadTable.AssetsSettings.Contains(loadSetting);
                        bool enableChange = EditorGUILayout.Toggle(enable);
                        if (enable != enableChange)
                        {
                            if (enableChange)
                            {
                                _assetLoadTable.AssetsSettings.Add(loadSetting);
                            }
                            else
                            {
                                _assetLoadTable.AssetsSettings.Remove(loadSetting);
                            }
                            needFlush = true;
                        }
                    }
                    GUI.color = new Color(0.9921569F, 0.7960784F, 0.509804F);
                    if (GUILayout.Button("选择查看此分包配置信息", GUILayout.Width(_w / 4), GUILayout.ExpandWidth(false)))
                    {
                        _selectAssetsSetting = loadSetting;
                        _viewSub = true;
                    }
                    GUI.color = Color.white;
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndHorizontal();
            }
            else
            {
                if (_selectAssetsSetting is AssetsLoadSetting)
                {
                    AssetsLoadSettingRender();
                }
                else
                {
                    
                }
            }
            
            if (needFlush && _autoFlush)
            {
                needFlush = false;
                Flush();
            }
        }
        
        public void OnDestroy()
        {
            _instance = null;
        }
    }
}