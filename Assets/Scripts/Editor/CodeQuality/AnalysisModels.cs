using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripMeta.Editor.CodeQuality
{
    /// <summary>
    /// 文件分析信息
    /// </summary>
    [Serializable]
    public class FileAnalysisInfo
    {
        public string filePath;
        public string content;
        public string[] lines;
        public DateTime lastModified;
        public long fileSize;
        
        public FileAnalysisInfo()
        {
            lines = new string[0];
        }
    }
    
    /// <summary>
    /// 代码问题
    /// </summary>
    [Serializable]
    public class CodeIssue
    {
        public string id;
        public string title;
        public string description;
        public IssueSeverity severity;
        public IssueCategory category;
        public string filePath;
        public int lineNumber;
        public int columnNumber;
        public string suggestion;
        public string autoFixCode;
        public DateTime detectedAt;
        public bool isIgnored;
        public string ruleId;
        
        public CodeIssue()
        {
            id = Guid.NewGuid().ToString();
            detectedAt = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 问题严重程度
    /// </summary>
    public enum IssueSeverity
    {
        Info,       // 信息
        Minor,      // 轻微
        Major,      // 重要
        Critical    // 严重
    }
    
    /// <summary>
    /// 问题分类
    /// </summary>
    public enum IssueCategory
    {
        Complexity,     // 复杂度
        Duplication,    // 重复代码
        Style,          // 代码风格
        Security,       // 安全问题
        Performance,    // 性能问题
        Documentation,  // 文档问题
        Design,         // 设计问题
        Maintainability // 可维护性
    }
    
    /// <summary>
    /// 代码分析报告
    /// </summary>
    [Serializable]
    public class CodeAnalysisReport
    {
        public string reportId;
        public DateTime analysisDate;
        public string projectName;
        public string projectVersion;
        
        // 统计信息
        public int totalFiles;
        public int totalLines;
        public int totalIssues;
        public int criticalIssues;
        public int majorIssues;
        public int minorIssues;
        public int infoIssues;
        
        // 质量评分
        public float maintainabilityScore;
        public float readabilityScore;
        public float complexityScore;
        public float testCoverage;
        public float overallScore;
        
        // 分类统计
        public Dictionary<IssueCategory, int> categoryStats;
        public Dictionary<string, int> fileStats;
        
        // 趋势数据
        public List<QualityTrend> qualityTrends;
        
        public CodeAnalysisReport()
        {
            reportId = Guid.NewGuid().ToString();
            analysisDate = DateTime.Now;
            categoryStats = new Dictionary<IssueCategory, int>();
            fileStats = new Dictionary<string, int>();
            qualityTrends = new List<QualityTrend>();
        }
    }
    
    /// <summary>
    /// 质量趋势
    /// </summary>
    [Serializable]
    public class QualityTrend
    {
        public DateTime date;
        public float qualityScore;
        public int totalIssues;
        public int linesOfCode;
        public float testCoverage;
    }
    
    /// <summary>
    /// 代码块
    /// </summary>
    [Serializable]
    public class CodeBlock
    {
        public string content;
        public int startLine;
        public int endLine;
        public string hash;
        
        public CodeBlock()
        {
            hash = Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// 重构建议
    /// </summary>
    [Serializable]
    public class Refactoringsuggestion
    {
        public string id;
        public string title;
        public string description;
        public RefactoringType type;
        public RefactoringPriority priority;
        public string filePath;
        public int startLine;
        public int endLine;
        public string originalCode;
        public string suggestedCode;
        public string reason;
        public List<string> benefits;
        public float estimatedEffort; // 小时
        public bool canAutoApply;
        
        public Refactoringsuggestion()
        {
            id = Guid.NewGuid().ToString();
            benefits = new List<string>();
        }
    }
    
    /// <summary>
    /// 重构类型
    /// </summary>
    public enum RefactoringType
    {
        ExtractMethod,      // 提取方法
        ExtractClass,       // 提取类
        RenameVariable,     // 重命名变量
        SimplifyCondition,  // 简化条件
        RemoveDuplication,  // 移除重复
        OptimizeLoop,       // 优化循环
        ImproveNaming,      // 改进命名
        AddDocumentation,   // 添加文档
        ReduceComplexity,   // 降低复杂度
        ImprovePerformance  // 改进性能
    }
    
    /// <summary>
    /// 重构优先级
    /// </summary>
    public enum RefactoringPriority
    {
        Low,        // 低
        Medium,     // 中
        High,       // 高
        Critical    // 关键
    }
    
    /// <summary>
    /// 代码度量
    /// </summary>
    [Serializable]
    public class CodeMetrics
    {
        public string filePath;
        public int linesOfCode;
        public int cyclomaticComplexity;
        public int cognitiveComplexity;
        public int maintainabilityIndex;
        public float duplicationRatio;
        public int numberOfMethods;
        public int numberOfClasses;
        public float averageMethodLength;
        public float averageClassLength;
        public int depthOfInheritance;
        public int couplingBetweenObjects;
        public float testCoverage;
        
        public CodeMetrics()
        {
            // 初始化默认值
        }
    }
    
    /// <summary>
    /// 技术债务
    /// </summary>
    [Serializable]
    public class TechnicalDebt
    {
        public string id;
        public string title;
        public string description;
        public DebtType type;
        public DebtSeverity severity;
        public string filePath;
        public int lineNumber;
        public float estimatedFixTime; // 小时
        public float interestRate; // 每月增长的维护成本
        public DateTime introducedDate;
        public string introducedBy;
        public List<string> impacts;
        
        public TechnicalDebt()
        {
            id = Guid.NewGuid().ToString();
            impacts = new List<string>();
        }
    }
    
    /// <summary>
    /// 技术债务类型
    /// </summary>
    public enum DebtType
    {
        CodeSmell,          // 代码异味
        ArchitecturalDebt,  // 架构债务
        DocumentationDebt,  // 文档债务
        TestDebt,           // 测试债务
        DesignDebt,         // 设计债务
        InfrastructureDebt  // 基础设施债务
    }
    
    /// <summary>
    /// 技术债务严重程度
    /// </summary>
    public enum DebtSeverity
    {
        Low,        // 低
        Medium,     // 中
        High,       // 高
        Critical    // 关键
    }
    
    /// <summary>
    /// 代码审查结果
    /// </summary>
    [Serializable]
    public class CodeReviewResult
    {
        public string reviewId;
        public DateTime reviewDate;
        public string reviewer;
        public string filePath;
        public List<ReviewComment> comments;
        public ReviewStatus status;
        public float qualityScore;
        
        public CodeReviewResult()
        {
            reviewId = Guid.NewGuid().ToString();
            reviewDate = DateTime.Now;
            comments = new List<ReviewComment>();
        }
    }
    
    /// <summary>
    /// 审查评论
    /// </summary>
    [Serializable]
    public class ReviewComment
    {
        public string commentId;
        public int lineNumber;
        public string comment;
        public CommentType type;
        public CommentSeverity severity;
        public bool isResolved;
        
        public ReviewComment()
        {
            commentId = Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// 评论类型
    /// </summary>
    public enum CommentType
    {
        Suggestion,     // 建议
        Issue,          // 问题
        Question,       // 疑问
        Praise,         // 表扬
        Nitpick         // 吹毛求疵
    }
    
    /// <summary>
    /// 评论严重程度
    /// </summary>
    public enum CommentSeverity
    {
        Info,       // 信息
        Minor,      // 轻微
        Major,      // 重要
        Blocker     // 阻塞
    }
    
    /// <summary>
    /// 审查状态
    /// </summary>
    public enum ReviewStatus
    {
        Pending,        // 待审查
        InProgress,     // 审查中
        Approved,       // 已批准
        NeedsChanges,   // 需要修改
        Rejected        // 已拒绝
    }
    
    /// <summary>
    /// 质量门禁
    /// </summary>
    [Serializable]
    public class QualityGate
    {
        public string name;
        public List<QualityCondition> conditions;
        public bool isPassed;
        public string failureReason;
        
        public QualityGate()
        {
            conditions = new List<QualityCondition>();
        }
    }
    
    /// <summary>
    /// 质量条件
    /// </summary>
    [Serializable]
    public class QualityCondition
    {
        public string metric;
        public ComparisonOperator @operator;
        public float threshold;
        public float actualValue;
        public bool isPassed;
        
        public enum ComparisonOperator
        {
            LessThan,           // 小于
            LessThanOrEqual,    // 小于等于
            GreaterThan,        // 大于
            GreaterThanOrEqual, // 大于等于
            Equal,              // 等于
            NotEqual            // 不等于
        }
    }
    
    /// <summary>
    /// 代码覆盖率信息
    /// </summary>
    [Serializable]
    public class CoverageInfo
    {
        public string filePath;
        public float lineCoverage;
        public float branchCoverage;
        public float methodCoverage;
        public int coveredLines;
        public int totalLines;
        public int coveredBranches;
        public int totalBranches;
        public List<UncoveredLine> uncoveredLines;
        
        public CoverageInfo()
        {
            uncoveredLines = new List<UncoveredLine>();
        }
    }
    
    /// <summary>
    /// 未覆盖的行
    /// </summary>
    [Serializable]
    public class UncoveredLine
    {
        public int lineNumber;
        public string code;
        public string reason;
    }
}