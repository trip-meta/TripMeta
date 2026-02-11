using System;
using System.IO;
using UnityEngine;

namespace TripMeta.Core.ErrorHandling
{
    /// <summary>
    /// 统一日志系统
    /// </summary>
    public static class Logger
    {
        private static string logFilePath;
        private static bool isInitialized = false;
        
        static Logger()
        {
            Initialize();
        }
        
        private static void Initialize()
        {
            if (isInitialized) return;
            
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFilePath = Path.Combine(logDirectory, $"TripMeta_{timestamp}.log");
            
            isInitialized = true;
            
            // 记录启动信息
            LogInfo("=== TripMeta VR Application Started ===");
            LogInfo($"Unity Version: {Application.unityVersion}");
            LogInfo($"Platform: {Application.platform}");
            LogInfo($"Device Model: {SystemInfo.deviceModel}");
        }
        
        public static void LogInfo(string message, string category = "INFO")
        {
            string logEntry = FormatLogEntry(LogLevel.Info, category, message);
            Debug.Log(logEntry);
            WriteToFile(logEntry);
        }
        
        public static void LogWarning(string message, string category = "WARNING")
        {
            string logEntry = FormatLogEntry(LogLevel.Warning, category, message);
            Debug.LogWarning(logEntry);
            WriteToFile(logEntry);
        }
        
        public static void LogError(string message, string category = "ERROR")
        {
            string logEntry = FormatLogEntry(LogLevel.Error, category, message);
            Debug.LogError(logEntry);
            WriteToFile(logEntry);
        }
        
        public static void LogException(Exception exception, string context = "")
        {
            string message = string.IsNullOrEmpty(context) 
                ? exception.ToString() 
                : $"{context}: {exception}";
            
            string logEntry = FormatLogEntry(LogLevel.Error, "EXCEPTION", message);
            Debug.LogError(logEntry);
            WriteToFile(logEntry);
        }
        
        public static void LogPerformance(string operation, float duration)
        {
            string message = $"{operation} completed in {duration:F2}ms";
            string logEntry = FormatLogEntry(LogLevel.Info, "PERFORMANCE", message);
            Debug.Log(logEntry);
            WriteToFile(logEntry);
        }
        
        private static string FormatLogEntry(LogLevel level, string category, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"[{timestamp}] [{level}] [{category}] {message}";
        }
        
        private static void WriteToFile(string logEntry)
        {
            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }
        
        public static string GetLogFilePath()
        {
            return logFilePath;
        }
    }
    
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}