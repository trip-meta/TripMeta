using UnityEngine;
using System.Collections.Generic;

namespace TripMeta.Core
{
    /// <summary>
    /// 启动验证器 - 检查项目运行所需的基础条件
    /// </summary>
    public class StartupValidator : MonoBehaviour
    {
        [Header("验证设置")]
        [SerializeField] private bool runValidationOnStart = true;
        [SerializeField] private bool showDetailedLog = true;
        
        private List<string> validationErrors = new List<string>();
        private List<string> validationWarnings = new List<string>();
        
        void Awake()
        {
            if (runValidationOnStart)
            {
                RunStartupValidation();
            }
        }
        
        /// <summary>
        /// 运行启动验证
        /// </summary>
        public bool RunStartupValidation()
        {
            validationErrors.Clear();
            validationWarnings.Clear();
            
            Debug.Log("=== TripMeta 启动验证开始 ===");
            
            // 验证Unity版本
            ValidateUnityVersion();
            
            // 验证平台设置
            ValidatePlatformSettings();
            
            // 验证PICO SDK
            ValidatePICOSDK();
            
            // 验证必需组件
            ValidateRequiredComponents();
            
            // 验证项目设置
            ValidateProjectSettings();
            
            // 输出验证结果
            OutputValidationResults();
            
            return validationErrors.Count == 0;
        }
        
        /// <summary>
        /// 验证Unity版本
        /// </summary>
        private void ValidateUnityVersion()
        {
            string unityVersion = Application.unityVersion;
            
            if (showDetailedLog)
                Debug.Log($"Unity版本: {unityVersion}");
            
            // 检查是否为推荐版本
            if (!unityVersion.StartsWith("2022.3") && !unityVersion.StartsWith("2023.2"))
            {
                validationWarnings.Add($"建议使用Unity 2022.3 LTS或2023.2 LTS，当前版本: {unityVersion}");
            }
        }
        
        /// <summary>
        /// 验证平台设置
        /// </summary>
        private void ValidatePlatformSettings()
        {
            var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            
            if (showDetailedLog)
                Debug.Log($"构建目标: {buildTarget}");
            
            // 检查是否为Android平台（PICO设备）
            if (buildTarget != UnityEditor.BuildTarget.Android)
            {
                validationWarnings.Add("PICO设备需要Android构建目标");
            }
            
            // 检查API级别
            var apiLevel = UnityEditor.PlayerSettings.Android.minSdkVersion;
            if (apiLevel < UnityEditor.AndroidSdkVersions.AndroidApiLevel23)
            {
                validationErrors.Add($"Android API级别过低，需要API 23+，当前: {apiLevel}");
            }
        }
        
        /// <summary>
        /// 验证PICO SDK
        /// </summary>
        private void ValidatePICOSDK()
        {
            try
            {
                // 检查PICO SDK是否存在
                var picoSDKPath = "Assets/PICO Unity Integration SDK v211";
                if (!System.IO.Directory.Exists(picoSDKPath))
                {
                    validationErrors.Add("未找到PICO Unity Integration SDK");
                    return;
                }
                
                if (showDetailedLog)
                    Debug.Log("PICO SDK: 已找到");
                
                // 检查关键PICO组件
                var picoManagerType = System.Type.GetType("Unity.XR.PXR.PXR_Manager");
                if (picoManagerType == null)
                {
                    validationErrors.Add("PICO SDK未正确导入或配置");
                }
                else
                {
                    if (showDetailedLog)
                        Debug.Log("PICO SDK: 组件正常");
                }
            }
            catch (System.Exception e)
            {
                validationErrors.Add($"PICO SDK验证失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 验证必需组件
        /// </summary>
        private void ValidateRequiredComponents()
        {
            // 检查GameManager
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                validationWarnings.Add("场景中未找到GameManager，将自动创建");
            }
            else
            {
                if (showDetailedLog)
                    Debug.Log("GameManager: 已找到");
            }
            
            // 检查主摄像机
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                validationErrors.Add("场景中未找到主摄像机");
            }
            else
            {
                if (showDetailedLog)
                    Debug.Log("主摄像机: 已找到");
            }
            
            // 检查XR Rig
            var xrRig = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRRig>();
            if (xrRig == null)
            {
                validationWarnings.Add("未找到XR Rig，VR功能可能无法正常工作");
            }
            else
            {
                if (showDetailedLog)
                    Debug.Log("XR Rig: 已找到");
            }
        }
        
        /// <summary>
        /// 验证项目设置
        /// </summary>
        private void ValidateProjectSettings()
        {
            // 检查XR设置
            var xrSettings = UnityEngine.XR.XRGeneralSettings.Instance;
            if (xrSettings == null)
            {
                validationErrors.Add("XR设置未配置");
            }
            else
            {
                if (showDetailedLog)
                    Debug.Log("XR设置: 已配置");
            }
            
            // 检查渲染管线
            var renderPipeline = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
            if (renderPipeline == null)
            {
                validationWarnings.Add("建议使用URP渲染管线以获得更好的VR性能");
            }
            else
            {
                if (showDetailedLog)
                    Debug.Log($"渲染管线: {renderPipeline.GetType().Name}");
            }
            
            // 检查质量设置
            var qualityLevel = QualitySettings.GetQualityLevel();
            if (showDetailedLog)
                Debug.Log($"质量级别: {QualitySettings.names[qualityLevel]}");
        }
        
        /// <summary>
        /// 输出验证结果
        /// </summary>
        private void OutputValidationResults()
        {
            Debug.Log("=== 验证结果 ===");
            
            if (validationErrors.Count == 0 && validationWarnings.Count == 0)
            {
                Debug.Log("✅ 所有验证通过，项目可以正常运行");
            }
            else
            {
                if (validationErrors.Count > 0)
                {
                    Debug.LogError($"❌ 发现 {validationErrors.Count} 个错误:");
                    foreach (var error in validationErrors)
                    {
                        Debug.LogError($"   • {error}");
                    }
                }
                
                if (validationWarnings.Count > 0)
                {
                    Debug.LogWarning($"⚠️ 发现 {validationWarnings.Count} 个警告:");
                    foreach (var warning in validationWarnings)
                    {
                        Debug.LogWarning($"   • {warning}");
                    }
                }
            }
            
            Debug.Log("=== 验证完成 ===");
        }
        
        /// <summary>
        /// 获取验证结果
        /// </summary>
        public ValidationResult GetValidationResult()
        {
            return new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors.ToArray(),
                Warnings = validationWarnings.ToArray()
            };
        }
    }
    
    /// <summary>
    /// 验证结果数据结构
    /// </summary>
    [System.Serializable]
    public class ValidationResult
    {
        public bool IsValid;
        public string[] Errors;
        public string[] Warnings;
    }
}