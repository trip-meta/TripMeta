using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 全局异常处理器 - 统一处理应用程序异常
    /// </summary>
    public class GlobalExceptionHandler : MonoBehaviour, IService
    {
        [Header("异常处理配置")]
        [SerializeField] private bool enableGlobalHandling = true;
        [SerializeField] private bool enableRecovery = true;
        [SerializeField] private bool enableReporting = true;
        [SerializeField] private bool showErrorDialog = true;
        
        [Header("恢复策略")]
        [SerializeField] private int maxRecoveryAttempts = 3;
        [SerializeField] private float recoveryDelay = 1f;
        [SerializeField] private bool enableAutoRestart = false;
        
        [Header("错误报告")]
        [SerializeField] private string errorReportEndpoint = "";
        [SerializeField] private bool includeSystemInfo = true;
        [SerializeField] private bool includeScreenshot = false;
        
        // 异常统计
        private Dictionary<Type, ExceptionStatistics> exceptionStats;
        private List<ExceptionRecord> recentExceptions;
        private int totalExceptions = 0;
        
        // 恢复机制
        private Dictionary<Type, IExceptionRecoveryStrategy> recoveryStrategies;
        private Dictionary<string, int> recoveryAttempts;
        
        // 事件
        public event Action<Exception, ExceptionContext> OnExceptionOccurred;
        public event Action<Exception, bool> OnExceptionRecovered;
        public event Action<ExceptionReport> OnExceptionReported;
        
        public bool IsInitialized { get; private set; }
        public ExceptionStatistics GetStatistics(Type exceptionType) => exceptionStats.GetValueOrDefault(exceptionType);
        public int TotalExceptions => totalExceptions;
        
        private static GlobalExceptionHandler instance;
        public static GlobalExceptionHandler Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeExceptionHandler();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            ServiceLocator.RegisterService<GlobalExceptionHandler>(this);
        }
        
        /// <summary>
        /// 初始化异常处理器
        /// </summary>
        private void InitializeExceptionHandler()
        {
            try
            {
                // 初始化数据结构
                exceptionStats = new Dictionary<Type, ExceptionStatistics>();
                recentExceptions = new List<ExceptionRecord>();
                recoveryStrategies = new Dictionary<Type, IExceptionRecoveryStrategy>();
                recoveryAttempts = new Dictionary<string, int>();
                
                // 注册默认恢复策略
                RegisterDefaultRecoveryStrategies();
                
                // 设置Unity异常处理
                if (enableGlobalHandling)
                {
                    Application.logMessageReceived += HandleUnityLogMessage;
                    AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
                }
                
                IsInitialized = true;
                
                Logger.LogInfo("全局异常处理器初始化完成", "GlobalExceptionHandler");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// 注册默认恢复策略
        /// </summary>
        private void RegisterDefaultRecoveryStrategies()
        {
            // 空引用异常恢复
            RegisterRecoveryStrategy<NullReferenceException>(new NullReferenceRecoveryStrategy());
            
            // 索引越界异常恢复
            RegisterRecoveryStrategy<IndexOutOfRangeException>(new IndexOutOfRangeRecoveryStrategy());
            
            // 网络异常恢复
            RegisterRecoveryStrategy<System.Net.WebException>(new NetworkExceptionRecoveryStrategy());
            
            // 文件异常恢复
            RegisterRecoveryStrategy<System.IO.IOException>(new FileExceptionRecoveryStrategy());
            
            // 内存不足异常恢复
            RegisterRecoveryStrategy<OutOfMemoryException>(new OutOfMemoryRecoveryStrategy());
        }
        
        /// <summary>
        /// 处理Unity日志消息
        /// </summary>
        private void HandleUnityLogMessage(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                var exception = new UnityException(logString, stackTrace);
                var context = new ExceptionContext
                {
                    source = "Unity",
                    timestamp = DateTime.Now,
                    stackTrace = stackTrace,
                    logType = type
                };
                
                HandleException(exception, context);
            }
        }
        
        /// <summary>
        /// 处理未处理的异常
        /// </summary>
        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                var context = new ExceptionContext
                {
                    source = "AppDomain",
                    timestamp = DateTime.Now,
                    isTerminating = e.IsTerminating
                };
                
                HandleException(exception, context);
            }
        }
        
        /// <summary>
        /// 处理异常
        /// </summary>
        public void HandleException(Exception exception, ExceptionContext context = null)
        {
            if (!IsInitialized) return;
            
            try
            {
                context = context ?? new ExceptionContext
                {
                    source = "Manual",
                    timestamp = DateTime.Now
                };
                
                // 更新统计信息
                UpdateExceptionStatistics(exception);
                
                // 记录异常
                RecordException(exception, context);
                
                // 记录日志
                Logger.LogError($"异常发生: {exception.Message}", "GlobalExceptionHandler", exception);
                
                // 触发事件
                OnExceptionOccurred?.Invoke(exception, context);
                
                // 尝试恢复
                bool recovered = false;
                if (enableRecovery)
                {
                    recovered = TryRecoverFromException(exception, context);
                }
                
                // 生成错误报告
                if (enableReporting)
                {
                    GenerateErrorReport(exception, context, recovered);
                }
                
                // 显示错误对话框
                if (showErrorDialog && !recovered)
                {
                    ShowErrorDialog(exception, context);
                }
                
                // 检查是否需要重启
                if (enableAutoRestart && !recovered && ShouldRestart(exception))
                {
                    RestartApplication();
                }
            }
            catch (Exception handlerException)
            {
                // 异常处理器本身出现异常
                Debug.LogException(handlerException);
            }
        }
        
        /// <summary>
        /// 更新异常统计信息
        /// </summary>
        private void UpdateExceptionStatistics(Exception exception)
        {
            totalExceptions++;
            
            var exceptionType = exception.GetType();
            if (!exceptionStats.ContainsKey(exceptionType))
            {
                exceptionStats[exceptionType] = new ExceptionStatistics
                {
                    exceptionType = exceptionType,
                    count = 0,
                    firstOccurrence = DateTime.Now,
                    lastOccurrence = DateTime.Now
                };
            }
            
            var stats = exceptionStats[exceptionType];
            stats.count++;
            stats.lastOccurrence = DateTime.Now;
        }
        
        /// <summary>
        /// 记录异常
        /// </summary>
        private void RecordException(Exception exception, ExceptionContext context)
        {
            var record = new ExceptionRecord
            {
                exception = exception,
                context = context,
                timestamp = DateTime.Now,
                id = Guid.NewGuid().ToString()
            };
            
            recentExceptions.Add(record);
            
            // 保持最近100个异常记录
            if (recentExceptions.Count > 100)
            {
                recentExceptions.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 尝试从异常中恢复
        /// </summary>
        private bool TryRecoverFromException(Exception exception, ExceptionContext context)
        {
            var exceptionType = exception.GetType();
            var recoveryKey = $"{exceptionType.Name}_{context.source}";
            
            // 检查恢复尝试次数
            if (!recoveryAttempts.ContainsKey(recoveryKey))
            {
                recoveryAttempts[recoveryKey] = 0;
            }
            
            if (recoveryAttempts[recoveryKey] >= maxRecoveryAttempts)
            {
                Logger.LogWarning($"异常恢复尝试次数已达上限: {exceptionType.Name}", "GlobalExceptionHandler");
                return false;
            }
            
            // 查找恢复策略
            IExceptionRecoveryStrategy strategy = null;
            
            // 精确匹配
            if (recoveryStrategies.ContainsKey(exceptionType))
            {
                strategy = recoveryStrategies[exceptionType];
            }
            else
            {
                // 查找基类匹配
                foreach (var kvp in recoveryStrategies)
                {
                    if (kvp.Key.IsAssignableFrom(exceptionType))
                    {
                        strategy = kvp.Value;
                        break;
                    }
                }
            }
            
            if (strategy == null)
            {
                Logger.LogWarning($"未找到异常恢复策略: {exceptionType.Name}", "GlobalExceptionHandler");
                return false;
            }
            
            try
            {
                recoveryAttempts[recoveryKey]++;
                
                Logger.LogInfo($"尝试恢复异常: {exceptionType.Name} (第{recoveryAttempts[recoveryKey]}次)", "GlobalExceptionHandler");
                
                // 延迟恢复
                if (recoveryDelay > 0)
                {
                    System.Threading.Thread.Sleep((int)(recoveryDelay * 1000));
                }
                
                bool recovered = strategy.TryRecover(exception, context);
                
                OnExceptionRecovered?.Invoke(exception, recovered);
                
                if (recovered)
                {
                    Logger.LogInfo($"异常恢复成功: {exceptionType.Name}", "GlobalExceptionHandler");
                    // 重置恢复计数
                    recoveryAttempts[recoveryKey] = 0;
                }
                else
                {
                    Logger.LogWarning($"异常恢复失败: {exceptionType.Name}", "GlobalExceptionHandler");
                }
                
                return recovered;
            }
            catch (Exception recoveryException)
            {
                Logger.LogError($"异常恢复过程中发生错误: {recoveryException.Message}", "GlobalExceptionHandler", recoveryException);
                return false;
            }
        }
        
        /// <summary>
        /// 生成错误报告
        /// </summary>
        private void GenerateErrorReport(Exception exception, ExceptionContext context, bool recovered)
        {
            try
            {
                var report = new ExceptionReport
                {
                    id = Guid.NewGuid().ToString(),
                    timestamp = DateTime.Now,
                    exception = exception,
                    context = context,
                    recovered = recovered,
                    systemInfo = includeSystemInfo ? CollectSystemInfo() : null,
                    screenshot = includeScreenshot ? CaptureScreenshot() : null
                };
                
                OnExceptionReported?.Invoke(report);
                
                // 保存本地报告
                SaveErrorReport(report);
                
                // 发送远程报告
                if (!string.IsNullOrEmpty(errorReportEndpoint))
                {
                    SendErrorReport(report);
                }
            }
            catch (Exception reportException)
            {
                Logger.LogError($"生成错误报告失败: {reportException.Message}", "GlobalExceptionHandler", reportException);
            }
        }
        
        /// <summary>
        /// 收集系统信息
        /// </summary>
        private Dictionary<string, object> CollectSystemInfo()
        {
            return new Dictionary<string, object>
            {
                ["Platform"] = Application.platform.ToString(),
                ["UnityVersion"] = Application.unityVersion,
                ["ApplicationVersion"] = Application.version,
                ["DeviceModel"] = SystemInfo.deviceModel,
                ["OperatingSystem"] = SystemInfo.operatingSystem,
                ["ProcessorType"] = SystemInfo.processorType,
                ["SystemMemorySize"] = SystemInfo.systemMemorySize,
                ["GraphicsDeviceName"] = SystemInfo.graphicsDeviceName,
                ["GraphicsMemorySize"] = SystemInfo.graphicsMemorySize,
                ["Timestamp"] = DateTime.Now,
                ["SessionId"] = SystemInfo.deviceUniqueIdentifier
            };
        }
        
        /// <summary>
        /// 捕获屏幕截图
        /// </summary>
        private byte[] CaptureScreenshot()
        {
            try
            {
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                var bytes = texture.EncodeToPNG();
                DestroyImmediate(texture);
                return bytes;
            }
            catch (Exception ex)
            {
                Logger.LogError($"捕获屏幕截图失败: {ex.Message}", "GlobalExceptionHandler");
                return null;
            }
        }
        
        /// <summary>
        /// 保存错误报告
        /// </summary>
        private void SaveErrorReport(ExceptionReport report)
        {
            try
            {
                var reportDir = System.IO.Path.Combine(Application.persistentDataPath, "ErrorReports");
                if (!System.IO.Directory.Exists(reportDir))
                {
                    System.IO.Directory.CreateDirectory(reportDir);
                }
                
                var reportFile = System.IO.Path.Combine(reportDir, $"ErrorReport_{report.id}.json");
                var json = JsonUtility.ToJson(report, true);
                System.IO.File.WriteAllText(reportFile, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"保存错误报告失败: {ex.Message}", "GlobalExceptionHandler");
            }
        }
        
        /// <summary>
        /// 发送错误报告
        /// </summary>
        private async void SendErrorReport(ExceptionReport report)
        {
            try
            {
                // 这里实现发送错误报告到远程服务器的逻辑
                Logger.LogInfo($"错误报告已发送: {report.id}", "GlobalExceptionHandler");
            }
            catch (Exception ex)
            {
                Logger.LogError($"发送错误报告失败: {ex.Message}", "GlobalExceptionHandler");
            }
        }
        
        /// <summary>
        /// 显示错误对话框
        /// </summary>
        private void ShowErrorDialog(Exception exception, ExceptionContext context)
        {
            var message = $"应用程序遇到错误:\n\n{exception.Message}\n\n是否继续运行?";
            
            // 这里可以显示自定义的错误对话框
            // 简化实现使用Unity的对话框
            Logger.LogError($"错误对话框: {message}", "GlobalExceptionHandler");
        }
        
        /// <summary>
        /// 判断是否应该重启应用
        /// </summary>
        private bool ShouldRestart(Exception exception)
        {
            // 严重异常需要重启
            return exception is OutOfMemoryException ||
                   exception is StackOverflowException ||
                   exception is AccessViolationException;
        }
        
        /// <summary>
        /// 重启应用程序
        /// </summary>
        private void RestartApplication()
        {
            Logger.LogInfo("准备重启应用程序...", "GlobalExceptionHandler");
            
            // 保存重要数据
            // ...
            
            // 重启应用
            Application.Quit();
            
            // 在某些平台上可能需要特殊处理
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        
        /// <summary>
        /// 注册恢复策略
        /// </summary>
        public void RegisterRecoveryStrategy<T>(IExceptionRecoveryStrategy strategy) where T : Exception
        {
            recoveryStrategies[typeof(T)] = strategy;
        }
        
        /// <summary>
        /// 获取异常历史
        /// </summary>
        public List<ExceptionRecord> GetExceptionHistory(int maxCount = 50)
        {
            var count = Math.Min(maxCount, recentExceptions.Count);
            return recentExceptions.GetRange(recentExceptions.Count - count, count);
        }
        
        /// <summary>
        /// 清除异常历史
        /// </summary>
        public void ClearExceptionHistory()
        {
            recentExceptions.Clear();
            recoveryAttempts.Clear();
        }
        
        public void Initialize()
        {
            if (!IsInitialized)
            {
                InitializeExceptionHandler();
            }
        }
        
        public void Cleanup()
        {
            if (enableGlobalHandling)
            {
                Application.logMessageReceived -= HandleUnityLogMessage;
                AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
    }
}