using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripMeta.AI
{
    /// <summary>
    /// AI服务配置
    /// </summary>
    [Serializable]
    public class AIServiceConfig
    {
        public GPTConfig gptConfig;
        public AzureSpeechConfig speechConfig;
        public ComputerVisionConfig visionConfig;
        public RecommendationConfig recommendationConfig;
    }
    
    /// <summary>
    /// GPT配置
    /// </summary>
    [Serializable]
    public class GPTConfig
    {
        [Header("API设置")]
        public string apiKey = "";
        public string apiEndpoint = "https://api.openai.com/v1/chat/completions";
        public string model = "gpt-4";
        
        [Header("生成参数")]
        public int maxTokens = 2000;
        public float temperature = 0.7f;
        public float topP = 1.0f;
        public float frequencyPenalty = 0.0f;
        public float presencePenalty = 0.0f;
        
        [Header("限制设置")]
        public int maxRequestsPerMinute = 60;
        public float requestTimeout = 30f;
        public int maxConversationLength = 20;
    }
    
    /// <summary>
    /// Azure语音配置
    /// </summary>
    [Serializable]
    public class AzureSpeechConfig
    {
        [Header("认证设置")]
        public string subscriptionKey = "";
        public string region = "eastus";
        
        [Header("语音设置")]
        public string language = "zh-CN";
        public string voiceName = "zh-CN-XiaoxiaoNeural";
        public int sampleRate = 16000;
        
        [Header("识别设置")]
        public float recognitionTimeout = 10f;
        public bool enableContinuousRecognition = true;
        public float silenceTimeout = 2f;
    }
    
    /// <summary>
    /// 计算机视觉配置
    /// </summary>
    [Serializable]
    public class ComputerVisionConfig
    {
        [Header("认证设置")]
        public string subscriptionKey = "";
        public string endpoint = "";
        
        [Header("分析设置")]
        public float confidenceThreshold = 0.7f;
        public int maxImageSize = 4194304; // 4MB
        public string[] supportedFormats = { "jpg", "jpeg", "png", "bmp" };
        
        [Header("功能开关")]
        public bool enableObjectDetection = true;
        public bool enableTextRecognition = true;
        public bool enableFaceDetection = true;
        public bool enableImageDescription = true;
    }
    
    /// <summary>
    /// 推荐配置
    /// </summary>
    [Serializable]
    public class RecommendationConfig
    {
        [Header("推荐设置")]
        public int maxRecommendations = 10;
        public float similarityThreshold = 0.6f;
        public int maxUserHistory = 100;
        
        [Header("算法开关")]
        public bool enableCollaborativeFiltering = true;
        public bool enableContentBasedFiltering = true;
        public bool enableHybridFiltering = true;
        
        [Header("权重设置")]
        public float collaborativeWeight = 0.6f;
        public float contentBasedWeight = 0.4f;
        public float recencyWeight = 0.2f;
        public float popularityWeight = 0.1f;
    }
    
    /// <summary>
    /// GPT对话
    /// </summary>
    public class GPTConversation
    {
        public string Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdated { get; private set; }
        
        private readonly List<GPTMessage> messages = new List<GPTMessage>();
        private readonly int maxLength;
        
        public GPTConversation(string id, int maxLength = 20)
        {
            Id = id;
            CreatedAt = DateTime.Now;
            LastUpdated = DateTime.Now;
            this.maxLength = maxLength;
        }
        
        public void AddMessage(string role, string content)
        {
            messages.Add(new GPTMessage { role = role, content = content });
            LastUpdated = DateTime.Now;
            
            // 保持对话长度限制
            while (messages.Count > maxLength)
            {
                messages.RemoveAt(0);
            }
        }
        
        public List<GPTMessage> GetMessages()
        {
            return new List<GPTMessage>(messages);
        }
        
        public void Clear()
        {
            messages.Clear();
            LastUpdated = DateTime.Now;
        }
        
        public int MessageCount => messages.Count;
    }
    
    /// <summary>
    /// GPT消息
    /// </summary>
    [Serializable]
    public class GPTMessage
    {
        public string role;
        public string content;
    }
    
    /// <summary>
    /// GPT生成选项
    /// </summary>
    [Serializable]
    public class GPTGenerationOptions
    {
        public string systemPrompt;
        public int? maxTokens;
        public float? temperature;
        public float? topP;
        public string[] stopSequences;
    }
    
    /// <summary>
    /// GPT请求
    /// </summary>
    public class GPTRequest
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string Message { get; set; }
        public GPTGenerationOptions Options { get; set; }
        public DateTime CreatedAt { get; set; }
        public Action<string> OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }
    }
    
    /// <summary>
    /// 语音信息
    /// </summary>
    [Serializable]
    public class VoiceInfo
    {
        public string Name;
        public string DisplayName;
        public string Gender;
        public string Locale;
        public string[] StyleList;
    }
    
    /// <summary>
    /// 语音请求
    /// </summary>
    public class SpeechRequest
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string VoiceName { get; set; }
        public DateTime CreatedAt { get; set; }
        public Action<AudioClip> OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }
    }
    
    /// <summary>
    /// 图像分析结果
    /// </summary>
    [Serializable]
    public class ImageAnalysisResult
    {
        public string description;
        public List<string> tags;
        public List<ObjectDetectionResult> objects;
        public List<FaceDetectionResult> faces;
        public string extractedText;
        public float confidence;
        public DateTime analyzedAt;
    }
    
    /// <summary>
    /// 物体检测结果
    /// </summary>
    [Serializable]
    public class ObjectDetectionResult
    {
        public string objectName;
        public float confidence;
        public Rect boundingBox;
        public Dictionary<string, object> properties;
    }
    
    /// <summary>
    /// 人脸检测结果
    /// </summary>
    [Serializable]
    public class FaceDetectionResult
    {
        public Rect faceRectangle;
        public float confidence;
        public int estimatedAge;
        public string gender;
        public string emotion;
        public Dictionary<string, float> emotionScores;
    }
    
    /// <summary>
    /// 推荐项目
    /// </summary>
    [Serializable]
    public class RecommendationItem
    {
        public string itemId;
        public string title;
        public string description;
        public string category;
        public float score;
        public string imageUrl;
        public Dictionary<string, object> metadata;
        public DateTime createdAt;
    }
    
    /// <summary>
    /// 推荐上下文
    /// </summary>
    [Serializable]
    public class RecommendationContext
    {
        public string location;
        public string timeOfDay;
        public string weather;
        public string[] interests;
        public Dictionary<string, object> additionalContext;
    }
    
    /// <summary>
    /// 用户偏好
    /// </summary>
    [Serializable]
    public class UserPreferences
    {
        public Dictionary<string, float> categoryPreferences;
        public Dictionary<string, float> featurePreferences;
        public string[] favoriteItems;
        public string[] blockedItems;
        public DateTime lastUpdated;
    }
    
    /// <summary>
    /// 交互类型
    /// </summary>
    public enum InteractionType
    {
        View,
        Like,
        Share,
        Comment,
        Purchase,
        Bookmark,
        Skip,
        Dislike
    }

    /// <summary>
    /// 用户档案
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public string userId;
        public string name;
        public Language preferredLanguage = Language.Chinese;
        public float preferredSpeechSpeed = 1.0f;
        public DetailLevel preferredDetailLevel = DetailLevel.Medium;
        public string[] interests = { "历史", "文化", "艺术" };
        public DateTime createdAt;
        public DateTime lastActiveAt;
    }

    /// <summary>
    /// 位置上下文
    /// </summary>
    [Serializable]
    public class LocationContext
    {
        public string locationId;
        public string locationName;
        public string description;
        public Vector3 position;
        public string[] tags;
        public LocationType locationType;
    }

    /// <summary>
    /// 位置类型
    /// </summary>
    public enum LocationType
    {
        City,
        Landmark,
        Museum,
        Park,
        HistoricalSite,
        Entertainment,
        Shopping,
        Nature
    }

    /// <summary>
    /// 详细程度
    /// </summary>
    public enum DetailLevel
    {
        Brief,
        Medium,
        Detailed
    }

    /// <summary>
    /// 语言类型
    /// </summary>
    public enum Language
    {
        Auto,
        Chinese,
        English,
        Japanese,
        Korean
    }

    /// <summary>
    /// 语音配置
    /// </summary>
    [Serializable]
    public class VoiceProfile
    {
        public string voiceName = "zh-CN-XiaoxiaoNeural";
        public string gender = "Female";
        public string style = "Friendly";
        public float pitch = 1.0f;
    }

    /// <summary>
    /// 知识图谱配置
    /// </summary>
    [Serializable]
    public class KnowledgeGraphConfig
    {
        public bool enableLocalCache = true;
        public int maxCacheSize = 1000;
        public float cacheExpirationHours = 24f;
        public string[] knowledgeDomains = { "历史", "文化", "艺术", "建筑", "地理" };
    }

    /// <summary>
    /// LLM请求
    /// </summary>
    public class LLMRequest : AIRequest
    {
        public string prompt;
        public int maxTokens = 500;
        public float temperature = 0.7f;
        public string systemPrompt;

        public LLMRequest()
        {
            ServiceType = AIServiceType.LLM;
        }
    }

    /// <summary>
    /// LLM响应
    /// </summary>
    public class LLMResponse : AIResponse
    {
        public string generatedText;
        public int tokensUsed;
        public float finishReason;
    }

    /// <summary>
    /// 语音识别请求
    /// </summary>
    public class VoiceRecognitionRequest : AIRequest
    {
        public Language language = Language.Chinese;
        public float timeout = 10f;
        public bool enablePunctuation = true;

        public VoiceRecognitionRequest()
        {
            ServiceType = AIServiceType.Speech;
        }
    }

    /// <summary>
    /// 语音识别响应
    /// </summary>
    public class VoiceRecognitionResponse : AIResponse
    {
        public string recognizedText;
        public float confidence;
        public Language detectedLanguage;
    }

    /// <summary>
    /// 语音合成响应
    /// </summary>
    public class SpeechResponse : AIResponse
    {
        public AudioClip audioClip;
        public float duration;
        public int sampleRate;
    }

    /// <summary>
    /// 推荐请求
    /// </summary>
    public class RecommendationRequest : AIRequest
    {
        public string userId;
        public string locationId;
        public string[] userPreferences;
        public LocationContext currentContext;
        public int count = 10;

        public RecommendationRequest()
        {
            ServiceType = AIServiceType.Recommendation;
        }
    }

    /// <summary>
    /// 推荐响应
    /// </summary>
    public class RecommendationResponse : AIResponse
    {
        public List<Recommendation> recommendations;
    }

    /// <summary>
    /// 推荐项
    /// </summary>
    [Serializable]
    public class Recommendation
    {
        public string id;
        public string title;
        public string description;
        public string category;
        public float score;
        public string imageUrl;
        public Vector3? position;
        public string actionType;
    }
}