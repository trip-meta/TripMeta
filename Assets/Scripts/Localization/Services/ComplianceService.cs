using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Localization
{
    /// <summary>
    /// 合规服务
    /// 检查内容是否符合区域法规和文化要求
    /// </summary>
    public class ComplianceService
    {
        /// <summary>
        /// 检查体验合规性
        /// </summary>
        public async Task<ComplianceReport> CheckExperience(string experienceId, RegionType region, string[] restrictions)
        {
            await Task.Delay(200);

            // 模拟合规检查
            var violations = new System.Collections.Generic.List<string>();
            var recommendations = new System.Collections.Generic.List<string>();

            // 检查区域特定限制
            foreach (var restriction in restrictions)
            {
                if (Random.value > 0.9f) // 10% 概率发现问题
                {
                    violations.Add($"Potential issue with: {restriction}");
                    recommendations.Add($"Review content for {restriction}");
                }
            }

            return new ComplianceReport
            {
                experienceId = experienceId,
                isCompliant = violations.Count == 0,
                violations = violations.ToArray(),
                recommendations = recommendations.ToArray(),
                checkedAt = System.DateTime.Now
            };
        }

        /// <summary>
        /// 获取区域合规建议
        /// </summary>
        public CulturalAdvice GetCulturalAdvice(string region)
        {
            return new CulturalAdvice
            {
                region = region,
                recommendations = new[] { "Be respectful", "Follow local customs" },
                avoidTopics = new[] { "Sensitive politics", "Religious conflicts" }
            };
        }
    }

    /// <summary>
    /// 文化建议
    /// </summary>
    public class CulturalAdvice
    {
        public string region;
        public string[] recommendations;
        public string[] avoidTopics;
    }
}
