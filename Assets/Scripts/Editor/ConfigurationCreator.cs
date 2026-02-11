using UnityEngine;
using UnityEditor;
using System.IO;
using TripMeta.Core.Configuration;

namespace TripMeta.Editor
{
    /// <summary>
    /// 配置文件创建工具 - 编辑器脚本
    /// </summary>
    public class ConfigurationCreator : EditorWindow
    {
        [MenuItem("TripMeta/Create Configuration Assets")]
        public static void ShowWindow()
        {
            GetWindow<ConfigurationCreator>("TripMeta Config Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("TripMeta Configuration Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will create the required configuration assets for TripMeta.\n\n" +
                "The following assets will be created in Resources/Config/:",
                MessageType.Info);

            GUILayout.Label("Configuration Assets:", EditorStyles.boldLabel);
            GUILayout.Label("• TripMetaConfig.asset - Main configuration");
            GUILayout.Label("• AppSettings.asset - Application settings");
            GUILayout.Space(10);

            if (GUILayout.Button("Create All Configuration Assets", GUILayout.Height(40)))
            {
                CreateAllConfigurations();
            }
        }

        private static void CreateAllConfigurations()
        {
            string configPath = "Assets/Resources/Config";

            // Create directory if it doesn't exist
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
                AssetDatabase.Refresh();
            }

            // Create TripMetaConfig
            CreateTripMetaConfig(configPath);

            // Create AppSettings
            CreateAppSettings(configPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Configuration assets created successfully!", "OK");
            Debug.Log("[ConfigurationCreator] All configuration assets created at: " + configPath);
        }

        private static void CreateTripMetaConfig(string path)
        {
            var config = ScriptableObject.CreateInstance<TripMetaConfig>();
            config.ResetToDefault();

            string assetPath = Path.Combine(path, "TripMetaConfig.asset");
            AssetDatabase.CreateAsset(config, assetPath);
            Debug.Log("[ConfigurationCreator] Created TripMetaConfig");
        }

        private static void CreateAppSettings(string path)
        {
            var settings = ScriptableObject.CreateInstance<AppSettings>();

            string assetPath = Path.Combine(path, "AppSettings.asset");
            AssetDatabase.CreateAsset(settings, assetPath);
            Debug.Log("[ConfigurationCreator] Created AppSettings");
        }
    }
}
