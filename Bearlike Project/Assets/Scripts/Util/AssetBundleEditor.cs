using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Util
{
    public class AssetBundleEditor : EditorWindow
    {
        [MenuItem("Tools/AssetBundle")]
        public static void ShowWindow()
        {
            GetWindow<AssetBundleEditor>("Asset Bundle");
        }

        void OnGUI()
        {
            if (GUILayout.Button("Asset Bundle"))
            {
                MakeAssetBundle();
            }
        }

        void MakeAssetBundle()
        {
        }
        
        [MenuItem("Tool/AssetBundle/Scene")]
        public static void AssetBundleSceneMake()
        {
            // 에셋 번들이 저장될 폴더 경로
            string outputPath = "Assets/AssetBundles";  

            // 에셋 번들 빌드 옵션
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

            // 선택한 플랫폼에 따라 빌드
            BuildTarget targetPlatform = EditorUserBuildSettings.activeBuildTarget;
            BuildPipeline.BuildAssetBundles(outputPath, options, targetPlatform);
            BuildPipeline.BuildPlayer(new string[] { "Assets/YourScene.unity" }, "Assets/AssetBundles/YourSceneBundle", BuildTarget.StandaloneWindows, BuildOptions.BuildAdditionalStreamedScenes);
        }
    }
}

