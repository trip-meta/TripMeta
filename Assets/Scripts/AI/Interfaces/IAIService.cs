using System;
using System.Threading.Tasks;

namespace TripMeta.AI
{
    /// <summary>
    /// AI服务基础接口
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初始化服务
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// 检查服务健康状态
        /// </summary>
        Task<bool> CheckHealthAsync();
        
        /// <summary>
        /// 重新初始化服务
        /// </summary>
        Task ReinitializeAsync();
        
        /// <summary>
        /// 暂停服务
        /// </summary>
        void Pause();
        
        /// <summary>
        /// 恢复服务
        /// </summary>
        void Resume();
        
        /// <summary>
        /// 释放资源
        /// </summary>
        Task DisposeAsync();
    }
    
    /// <summary>
    /// AI服务管理器接口
    /// </summary>
    public interface IAIServiceManager
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初始化状态变化事件
        /// </summary>
        event Action<bool> OnInitializationStatusChanged;
        
        /// <summary>
        /// 初始化AI服务管理器
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// 获取AI服务
        /// </summary>
        T GetService<T>() where T : class, IAIService;
        
        /// <summary>
        /// 检查服务是否可用
        /// </summary>
        bool IsServiceAvailable<T>() where T : class, IAIService;
        
        /// <summary>
        /// 重新初始化指定服务
        /// </summary>
        Task ReinitializeServiceAsync<T>() where T : class, IAIService;
        
        /// <summary>
        /// 获取服务健康状态
        /// </summary>
        Task<System.Collections.Generic.Dictionary<string, bool>> GetServicesHealthAsync();
        
        /// <summary>
        /// 释放资源
        /// </summary>
        Task DisposeAsync();
    }
    
    /// <summary>
    /// GPT服务接口
    /// </summary>
    public interface IGPTService : IAIService
    {
        /// <summary>
        /// 响应接收事件
        /// </summary>
        event Action<string, string> OnResponseReceived;
        
        /// <summary>
        /// 错误事件
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// 发送聊天请求
        /// </summary>
        Task<string> SendChatAsync(string message, string conversationId = null);
        
        /// <summary>
        /// 生成内容
        /// </summary>
        Task<string> GenerateContentAsync(string prompt, GPTGenerationOptions options = null);
        
        /// <summary>
        /// 流式聊天
        /// </summary>
        Task SendStreamChatAsync(string message, Action<string> onPartialResponse, string conversationId = null);
        
        /// <summary>
        /// 获取对话历史
        /// </summary>
        GPTConversation GetConversation(string conversationId);
        
        /// <summary>
        /// 清除对话历史
        /// </summary>
        void ClearConversation(string conversationId = null);
    }
    
    /// <summary>
    /// Azure语音服务接口
    /// </summary>
    public interface IAzureSpeechService : IAIService
    {
        /// <summary>
        /// 是否正在录音
        /// </summary>
        bool IsRecording { get; }
        
        /// <summary>
        /// 是否正在播放语音
        /// </summary>
        bool IsSpeaking { get; }
        
        /// <summary>
        /// 语音识别事件
        /// </summary>
        event Action<string> OnSpeechRecognized;
        
        /// <summary>
        /// 语音合成事件
        /// </summary>
        event Action<string> OnSpeechSynthesized;
        
        /// <summary>
        /// 错误事件
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// 录音开始事件
        /// </summary>
        event Action OnRecordingStarted;
        
        /// <summary>
        /// 录音停止事件
        /// </summary>
        event Action OnRecordingStopped;
        
        /// <summary>
        /// 开始语音识别
        /// </summary>
        Task<string> StartSpeechRecognitionAsync(float maxDuration = 10f);
        
        /// <summary>
        /// 语音合成
        /// </summary>
        Task<UnityEngine.AudioClip> SynthesizeSpeechAsync(string text, string voiceName = null);
        
        /// <summary>
        /// 播放合成的语音
        /// </summary>
        Task PlaySynthesizedSpeechAsync(string text, string voiceName = null);
        
        /// <summary>
        /// 连续语音识别
        /// </summary>
        Task StartContinuousRecognitionAsync(Action<string> onPartialResult, Action<string> onFinalResult);
        
        /// <summary>
        /// 停止连续识别
        /// </summary>
        void StopContinuousRecognition();
        
        /// <summary>
        /// 获取支持的语音列表
        /// </summary>
        Task<System.Collections.Generic.List<VoiceInfo>> GetAvailableVoicesAsync();
    }
    
    /// <summary>
    /// 计算机视觉服务接口
    /// </summary>
    public interface IComputerVisionService : IAIService
    {
        /// <summary>
        /// 图像分析完成事件
        /// </summary>
        event Action<ImageAnalysisResult> OnImageAnalyzed;
        
        /// <summary>
        /// 错误事件
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// 分析图像
        /// </summary>
        Task<ImageAnalysisResult> AnalyzeImageAsync(UnityEngine.Texture2D image);
        
        /// <summary>
        /// 检测物体
        /// </summary>
        Task<System.Collections.Generic.List<ObjectDetectionResult>> DetectObjectsAsync(UnityEngine.Texture2D image);
        
        /// <summary>
        /// 识别文字
        /// </summary>
        Task<string> RecognizeTextAsync(UnityEngine.Texture2D image);
        
        /// <summary>
        /// 生成图像描述
        /// </summary>
        Task<string> GenerateImageDescriptionAsync(UnityEngine.Texture2D image);
        
        /// <summary>
        /// 检测人脸
        /// </summary>
        Task<System.Collections.Generic.List<FaceDetectionResult>> DetectFacesAsync(UnityEngine.Texture2D image);
    }
    
    /// <summary>
    /// 推荐服务接口
    /// </summary>
    public interface IRecommendationService : IAIService
    {
        /// <summary>
        /// 推荐生成事件
        /// </summary>
        event Action<System.Collections.Generic.List<RecommendationItem>> OnRecommendationsGenerated;
        
        /// <summary>
        /// 错误事件
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// 获取个性化推荐
        /// </summary>
        Task<System.Collections.Generic.List<RecommendationItem>> GetPersonalizedRecommendationsAsync(string userId, RecommendationContext context = null);
        
        /// <summary>
        /// 获取相似内容推荐
        /// </summary>
        Task<System.Collections.Generic.List<RecommendationItem>> GetSimilarItemsAsync(string itemId, int maxResults = 10);
        
        /// <summary>
        /// 更新用户偏好
        /// </summary>
        Task UpdateUserPreferencesAsync(string userId, UserPreferences preferences);
        
        /// <summary>
        /// 记录用户行为
        /// </summary>
        Task RecordUserInteractionAsync(string userId, string itemId, InteractionType interactionType);
        
        /// <summary>
        /// 获取热门推荐
        /// </summary>
        Task<System.Collections.Generic.List<RecommendationItem>> GetTrendingItemsAsync(string category = null, int maxResults = 10);
    }
}