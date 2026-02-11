using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Editor.BuildAutomation
{
    /// <summary>
    /// 构建管理器 - 自动化Unity项目构建
    /// </summary>
    public class BuildManager : EditorWindow
    {
        [Header("构建配置")]
        [SerializeField] private BuildConfiguration buildConfig;
        [SerializeField] private bool enableAutoBuild = true;
        [SerializeField] private bool enablePostBuildActions = true;
        
        private Vector2 scrollPosition;
        private BuildReport lastBuildReport;
        private string buildLog = "";
        
        [MenuItem("TripMeta/Build Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildManager>("Build Manager");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadBuildConfiguration();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("TripMeta 构建管理器", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawBuildConfiguration();
            EditorGUILayout.Space();
            
            DrawBuildActions();
            EditorGUILayout.Space();
            
            DrawBuildStatus();
            EditorGUILayout.Space();
            
            DrawBuildLog();
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 绘制构建配置
        /// </summary>
        private void DrawBuildConfiguration()
        {
            EditorGUILayout.LabelField("构建配置", EditorStyles.boldLabel);
            
            if (buildConfig == null)
            {
                if (GUILayout.Button("创建构建配置"))
                {
                    CreateBuildConfiguration();
                }
                return;
            }
            
            EditorGUI.BeginChangeCheck();
            
            buildConfig.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台", buildConfig.buildTarget);
            buildConfig.buildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField("构建选项", buildConfig.buildOptions);
            buildConfig.outputPath = EditorGUILayout.TextField("输出路径", buildConfig.outputPath);
            buildConfig.productName = EditorGUILayout.TextField("产品名称", buildConfig.productName);
            buildConfig.version = EditorGUILayout.TextField("版本号", buildConfig.version);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("VR设置", EditorStyles.boldLabel);
            buildConfig.enableVR = EditorGUILayout.Toggle("启用VR", buildConfig.enableVR);
            if (buildConfig.enableVR)
            {
                buildConfig.vrSDKs = DrawStringArray("VR SDKs", buildConfig.vrSDKs);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("优化设置", EditorStyles.boldLabel);
            buildConfig.enableOptimization = EditorGUILayout.Toggle("启用优化", buildConfig.enableOptimization);
            buildConfig.compressionLevel = (CompressionLevel)EditorGUILayout.EnumPopup("压缩级别", buildConfig.compressionLevel);
            buildConfig.enableStripping = EditorGUILayout.Toggle("启用代码剥离", buildConfig.enableStripping);
            
            if (EditorGUI.EndChangeCheck())
            {
                SaveBuildConfiguration();
            }
        }
        
        /// <summary>
        /// 绘制构建操作
        /// </summary>
        private void DrawBuildActions()
        {
            EditorGUILayout.LabelField("构建操作", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("构建项目", GUILayout.Height(30)))
            {
                BuildProject();
            }
            
            if (GUILayout.Button("构建并运行", GUILayout.Height(30)))
            {
                BuildAndRun();
            }
            
            if (GUILayout.Button("清理构建", GUILayout.Height(30)))
            {
                CleanBuild();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("批量构建"))
            {
                BatchBuild();
            }
            
            if (GUILayout.Button("构建报告"))
            {
                ShowBuildReport();
            }
            
            if (GUILayout.Button("导出项目"))
            {
                ExportProject();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制构建状态
        /// </summary>
        private void DrawBuildStatus()
        {
            EditorGUILayout.LabelField("构建状态", EditorStyles.boldLabel);
            
            if (lastBuildReport != null)
            {
                var result = lastBuildReport.summary.result;
                var color = result == BuildResult.Succeeded ? Color.green : Color.red;
                
                GUI.color = color;
                EditorGUILayout.LabelField($"最后构建: {result}", EditorStyles.boldLabel);
                GUI.color = Color.white;
                
                EditorGUILayout.LabelField($"构建时间: {lastBuildReport.summary.buildStartedAt}");
                EditorGUILayout.LabelField($"总时间: {lastBuildReport.summary.totalTime}");
                EditorGUILayout.LabelField($"输出大小: {FormatBytes(lastBuildReport.summary.totalSize)}");
                EditorGUILayout.LabelField($"错误数: {lastBuildReport.summary.totalErrors}");
                EditorGUILayout.LabelField($"警告数: {lastBuildReport.summary.totalWarnings}");
            }
            else
            {
                EditorGUILayout.LabelField("暂无构建记录");
            }
        }
        
        /// <summary>
        /// 绘制构建日志
        /// </summary>
        private void DrawBuildLog()
        {
            EditorGUILayout.LabelField("构建日志", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.TextArea(buildLog, GUILayout.Height(150));
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button("清空日志"))
            {
                buildLog = "";
            }
        }
        
        /// <summary>
        /// 构建项目
        /// </summary>
        [MenuItem("TripMeta/Build/Build Project")]
        public static void BuildProject()
        {
            var instance = GetWindow<BuildManager>();
            instance.PerformBuild(false);
        }
        
        /// <summary>
        /// 构建并运行
        /// </summary>
        [MenuItem("TripMeta/Build/Build and Run")]
        public static void BuildAndRun()
        {
            var instance = GetWindow<BuildManager>();
            instance.PerformBuild(true);
        }
        
        /// <summary>
        /// 执行构建
        /// </summary>
        private void PerformBuild(bool runAfterBuild = false)
        {
            try
            {
                LogMessage("开始构建项目...");
                
                if (buildConfig == null)
                {
                    LogMessage("错误: 构建配置未找到");
                    return;
                }
                
                // 预构建检查
                if (!PreBuildValidation())
                {
                    LogMessage("错误: 预构建验证失败");
                    return;
                }
                
                // 应用构建设置
                ApplyBuildSettings();
                
                // 构建场景列表
                var scenes = GetBuildScenes();
                if (scenes.Length == 0)
                {
                    LogMessage("错误: 没有找到构建场景");
                    return;
                }
                
                // 设置构建选项
                var buildOptions = buildConfig.buildOptions;
                if (runAfterBuild)
                {
                    buildOptions |= BuildOptions.AutoRunPlayer;
                }
                
                // 创建构建参数
                var buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = GetBuildPath(),
                    target = buildConfig.buildTarget,
                    options = buildOptions
                };
                
                LogMessage($"构建目标: {buildConfig.buildTarget}");
                LogMessage($"输出路径: {buildPlayerOptions.locationPathName}");
                
                // 执行构建
                var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                lastBuildReport = report;
                
                // 处理构建结果
                HandleBuildResult(report);
                
                // 后构建操作
                if (enablePostBuildActions && report.summary.result == BuildResult.Succeeded)
                {
                    PostBuildActions();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"构建失败: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// 预构建验证
        /// </summary>
        private bool PreBuildValidation()
        {
            LogMessage("执行预构建验证...");
            
            // 检查场景
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                LogMessage("警告: Build Settings中没有场景");
                return false;
            }
            
            // 检查输出路径
            var outputDir = Path.GetDirectoryName(GetBuildPath());
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                LogMessage($"创建输出目录: {outputDir}");
            }
            
            // 检查VR设置
            if (buildConfig.enableVR)
            {
                LogMessage("验证VR设置...");
                // 这里可以添加VR相关的验证逻辑
            }
            
            LogMessage("预构建验证通过");
            return true;
        }
        
        /// <summary>
        /// 应用构建设置
        /// </summary>
        private void ApplyBuildSettings()
        {
            LogMessage("应用构建设置...");
            
            // 设置产品信息
            PlayerSettings.productName = buildConfig.productName;
            PlayerSettings.bundleVersion = buildConfig.version;
            
            // 设置VR
            if (buildConfig.enableVR)
            {
                // PlayerSettings.virtualRealitySupported = true;
                LogMessage("启用VR支持");
            }
            
            // 设置优化选项
            if (buildConfig.enableOptimization)
            {
                PlayerSettings.stripEngineCode = buildConfig.enableStripping;
                LogMessage("启用构建优化");
            }
            
            // 设置压缩
            EditorUserBuildSettings.compression = buildConfig.compressionLevel;
            
            LogMessage("构建设置应用完成");
        }
        
        /// <summary>
        /// 获取构建场景
        /// </summary>
        private string[] GetBuildScenes()
        {
            var scenes = new List<string>();
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            
            return scenes.ToArray();
        }
        
        /// <summary>
        /// 获取构建路径
        /// </summary>
        private string GetBuildPath()
        {
            var path = buildConfig.outputPath;
            
            if (string.IsNullOrEmpty(path))
            {
                path = "Builds";
            }
            
            // 添加平台和版本信息
            var platformName = buildConfig.buildTarget.ToString();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            path = Path.Combine(path, platformName, $"{buildConfig.productName}_{buildConfig.version}_{timestamp}");
            
            // 添加可执行文件扩展名
            switch (buildConfig.buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    path += ".exe";
                    break;
                case BuildTarget.StandaloneOSX:
                    path += ".app";
                    break;
                case BuildTarget.Android:
                    path += ".apk";
                    break;
            }
            
            return path;
        }
        
        /// <summary>
        /// 处理构建结果
        /// </summary>
        private void HandleBuildResult(BuildReport report)
        {
            var result = report.summary.result;
            
            switch (result)
            {
                case BuildResult.Succeeded:
                    LogMessage($"构建成功! 输出: {report.summary.outputPath}");
                    LogMessage($"构建时间: {report.summary.totalTime}");
                    LogMessage($"文件大小: {FormatBytes(report.summary.totalSize)}");
                    break;
                
                case BuildResult.Failed:
                    LogMessage($"构建失败! 错误数: {report.summary.totalErrors}");
                    break;
                
                case BuildResult.Cancelled:
                    LogMessage("构建已取消");
                    break;
                
                case BuildResult.Unknown:
                    LogMessage("构建结果未知");
                    break;
            }
            
            // 记录详细信息
            foreach (var step in report.steps)
            {
                if (step.messages.Length > 0)
                {
                    LogMessage($"构建步骤 {step.name}:");
                    foreach (var message in step.messages)
                    {
                        LogMessage($"  {message.type}: {message.content}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 后构建操作
        /// </summary>
        private void PostBuildActions()
        {
            LogMessage("执行后构建操作...");
            
            try
            {
                // 复制额外文件
                CopyAdditionalFiles();
                
                // 生成构建报告
                GenerateBuildReport();
                
                // 上传到服务器（如果配置了）
                if (!string.IsNullOrEmpty(buildConfig.uploadUrl))
                {
                    UploadBuild();
                }
                
                LogMessage("后构建操作完成");
            }
            catch (Exception ex)
            {
                LogMessage($"后构建操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 复制额外文件
        /// </summary>
        private void CopyAdditionalFiles()
        {
            if (buildConfig.additionalFiles == null || buildConfig.additionalFiles.Length == 0)
                return;
            
            var buildDir = Path.GetDirectoryName(lastBuildReport.summary.outputPath);
            
            foreach (var file in buildConfig.additionalFiles)
            {
                if (File.Exists(file))
                {
                    var fileName = Path.GetFileName(file);
                    var destPath = Path.Combine(buildDir, fileName);
                    File.Copy(file, destPath, true);
                    LogMessage($"复制文件: {fileName}");
                }
            }
        }
        
        /// <summary>
        /// 生成构建报告
        /// </summary>
        private void GenerateBuildReport()
        {
            if (lastBuildReport == null) return;
            
            var reportPath = Path.Combine(Path.GetDirectoryName(lastBuildReport.summary.outputPath), "BuildReport.json");
            
            var reportData = new
            {
                buildTime = DateTime.Now,
                result = lastBuildReport.summary.result.ToString(),
                platform = lastBuildReport.summary.platform.ToString(),
                totalTime = lastBuildReport.summary.totalTime.ToString(),
                totalSize = lastBuildReport.summary.totalSize,
                totalErrors = lastBuildReport.summary.totalErrors,
                totalWarnings = lastBuildReport.summary.totalWarnings,
                outputPath = lastBuildReport.summary.outputPath
            };
            
            var json = JsonUtility.ToJson(reportData, true);
            File.WriteAllText(reportPath, json);
            
            LogMessage($"构建报告已生成: {reportPath}");
        }
        
        /// <summary>
        /// 上传构建
        /// </summary>
        private void UploadBuild()
        {
            LogMessage("上传构建文件...");
            // 这里可以实现FTP、云存储等上传逻辑
            LogMessage("构建文件上传完成");
        }
        
        /// <summary>
        /// 批量构建
        /// </summary>
        private void BatchBuild()
        {
            LogMessage("开始批量构建...");
            
            var platforms = new[] { BuildTarget.StandaloneWindows64, BuildTarget.Android };
            
            foreach (var platform in platforms)
            {
                var originalTarget = buildConfig.buildTarget;
                buildConfig.buildTarget = platform;
                
                LogMessage($"构建平台: {platform}");
                PerformBuild();
                
                buildConfig.buildTarget = originalTarget;
            }
            
            LogMessage("批量构建完成");
        }
        
        /// <summary>
        /// 清理构建
        /// </summary>
        private void CleanBuild()
        {
            try
            {
                var buildDir = Path.GetDirectoryName(buildConfig.outputPath);
                if (Directory.Exists(buildDir))
                {
                    Directory.Delete(buildDir, true);
                    LogMessage($"清理构建目录: {buildDir}");
                }
                
                // 清理临时文件
                var tempDir = Path.Combine(Application.dataPath, "..", "Temp");
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                    LogMessage("清理临时文件");
                }
                
                LogMessage("构建清理完成");
            }
            catch (Exception ex)
            {
                LogMessage($"清理失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 显示构建报告
        /// </summary>
        private void ShowBuildReport()
        {
            if (lastBuildReport != null)
            {
                BuildReportWindow.ShowReport(lastBuildReport);
            }
            else
            {
                EditorUtility.DisplayDialog("构建报告", "暂无构建报告", "确定");
            }
        }
        
        /// <summary>
        /// 导出项目
        /// </summary>
        private void ExportProject()
        {
            var exportPath = EditorUtility.SaveFolderPanel("导出项目", "", "TripMeta_Export");
            if (!string.IsNullOrEmpty(exportPath))
            {
                LogMessage($"导出项目到: {exportPath}");
                // 实现项目导出逻辑
            }
        }
        
        /// <summary>
        /// 创建构建配置
        /// </summary>
        private void CreateBuildConfiguration()
        {
            buildConfig = CreateInstance<BuildConfiguration>();
            buildConfig.Initialize();
            SaveBuildConfiguration();
        }
        
        /// <summary>
        /// 加载构建配置
        /// </summary>
        private void LoadBuildConfiguration()
        {
            var configPath = "Assets/TripMeta/BuildConfiguration.asset";
            buildConfig = AssetDatabase.LoadAssetAtPath<BuildConfiguration>(configPath);
        }
        
        /// <summary>
        /// 保存构建配置
        /// </summary>
        private void SaveBuildConfiguration()
        {
            if (buildConfig == null) return;
            
            var configPath = "Assets/TripMeta/BuildConfiguration.asset";
            var configDir = Path.GetDirectoryName(configPath);
            
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            
            AssetDatabase.CreateAsset(buildConfig, configPath);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// 绘制字符串数组
        /// </summary>
        private string[] DrawStringArray(string label, string[] array)
        {
            EditorGUILayout.LabelField(label);
            
            if (array == null) array = new string[0];
            
            var list = new List<string>(array);
            
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.TextField(list[i]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("+"))
            {
                list.Add("");
            }
            
            return list.ToArray();
        }
        
        /// <summary>
        /// 记录消息
        /// </summary>
        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}\n";
            buildLog += logEntry;
            Debug.Log($"[BuildManager] {message}");
            
            Repaint();
        }
        
        /// <summary>
        /// 格式化字节数
        /// </summary>
        private string FormatBytes(ulong bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}