using System;
using UnityEngine;
using UnityEditor;

namespace TripMeta.Editor.BuildAutomation
{
    /// <summary>
    /// 构建配置 - 存储构建相关的设置
    /// </summary>
    [CreateAssetMenu(fileName = "BuildConfiguration", menuName = "TripMeta/Build Configuration")]
    public class BuildConfiguration : ScriptableObject
    {
        [Header("基本设置")]
        public string productName = "TripMeta";
        public string version = "1.0.0";
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        public BuildOptions buildOptions = BuildOptions.None;
        public string outputPath = "Builds";
        
        [Header("VR设置")]
        public bool enableVR = true;
        public string[] vrSDKs = { "OpenXR", "Oculus" };
        public bool enableHandTracking = true;
        public bool enableEyeTracking = false;
        
        [Header("优化设置")]
        public bool enableOptimization = true;
        public CompressionLevel compressionLevel = CompressionLevel.Normal;
        public bool enableStripping = true;
        public bool enableIL2CPP = true;
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
        
        [Header("平台特定设置")]
        public AndroidBuildSettings androidSettings;
        public WindowsBuildSettings windowsSettings;
        public IOSBuildSettings iosSettings;
        
        [Header("后处理设置")]
        public bool enablePostProcessing = true;
        public string[] additionalFiles;
        public string uploadUrl = "";
        public bool generateReport = true;
        public bool notifyOnComplete = false;
        
        [Header("CI/CD设置")]
        public bool enableCICD = false;
        public string cicdProvider = "GitHub Actions";
        public string repositoryUrl = "";
        public string buildTrigger = "push";
        
        /// <summary>
        /// 初始化默认配置
        /// </summary>
        public void Initialize()
        {
            productName = Application.productName;
            version = Application.version;
            buildTarget = EditorUserBuildSettings.activeBuildTarget;
            
            // 初始化平台设置
            androidSettings = new AndroidBuildSettings();
            windowsSettings = new WindowsBuildSettings();
            iosSettings = new IOSBuildSettings();
            
            // 设置默认输出路径
            outputPath = System.IO.Path.Combine(Application.dataPath, "..", "Builds");
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(productName))
            {
                Debug.LogError("产品名称不能为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(version))
            {
                Debug.LogError("版本号不能为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(outputPath))
            {
                Debug.LogError("输出路径不能为空");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 应用到Unity设置
        /// </summary>
        public void ApplyToUnitySettings()
        {
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = version;
            
            // 应用VR设置
            if (enableVR)
            {
                // PlayerSettings.virtualRealitySupported = true;
                // XRSettings.LoadDeviceByName(vrSDKs);
            }
            
            // 应用优化设置
            PlayerSettings.stripEngineCode = enableStripping;
            
            if (enableIL2CPP)
            {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, scriptingBackend);
            }
            
            // 应用平台特定设置
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    ApplyAndroidSettings();
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    ApplyWindowsSettings();
                    break;
                case BuildTarget.iOS:
                    ApplyIOSSettings();
                    break;
            }
        }
        
        /// <summary>
        /// 应用Android设置
        /// </summary>
        private void ApplyAndroidSettings()
        {
            if (androidSettings == null) return;
            
            PlayerSettings.Android.bundleVersionCode = androidSettings.versionCode;
            PlayerSettings.Android.minSdkVersion = androidSettings.minSdkVersion;
            PlayerSettings.Android.targetSdkVersion = androidSettings.targetSdkVersion;
            
            if (!string.IsNullOrEmpty(androidSettings.keystorePath))
            {
                PlayerSettings.Android.keystoreName = androidSettings.keystorePath;
                PlayerSettings.Android.keystorePass = androidSettings.keystorePassword;
                PlayerSettings.Android.keyaliasName = androidSettings.keyAlias;
                PlayerSettings.Android.keyaliasPass = androidSettings.keyPassword;
            }
        }
        
        /// <summary>
        /// 应用Windows设置
        /// </summary>
        private void ApplyWindowsSettings()
        {
            if (windowsSettings == null) return;
            
            PlayerSettings.runInBackground = windowsSettings.runInBackground;
            PlayerSettings.displayResolutionDialog = windowsSettings.displayResolutionDialog;
            PlayerSettings.defaultIsNativeResolution = windowsSettings.defaultIsNativeResolution;
        }
        
        /// <summary>
        /// 应用iOS设置
        /// </summary>
        private void ApplyIOSSettings()
        {
            if (iosSettings == null) return;
            
            PlayerSettings.iOS.buildNumber = iosSettings.buildNumber;
            PlayerSettings.iOS.targetOSVersionString = iosSettings.targetOSVersion;
            PlayerSettings.iOS.requiresFullScreen = iosSettings.requiresFullScreen;
        }
        
        /// <summary>
        /// 获取构建摘要
        /// </summary>
        public string GetBuildSummary()
        {
            return $"产品: {productName} v{version}\n" +
                   $"平台: {buildTarget}\n" +
                   $"输出: {outputPath}\n" +
                   $"VR: {(enableVR ? "启用" : "禁用")}\n" +
                   $"优化: {(enableOptimization ? "启用" : "禁用")}";
        }
    }
    
    /// <summary>
    /// Android构建设置
    /// </summary>
    [Serializable]
    public class AndroidBuildSettings
    {
        [Header("版本设置")]
        public int versionCode = 1;
        public AndroidSdkVersions minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        public AndroidSdkVersions targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        
        [Header("签名设置")]
        public string keystorePath = "";
        public string keystorePassword = "";
        public string keyAlias = "";
        public string keyPassword = "";
        
        [Header("性能设置")]
        public bool useGradleBuild = true;
        public bool exportProject = false;
        public AndroidArchitecture targetArchitectures = AndroidArchitecture.ARM64;
        
        [Header("VR设置")]
        public bool enableOculusSupport = true;
        public bool enablePicoSupport = true;
        public bool enableOpenXR = true;
    }
    
    /// <summary>
    /// Windows构建设置
    /// </summary>
    [Serializable]
    public class WindowsBuildSettings
    {
        [Header("显示设置")]
        public bool runInBackground = true;
        public ResolutionDialogSetting displayResolutionDialog = ResolutionDialogSetting.Enabled;
        public bool defaultIsNativeResolution = true;
        
        [Header("性能设置")]
        public bool enableDirectX12 = false;
        public bool enableVulkan = false;
        
        [Header("VR设置")]
        public bool enableSteamVR = true;
        public bool enableOculusPC = true;
        public bool enableWindowsMR = true;
    }
    
    /// <summary>
    /// iOS构建设置
    /// </summary>
    [Serializable]
    public class IOSBuildSettings
    {
        [Header("版本设置")]
        public string buildNumber = "1";
        public string targetOSVersion = "11.0";
        
        [Header("显示设置")]
        public bool requiresFullScreen = true;
        public bool hideHomeButton = false;
        
        [Header("性能设置")]
        public bool enableMetal = true;
        public bool enableBitcode = false;
        
        [Header("VR设置")]
        public bool enableARKit = false;
    }
    
    /// <summary>
    /// 压缩级别枚举
    /// </summary>
    public enum CompressionLevel
    {
        None,
        Fast,
        Normal,
        High,
        Maximum
    }
}