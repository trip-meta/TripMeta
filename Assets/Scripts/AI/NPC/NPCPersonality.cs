using System;
using UnityEngine;

namespace TripMeta.AI.NPC
{
    /// <summary>
    /// NPC角色类型
    /// </summary>
    public enum NPCRole
    {
        TourGuide,      // 导游
        Merchant,       // 商人
        Resident,       // 居民
        Scholar,        // 学者
        Storyteller,    // 讲故事的人
        Guardian        // 守护者
    }
    
    /// <summary>
    /// NPC人设配置 - ScriptableObject
    /// 定义NPC的个性、知识领域和行为特征
    /// </summary>
    [CreateAssetMenu(fileName = "NPCPersonality", menuName = "TripMeta/NPC/Personality")]
    public class NPCPersonality : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("NPC名称")]
        public string npcName = "AI Guide";
        
        [Tooltip("NPC角色")]
        public NPCRole role = NPCRole.TourGuide;
        
        [Tooltip("NPC唯一标识符")]
        public string npcId = Guid.NewGuid().ToString();
        
        [Header("人设配置")]
        [TextArea(10, 20)]
        [Tooltip("LLM系统提示词 - 定义NPC的个性和行为")]
        public string systemPrompt = "You are a friendly and knowledgeable tour guide.";
        
        [Tooltip("个性特征标签")]
        public string[] personalityTraits = { "friendly", "knowledgeable", "patient" };
        
        [Tooltip("知识领域")]
        public string[] knowledgeDomains = { "history", "culture", "art" };
        
        [Header("语音配置")]
        [Tooltip("语音ID（Azure TTS）")]
        public string voiceId = "zh-CN-XiaoxiaoNeural";
        
        [Tooltip("语言")]
        public string language = "zh-CN";
        
        [Tooltip("语音速度")]
        [Range(0.5f, 2.0f)]
        public float speechSpeed = 1.0f;
        
        [Tooltip("语音音量")]
        [Range(0.0f, 1.0f)]
        public float speechVolume = 0.8f;
        
        [Header("交互配置")]
        [Tooltip("问候语")]
        [TextArea(2, 5)]
        public string greetingMessage = "Hello! Welcome to our tour.";
        
        [Tooltip("告别语")]
        [TextArea(2, 5)]
        public string farewellMessage = "Thank you for visiting. Goodbye!";
        
        [Tooltip("触发距离（米）")]
        public float triggerDistance = 3.0f;
        
        [Tooltip("对话距离（米）")]
        public float conversationDistance = 2.0f;
        
        [Tooltip("自动问候")]
        public bool autoGreeting = true;
        
        [Tooltip("问候冷却时间（秒）")]
        public float greetingCooldown = 30.0f;
        
        [Header("行为配置")]
        [Tooltip("是否启用巡逻")]
        public bool enablePatrol = false;
        
        [Tooltip("巡逻路径点")]
        public Vector3[] patrolWaypoints;
        
        [Tooltip("巡逻速度")]
        public float patrolSpeed = 1.0f;
        
        [Tooltip("在路径点停留时间（秒）")]
        public float waypointWaitTime = 5.0f;
        
        [Header("LLM配置")]
        [Tooltip("使用流式响应")]
        public bool useStreamingResponse = true;
        
        [Tooltip("最大对话历史长度")]
        public int maxConversationHistory = 20;
        
        [Tooltip("响应温度")]
        [Range(0.0f, 2.0f)]
        public float temperature = 0.7f;
        
        [Tooltip("最大Token数")]
        public int maxTokens = 500;
        
        [Tooltip("请求超时（秒）")]
        public float requestTimeout = 10.0f;
        
        [Header("记忆配置")]
        [Tooltip("启用长期记忆")]
        public bool enableLongTermMemory = false;
        
        [Tooltip("记忆容量")]
        public int memoryCapacity = 100;
        
        /// <summary>
        /// 生成完整的系统提示词
        /// </summary>
        public string GenerateFullSystemPrompt()
        {
            var traitsStr = string.Join(", ", personalityTraits);
            var domainsStr = string.Join(", ", knowledgeDomains);
            
            var fullPrompt = $@"{systemPrompt}

# Your Profile
- Name: {npcName}
- Role: {role}
- Personality Traits: {traitsStr}
- Knowledge Domains: {domainsStr}

# Behavioral Guidelines
1. Stay in character as {npcName}
2. Use your expertise in {domainsStr} to provide helpful information
3. Be {traitsStr}
4. Keep responses concise and engaging for VR users
5. Use natural, conversational language
6. If you don't know something, admit it honestly

# Response Format
- Keep responses under 100 words when possible
- Use clear, simple language
- Be ready to elaborate if the user asks for more details";

            return fullPrompt;
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(npcName))
            {
                Debug.LogError($"[NPCPersonality] NPC name is required");
                return false;
            }
            
            if (string.IsNullOrEmpty(systemPrompt))
            {
                Debug.LogError($"[NPCPersonality] System prompt is required for {npcName}");
                return false;
            }
            
            if (triggerDistance <= 0 || conversationDistance <= 0)
            {
                Debug.LogError($"[NPCPersonality] Invalid distance configuration for {npcName}");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 创建默认人设
        /// </summary>
        public static NPCPersonality CreateDefaultTourGuide()
        {
            var personality = CreateInstance<NPCPersonality>();
            personality.npcName = "Tour Guide";
            personality.role = NPCRole.TourGuide;
            personality.systemPrompt = @"You are a friendly and knowledgeable tour guide named Tour Guide.
Your role is to help visitors explore and understand the attractions around them.
You are passionate about sharing interesting facts and stories about the places you guide people through.
Always be welcoming, informative, and ready to answer questions.";
            personality.personalityTraits = new[] { "friendly", "knowledgeable", "enthusiastic" };
            personality.knowledgeDomains = new[] { "history", "culture", "architecture" };
            return personality;
        }
        
        /// <summary>
        /// 创建默认商人
        /// </summary>
        public static NPCPersonality CreateDefaultMerchant()
        {
            var personality = CreateInstance<NPCPersonality>();
            personality.npcName = "Merchant";
            personality.role = NPCRole.Merchant;
            personality.systemPrompt = @"You are a local merchant who sells souvenirs and local specialties.
You are friendly, persuasive, and know a lot about local products.
You love to share stories about the items you sell and their cultural significance.";
            personality.personalityTraits = new[] { "friendly", "persuasive", "helpful" };
            personality.knowledgeDomains = new[] { "local products", "craftsmanship", "culture" };
            return personality;
        }
    }
}
