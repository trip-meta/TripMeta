using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 日志输出接口
    /// </summary>
    public interface ILogOutput
    {
        void WriteLog(LogEntry logEntry);
    }
    
    /// <summary>
    /// 控制台日志输出
    /// </summary>
    public class ConsoleLogOutput : ILogOutput
    {
        public void WriteLog(LogEntry logEntry)
        {
            var message = FormatLogMessage(logEntry);
            
            switch (logEntry.level)
            {
                case LogLevel.Debug:
                    Console.WriteLine($"[DEBUG] {message}");
                    break;
                case LogLevel.Info:
                    Console.WriteLine($"[INFO] {message}");
                    break;
                case LogLevel.Warning:
                    Console.WriteLine($"[WARNING] {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    Console.WriteLine($"[ERROR] {message}");
                    break;
            }
        }
        
        private string FormatLogMessage(LogEntry logEntry)
        {
            var sb = new StringBuilder();
            sb.Append($"{logEntry.timestamp:HH:mm:ss.fff} ");
            
            if (!string.IsNullOrEmpty(logEntry.category))
            {
                sb.Append($"[{logEntry.category}] ");
            }
            
            sb.Append(logEntry.message);
            
            if (logEntry.exception != null)
            {
                sb.AppendLine();
                sb.Append($"Exception: {logEntry.exception.Message}");
            }
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// Unity日志输出
    /// </summary>
    public class UnityLogOutput : ILogOutput
    {
        public void WriteLog(LogEntry logEntry)
        {
            var message = FormatLogMessage(logEntry);
            
            switch (logEntry.level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    if (logEntry.exception != null)
                    {
                        Debug.LogException(logEntry.exception);
                    }
                    else
                    {
                        Debug.LogError(message);
                    }
                    break;
            }
        }
        
        private string FormatLogMessage(LogEntry logEntry)
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(logEntry.category))
            {
                sb.Append($"[{logEntry.category}] ");
            }
            
            sb.Append(logEntry.message);
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// 文件日志输出
    /// </summary>
    public class FileLogOutput : ILogOutput, IDisposable
    {
        private StreamWriter writer;
        private readonly object lockObject = new object();
        
        public void WriteLog(LogEntry logEntry)
        {
            if (writer == null) return;
            
            lock (lockObject)
            {
                try
                {
                    var message = FormatLogMessage(logEntry);
                    writer.WriteLine(message);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        
        private string FormatLogMessage(LogEntry logEntry)
        {
            var sb = new StringBuilder();
            sb.Append($"{logEntry.timestamp:yyyy-MM-dd HH:mm:ss.fff} ");
            sb.Append($"[{logEntry.level}] ");
            sb.Append($"[Thread-{logEntry.threadId}] ");
            
            if (!string.IsNullOrEmpty(logEntry.category))
            {
                sb.Append($"[{logEntry.category}] ");
            }
            
            sb.Append(logEntry.message);
            
            if (logEntry.exception != null)
            {
                sb.AppendLine();
                sb.Append($"Exception: {logEntry.exception}");
            }
            
            if (!string.IsNullOrEmpty(logEntry.stackTrace) && logEntry.level >= LogLevel.Error)
            {
                sb.AppendLine();
                sb.Append($"StackTrace: {logEntry.stackTrace}");
            }
            
            if (logEntry.metadata != null && logEntry.metadata.Count > 0)
            {
                sb.AppendLine();
                sb.Append("Metadata: ");
                foreach (var kvp in logEntry.metadata)
                {
                    sb.Append($"{kvp.Key}={kvp.Value}; ");
                }
            }
            
            return sb.ToString();
        }
        
        public void SetWriter(StreamWriter streamWriter)
        {
            lock (lockObject)
            {
                writer?.Dispose();
                writer = streamWriter;
            }
        }
        
        public void Dispose()
        {
            lock (lockObject)
            {
                writer?.Dispose();
                writer = null;
            }
        }
    }
    
    /// <summary>
    /// 远程日志输出
    /// </summary>
    public class RemoteLogOutput : ILogOutput, IDisposable
    {
        private readonly string endpoint;
        private readonly string apiKey;
        private readonly HttpClient httpClient;
        private readonly Queue<LogEntry> pendingLogs;
        private readonly object lockObject = new object();
        private bool isUploading = false;
        
        public RemoteLogOutput(string endpoint, string apiKey)
        {
            this.endpoint = endpoint;
            this.apiKey = apiKey;
            this.httpClient = new HttpClient();
            this.pendingLogs = new Queue<LogEntry>();
            
            // 设置HTTP客户端
            if (!string.IsNullOrEmpty(apiKey))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }
            httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }
        
        public void WriteLog(LogEntry logEntry)
        {
            lock (lockObject)
            {
                pendingLogs.Enqueue(logEntry);
            }
            
            // 异步上传日志
            _ = UploadLogsAsync();
        }
        
        private async Task UploadLogsAsync()
        {
            if (isUploading) return;
            
            isUploading = true;
            
            try
            {
                var logsToUpload = new List<LogEntry>();
                
                lock (lockObject)
                {
                    while (pendingLogs.Count > 0 && logsToUpload.Count < 100) // 批量上传，最多100条
                    {
                        logsToUpload.Add(pendingLogs.Dequeue());
                    }
                }
                
                if (logsToUpload.Count == 0) return;
                
                var logData = new
                {
                    timestamp = DateTime.UtcNow,
                    application = Application.productName,
                    version = Application.version,
                    platform = Application.platform.ToString(),
                    logs = logsToUpload.Select(log => new
                    {
                        timestamp = log.timestamp,
                        level = log.level.ToString(),
                        message = log.message,
                        category = log.category,
                        exception = log.exception?.ToString(),
                        metadata = log.metadata,
                        threadId = log.threadId
                    })
                };
                
                var json = JsonConvert.SerializeObject(logData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogWarning($"远程日志上传失败: {response.StatusCode} - {response.ReasonPhrase}");
                    
                    // 将失败的日志重新加入队列
                    lock (lockObject)
                    {
                        foreach (var log in logsToUpload)
                        {
                            pendingLogs.Enqueue(log);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                
                // 将失败的日志重新加入队列
                lock (lockObject)
                {
                    foreach (var log in logsToUpload)
                    {
                        pendingLogs.Enqueue(log);
                    }
                }
            }
            finally
            {
                isUploading = false;
            }
        }
        
        public void Dispose()
        {
            // 上传剩余日志
            _ = UploadLogsAsync();
            
            httpClient?.Dispose();
        }
    }
    
    /// <summary>
    /// 内存日志输出（用于调试面板）
    /// </summary>
    public class MemoryLogOutput : ILogOutput
    {
        private readonly Queue<LogEntry> logHistory;
        private readonly int maxHistorySize;
        private readonly object lockObject = new object();
        
        public MemoryLogOutput(int maxHistorySize = 1000)
        {
            this.maxHistorySize = maxHistorySize;
            this.logHistory = new Queue<LogEntry>();
        }
        
        public void WriteLog(LogEntry logEntry)
        {
            lock (lockObject)
            {
                logHistory.Enqueue(logEntry);
                
                while (logHistory.Count > maxHistorySize)
                {
                    logHistory.Dequeue();
                }
            }
        }
        
        public List<LogEntry> GetLogHistory()
        {
            lock (lockObject)
            {
                return new List<LogEntry>(logHistory);
            }
        }
        
        public void ClearHistory()
        {
            lock (lockObject)
            {
                logHistory.Clear();
            }
        }
    }
    
    /// <summary>
    /// 过滤日志输出
    /// </summary>
    public class FilteredLogOutput : ILogOutput
    {
        private readonly ILogOutput baseOutput;
        private readonly Func<LogEntry, bool> filter;
        
        public FilteredLogOutput(ILogOutput baseOutput, Func<LogEntry, bool> filter)
        {
            this.baseOutput = baseOutput;
            this.filter = filter;
        }
        
        public void WriteLog(LogEntry logEntry)
        {
            if (filter(logEntry))
            {
                baseOutput.WriteLog(logEntry);
            }
        }
    }
    
    /// <summary>
    /// 彩色控制台日志输出
    /// </summary>
    public class ColoredConsoleLogOutput : ILogOutput
    {
        public void WriteLog(LogEntry logEntry)
        {
            var originalColor = Console.ForegroundColor;
            
            try
            {
                // 设置颜色
                switch (logEntry.level)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                }
                
                var message = FormatLogMessage(logEntry);
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        private string FormatLogMessage(LogEntry logEntry)
        {
            var sb = new StringBuilder();
            sb.Append($"{logEntry.timestamp:HH:mm:ss.fff} ");
            sb.Append($"[{logEntry.level}] ");
            
            if (!string.IsNullOrEmpty(logEntry.category))
            {
                sb.Append($"[{logEntry.category}] ");
            }
            
            sb.Append(logEntry.message);
            
            if (logEntry.exception != null)
            {
                sb.AppendLine();
                sb.Append($"Exception: {logEntry.exception.Message}");
            }
            
            return sb.ToString();
        }
    }
}