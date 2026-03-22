using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Enterprise.Services
{
    /// <summary>
    /// 审计日志服务
    /// 持久化存储和查询审计日志
    /// </summary>
    public class AuditLogService
    {
        private readonly int retentionDays;
        private readonly List<AuditLogEntry> persistedLogs = new List<AuditLogEntry>();
        private int totalLogsWritten = 0;

        public int TotalLogsWritten => totalLogsWritten;
        public int PersistedCount => persistedLogs.Count;

        public AuditLogService(int retentionDays = 365)
        {
            this.retentionDays = retentionDays;
        }

        /// <summary>
        /// 批量保存审计日志
        /// </summary>
        public async Task SaveAuditLogs(List<AuditLogEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            await Task.Delay(10);

            persistedLogs.AddRange(entries);
            totalLogsWritten += entries.Count;

            // 清理过期日志
            PurgeExpiredLogs();

            Debug.Log($"[AuditLogService] Saved {entries.Count} audit log entries. Total: {totalLogsWritten}");
        }

        /// <summary>
        /// 查询审计日志
        /// </summary>
        public async Task<AuditLogEntry[]> GetLogs(
            DateTime startDate,
            DateTime endDate,
            string action = null,
            string userId = null,
            LogSeverity? minSeverity = null,
            int maxResults = 1000)
        {
            await Task.Delay(10);

            var query = persistedLogs.Where(e =>
                e.timestamp >= startDate && e.timestamp <= endDate);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(e => e.action.Contains(action, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(e => e.userId == userId);

            if (minSeverity.HasValue)
                query = query.Where(e => e.severity >= minSeverity.Value);

            return query
                .OrderByDescending(e => e.timestamp)
                .Take(maxResults)
                .ToArray();
        }

        /// <summary>
        /// 获取安全事件（高危以上）
        /// </summary>
        public async Task<AuditLogEntry[]> GetSecurityEvents(DateTime since)
        {
            return await GetLogs(
                since,
                DateTime.Now,
                minSeverity: LogSeverity.High
            );
        }

        /// <summary>
        /// 按操作类型统计
        /// </summary>
        public async Task<Dictionary<string, int>> GetActionStats(DateTime startDate, DateTime endDate)
        {
            await Task.Delay(5);

            return persistedLogs
                .Where(e => e.timestamp >= startDate && e.timestamp <= endDate)
                .GroupBy(e => e.action)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// 导出审计报告
        /// </summary>
        public async Task<AuditReport> GenerateReport(DateTime startDate, DateTime endDate)
        {
            await Task.Delay(20);

            var logs = persistedLogs
                .Where(e => e.timestamp >= startDate && e.timestamp <= endDate)
                .ToList();

            var criticalEvents = logs.Where(e => e.severity == LogSeverity.Critical).ToList();
            var highEvents = logs.Where(e => e.severity == LogSeverity.High).ToList();

            return new AuditReport
            {
                reportId = Guid.NewGuid().ToString(),
                generatedAt = DateTime.Now,
                startDate = startDate,
                endDate = endDate,
                totalEvents = logs.Count,
                criticalEventCount = criticalEvents.Count,
                highEventCount = highEvents.Count,
                topActions = logs.GroupBy(e => e.action)
                                 .OrderByDescending(g => g.Count())
                                 .Take(10)
                                 .Select(g => new ActionSummary { action = g.Key, count = g.Count() })
                                 .ToArray(),
                criticalEvents = criticalEvents.Take(20).ToArray()
            };
        }

        /// <summary>
        /// 清理过期日志
        /// </summary>
        private void PurgeExpiredLogs()
        {
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            int before = persistedLogs.Count;
            persistedLogs.RemoveAll(e => e.timestamp < cutoffDate);
            int removed = before - persistedLogs.Count;
            if (removed > 0)
            {
                Debug.Log($"[AuditLogService] Purged {removed} expired log entries");
            }
        }
    }

    /// <summary>
    /// 审计报告
    /// </summary>
    public class AuditReport
    {
        public string reportId;
        public DateTime generatedAt;
        public DateTime startDate;
        public DateTime endDate;
        public int totalEvents;
        public int criticalEventCount;
        public int highEventCount;
        public ActionSummary[] topActions;
        public AuditLogEntry[] criticalEvents;
    }

    /// <summary>
    /// 操作摘要
    /// </summary>
    public class ActionSummary
    {
        public string action;
        public int count;
    }
}
