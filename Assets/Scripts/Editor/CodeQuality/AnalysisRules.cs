using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TripMeta.Editor.CodeQuality
{
    /// <summary>
    /// 代码分析规则接口
    /// </summary>
    public interface ICodeAnalysisRule
    {
        string RuleName { get; }
        string Description { get; }
        IssueCategory Category { get; }
        List<CodeIssue> Analyze(FileAnalysisInfo fileInfo);
    }
    
    /// <summary>
    /// 复杂度分析规则
    /// </summary>
    public class ComplexityAnalysisRule : ICodeAnalysisRule
    {
        public string RuleName => "复杂度分析";
        public string Description => "检测方法和类的复杂度";
        public IssueCategory Category => IssueCategory.Complexity;
        
        private readonly int maxMethodComplexity;
        private readonly int maxClassSize;
        private readonly int maxMethodSize;
        
        public ComplexityAnalysisRule(int maxMethodComplexity, int maxClassSize, int maxMethodSize)
        {
            this.maxMethodComplexity = maxMethodComplexity;
            this.maxClassSize = maxClassSize;
            this.maxMethodSize = maxMethodSize;
        }
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            // 检查类大小
            CheckClassSize(fileInfo, issues);
            
            // 检查方法复杂度
            CheckMethodComplexity(fileInfo, issues);
            
            // 检查方法大小
            CheckMethodSize(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckClassSize(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var classPattern = @"class\s+(\w+)";
            var matches = Regex.Matches(fileInfo.content, classPattern);
            
            foreach (Match match in matches)
            {
                var className = match.Groups[1].Value;
                var classStartLine = GetLineNumber(fileInfo.content, match.Index);
                var classEndLine = FindClassEndLine(fileInfo.lines, classStartLine);
                var classSize = classEndLine - classStartLine;
                
                if (classSize > maxClassSize)
                {
                    issues.Add(new CodeIssue
                    {
                        title = $"类 '{className}' 过大",
                        description = $"类有 {classSize} 行代码，超过了建议的 {maxClassSize} 行",
                        severity = classSize > maxClassSize * 1.5 ? IssueSeverity.Major : IssueSeverity.Minor,
                        category = IssueCategory.Complexity,
                        filePath = fileInfo.filePath,
                        lineNumber = classStartLine,
                        suggestion = "考虑将类拆分为多个更小的类，每个类负责单一职责"
                    });
                }
            }
        }
        
        private void CheckMethodComplexity(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var methodPattern = @"(public|private|protected|internal)?\s*(static)?\s*(virtual|override)?\s*\w+\s+(\w+)\s*\([^)]*\)\s*{";
            var matches = Regex.Matches(fileInfo.content, methodPattern);
            
            foreach (Match match in matches)
            {
                var methodName = match.Groups[4].Value;
                var methodStartLine = GetLineNumber(fileInfo.content, match.Index);
                var methodEndLine = FindMethodEndLine(fileInfo.lines, methodStartLine);
                
                var complexity = CalculateCyclomaticComplexity(fileInfo.lines, methodStartLine, methodEndLine);
                
                if (complexity > maxMethodComplexity)
                {
                    issues.Add(new CodeIssue
                    {
                        title = $"方法 '{methodName}' 复杂度过高",
                        description = $"方法的圈复杂度为 {complexity}，超过了建议的 {maxMethodComplexity}",
                        severity = complexity > maxMethodComplexity * 1.5 ? IssueSeverity.Major : IssueSeverity.Minor,
                        category = IssueCategory.Complexity,
                        filePath = fileInfo.filePath,
                        lineNumber = methodStartLine,
                        suggestion = "考虑将方法拆分为多个更小的方法，或使用设计模式简化逻辑"
                    });
                }
            }
        }
        
        private void CheckMethodSize(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var methodPattern = @"(public|private|protected|internal)?\s*(static)?\s*(virtual|override)?\s*\w+\s+(\w+)\s*\([^)]*\)\s*{";
            var matches = Regex.Matches(fileInfo.content, methodPattern);
            
            foreach (Match match in matches)
            {
                var methodName = match.Groups[4].Value;
                var methodStartLine = GetLineNumber(fileInfo.content, match.Index);
                var methodEndLine = FindMethodEndLine(fileInfo.lines, methodStartLine);
                var methodSize = methodEndLine - methodStartLine;
                
                if (methodSize > maxMethodSize)
                {
                    issues.Add(new CodeIssue
                    {
                        title = $"方法 '{methodName}' 过长",
                        description = $"方法有 {methodSize} 行代码，超过了建议的 {maxMethodSize} 行",
                        severity = methodSize > maxMethodSize * 1.5 ? IssueSeverity.Major : IssueSeverity.Minor,
                        category = IssueCategory.Complexity,
                        filePath = fileInfo.filePath,
                        lineNumber = methodStartLine,
                        suggestion = "考虑将方法拆分为多个更小的方法"
                    });
                }
            }
        }
        
        private int CalculateCyclomaticComplexity(string[] lines, int startLine, int endLine)
        {
            int complexity = 1; // 基础复杂度
            
            for (int i = startLine; i < endLine && i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                // 计算决策点
                if (line.Contains("if ") || line.Contains("else if"))
                    complexity++;
                if (line.Contains("while ") || line.Contains("for ") || line.Contains("foreach "))
                    complexity++;
                if (line.Contains("switch "))
                    complexity++;
                if (line.Contains("case "))
                    complexity++;
                if (line.Contains("catch "))
                    complexity++;
                if (line.Contains("&&") || line.Contains("||"))
                    complexity += CountOccurrences(line, "&&") + CountOccurrences(line, "||");
            }
            
            return complexity;
        }
        
        private int CountOccurrences(string text, string pattern)
        {
            return (text.Length - text.Replace(pattern, "").Length) / pattern.Length;
        }
        
        private int GetLineNumber(string content, int index)
        {
            return content.Substring(0, index).Count(c => c == '\n') + 1;
        }
        
        private int FindClassEndLine(string[] lines, int startLine)
        {
            int braceCount = 0;
            bool foundStart = false;
            
            for (int i = startLine - 1; i < lines.Length; i++)
            {
                var line = lines[i];
                
                foreach (char c in line)
                {
                    if (c == '{')
                    {
                        braceCount++;
                        foundStart = true;
                    }
                    else if (c == '}')
                    {
                        braceCount--;
                        if (foundStart && braceCount == 0)
                        {
                            return i + 1;
                        }
                    }
                }
            }
            
            return lines.Length;
        }
        
        private int FindMethodEndLine(string[] lines, int startLine)
        {
            return FindClassEndLine(lines, startLine); // 使用相同的逻辑
        }
    }
    
    /// <summary>
    /// 重复代码检测规则
    /// </summary>
    public class DuplicationDetectionRule : ICodeAnalysisRule
    {
        public string RuleName => "重复代码检测";
        public string Description => "检测重复的代码块";
        public IssueCategory Category => IssueCategory.Duplication;
        
        private readonly float duplicateThreshold;
        
        public DuplicationDetectionRule(float duplicateThreshold)
        {
            this.duplicateThreshold = duplicateThreshold;
        }
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            // 检测重复的代码块
            CheckDuplicateBlocks(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckDuplicateBlocks(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var codeBlocks = ExtractCodeBlocks(fileInfo.lines);
            
            for (int i = 0; i < codeBlocks.Count; i++)
            {
                for (int j = i + 1; j < codeBlocks.Count; j++)
                {
                    var similarity = CalculateSimilarity(codeBlocks[i].content, codeBlocks[j].content);
                    
                    if (similarity >= duplicateThreshold)
                    {
                        issues.Add(new CodeIssue
                        {
                            title = "发现重复代码",
                            description = $"代码块相似度为 {similarity:P1}，建议提取为公共方法",
                            severity = similarity > 0.9f ? IssueSeverity.Major : IssueSeverity.Minor,
                            category = IssueCategory.Duplication,
                            filePath = fileInfo.filePath,
                            lineNumber = codeBlocks[i].startLine,
                            suggestion = "将重复的代码提取为公共方法或使用继承/组合模式"
                        });
                    }
                }
            }
        }
        
        private List<CodeBlock> ExtractCodeBlocks(string[] lines)
        {
            var blocks = new List<CodeBlock>();
            var currentBlock = new List<string>();
            int startLine = 0;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("/*"))
                {
                    if (currentBlock.Count >= 5) // 至少5行代码才考虑为代码块
                    {
                        blocks.Add(new CodeBlock
                        {
                            content = string.Join("\n", currentBlock),
                            startLine = startLine + 1,
                            endLine = i
                        });
                    }
                    currentBlock.Clear();
                    startLine = i + 1;
                }
                else
                {
                    currentBlock.Add(line);
                }
            }
            
            return blocks;
        }
        
        private float CalculateSimilarity(string text1, string text2)
        {
            // 使用Levenshtein距离计算相似度
            var distance = LevenshteinDistance(text1, text2);
            var maxLength = Math.Max(text1.Length, text2.Length);
            
            return maxLength == 0 ? 1f : 1f - (float)distance / maxLength;
        }
        
        private int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];
            
            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;
            
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return matrix[s1.Length, s2.Length];
        }
    }
    
    /// <summary>
    /// 代码风格检查规则
    /// </summary>
    public class StyleCheckRule : ICodeAnalysisRule
    {
        public string RuleName => "代码风格检查";
        public string Description => "检查代码风格和格式";
        public IssueCategory Category => IssueCategory.Style;
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            CheckIndentation(fileInfo, issues);
            CheckBraceStyle(fileInfo, issues);
            CheckLineLength(fileInfo, issues);
            CheckTrailingWhitespace(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckIndentation(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                // 检查是否使用Tab而不是空格
                if (line.StartsWith("\t"))
                {
                    issues.Add(new CodeIssue
                    {
                        title = "使用Tab缩进",
                        description = "建议使用4个空格而不是Tab进行缩进",
                        severity = IssueSeverity.Info,
                        category = IssueCategory.Style,
                        filePath = fileInfo.filePath,
                        lineNumber = i + 1,
                        suggestion = "将Tab替换为4个空格",
                        autoFixCode = line.Replace("\t", "    ")
                    });
                }
            }
        }
        
        private void CheckBraceStyle(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i].Trim();
                
                // 检查左大括号是否在同一行
                if (line.EndsWith("{") && !line.StartsWith("{"))
                {
                    var beforeBrace = line.Substring(0, line.Length - 1).Trim();
                    if (beforeBrace.Contains("if") || beforeBrace.Contains("for") || beforeBrace.Contains("while"))
                    {
                        issues.Add(new CodeIssue
                        {
                            title = "大括号风格不一致",
                            description = "建议将左大括号放在下一行",
                            severity = IssueSeverity.Info,
                            category = IssueCategory.Style,
                            filePath = fileInfo.filePath,
                            lineNumber = i + 1,
                            suggestion = "将左大括号移到下一行"
                        });
                    }
                }
            }
        }
        
        private void CheckLineLength(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            const int maxLineLength = 120;
            
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                if (line.Length > maxLineLength)
                {
                    issues.Add(new CodeIssue
                    {
                        title = "行长度过长",
                        description = $"行长度为 {line.Length} 字符，超过建议的 {maxLineLength} 字符",
                        severity = IssueSeverity.Minor,
                        category = IssueCategory.Style,
                        filePath = fileInfo.filePath,
                        lineNumber = i + 1,
                        suggestion = "考虑将长行拆分为多行"
                    });
                }
            }
        }
        
        private void CheckTrailingWhitespace(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                if (line.Length > 0 && char.IsWhiteSpace(line[line.Length - 1]))
                {
                    issues.Add(new CodeIssue
                    {
                        title = "行尾有多余空格",
                        description = "行尾存在不必要的空白字符",
                        severity = IssueSeverity.Info,
                        category = IssueCategory.Style,
                        filePath = fileInfo.filePath,
                        lineNumber = i + 1,
                        suggestion = "删除行尾的空白字符",
                        autoFixCode = line.TrimEnd()
                    });
                }
            }
        }
    }
    
    /// <summary>
    /// 安全检查规则
    /// </summary>
    public class SecurityCheckRule : ICodeAnalysisRule
    {
        public string RuleName => "安全检查";
        public string Description => "检查潜在的安全问题";
        public IssueCategory Category => IssueCategory.Security;
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            CheckHardcodedSecrets(fileInfo, issues);
            CheckSQLInjection(fileInfo, issues);
            CheckUnsafeOperations(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckHardcodedSecrets(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var secretPatterns = new[]
            {
                @"password\s*=\s*[""']([^""']+)[""']",
                @"apikey\s*=\s*[""']([^""']+)[""']",
                @"secret\s*=\s*[""']([^""']+)[""']",
                @"token\s*=\s*[""']([^""']+)[""']"
            };
            
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i].ToLower();
                
                foreach (var pattern in secretPatterns)
                {
                    if (Regex.IsMatch(line, pattern))
                    {
                        issues.Add(new CodeIssue
                        {
                            title = "硬编码的敏感信息",
                            description = "代码中包含硬编码的密码、API密钥或其他敏感信息",
                            severity = IssueSeverity.Critical,
                            category = IssueCategory.Security,
                            filePath = fileInfo.filePath,
                            lineNumber = i + 1,
                            suggestion = "将敏感信息移到配置文件或环境变量中"
                        });
                    }
                }
            }
        }
        
        private void CheckSQLInjection(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                
                if (line.Contains("SELECT") && line.Contains("+"))
                {
                    issues.Add(new CodeIssue
                    {
                        title = "潜在的SQL注入风险",
                        description = "使用字符串拼接构建SQL查询可能导致SQL注入攻击",
                        severity = IssueSeverity.Major,
                        category = IssueCategory.Security,
                        filePath = fileInfo.filePath,
                        lineNumber = i + 1,
                        suggestion = "使用参数化查询或ORM框架"
                    });
                }
            }
        }
        
        private void CheckUnsafeOperations(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var unsafePatterns = new[]
            {
                "System.IO.File.Delete",
                "System.Diagnostics.Process.Start",
                "System.Reflection.Assembly.LoadFrom"
            };
            
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                
                foreach (var pattern in unsafePatterns)
                {
                    if (line.Contains(pattern))
                    {
                        issues.Add(new CodeIssue
                        {
                            title = "不安全的操作",
                            description = $"使用了潜在不安全的操作: {pattern}",
                            severity = IssueSeverity.Major,
                            category = IssueCategory.Security,
                            filePath = fileInfo.filePath,
                            lineNumber = i + 1,
                            suggestion = "确保对输入进行适当的验证和清理"
                        });
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 性能检查规则
    /// </summary>
    public class PerformanceCheckRule : ICodeAnalysisRule
    {
        public string RuleName => "性能检查";
        public string Description => "检查潜在的性能问题";
        public IssueCategory Category => IssueCategory.Performance;
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            CheckStringConcatenation(fileInfo, issues);
            CheckUnitySpecificIssues(fileInfo, issues);
            CheckLoopPerformance(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckStringConcatenation(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                
                // 检查循环中的字符串拼接
                if (line.Contains("for") || line.Contains("while") || line.Contains("foreach"))
                {
                    // 查找循环体中的字符串拼接
                    for (int j = i + 1; j < Math.Min(i + 20, fileInfo.lines.Length); j++)
                    {
                        if (fileInfo.lines[j].Contains("+=") && fileInfo.lines[j].Contains("string"))
                        {
                            issues.Add(new CodeIssue
                            {
                                title = "循环中的字符串拼接",
                                description = "在循环中使用字符串拼接可能导致性能问题",
                                severity = IssueSeverity.Minor,
                                category = IssueCategory.Performance,
                                filePath = fileInfo.filePath,
                                lineNumber = j + 1,
                                suggestion = "使用StringBuilder进行字符串拼接"
                            });
                        }
                    }
                }
            }
        }
        
        private void CheckUnitySpecificIssues(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                
                // 检查Update中的昂贵操作
                if (line.Contains("void Update()"))
                {
                    for (int j = i + 1; j < Math.Min(i + 50, fileInfo.lines.Length); j++)
                    {
                        var updateLine = fileInfo.lines[j];
                        
                        if (updateLine.Contains("GameObject.Find") || 
                            updateLine.Contains("GetComponent") ||
                            updateLine.Contains("Resources.Load"))
                        {
                            issues.Add(new CodeIssue
                            {
                                title = "Update中的昂贵操作",
                                description = "在Update方法中执行昂贵的操作会影响帧率",
                                severity = IssueSeverity.Major,
                                category = IssueCategory.Performance,
                                filePath = fileInfo.filePath,
                                lineNumber = j + 1,
                                suggestion = "将昂贵操作移到Start或缓存结果"
                            });
                        }
                    }
                }
                
                // 检查Camera.main的使用
                if (line.Contains("Camera.main"))
                {
                    issues.Add(new CodeIssue
                    {
                        title = "频繁使用Camera.main",
                        description = "Camera.main使用FindGameObjectWithTag，性能较差",
                        severity = IssueSeverity.Minor,
                        category = IssueCategory.Performance,
                        filePath = fileInfo.filePath,
                        lineNumber = i + 1,
                        suggestion = "缓存Camera引用而不是每次调用Camera.main"
                    });
                }
            }
        }
        
        private void CheckLoopPerformance(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            for (int i = 0; i < fileInfo.lines.Length; i++)
            {
                var line = fileInfo.lines[i];
                
                // 检查嵌套循环
                if (line.Contains("for") || line.Contains("while"))
                {
                    int nestedLevel = 0;
                    for (int j = i + 1; j < Math.Min(i + 100, fileInfo.lines.Length); j++)
                    {
                        if (fileInfo.lines[j].Contains("for") || fileInfo.lines[j].Contains("while"))
                        {
                            nestedLevel++;
                            if (nestedLevel >= 2)
                            {
                                issues.Add(new CodeIssue
                                {
                                    title = "深度嵌套循环",
                                    description = "深度嵌套的循环可能导致性能问题",
                                    severity = IssueSeverity.Minor,
                                    category = IssueCategory.Performance,
                                    filePath = fileInfo.filePath,
                                    lineNumber = j + 1,
                                    suggestion = "考虑优化算法或使用更高效的数据结构"
                                });
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 命名约定规则
    /// </summary>
    public class NamingConventionRule : ICodeAnalysisRule
    {
        public string RuleName => "命名约定";
        public string Description => "检查命名约定";
        public IssueCategory Category => IssueCategory.Style;
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            CheckClassNaming(fileInfo, issues);
            CheckMethodNaming(fileInfo, issues);
            CheckVariableNaming(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckClassNaming(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var classPattern = @"class\s+(\w+)";
            var matches = Regex.Matches(fileInfo.content, classPattern);
            
            foreach (Match match in matches)
            {
                var className = match.Groups[1].Value;
                if (!char.IsUpper(className[0]))
                {
                    var lineNumber = GetLineNumber(fileInfo.content, match.Index);
                    issues.Add(new CodeIssue
                    {
                        title = "类名命名不规范",
                        description = $"类名 '{className}' 应该以大写字母开头",
                        severity = IssueSeverity.Minor,
                        category = IssueCategory.Style,
                        filePath = fileInfo.filePath,
                        lineNumber = lineNumber,
                        suggestion = "使用PascalCase命名类"
                    });
                }
            }
        }
        
        private void CheckMethodNaming(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var methodPattern = @"(public|private|protected)\s+\w+\s+(\w+)\s*\(";
            var matches = Regex.Matches(fileInfo.content, methodPattern);
            
            foreach (Match match in matches)
            {
                var methodName = match.Groups[2].Value;
                if (!char.IsUpper(methodName[0]))
                {
                    var lineNumber = GetLineNumber(fileInfo.content, match.Index);
                    issues.Add(new CodeIssue
                    {
                        title = "方法名命名不规范",
                        description = $"方法名 '{methodName}' 应该以大写字母开头",
                        severity = IssueSeverity.Minor,
                        category = IssueCategory.Style,
                        filePath = fileInfo.filePath,
                        lineNumber = lineNumber,
                        suggestion = "使用PascalCase命名公共方法"
                    });
                }
            }
        }
        
        private void CheckVariableNaming(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var fieldPattern = @"private\s+\w+\s+(\w+);";
            var matches = Regex.Matches(fileInfo.content, fieldPattern);
            
            foreach (Match match in matches)
            {
                var fieldName = match.Groups[1].Value;
                if (!char.IsLower(fieldName[0]) && !fieldName.StartsWith("_"))
                {
                    var lineNumber = GetLineNumber(fileInfo.content, match.Index);
                    issues.Add(new CodeIssue
                    {
                        title = "字段名命名不规范",
                        description = $"私有字段 '{fieldName}' 应该以小写字母或下划线开头",
                        severity = IssueSeverity.Info,
                        category = IssueCategory.Style,
                        filePath = fileInfo.filePath,
                        lineNumber = lineNumber,
                        suggestion = "使用camelCase或_camelCase命名私有字段"
                    });
                }
            }
        }
        
        private int GetLineNumber(string content, int index)
        {
            return content.Substring(0, index).Count(c => c == '\n') + 1;
        }
    }
    
    /// <summary>
    /// 文档规则
    /// </summary>
    public class DocumentationRule : ICodeAnalysisRule
    {
        public string RuleName => "文档检查";
        public string Description => "检查代码文档";
        public IssueCategory Category => IssueCategory.Documentation;
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            CheckPublicMethodDocumentation(fileInfo, issues);
            CheckClassDocumentation(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckPublicMethodDocumentation(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var methodPattern = @"public\s+\w+\s+(\w+)\s*\([^)]*\)";
            var matches = Regex.Matches(fileInfo.content, methodPattern);
            
            foreach (Match match in matches)
            {
                var methodName = match.Groups[1].Value;
                var lineNumber = GetLineNumber(fileInfo.content, match.Index);
                
                // 检查前面是否有XML文档注释
                if (lineNumber > 1)
                {
                    var previousLine = fileInfo.lines[lineNumber - 2].Trim();
                    if (!previousLine.Contains("///"))
                    {
                        issues.Add(new CodeIssue
                        {
                            title = "缺少方法文档",
                            description = $"公共方法 '{methodName}' 缺少XML文档注释",
                            severity = IssueSeverity.Info,
                            category = IssueCategory.Documentation,
                            filePath = fileInfo.filePath,
                            lineNumber = lineNumber,
                            suggestion = "为公共方法添加XML文档注释"
                        });
                    }
                }
            }
        }
        
        private void CheckClassDocumentation(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            var classPattern = @"public\s+class\s+(\w+)";
            var matches = Regex.Matches(fileInfo.content, classPattern);
            
            foreach (Match match in matches)
            {
                var className = match.Groups[1].Value;
                var lineNumber = GetLineNumber(fileInfo.content, match.Index);
                
                // 检查前面是否有XML文档注释
                if (lineNumber > 1)
                {
                    var previousLine = fileInfo.lines[lineNumber - 2].Trim();
                    if (!previousLine.Contains("///"))
                    {
                        issues.Add(new CodeIssue
                        {
                            title = "缺少类文档",
                            description = $"公共类 '{className}' 缺少XML文档注释",
                            severity = IssueSeverity.Info,
                            category = IssueCategory.Documentation,
                            filePath = fileInfo.filePath,
                            lineNumber = lineNumber,
                            suggestion = "为公共类添加XML文档注释"
                        });
                    }
                }
            }
        }
        
        private int GetLineNumber(string content, int index)
        {
            return content.Substring(0, index).Count(c => c == '\n') + 1;
        }
    }
    
    /// <summary>
    /// 设计模式规则
    /// </summary>
    public class DesignPatternRule : ICodeAnalysisRule
    {
        public string RuleName => "设计模式建议";
        public string Description => "建议使用设计模式";
        public IssueCategory Category => IssueCategory.Design;
        
        public List<CodeIssue> Analyze(FileAnalysisInfo fileInfo)
        {
            var issues = new List<CodeIssue>();
            
            CheckSingletonPattern(fileInfo, issues);
            CheckFactoryPattern(fileInfo, issues);
            
            return issues;
        }
        
        private void CheckSingletonPattern(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            if (fileInfo.content.Contains("static") && fileInfo.content.Contains("Instance"))
            {
                // 可能是单例模式的实现，检查是否线程安全
                if (!fileInfo.content.Contains("lock") && !fileInfo.content.Contains("Lazy"))
                {
                    issues.Add(new CodeIssue
                    {
                        title = "单例模式可能不是线程安全的",
                        description = "单例模式的实现可能在多线程环境下不安全",
                        severity = IssueSeverity.Minor,
                        category = IssueCategory.Design,
                        filePath = fileInfo.filePath,
                        lineNumber = 1,
                        suggestion = "考虑使用Lazy<T>或双重检查锁定模式"
                    });
                }
            }
        }
        
        private void CheckFactoryPattern(FileAnalysisInfo fileInfo, List<CodeIssue> issues)
        {
            // 检查是否有大量的switch/if-else用于对象创建
            var switchCount = Regex.Matches(fileInfo.content, @"switch\s*\([^)]+\)").Count;
            var newCount = Regex.Matches(fileInfo.content, @"new\s+\w+\s*\(").Count;
            
            if (switchCount > 0 && newCount > 5)
            {
                issues.Add(new CodeIssue
                {
                    title = "考虑使用工厂模式",
                    description = "大量的条件判断用于对象创建，建议使用工厂模式",
                    severity = IssueSeverity.Info,
                    category = IssueCategory.Design,
                    filePath = fileInfo.filePath,
                    lineNumber = 1,
                    suggestion = "使用工厂模式或抽象工厂模式来管理对象创建"
                });
            }
        }
    }
}