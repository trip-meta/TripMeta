using System.Threading.Tasks;

namespace TripMeta.AI
{
    /// <summary>
    /// 边缘AI模型接口
    /// </summary>
    public interface IEdgeAIModel
    {
        string ModelId { get; }
        string ModelName { get; }
        bool IsLoaded { get; }
        float LastInferenceLatency { get; }

        Task LoadAsync(EdgeAIModelConfig config);
        Task UnloadAsync();
        Task<T> RunInferenceAsync<T>(object inputData) where T : class;
    }

    /// <summary>
    /// 模型性能指标
    /// </summary>
    public class ModelPerformanceMetrics
    {
        public string ModelId { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailureCount { get; private set; }
        public float TotalLatency { get; private set; }
        public float MinLatency { get; private set; } = float.MaxValue;
        public float MaxLatency { get; private set; }
        public float AverageLatency => SuccessCount > 0 ? TotalLatency / SuccessCount : 0;

        public ModelPerformanceMetrics(string modelId)
        {
            ModelId = modelId;
        }

        public void RecordInference(float latency, bool success)
        {
            if (success)
            {
                SuccessCount++;
                TotalLatency += latency;
                MinLatency = UnityEngine.Mathf.Min(MinLatency, latency);
                MaxLatency = UnityEngine.Mathf.Max(MaxLatency, latency);
            }
            else
            {
                FailureCount++;
            }
        }

        public ModelPerformanceReport GetReport()
        {
            return new ModelPerformanceReport
            {
                ModelId = ModelId,
                SuccessCount = SuccessCount,
                FailureCount = FailureCount,
                AverageLatency = AverageLatency,
                MinLatency = MinLatency == float.MaxValue ? 0 : MinLatency,
                MaxLatency = MaxLatency,
                SuccessRate = SuccessCount + FailureCount > 0 ? (float)SuccessCount / (SuccessCount + FailureCount) : 0
            };
        }
    }

    /// <summary>
    /// 模型性能报告
    /// </summary>
    public class ModelPerformanceReport
    {
        public string ModelId;
        public int SuccessCount;
        public int FailureCount;
        public float AverageLatency;
        public float MinLatency;
        public float MaxLatency;
        public float SuccessRate;

        public override string ToString()
        {
            return $"{ModelId}: 成功{SuccessCount}, 失败{FailureCount}, 平均延迟{AverageLatency:F1}ms, 成功率{SuccessRate:P1}";
        }
    }
}
