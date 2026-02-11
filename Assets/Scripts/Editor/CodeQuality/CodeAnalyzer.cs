using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Editor.CodeQuality
{
    /// <summary>
    /// 代码分析器 - 静态代码分析和质量检测
    /// </summary>
    public class CodeAnalyzer : EditorWindow
    {
        [Header("分析配置")]
        [SerializeField] private bool enableComplexityAnalysis = true;
        [SerializeField] private bool enableDuplicationDetection = true;
        [SerializeField] private bool enableStyleCheck = true;
        [SerializeField] private bool enableSecurityCheck = true;
        [SerializeField] private bool enablePerformanceCheck = true;
        
        [Header("阈值设置")]
        [SerializeField] private int maxMethodComplexity = 10;
        [SerializeField] private int maxClassSize = 500;
        [SerializeField] private int maxMethodSize = 50;
        [SerializeField] private float duplicateThreshold = 0.8f;
        
        // 分析结果
        private CodeAnalysisReport analysisReport;
        private List<CodeIssue> currentIssues;
        private Vector2 scrollPosition;
        private int selectedTabIndex = 0;
        private string[] tabNames = { "概览", "复杂度", "重复代码", "代码风格", "安全问题", "性能问题" };
        
        // 分析规则
        private List<ICodeAnalysisRule> analysisRules;
        
        [MenuItem("TripMeta/Code Quality/Code Analyzer")]
        public static void ShowWindow()
        {
            var window = GetWindow<CodeAnalyzer>("代码分析器");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeAnalyzer();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("TripMeta 代码分析器", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawAnalysisControls();
            EditorGUILayout.Space();
            
            DrawAnalysisResults();
        }
        
        /// <summary>
        /// 初始化分析器
        /// </summary>
        private void InitializeAnalyzer()
        {
            currentIssues = new List<CodeIssue>();
            InitializeAnalysisRules();
        }
        
        /// <summary>
        /// 初始化分析规则
        /// </summary>
        private void InitializeAnalysisRules()
        {
            analysisRules = new List<ICodeAnalysisRule>
            {
                new ComplexityAnalysisRule(maxMethodComplexity, maxClassSize, maxMethodSize),
                new DuplicationDetectionRule(duplicateThreshold),
                new StyleCheckRule(),
                new SecurityCheckRule(),
                new PerformanceCheckRule(),
                new NamingConventionRule(),
                new DocumentationRule(),
                new DesignPatternRule()
            };
        }
        
        /// <summary>
        /// 绘制分析控制面板
        /// </summary>
        private void DrawAnalysisControls()
        {
            EditorGUILayout.LabelField("分析设置", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            enableComplexityAnalysis = EditorGUILayout.Toggle("复杂度分析", enableComplexityAnalysis);
            enableDuplicationDetection = EditorGUILayout.Toggle("重复检测", enableDuplicationDetection);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            enableStyleCheck = EditorGUILayout.Toggle("代码风格", enableStyleCheck);
            enableSecurityCheck = EditorGUILayout.Toggle("安全检查", enableSecurityCheck);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            enablePerformanceCheck = EditorGUILayout.Toggle("性能检查", enablePerformanceCheck);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("阈值设置", EditorStyles.boldLabel);
            maxMethodComplexity = EditorGUILayout.IntField("最大方法复杂度", maxMethodComplexity);
            maxClassSize = EditorGUILayout.IntField("最大类大小", maxClassSize);
            maxMethodSize = EditorGUILayout.IntField("最大方法大小", maxMethodSize);
            duplicateThreshold = EditorGUILayout.Slider("重复阈值", duplicateThreshold, 0.5f, 1f);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("分析项目", GUILayout.Height(30)))
            {
                AnalyzeProject();
            }
            
            if (GUILayout.Button("分析选中文件", GUILayout.Height(30)))
            {
                AnalyzeSelectedFiles();
            }
            
            if (GUILayout.Button("导出报告", GUILayout.Height(30)))
            {
                ExportReport();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制分析结果
        /// </summary>
        private void DrawAnalysisResults()
        {
            if (analysisReport == null) return;
            
            EditorGUILayout.LabelField("分析结果", EditorStyles.boldLabel);
            
            // 标签页
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTabIndex)
            {
                case 0:
                    DrawOverview();
                    break;
                case 1:
                    DrawComplexityResults();
                    break;
                case 2:
                    DrawDuplicationResults();
                    break;
                case 3:
                    DrawStyleResults();
                    break;
                case 4:
                    DrawSecurityResults();
                    break;
                case 5:
                    DrawPerformanceResults();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 绘制概览
        /// </summary>
        private void DrawOverview()
        {
            EditorGUILayout.LabelField("项目概览", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"分析文件数: {analysisReport.totalFiles}");
            EditorGUILayout.LabelField($"代码行数: {analysisReport.totalLines}");
            EditorGUILayout.LabelField($"总问题数: {analysisReport.totalIssues}");
            
            EditorGUILayout.Space();
            
            // 问题分布
            EditorGUILayout.LabelField("问题分布", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"严重: {analysisReport.criticalIssues}");
            EditorGUILayout.LabelField($"重要: {analysisReport.majorIssues}");
            EditorGUILayout.LabelField($"一般: {analysisReport.minorIssues}");
            EditorGUILayout.LabelField($"信息: {analysisReport.infoIssues}");
            
            EditorGUILayout.Space();
            
            // 质量评分
            EditorGUILayout.LabelField("质量评分", EditorStyles.boldLabel);
            var qualityScore = CalculateQualityScore();
            var scoreColor = qualityScore >= 80 ? Color.green : qualityScore >= 60 ? Color.yellow : Color.red;
            
            GUI.color = scoreColor;
            EditorGUILayout.LabelField($"总体评分: {qualityScore:F1}/100", EditorStyles.boldLabel);
            GUI.color = Color.white;
            
            // 评分详情
            EditorGUILayout.LabelField($"可维护性: {analysisReport.maintainabilityScore:F1}");
            EditorGUILayout.LabelField($"可读性: {analysisReport.readabilityScore:F1}");
            EditorGUILayout.LabelField($"复杂度: {analysisReport.complexityScore:F1}");
            EditorGUILayout.LabelField($"测试覆盖率: {analysisReport.testCoverage:F1}%");
        }
        
        /// <summary>
        /// 绘制复杂度结果
        /// </summary>
        private void DrawComplexityResults()
        {
            EditorGUILayout.LabelField("复杂度分析", EditorStyles.boldLabel);
            
            var complexityIssues = currentIssues.Where(i => i.category == IssueCategory.Complexity).ToList();
            
            foreach (var issue in complexityIssues)
            {
                DrawIssueItem(issue);
            }
            
            if (complexityIssues.Count == 0)
            {
                EditorGUILayout.LabelField("未发现复杂度问题");
            }
        }
        
        /// <summary>
        /// 绘制重复代码结果
        /// </summary>
        private void DrawDuplicationResults()
        {
            EditorGUILayout.LabelField("重复代码检测", EditorStyles.boldLabel);
            
            var duplicationIssues = currentIssues.Where(i => i.category == IssueCategory.Duplication).ToList();
            
            foreach (var issue in duplicationIssues)
            {
                DrawIssueItem(issue);
            }
            
            if (duplicationIssues.Count == 0)
            {
                EditorGUILayout.LabelField("未发现重复代码");
            }
        }
        
        /// <summary>
        /// 绘制代码风格结果
        /// </summary>
        private void DrawStyleResults()
        {
            EditorGUILayout.LabelField("代码风格检查", EditorStyles.boldLabel);
            
            var styleIssues = currentIssues.Where(i => i.category == IssueCategory.Style).ToList();
            
            foreach (var issue in styleIssues)
            {
                DrawIssueItem(issue);
            }
            
            if (styleIssues.Count == 0)
            {
                EditorGUILayout.LabelField("代码风格良好");
            }
        }
        
        /// <summary>
        /// 绘制安全问题结果
        /// </summary>
        private void DrawSecurityResults()
        {
            EditorGUILayout.LabelField("安全问题检查", EditorStyles.boldLabel);
            
            var securityIssues = currentIssues.Where(i => i.category == IssueCategory.Security).ToList();
            
            foreach (var issue in securityIssues)
            {
                DrawIssueItem(issue);
            }
            
            if (securityIssues.Count == 0)
            {
                EditorGUILayout.LabelField("未发现安全问题");
            }
        }
        
        /// <summary>
        /// 绘制性能问题结果
        /// </summary>
        private void DrawPerformanceResults()
        {
            EditorGUILayout.LabelField("性能问题检查", EditorStyles.boldLabel);
            
            var performanceIssues = currentIssues.Where(i => i.category == IssueCategory.Performance).ToList();
            
            foreach (var issue in performanceIssues)
            {
                DrawIssueItem(issue);
            }
            
            if (performanceIssues.Count == 0)
            {
                EditorGUILayout.LabelField("未发现性能问题");
            }
        }
        
        /// <summary>
        /// 绘制问题项
        /// </summary>
        private void DrawIssueItem(CodeIssue issue)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            // 问题标题
            var severityColor = GetSeverityColor(issue.severity);
            GUI.color = severityColor;
            EditorGUILayout.LabelField($"[{issue.severity}] {issue.title}", EditorStyles.boldLabel);
            GUI.color = Color.white;
            
            // 问题描述
            EditorGUILayout.LabelField(issue.description, EditorStyles.wordWrappedLabel);
            
            // 文件位置
            EditorGUILayout.LabelField($"文件: {issue.filePath}:{issue.lineNumber}");
            
            // 建议修复
            if (!string.IsNullOrEmpty(issue.suggestion))
            {
                EditorGUILayout.LabelField("建议:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(issue.suggestion, EditorStyles.wordWrappedLabel);
            }
            
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("定位到文件"))
            {
                OpenFileAtLine(issue.filePath, issue.lineNumber);
            }
            
            if (GUILayout.Button("忽略此问题"))
            {
                IgnoreIssue(issue);
            }
            
            if (!string.IsNullOrEmpty(issue.autoFixCode) && GUILayout.Button("自动修复"))
            {
                AutoFixIssue(issue);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// 分析项目
        /// </summary>
        private void AnalyzeProject()
        {
            try
            {
                EditorUtility.DisplayProgressBar("代码分析", "正在分析项目...", 0f);
                
                var scriptFiles = GetAllScriptFiles();
                analysisReport = new CodeAnalysisReport();
                currentIssues.Clear();
                
                for (int i = 0; i < scriptFiles.Count; i++)
                {
                    var file = scriptFiles[i];
                    EditorUtility.DisplayProgressBar("代码分析", $"分析文件: {Path.GetFileName(file)}", (float)i / scriptFiles.Count);
                    
                    AnalyzeFile(file);
                }
                
                // 生成报告
                GenerateAnalysisReport();
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log($"代码分析完成，发现 {currentIssues.Count} 个问题");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Logger.LogException(ex, "代码分析失败");
            }
        }
        
        /// <summary>
        /// 分析选中文件
        /// </summary>
        private void AnalyzeSelectedFiles()
        {
            var selectedFiles = GetSelectedScriptFiles();
            if (selectedFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请选择要分析的C#脚本文件", "确定");
                return;
            }
            
            analysisReport = new CodeAnalysisReport();
            currentIssues.Clear();
            
            foreach (var file in selectedFiles)
            {
                AnalyzeFile(file);
            }
            
            GenerateAnalysisReport();
            
            Debug.Log($"选中文件分析完成，发现 {currentIssues.Count} 个问题");
        }
        
        /// <summary>
        /// 分析单个文件
        /// </summary>
        private void AnalyzeFile(string filePath)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                var fileInfo = new FileAnalysisInfo
                {
                    filePath = filePath,
                    content = fileContent,
                    lines = fileContent.Split('\n')
                };
                
                // 应用所有分析规则
                foreach (var rule in analysisRules)
                {
                    if (ShouldApplyRule(rule))
                    {
                        var issues = rule.Analyze(fileInfo);
                        currentIssues.AddRange(issues);
                    }
                }
                
                analysisReport.totalFiles++;
                analysisReport.totalLines += fileInfo.lines.Length;
            }
            catch (Exception ex)
            {
                Logger.LogError($"分析文件失败: {filePath} - {ex.Message}", "CodeAnalyzer");
            }
        }
        
        /// <summary>
        /// 判断是否应用规则
        /// </summary>
        private bool ShouldApplyRule(ICodeAnalysisRule rule)
        {
            return rule switch
            {
                ComplexityAnalysisRule => enableComplexityAnalysis,
                DuplicationDetectionRule => enableDuplicationDetection,
                StyleCheckRule => enableStyleCheck,
                SecurityCheckRule => enableSecurityCheck,
                PerformanceCheckRule => enablePerformanceCheck,
                _ => true
            };
        }
        
        /// <summary>
        /// 生成分析报告
        /// </summary>
        private void GenerateAnalysisReport()
        {
            analysisReport.totalIssues = currentIssues.Count;
            analysisReport.criticalIssues = currentIssues.Count(i => i.severity == IssueSeverity.Critical);
            analysisReport.majorIssues = currentIssues.Count(i => i.severity == IssueSeverity.Major);
            analysisReport.minorIssues = currentIssues.Count(i => i.severity == IssueSeverity.Minor);
            analysisReport.infoIssues = currentIssues.Count(i => i.severity == IssueSeverity.Info);
            
            // 计算质量评分
            analysisReport.maintainabilityScore = CalculateMaintainabilityScore();
            analysisReport.readabilityScore = CalculateReadabilityScore();
            analysisReport.complexityScore = CalculateComplexityScore();
            analysisReport.testCoverage = CalculateTestCoverage();
            
            analysisReport.analysisDate = DateTime.Now;
        }
        
        /// <summary>
        /// 计算质量评分
        /// </summary>
        private float CalculateQualityScore()
        {
            if (analysisReport == null) return 0f;
            
            var maintainability = analysisReport.maintainabilityScore * 0.3f;
            var readability = analysisReport.readabilityScore * 0.25f;
            var complexity = analysisReport.complexityScore * 0.25f;
            var testCoverage = analysisReport.testCoverage * 0.2f;
            
            return maintainability + readability + complexity + testCoverage;
        }
        
        /// <summary>
        /// 计算可维护性评分
        /// </summary>
        private float CalculateMaintainabilityScore()
        {
            // 基于代码复杂度、重复度、文档等因素计算
            var baseScore = 100f;
            
            // 扣除复杂度问题
            var complexityIssues = currentIssues.Count(i => i.category == IssueCategory.Complexity);
            baseScore -= complexityIssues * 5f;
            
            // 扣除重复代码问题
            var duplicationIssues = currentIssues.Count(i => i.category == IssueCategory.Duplication);
            baseScore -= duplicationIssues * 3f;
            
            return Math.Max(0f, Math.Min(100f, baseScore));
        }
        
        /// <summary>
        /// 计算可读性评分
        /// </summary>
        private float CalculateReadabilityScore()
        {
            var baseScore = 100f;
            
            // 扣除命名和风格问题
            var styleIssues = currentIssues.Count(i => i.category == IssueCategory.Style);
            baseScore -= styleIssues * 2f;
            
            return Math.Max(0f, Math.Min(100f, baseScore));
        }
        
        /// <summary>
        /// 计算复杂度评分
        /// </summary>
        private float CalculateComplexityScore()
        {
            var baseScore = 100f;
            
            var complexityIssues = currentIssues.Where(i => i.category == IssueCategory.Complexity);
            foreach (var issue in complexityIssues)
            {
                baseScore -= issue.severity switch
                {
                    IssueSeverity.Critical => 10f,
                    IssueSeverity.Major => 5f,
                    IssueSeverity.Minor => 2f,
                    _ => 1f
                };
            }
            
            return Math.Max(0f, Math.Min(100f, baseScore));
        }
        
        /// <summary>
        /// 计算测试覆盖率
        /// </summary>
        private float CalculateTestCoverage()
        {
            // 简化实现，实际需要集成测试覆盖率工具
            var testFiles = GetAllScriptFiles().Count(f => f.Contains("Test"));
            var totalFiles = analysisReport.totalFiles;
            
            return totalFiles > 0 ? (float)testFiles / totalFiles * 100f : 0f;
        }
        
        /// <summary>
        /// 获取所有脚本文件
        /// </summary>
        private List<string> GetAllScriptFiles()
        {
            var scriptFiles = new List<string>();
            var assetsPath = Application.dataPath;
            
            var files = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);
            scriptFiles.AddRange(files.Where(f => !f.Contains("Editor") || f.Contains("TripMeta")));
            
            return scriptFiles;
        }
        
        /// <summary>
        /// 获取选中的脚本文件
        /// </summary>
        private List<string> GetSelectedScriptFiles()
        {
            var selectedFiles = new List<string>();
            
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (path.EndsWith(".cs"))
                {
                    selectedFiles.Add(Path.Combine(Application.dataPath, path.Substring("Assets/".Length)));
                }
            }
            
            return selectedFiles;
        }
        
        /// <summary>
        /// 获取严重程度颜色
        /// </summary>
        private Color GetSeverityColor(IssueSeverity severity)
        {
            return severity switch
            {
                IssueSeverity.Critical => Color.red,
                IssueSeverity.Major => new Color(1f, 0.5f, 0f), // 橙色
                IssueSeverity.Minor => Color.yellow,
                IssueSeverity.Info => Color.cyan,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 在指定行打开文件
        /// </summary>
        private void OpenFileAtLine(string filePath, int lineNumber)
        {
            var relativePath = "Assets" + filePath.Substring(Application.dataPath.Length);
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
            
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, lineNumber);
            }
        }
        
        /// <summary>
        /// 忽略问题
        /// </summary>
        private void IgnoreIssue(CodeIssue issue)
        {
            currentIssues.Remove(issue);
            Repaint();
        }
        
        /// <summary>
        /// 自动修复问题
        /// </summary>
        private void AutoFixIssue(CodeIssue issue)
        {
            try
            {
                if (!string.IsNullOrEmpty(issue.autoFixCode))
                {
                    var fileContent = File.ReadAllText(issue.filePath);
                    var lines = fileContent.Split('\n');
                    
                    if (issue.lineNumber > 0 && issue.lineNumber <= lines.Length)
                    {
                        lines[issue.lineNumber - 1] = issue.autoFixCode;
                        File.WriteAllText(issue.filePath, string.Join("\n", lines));
                        
                        AssetDatabase.Refresh();
                        currentIssues.Remove(issue);
                        Repaint();
                        
                        Debug.Log($"已自动修复问题: {issue.title}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"自动修复失败: {ex.Message}", "CodeAnalyzer");
            }
        }
        
        /// <summary>
        /// 导出报告
        /// </summary>
        private void ExportReport()
        {
            if (analysisReport == null)
            {
                EditorUtility.DisplayDialog("提示", "请先进行代码分析", "确定");
                return;
            }
            
            var reportPath = EditorUtility.SaveFilePanel("导出分析报告", "", "CodeAnalysisReport", "html");
            if (!string.IsNullOrEmpty(reportPath))
            {
                GenerateHTMLReport(reportPath);
                Debug.Log($"分析报告已导出: {reportPath}");
            }
        }
        
        /// <summary>
        /// 生成HTML报告
        /// </summary>
        private void GenerateHTMLReport(string filePath)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>TripMeta 代码分析报告</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .summary {{ margin: 20px 0; }}
        .issue {{ margin: 10px 0; padding: 10px; border-left: 4px solid #ccc; }}
        .critical {{ border-left-color: #f44336; }}
        .major {{ border-left-color: #ff9800; }}
        .minor {{ border-left-color: #ffeb3b; }}
        .info {{ border-left-color: #2196f3; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>TripMeta 代码分析报告</h1>
        <p>生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    </div>
    
    <div class='summary'>
        <h2>分析摘要</h2>
        <p>分析文件数: {analysisReport.totalFiles}</p>
        <p>代码行数: {analysisReport.totalLines}</p>
        <p>总问题数: {analysisReport.totalIssues}</p>
        <p>质量评分: {CalculateQualityScore():F1}/100</p>
    </div>
    
    <div class='issues'>
        <h2>问题详情</h2>";
            
            foreach (var issue in currentIssues)
            {
                var cssClass = issue.severity.ToString().ToLower();
                html += $@"
        <div class='issue {cssClass}'>
            <h3>[{issue.severity}] {issue.title}</h3>
            <p>{issue.description}</p>
            <p><strong>文件:</strong> {issue.filePath}:{issue.lineNumber}</p>
            {(!string.IsNullOrEmpty(issue.suggestion) ? $"<p><strong>建议:</strong> {issue.suggestion}</p>" : "")}
        </div>";
            }
            
            html += @"
    </div>
</body>
</html>";
            
            File.WriteAllText(filePath, html);
        }
    }
}