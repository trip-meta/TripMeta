using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 高级日志系统 - 提供完整的日志管理功能
    /// </summary>
    public class AdvancedLogger : MonoBehaviour, IService
    {
        [Header("日志配置")]
        [SerializeField] private LogLevel minLogLevel = LogLevel.Info;
        [SerializeField] private bool enableFileLogging = true;
        [SerializeField] private bool enableRemoteLogging = false;
        [SerializeField] private bool enableConsoleLogging = true;
        [SerializeField] private int maxLogFileSize = 10 * 1024 * 1024; // 10MB
        [SerializeField] private int maxLogFiles = 5;
        
        [Header("性能设置")]
        [SerializeField] private int logBufferSize = 1000;
        [SerializeField] private float flushInterval = 5f;
        [SerializeField] private bool asyncLogging = true;
        
        [Header("远程日志")]
        [SerializeField] private string remoteLogEndpoint = "";
        [SerializeField] private string apiKey = "";
        [SerializeField] private bool compressLogs = true;
        
        // 日志管理
        private Queue<LogEntry> logBuffer;
        private List<ILogOutput> logOutputs;
        private Dictionary<string, LogContext> logContexts;
        private LogStatistics statistics;
        
        // 文件日志
        private string logDirectory;
        private string currentLogFile;
        private StreamWriter logWriter;
        
        // 事件
        public event Action<LogEntry> OnLogEntryAdded;
        public event Action<LogLevel, string> OnCriticalError;
        
        public LogStatistics Statistics => statistics;
        public bool IsInitialized { get; private set; }
        
        private static AdvancedLogger instance;
        public static AdvancedLogger Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLogger();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            ServiceLocator.RegisterService<AdvancedLogger>(this);
            
            if (asyncLogging)
            {
                InvokeRepeating(nameof(FlushLogs), flushInterval, flushInterval);
            }
        }
        
        /// <summary>
        /// 初始化日志系统
        /// </summary>
        private void InitializeLogger()
        {
            try
            {
                // 初始化数据结构
                logBuffer = new Queue<LogEntry>();
                logOutputs = new List<ILogOutput>();
                logContexts = new Dictionary<string, LogContext>();
                statistics = new LogStatistics();
                
                // 设置日志目录
                logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                // 初始化日志输出
                InitializeLogOutputs();
                
                // 清理旧日志文件
                CleanupOldLogFiles();
                
                // 创建新的日志文件
                CreateNewLogFile();
                
                IsInitialized = true;
                
                // 记录初始化日志
                LogInfo("高级日志系统初始化完成", "AdvancedLogger");
                LogSystemInfo();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// 初始化日志输出
        /// </summary>
        private void InitializeLogOutputs()
        {
            // 控制台输出
            if (enableConsoleLogging)
            {
                logOutputs.Add(new ConsoleLogOutput());
            }
            
            // 文件输出
            if (enableFileLogging)
            {
                logOutputs.Add(new FileLogOutput());
            }
            
            // 远程输出
            if (enableRemoteLogging && !string.IsNullOrEmpty(remoteLogEndpoint))
            {
                logOutputs.Add(new RemoteLogOutput(remoteLogEndpoint, apiKey));
            }
            
            // Unity日志输出
            logOutputs.Add(new UnityLogOutput());
        }
        
        /// <summary>
        /// 记录系统信息
        /// </summary>
        private void LogSystemInfo()
        {
            var systemInfo = new StringBuilder();
            systemInfo.AppendLine("=== 系统信息 ===");
            systemInfo.AppendLine($"应用版本: {Application.version}");
            systemInfo.AppendLine($"Unity版本: {Application.unityVersion}");
            systemInfo.AppendLine($"平台: {Application.platform}");
            systemInfo.AppendLine($"设备型号: {SystemInfo.deviceModel}");
            systemInfo.AppendLine($"操作系统: {SystemInfo.operatingSystem}");
            systemInfo.AppendLine($"处理器: {SystemInfo.processorType}");
            systemInfo.AppendLine($"内存: {SystemInfo.systemMemorySize}MB");
            systemInfo.AppendLine($"显卡: {SystemInfo.graphicsDeviceName}");
            systemInfo.AppendLine($"显存: {SystemInfo.graphicsMemorySize}MB");
            systemInfo.AppendLine("==================");
            
            LogInfo(systemInfo.ToString(), "SystemInfo");
        }
        
        /// <summary>
        /// 记录日志
        /// </summary>
        public void Log(LogLevel level, string message, string category = "", Exception exception = null, Dictionary<string, object> metadata = null)
        {
            if (!IsInitialized || level < minLogLevel) return;
            
            var logEntry = new LogEntry
            {
                timestamp = DateTime.Now,
                level = level,
                message = message,
                category = category,
                exception = exception,
                metadata = metadata ?? new Dictionary<string, object>(),
                threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                stackTrace = level >= LogLevel.Warning ? Environment.StackTrace : null
            };
            
            // 添加上下文信息
            if (logContexts.ContainsKey(category))
            {
                var context = logContexts[category];
                logEntry.metadata["Context"] = context;
            }
            
            // 更新统计信息
            UpdateStatistics(logEntry);
            
            // 处理日志
            ProcessLogEntry(logEntry);
            
            // 触发事件
            OnLogEntryAdded?.Invoke(logEntry);
            
            // 检查关键错误
            if (level >= LogLevel.Error)
            {
                OnCriticalError?.Invoke(level, message);
            }
        }
        
        /// <summary>
        /// 处理日志条目
        /// </summary>
        private void ProcessLogEntry(LogEntry logEntry)
        {
            if (asyncLogging)
            {
                // 异步处理 - 添加到缓冲区
                lock (logBuffer)
                {
                    logBuffer.Enqueue(logEntry);
                    
                    // 防止缓冲区溢出
                    if (logBuffer.Count > logBufferSize)
                    {
                        logBuffer.Dequeue();
                    }
                }
            }
            else
            {
                // 同步处理 - 立即输出
                OutputLogEntry(logEntry);
            }
        }
        
        /// <summary>
        /// 输出日志条目
        /// </summary>
        private void OutputLogEntry(LogEntry logEntry)
        {
            foreach (var output in logOutputs)
            {
                try
                {
                    output.WriteLog(logEntry);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        
        /// <summary>
        /// 刷新日志缓冲区
        /// </summary>
        private void FlushLogs()
        {
            if (!asyncLogging || logBuffer.Count == 0) return;
            
            var logsToFlush = new List<LogEntry>();
            
            lock (logBuffer)
            {
                while (logBuffer.Count > 0)
                {
                    logsToFlush.Add(logBuffer.Dequeue());
                }
            }
            
            foreach (var logEntry in logsToFlush)
            {
                OutputLogEntry(logEntry);
            }
        }
        
        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics(LogEntry logEntry)
        {
            statistics.totalLogs++;
            
            switch (logEntry.level)
            {
                case LogLevel.Debug:
                    statistics.debugLogs++;
                    break;
                case LogLevel.Info:
                    statistics.infoLogs++;
                    break;
                case LogLevel.Warning:
                    statistics.warningLogs++;
                    break;
                case LogLevel.Error:
                    statistics.errorLogs++;
                    break;
                case LogLevel.Fatal:
                    statistics.fatalLogs++;
                    break;
            }
            
            if (!statistics.categoryStats.ContainsKey(logEntry.category))
            {
                statistics.categoryStats[logEntry.category] = 0;
            }
            statistics.categoryStats[logEntry.category]++;
        }
        
        /// <summary>
        /// 创建新的日志文件
        /// </summary>
        private void CreateNewLogFile()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                currentLogFile = Path.Combine(logDirectory, $"TripMeta_{timestamp}.log");
                
                logWriter?.Close();
                logWriter = new StreamWriter(currentLogFile, true, Encoding.UTF8);
                
                // 写入文件头
                logWriter.WriteLine($"=== TripMeta 日志文件 - {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                logWriter.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// 清理旧日志文件
        /// </summary>
        private void CleanupOldLogFiles()
        {
            try
            {
                var logFiles = Directory.GetFiles(logDirectory, "*.log");
                Array.Sort(logFiles, (x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));
                
                // 删除超出数量限制的文件
                for (int i = maxLogFiles; i < logFiles.Length; i++)
                {
                    File.Delete(logFiles[i]);
                }
                
                // 检查文件大小
                foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length > maxLogFileSize)
                    {
                        // 如果是当前文件，创建新文件
                        if (file == currentLogFile)
                        {
                            CreateNewLogFile();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// 设置日志上下文
        /// </summary>
        public void SetLogContext(string category, LogContext context)
        {
            logContexts[category] = context;
        }
        
        /// <summary>
        /// 移除日志上下文
        /// </summary>
        public void RemoveLogContext(string category)
        {
            logContexts.Remove(category);
        }
        
        /// <summary>
        /// 设置最小日志级别
        /// </summary>
        public void SetMinLogLevel(LogLevel level)
        {
            minLogLevel = level;
        }
        
        /// <summary>
        /// 获取日志历史
        /// </summary>
        public List<LogEntry> GetLogHistory(LogLevel minLevel = LogLevel.Debug, string category = "", int maxCount = 100)
        {
            var history = new List<LogEntry>();
            
            // 这里可以从文件或内存中读取历史日志
            // 简化实现，返回当前缓冲区的日志
            lock (logBuffer)
            {
                foreach (var entry in logBuffer)
                {
                    if (entry.level >= minLevel && 
                        (string.IsNullOrEmpty(category) || entry.category == category))
                    {
                        history.Add(entry);
                        
                        if (history.Count >= maxCount)
                            break;
                    }
                }
            }
            
            return history;
        }
        
        /// <summary>
        /// 导出日志
        /// </summary>
        public async Task<string> ExportLogs(DateTime? startTime = null, DateTime? endTime = null)
        {
            try
            {
                var exportPath = Path.Combine(logDirectory, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                
                using (var writer = new StreamWriter(exportPath, false, Encoding.UTF8))
                {
                    writer.WriteLine("=== TripMeta 日志导出 ===");
                    writer.WriteLine($"导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"时间范围: {startTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "开始"} - {endTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "结束"}");
                    writer.WriteLine("========================");
                    
                    // 读取所有日志文件
                    var logFiles = Directory.GetFiles(logDirectory, "*.log");
                    foreach (var file in logFiles)
                    {
                        if (file == exportPath) continue;
                        
                        var lines = await File.ReadAllLinesAsync(file);
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
                
                return exportPath;
            }
            catch (Exception ex)
            {
                LogError($"导出日志失败: {ex.Message}", "AdvancedLogger", ex);
                return null;
            }
        }
        
        // 便捷方法
        public void LogDebug(string message, string category = "", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Debug, message, category, null, metadata);
        }
        
        public void LogInfo(string message, string category = "", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Info, message, category, null, metadata);
        }
        
        public void LogWarning(string message, string category = "", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Warning, message, category, null, metadata);
        }
        
        public void LogError(string message, string category = "", Exception exception = null, Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Error, message, category, exception, metadata);
        }
        
        public void LogFatal(string message, string category = "", Exception exception = null, Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Fatal, message, category, exception, metadata);
        }
        
        public void Initialize()
        {
            if (!IsInitialized)
            {
                InitializeLogger();
            }
        }
        
        public void Cleanup()
        {
            FlushLogs();
            logWriter?.Close();
            logWriter?.Dispose();
            
            foreach (var output in logOutputs)
            {
                if (output is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                FlushLogs();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                FlushLogs();
            }
        }
    }
    
    /// <summary>
    /// 日志条目
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        public DateTime timestamp;
        public LogLevel level;
        public string message;
        public string category;
        public Exception exception;
        public Dictionary<string, object> metadata;
        public int threadId;
        public string stackTrace;
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
            sb.Append($"[{level}] ");
            
            if (!string.IsNullOrEmpty(category))
            {
                sb.Append($"[{category}] ");
            }
            
            sb.Append(message);
            
            if (exception != null)
            {
                sb.AppendLine();
                sb.Append($"Exception: {exception}");
            }
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
    
    /// <summary>
    /// 日志上下文
    /// </summary>
    [Serializable]
    public class LogContext
    {
        public string userId;
        public string sessionId;
        public string sceneId;
        public Dictionary<string, object> customData;
        
        public LogContext()
        {
            customData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// 日志统计信息
    /// </summary>
    [Serializable]
    public class LogStatistics
    {
        public int totalLogs;
        public int debugLogs;
        public int infoLogs;
        public int warningLogs;
        public int errorLogs;
        public int fatalLogs;
        public Dictionary<string, int> categoryStats;
        
        public LogStatistics()
        {
            categoryStats = new Dictionary<string, int>();
        }
    }
}