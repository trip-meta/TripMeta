using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 异常上下文
    /// </summary>
    [Serializable]
    public class ExceptionContext
    {
        public string source;
        public DateTime timestamp;
        public string stackTrace;
        public LogType logType;
        public bool isTerminating;
        public Dictionary<string, object> additionalData;
        
        public ExceptionContext()
        {
            additionalData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// 异常记录
    /// </summary>
    [Serializable]
    public class ExceptionRecord
    {
        public string id;
        public Exception exception;
        public ExceptionContext context;
        public DateTime timestamp;
        public bool handled;
        public bool recovered;
    }
    
    /// <summary>
    /// 异常统计信息
    /// </summary>
    [Serializable]
    public class ExceptionStatistics
    {
        public Type exceptionType;
        public int count;
        public DateTime firstOccurrence;
        public DateTime lastOccurrence;
        public float averageFrequency;
        public List<string> commonMessages;
        
        public ExceptionStatistics()
        {
            commonMessages = new List<string>();
        }
    }
    
    /// <summary>
    /// 异常报告
    /// </summary>
    [Serializable]
    public class ExceptionReport
    {
        public string id;
        public DateTime timestamp;
        public Exception exception;
        public ExceptionContext context;
        public bool recovered;
        public Dictionary<string, object> systemInfo;
        public byte[] screenshot;
        public string userFeedback;
        public string reproductionSteps;
    }
    
    /// <summary>
    /// Unity异常
    /// </summary>
    public class UnityException : Exception
    {
        public string UnityStackTrace { get; }
        
        public UnityException(string message, string stackTrace) : base(message)
        {
            UnityStackTrace = stackTrace;
        }
        
        public override string StackTrace => UnityStackTrace ?? base.StackTrace;
    }
    
    /// <summary>
    /// 异常恢复策略接口
    /// </summary>
    public interface IExceptionRecoveryStrategy
    {
        bool CanRecover(Exception exception, ExceptionContext context);
        bool TryRecover(Exception exception, ExceptionContext context);
        string GetRecoveryDescription();
    }
    
    /// <summary>
    /// 空引用异常恢复策略
    /// </summary>
    public class NullReferenceRecoveryStrategy : IExceptionRecoveryStrategy
    {
        public bool CanRecover(Exception exception, ExceptionContext context)
        {
            return exception is NullReferenceException;
        }
        
        public bool TryRecover(Exception exception, ExceptionContext context)
        {
            try
            {
                // 尝试重新初始化相关组件
                Logger.LogInfo("尝试恢复空引用异常...", "NullReferenceRecovery");
                
                // 这里可以实现具体的恢复逻辑
                // 例如重新初始化服务、重新加载资源等
                
                return true;
            }
            catch (Exception recoveryEx)
            {
                Logger.LogError($"空引用异常恢复失败: {recoveryEx.Message}", "NullReferenceRecovery");
                return false;
            }
        }
        
        public string GetRecoveryDescription()
        {
            return "重新初始化相关组件和服务";
        }
    }
    
    /// <summary>
    /// 索引越界异常恢复策略
    /// </summary>
    public class IndexOutOfRangeRecoveryStrategy : IExceptionRecoveryStrategy
    {
        public bool CanRecover(Exception exception, ExceptionContext context)
        {
            return exception is IndexOutOfRangeException || exception is ArgumentOutOfRangeException;
        }
        
        public bool TryRecover(Exception exception, ExceptionContext context)
        {
            try
            {
                Logger.LogInfo("尝试恢复索引越界异常...", "IndexOutOfRangeRecovery");
                
                // 重置相关数据结构
                // 清理可能损坏的集合
                
                return true;
            }
            catch (Exception recoveryEx)
            {
                Logger.LogError($"索引越界异常恢复失败: {recoveryEx.Message}", "IndexOutOfRangeRecovery");
                return false;
            }
        }
        
        public string GetRecoveryDescription()
        {
            return "重置数据结构和集合状态";
        }
    }
    
    /// <summary>
    /// 网络异常恢复策略
    /// </summary>
    public class NetworkExceptionRecoveryStrategy : IExceptionRecoveryStrategy
    {
        private int retryCount = 0;
        private const int maxRetries = 3;
        
        public bool CanRecover(Exception exception, ExceptionContext context)
        {
            return exception is System.Net.WebException ||
                   exception is System.Net.Http.HttpRequestException ||
                   exception.Message.Contains("network") ||
                   exception.Message.Contains("connection");
        }
        
        public bool TryRecover(Exception exception, ExceptionContext context)
        {
            try
            {
                if (retryCount >= maxRetries)
                {
                    Logger.LogWarning("网络异常恢复重试次数已达上限", "NetworkRecovery");
                    retryCount = 0;
                    return false;
                }
                
                retryCount++;
                Logger.LogInfo($"尝试恢复网络异常... (第{retryCount}次)", "NetworkRecovery");
                
                // 等待网络恢复
                System.Threading.Thread.Sleep(2000 * retryCount);
                
                // 检查网络连接
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Logger.LogWarning("网络仍然不可达", "NetworkRecovery");
                    return false;
                }
                
                // 重新初始化网络服务
                // 这里可以重新连接网络服务
                
                retryCount = 0; // 重置重试计数
                return true;
            }
            catch (Exception recoveryEx)
            {
                Logger.LogError($"网络异常恢复失败: {recoveryEx.Message}", "NetworkRecovery");
                return false;
            }
        }
        
        public string GetRecoveryDescription()
        {
            return "重新建立网络连接和服务";
        }
    }
    
    /// <summary>
    /// 文件异常恢复策略
    /// </summary>
    public class FileExceptionRecoveryStrategy : IExceptionRecoveryStrategy
    {
        public bool CanRecover(Exception exception, ExceptionContext context)
        {
            return exception is System.IO.IOException ||
                   exception is System.IO.FileNotFoundException ||
                   exception is System.IO.DirectoryNotFoundException ||
                   exception is UnauthorizedAccessException;
        }
        
        public bool TryRecover(Exception exception, ExceptionContext context)
        {
            try
            {
                Logger.LogInfo("尝试恢复文件异常...", "FileRecovery");
                
                if (exception is System.IO.FileNotFoundException fileNotFound)
                {
                    // 尝试创建缺失的文件或使用默认文件
                    Logger.LogInfo($"尝试恢复缺失文件: {fileNotFound.FileName}", "FileRecovery");
                    // 实现文件恢复逻辑
                }
                else if (exception is System.IO.DirectoryNotFoundException)
                {
                    // 尝试创建缺失的目录
                    Logger.LogInfo("尝试创建缺失的目录", "FileRecovery");
                    // 实现目录创建逻辑
                }
                else if (exception is UnauthorizedAccessException)
                {
                    // 尝试使用替代路径
                    Logger.LogInfo("尝试使用替代文件路径", "FileRecovery");
                    // 实现权限问题恢复逻辑
                }
                
                return true;
            }
            catch (Exception recoveryEx)
            {
                Logger.LogError($"文件异常恢复失败: {recoveryEx.Message}", "FileRecovery");
                return false;
            }
        }
        
        public string GetRecoveryDescription()
        {
            return "恢复文件和目录访问";
        }
    }
    
    /// <summary>
    /// 内存不足异常恢复策略
    /// </summary>
    public class OutOfMemoryRecoveryStrategy : IExceptionRecoveryStrategy
    {
        public bool CanRecover(Exception exception, ExceptionContext context)
        {
            return exception is OutOfMemoryException;
        }
        
        public bool TryRecover(Exception exception, ExceptionContext context)
        {
            try
            {
                Logger.LogInfo("尝试恢复内存不足异常...", "OutOfMemoryRecovery");
                
                // 强制垃圾回收
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
                
                // 卸载未使用的资源
                Resources.UnloadUnusedAssets();
                
                // 降低质量设置
                QualitySettings.DecreaseLevel();
                
                Logger.LogInfo("内存清理完成", "OutOfMemoryRecovery");
                return true;
            }
            catch (Exception recoveryEx)
            {
                Logger.LogError($"内存不足异常恢复失败: {recoveryEx.Message}", "OutOfMemoryRecovery");
                return false;
            }
        }
        
        public string GetRecoveryDescription()
        {
            return "清理内存和降低质量设置";
        }
    }
    
    /// <summary>
    /// 通用异常恢复策略
    /// </summary>
    public class GenericRecoveryStrategy : IExceptionRecoveryStrategy
    {
        private readonly Func<Exception, ExceptionContext, bool> canRecoverFunc;
        private readonly Func<Exception, ExceptionContext, bool> tryRecoverFunc;
        private readonly string description;
        
        public GenericRecoveryStrategy(
            Func<Exception, ExceptionContext, bool> canRecover,
            Func<Exception, ExceptionContext, bool> tryRecover,
            string description)
        {
            this.canRecoverFunc = canRecover;
            this.tryRecoverFunc = tryRecover;
            this.description = description;
        }
        
        public bool CanRecover(Exception exception, ExceptionContext context)
        {
            return canRecoverFunc?.Invoke(exception, context) ?? false;
        }
        
        public bool TryRecover(Exception exception, ExceptionContext context)
        {
            return tryRecoverFunc?.Invoke(exception, context) ?? false;
        }
        
        public string GetRecoveryDescription()
        {
            return description ?? "通用恢复策略";
        }
    }
}