using System;
using System.Collections.Generic;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 错误处理接口
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// 处理异常
        /// </summary>
        void HandleException(Exception exception, string context = null, Dictionary<string, object> additionalData = null);
        
        /// <summary>
        /// 处理错误
        /// </summary>
        void HandleError(string message, string context = null, Dictionary<string, object> additionalData = null);
        
        /// <summary>
        /// 处理警告
        /// </summary>
        void HandleWarning(string message, string context = null, Dictionary<string, object> additionalData = null);
        
        /// <summary>
        /// 处理信息
        /// </summary>
        void HandleInfo(string message, string context = null, Dictionary<string, object> additionalData = null);
        
        /// <summary>
        /// 处理调试信息
        /// </summary>
        void HandleDebug(string message, string context = null, Dictionary<string, object> additionalData = null);
        
        /// <summary>
        /// 获取错误统计
        /// </summary>
        ErrorStatistics GetErrorStatistics();
        
        /// <summary>
        /// 清除错误历史
        /// </summary>
        void ClearErrorHistory();
        
        /// <summary>
        /// 设置错误过滤器
        /// </summary>
        void SetErrorFilter(Func<ErrorInfo, bool> filter);
        
        /// <summary>
        /// 添加错误监听器
        /// </summary>
        void AddErrorListener(IErrorListener listener);
        
        /// <summary>
        /// 移除错误监听器
        /// </summary>
        void RemoveErrorListener(IErrorListener listener);
    }
    
    /// <summary>
    /// 错误监听器接口
    /// </summary>
    public interface IErrorListener
    {
        void OnError(ErrorInfo errorInfo);
    }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public class ErrorInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ErrorSeverity Severity { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public Exception Exception { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public bool IsHandled { get; set; }
        
        public override string ToString()
        {
            var contextStr = !string.IsNullOrEmpty(Context) ? $" [{Context}]" : "";
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Severity}{contextStr}: {Message}";
        }
    }
    
    /// <summary>
    /// 错误严重程度
    /// </summary>
    public enum ErrorSeverity
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
    
    /// <summary>
    /// 错误统计信息
    /// </summary>
    public class ErrorStatistics
    {
        public int TotalErrors { get; set; }
        public int DebugCount { get; set; }
        public int InfoCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int FatalCount { get; set; }
        public DateTime LastErrorTime { get; set; }
        public Dictionary<string, int> ErrorsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ErrorsByContext { get; set; } = new Dictionary<string, int>();
    }
}