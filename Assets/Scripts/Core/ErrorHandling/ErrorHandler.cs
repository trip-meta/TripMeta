using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;
using TripMeta.Core.Configuration;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 错误处理器实现
    /// </summary>
    public class ErrorHandler : IErrorHandler, IDisposable
    {
        private readonly ConcurrentQueue<ErrorInfo> errorHistory;
        private readonly List<IErrorListener> errorListeners;
        private readonly object lockObject = new object();
        private Func<ErrorInfo, bool> errorFilter;
        private DebugConfiguration debugConfig;
        private bool disposed = false;
        
        // 错误统计
        private ErrorStatistics statistics;
        
        public ErrorHandler(DebugConfiguration config = null)
        {
            errorHistory = new ConcurrentQueue<ErrorInfo>();
            errorListeners = new List<IErrorListener>();
            statistics = new ErrorStatistics();
            debugConfig = config ?? new DebugConfiguration();
            
            // 注册Unity日志回调
            Application.logMessageReceived += OnUnityLogMessageReceived;
            
            Debug.Log("[ErrorHandler] 错误处理系统已初始化");
        }
        
        /// <summary>
        /// 处理异常
        /// </summary>
        public void HandleException(Exception exception, string context = null, Dictionary<string, object> additionalData = null)
        {
            if (exception == null) return;
            
            var errorInfo = new ErrorInfo
            {
                Severity = GetSeverityFromException(exception),
                Message = exception.Message,
                Context = context ?? "Unknown",
                Exception = exception,
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                StackTrace = exception.StackTrace,
                Source = exception.Source,
                Category = exception.GetType().Name
            };
            
            ProcessError(errorInfo);
        }
        
        /// <summary>
        /// 处理错误
        /// </summary>
        public void HandleError(string message, string context = null, Dictionary<string, object> additionalData = null)
        {
            var errorInfo = new ErrorInfo
            {
                Severity = ErrorSeverity.Error,
                Message = message,
                Context = context ?? "Unknown",
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                StackTrace = Environment.StackTrace,
                Category = "Application"
            };
            
            ProcessError(errorInfo);
        }
        
        /// <summary>
        /// 处理警告
        /// </summary>
        public void HandleWarning(string message, string context = null, Dictionary<string, object> additionalData = null)
        {
            var errorInfo = new ErrorInfo
            {
                Severity = ErrorSeverity.Warning,
                Message = message,
                Context = context ?? "Unknown",
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                Category = "Application"
            };
            
            ProcessError(errorInfo);
        }
        
        /// <summary>
        /// 处理信息
        /// </summary>
        public void HandleInfo(string message, string context = null, Dictionary<string, object> additionalData = null)
        {
            var errorInfo = new ErrorInfo
            {
                Severity = ErrorSeverity.Info,
                Message = message,
                Context = context ?? "Unknown",
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                Category = "Application"
            };
            
            ProcessError(errorInfo);
        }
        
        /// <summary>
        /// 处理调试信息
        /// </summary>
        public void HandleDebug(string message, string context = null, Dictionary<string, object> additionalData = null)
        {
            if (!debugConfig.enableDebugLogs) return;
            
            var errorInfo = new ErrorInfo
            {
                Severity = ErrorSeverity.Debug,
                Message = message,
                Context = context ?? "Unknown",
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                Category = "Debug"
            };
            
            ProcessError(errorInfo);
        }
        
        /// <summary>
        /// 处理错误信息
        /// </summary>
        private void ProcessError(ErrorInfo errorInfo)
        {
            if (disposed) return;
            
            // 应用过滤器
            if (errorFilter != null && !errorFilter(errorInfo))
                return;
            
            // 检查日志级别
            if (errorInfo.Severity < debugConfig.minLogLevel)
                return;
            
            // 添加到历史记录
            errorHistory.Enqueue(errorInfo);
            
            // 更新统计信息
            UpdateStatistics(errorInfo);
            
            // 输出到Unity控制台
            LogToUnityConsole(errorInfo);
            
            // 通知监听器
            NotifyListeners(errorInfo);
            
            // 处理严重错误
            if (errorInfo.Severity >= ErrorSeverity.Fatal)
            {
                HandleFatalError(errorInfo);
            }
        }
        
        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics(ErrorInfo errorInfo)
        {
            lock (lockObject)
            {
                statistics.TotalErrors++;
                statistics.LastErrorTime = errorInfo.Timestamp;
                
                switch (errorInfo.Severity)
                {
                    case ErrorSeverity.Debug:
                        statistics.DebugCount++;
                        break;
                    case ErrorSeverity.Info:
                        statistics.InfoCount++;
                        break;
                    case ErrorSeverity.Warning:
                        statistics.WarningCount++;
                        break;
                    case ErrorSeverity.Error:
                        statistics.ErrorCount++;
                        break;
                    case ErrorSeverity.Fatal:
                        statistics.FatalCount++;
                        break;
                }
                
                // 按类别统计
                if (!string.IsNullOrEmpty(errorInfo.Category))
                {
                    if (statistics.ErrorsByCategory.ContainsKey(errorInfo.Category))
                        statistics.ErrorsByCategory[errorInfo.Category]++;
                    else
                        statistics.ErrorsByCategory[errorInfo.Category] = 1;
                }
                
                // 按上下文统计
                if (!string.IsNullOrEmpty(errorInfo.Context))
                {
                    if (statistics.ErrorsByContext.ContainsKey(errorInfo.Context))
                        statistics.ErrorsByContext[errorInfo.Context]++;
                    else
                        statistics.ErrorsByContext[errorInfo.Context] = 1;
                }
            }
        }
        
        /// <summary>
        /// 输出到Unity控制台
        /// </summary>
        private void LogToUnityConsole(ErrorInfo errorInfo)
        {
            var message = FormatErrorMessage(errorInfo);
            
            switch (errorInfo.Severity)
            {
                case ErrorSeverity.Debug:
                case ErrorSeverity.Info:
                    Debug.Log(message);
                    break;
                case ErrorSeverity.Warning:
                    Debug.LogWarning(message);
                    break;
                case ErrorSeverity.Error:
                case ErrorSeverity.Fatal:
                    if (errorInfo.Exception != null)
                        Debug.LogException(errorInfo.Exception);
                    else
                        Debug.LogError(message);
                    break;
            }
        }
        
        /// <summary>
        /// 格式化错误消息
        /// </summary>
        private string FormatErrorMessage(ErrorInfo errorInfo)
        {
            var message = $"[{errorInfo.Context}] {errorInfo.Message}";
            
            if (debugConfig.enableVerboseLogs && errorInfo.AdditionalData.Count > 0)
            {
                message += "\n附加数据: " + string.Join(", ", 
                    errorInfo.AdditionalData.Select(kv => $"{kv.Key}={kv.Value}"));
            }
            
            return message;
        }
        
        /// <summary>
        /// 通知监听器
        /// </summary>
        private void NotifyListeners(ErrorInfo errorInfo)
        {
            lock (lockObject)
            {
                foreach (var listener in errorListeners.ToList())
                {
                    try
                    {
                        listener.OnError(errorInfo);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ErrorHandler] 错误监听器异常: {ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 处理致命错误
        /// </summary>
        private void HandleFatalError(ErrorInfo errorInfo)
        {
            Debug.LogError($"[ErrorHandler] 致命错误: {errorInfo.Message}");
            
            // 可以在这里添加崩溃报告、自动保存等逻辑
            // 例如：保存用户数据、发送崩溃报告等
        }
        
        /// <summary>
        /// 从异常获取严重程度
        /// </summary>
        private ErrorSeverity GetSeverityFromException(Exception exception)
        {
            switch (exception)
            {
                case ArgumentException _:
                case InvalidOperationException _:
                    return ErrorSeverity.Error;
                case OutOfMemoryException _:
                case StackOverflowException _:
                    return ErrorSeverity.Fatal;
                case NotImplementedException _:
                    return ErrorSeverity.Warning;
                default:
                    return ErrorSeverity.Error;
            }
        }
        
        /// <summary>
        /// Unity日志消息回调
        /// </summary>
        private void OnUnityLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!debugConfig.logToFile) return;
            
            var severity = type switch
            {
                LogType.Error => ErrorSeverity.Error,
                LogType.Assert => ErrorSeverity.Error,
                LogType.Warning => ErrorSeverity.Warning,
                LogType.Log => ErrorSeverity.Info,
                LogType.Exception => ErrorSeverity.Fatal,
                _ => ErrorSeverity.Info
            };
            
            var errorInfo = new ErrorInfo
            {
                Severity = severity,
                Message = condition,
                Context = "Unity",
                StackTrace = stackTrace,
                Category = "Unity" + type.ToString()
            };
            
            // 只记录，不重复输出到控制台
            errorHistory.Enqueue(errorInfo);
            UpdateStatistics(errorInfo);
        }
        
        /// <summary>
        /// 获取错误统计
        /// </summary>
        public ErrorStatistics GetErrorStatistics()
        {
            lock (lockObject)
            {
                return new ErrorStatistics
                {
                    TotalErrors = statistics.TotalErrors,
                    DebugCount = statistics.DebugCount,
                    InfoCount = statistics.InfoCount,
                    WarningCount = statistics.WarningCount,
                    ErrorCount = statistics.ErrorCount,
                    FatalCount = statistics.FatalCount,
                    LastErrorTime = statistics.LastErrorTime,
                    ErrorsByCategory = new Dictionary<string, int>(statistics.ErrorsByCategory),
                    ErrorsByContext = new Dictionary<string, int>(statistics.ErrorsByContext)
                };
            }
        }
        
        /// <summary>
        /// 清除错误历史
        /// </summary>
        public void ClearErrorHistory()
        {
            while (errorHistory.TryDequeue(out _)) { }
            
            lock (lockObject)
            {
                statistics = new ErrorStatistics();
            }
            
            Debug.Log("[ErrorHandler] 错误历史已清除");
        }
        
        /// <summary>
        /// 设置错误过滤器
        /// </summary>
        public void SetErrorFilter(Func<ErrorInfo, bool> filter)
        {
            errorFilter = filter;
        }
        
        /// <summary>
        /// 添加错误监听器
        /// </summary>
        public void AddErrorListener(IErrorListener listener)
        {
            if (listener == null) return;
            
            lock (lockObject)
            {
                if (!errorListeners.Contains(listener))
                {
                    errorListeners.Add(listener);
                }
            }
        }
        
        /// <summary>
        /// 移除错误监听器
        /// </summary>
        public void RemoveErrorListener(IErrorListener listener)
        {
            if (listener == null) return;
            
            lock (lockObject)
            {
                errorListeners.Remove(listener);
            }
        }
        
        /// <summary>
        /// 获取最近的错误
        /// </summary>
        public List<ErrorInfo> GetRecentErrors(int count = 100)
        {
            var errors = errorHistory.ToArray();
            return errors.TakeLast(count).ToList();
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            
            Application.logMessageReceived -= OnUnityLogMessageReceived;
            
            lock (lockObject)
            {
                errorListeners.Clear();
            }
            
            disposed = true;
            Debug.Log("[ErrorHandler] 错误处理系统已释放");
        }
    }
}