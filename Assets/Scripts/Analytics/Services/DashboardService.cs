using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Analytics
{
    /// <summary>
    /// 仪表板服务
    /// 处理商业智能仪表板数据聚合和报告
    /// </summary>
    public class DashboardService
    {
        /// <summary>
        /// 获取用户留存报告
        /// </summary>
        public async Task<RetentionReport> GetRetentionReport(int days = 30)
        {
            await Task.Delay(300);

            // 模拟留存数据
            return new RetentionReport
            {
                period = days,
                cohorts = new List<RetentionCohort>
                {
                    new RetentionCohort
                    {
                        cohortDate = System.DateTime.Now.AddDays(-30),
                        initialUsers = 1000,
                        retentionByDay = new float[] { 100, 45, 38, 35, 32, 30, 28, 27, 26, 25 }
                    }
                }
            };
        }

        /// <summary>
        /// 获取转化漏斗报告
        /// </summary>
        public async Task<FunnelReport> GetFunnelReport(string funnelName)
        {
            await Task.Delay(300);

            return new FunnelReport
            {
                funnelName = funnelName,
                steps = new List<FunnelStep>
                {
                    new FunnelStep { stepName = "Visit", users = 10000, conversionRate = 100 },
                    new FunnelStep { stepName = "Sign Up", users = 3500, conversionRate = 35 },
                    new FunnelStep { stepName = "Trial", users = 1200, conversionRate = 34.3f },
                    new FunnelStep { stepName = "Subscribe", users = 400, conversionRate = 33.3f }
                }
            };
        }

        /// <summary>
        /// 获取收入报告
        /// </summary>
        public async Task<RevenueReport> GetRevenueReport(int months = 12)
        {
            await Task.Delay(300);

            return new RevenueReport
            {
                totalRevenue = 1250000m,
                mrr = 98000m,
                arr = 1176000m,
                growthRate = 15.5f,
                monthlyData = new List<MonthlyRevenue>
                {
                    new MonthlyRevenue { month = "2024-01", revenue = 85000m, newCustomers = 120 },
                    new MonthlyRevenue { month = "2024-02", revenue = 92000m, newCustomers = 145 },
                    new MonthlyRevenue { month = "2024-03", revenue = 98000m, newCustomers = 160 }
                }
            };
        }
    }

    /// <summary>
    /// 留存报告
    /// </summary>
    public class RetentionReport
    {
        public int period;
        public List<RetentionCohort> cohorts;
    }

    /// <summary>
    /// 留存队列
    /// </summary>
    public class RetentionCohort
    {
        public System.DateTime cohortDate;
        public int initialUsers;
        public float[] retentionByDay;
    }

    /// <summary>
    /// 漏斗报告
    /// </summary>
    public class FunnelReport
    {
        public string funnelName;
        public List<FunnelStep> steps;
    }

    /// <summary>
    /// 漏斗步骤
    /// </summary>
    public class FunnelStep
    {
        public string stepName;
        public int users;
        public float conversionRate;
    }

    /// <summary>
    /// 收入报告
    /// </summary>
    public class RevenueReport
    {
        public decimal totalRevenue;
        public decimal mrr;
        public decimal arr;
        public float growthRate;
        public List<MonthlyRevenue> monthlyData;
    }

    /// <summary>
    /// 月度收入
    /// </summary>
    public class MonthlyRevenue
    {
        public string month;
        public decimal revenue;
        public int newCustomers;
    }
}
