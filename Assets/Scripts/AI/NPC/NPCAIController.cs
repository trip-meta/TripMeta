using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using TripMeta.Core.ErrorHandling;
using TripMeta.AI.NPC;

namespace TripMeta.AI.NPC
{
    /// <summary>
    /// NPC AI控制器 - 管理单个NPC的AI行为
    /// 负责对话管理、玩家交互和行为状态机
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCAIController : MonoBehaviour
    {
        [Header("NPC配置")]
        [SerializeField] private NPCPersonality personality;
        [SerializeField] private NPCBehaviorTree behaviorTree;
        
        [Header("对话配置")]
        [SerializeField] private float conversationTimeout = 30f;
        [SerializeField] private bool enableVoice = true;
        
        [Header("调试")]
        [SerializeField] private bool debugMode = false;
        
        // 组件引用
        private NavMeshAgent navMeshAgent;
        private AudioSource audioSource;
        
        // 对话状态
        private string conversationId;
        private List<ConversationMessage> conversationHistory;
        private bool isConversing = false;
        private GameObject currentPlayer;
        private float lastInteractionTime;
        
        // LLM服务
        private IGPTService gptService;
        private IAzureSpeechService speechService;
        
        // 状态
        private NPCState currentState = NPCState.Idle;
        private bool isInitialized = false;
        private bool isProcessingLLMRequest = false;
        
        // 事件
        public event Action<NPCState> OnStateChanged;
        public event Action<string> OnResponseGenerated;
        public event Action<string> OnSpeakStart;
        public event Action OnSpeakEnd;
        public event Action<GameObject> OnPlayerEnter;
        public event Action<GameObject> OnPlayerExit;
        
        // 属性
        public NPCPersonality Personality => personality;
        public NPCState CurrentState => currentState;
        public bool IsConversing => isConversing;
        public string ConversationId => conversationId;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            InitializeAI();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateBehavior();
            CheckPlayerProximity();
            UpdateConversationTimeout();
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            conversationHistory = new List<ConversationMessage>();
        }
        
        /// <summary>
        /// 初始化AI服务
        /// </summary>
        private void InitializeAI()
        {
            try
            {
                // 验证人设配置
                if (personality == null)
                {
                    Logger.LogError($"[NPCAIController] No personality assigned to {gameObject.name}");
                    return;
                }
                
                if (!personality.Validate())
                {
                    Logger.LogError($"[NPCAIController] Invalid personality configuration for {gameObject.name}");
                    return;
                }
                
                // 创建对话ID
                conversationId = $"npc_{personality.npcId}_{DateTime.Now.Ticks}";
                
                // 获取AI服务
                var aiManager = AIServiceManager.Instance;
                if (aiManager != null && aiManager.IsInitialized)
                {
                    gptService = aiManager.GetService<IGPTService>();
                    speechService = aiManager.GetService<IAzureSpeechService>();
                }
                
                // 初始化行为树
                if (behaviorTree == null)
                {
                    behaviorTree = gameObject.AddComponent<NPCBehaviorTree>();
                }
                behaviorTree.Initialize(this, personality);
                
                isInitialized = true;
                
                Logger.LogInfo($"[NPCAIController] {personality.npcName} initialized successfully", "NPC");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"[NPCAIController] Failed to initialize {gameObject.name}");
            }
        }
        
        /// <summary>
        /// 更新行为
        /// </summary>
        private void UpdateBehavior()
        {
            if (behaviorTree != null)
            {
                behaviorTree.UpdateBehavior();
            }
        }
        
        /// <summary>
        /// 检查玩家接近
        /// </summary>
        private void CheckPlayerProximity()
        {
            if (currentPlayer == null)
            {
                // 查找附近的玩家
                var colliders = Physics.OverlapSphere(transform.position, personality.triggerDistance);
                foreach (var collider in colliders)
                {
                    if (collider.CompareTag("Player") || collider.CompareTag("MainCamera"))
                    {
                        OnPlayerDetected(collider.gameObject);
                        break;
                    }
                }
            }
            else
            {
                // 检查玩家是否离开
                var distance = Vector3.Distance(transform.position, currentPlayer.transform.position);
                if (distance > personality.triggerDistance * 1.5f)
                {
                    OnPlayerLeft();
                }
            }
        }
        
        /// <summary>
        /// 玩家检测到
        /// </summary>
        private void OnPlayerDetected(GameObject player)
        {
            currentPlayer = player;
            OnPlayerEnter?.Invoke(player);
            
            // 自动问候
            if (personality.autoGreeting && Time.time - lastInteractionTime > personality.greetingCooldown)
            {
                GreetPlayer();
            }
            
            Logger.LogInfo($"[NPCAIController] Player detected by {personality.npcName}", "NPC");
        }
        
        /// <summary>
        /// 玩家离开
        /// </summary>
        private void OnPlayerLeft()
        {
            if (isConversing)
            {
                EndConversation();
            }
            
            if (currentState == NPCState.Greeting)
            {
                SayFarewell();
            }
            
            var player = currentPlayer;
            currentPlayer = null;
            OnPlayerExit?.Invoke(player);
            
            Logger.LogInfo($"[NPCAIController] Player left {personality.npcName}", "NPC");
        }
        
        /// <summary>
        /// 问候玩家
        /// </summary>
        public async void GreetPlayer()
        {
            if (!isInitialized) return;
            
            SetState(NPCState.Greeting);
            lastInteractionTime = Time.time;
            
            await SpeakAsync(personality.greetingMessage);
            
            SetState(NPCState.Idle);
        }
        
        /// <summary>
        /// 告别
        /// </summary>
        public async void SayFarewell()
        {
            if (!isInitialized) return;
            
            SetState(NPCState.Farewell);
            await SpeakAsync(personality.farewellMessage);
            SetState(NPCState.Idle);
        }
        
        /// <summary>
        /// 开始对话
        /// </summary>
        public async Task<string> StartConversation(string userMessage)
        {
            if (!isInitialized)
            {
                return "I'm not ready yet. Please wait.";
            }
            
            if (isProcessingLLMRequest)
            {
                return "I'm thinking about your previous question.";
            }
            
            try
            {
                isConversing = true;
                isProcessingLLMRequest = true;
                SetState(NPCState.Conversing);
                
                // 添加用户消息到历史
                conversationHistory.Add(new ConversationMessage
                {
                    role = "user",
                    content = userMessage,
                    timestamp = DateTime.Now
                });
                
                // 裁剪历史长度
                TrimConversationHistory();
                
                // 调用LLM
                var response = await GenerateResponseAsync(userMessage);
                
                // 添加助手回复到历史
                conversationHistory.Add(new ConversationMessage
                {
                    role = "assistant",
                    content = response,
                    timestamp = DateTime.Now
                });
                
                lastInteractionTime = Time.time;
                
                // 触发事件
                OnResponseGenerated?.Invoke(response);
                
                // 语音合成
                if (enableVoice)
                {
                    await SpeakAsync(response);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"[NPCAIController] Conversation error for {personality.npcName}");
                return "I'm sorry, I encountered an error. Please try again.";
            }
            finally
            {
                isProcessingLLMRequest = false;
            }
        }
        
        /// <summary>
        /// 生成LLM响应
        /// </summary>
        private async Task<string> GenerateResponseAsync(string userMessage)
        {
            if (gptService == null || !gptService.IsInitialized)
            {
                Logger.LogWarning($"[NPCAIController] GPT service not available for {personality.npcName}");
                return "I'm having trouble connecting to my knowledge base right now.";
            }
            
            try
            {
                // 构建包含系统提示的对话历史
                var messages = BuildConversationMessages();
                
                // 使用GPT服务生成响应
                var response = await gptService.SendChatAsync(
                    userMessage,
                    conversationId
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"[NPCAIController] Failed to generate response");
                throw;
            }
        }
        
        /// <summary>
        /// 构建对话消息列表（包含系统提示）
        /// </summary>
        private List<GPTMessage> BuildConversationMessages()
        {
            var messages = new List<GPTMessage>();
            
            // 添加系统提示
            messages.Add(new GPTMessage
            {
                role = "system",
                content = personality.GenerateFullSystemPrompt()
            });
            
            // 添加对话历史
            foreach (var msg in conversationHistory)
            {
                messages.Add(new GPTMessage
                {
                    role = msg.role,
                    content = msg.content
                });
            }
            
            return messages;
        }
        
        /// <summary>
        /// 流式对话
        /// </summary>
        public async Task StartStreamConversation(string userMessage, Action<string> onPartialResponse)
        {
            if (!isInitialized || !personality.useStreamingResponse)
            {
                var response = await StartConversation(userMessage);
                onPartialResponse?.Invoke(response);
                return;
            }
            
            if (isProcessingLLMRequest)
            {
                onPartialResponse?.Invoke("I'm thinking about your previous question.");
                return;
            }
            
            try
            {
                isConversing = true;
                isProcessingLLMRequest = true;
                SetState(NPCState.Conversing);
                
                // 添加用户消息
                conversationHistory.Add(new ConversationMessage
                {
                    role = "user",
                    content = userMessage,
                    timestamp = DateTime.Now
                });
                
                TrimConversationHistory();
                
                // 流式响应
                var fullResponse = "";
                await gptService.SendStreamChatAsync(
                    userMessage,
                    (partialResponse) =>
                    {
                        fullResponse = partialResponse;
                        onPartialResponse?.Invoke(partialResponse);
                    },
                    conversationId
                );
                
                // 添加完整响应到历史
                conversationHistory.Add(new ConversationMessage
                {
                    role = "assistant",
                    content = fullResponse,
                    timestamp = DateTime.Now
                });
                
                lastInteractionTime = Time.time;
                OnResponseGenerated?.Invoke(fullResponse);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"[NPCAIController] Stream conversation error");
                onPartialResponse?.Invoke("I'm sorry, I encountered an error.");
            }
            finally
            {
                isProcessingLLMRequest = false;
            }
        }
        
        /// <summary>
        /// 语音合成并播放
        /// </summary>
        private async Task SpeakAsync(string text)
        {
            if (!enableVoice || speechService == null || !speechService.IsInitialized)
            {
                return;
            }
            
            try
            {
                OnSpeakStart?.Invoke(text);
                await speechService.PlaySynthesizedSpeechAsync(text, personality.voiceId);
                OnSpeakEnd?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"[NPCAIController] Speech synthesis failed");
            }
        }
        
        /// <summary>
        /// 结束对话
        /// </summary>
        public void EndConversation()
        {
            isConversing = false;
            SetState(NPCState.Idle);
            
            Logger.LogInfo($"[NPCAIController] Conversation ended with {personality.npcName}", "NPC");
        }
        
        /// <summary>
        /// 清除对话历史
        /// </summary>
        public void ClearConversationHistory()
        {
            conversationHistory.Clear();
            conversationId = $"npc_{personality.npcId}_{DateTime.Now.Ticks}";
            
            if (gptService != null)
            {
                gptService.ClearConversation(conversationId);
            }
            
            Logger.LogInfo($"[NPCAIController] Conversation history cleared for {personality.npcName}", "NPC");
        }
        
        /// <summary>
        /// 裁剪对话历史
        /// </summary>
        private void TrimConversationHistory()
        {
            while (conversationHistory.Count > personality.maxConversationHistory)
            {
                conversationHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 检查对话超时
        /// </summary>
        private void UpdateConversationTimeout()
        {
            if (isConversing && Time.time - lastInteractionTime > conversationTimeout)
            {
                EndConversation();
            }
        }
        
        /// <summary>
        /// 设置状态
        /// </summary>
        private void SetState(NPCState newState)
        {
            if (currentState != newState)
            {
                var oldState = currentState;
                currentState = newState;
                OnStateChanged?.Invoke(newState);
                
                if (debugMode)
                {
                    Debug.Log($"[NPCAIController] {personality.npcName} state changed: {oldState} → {newState}");
                }
            }
        }
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.SetDestination(position);
            }
        }
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = true;
            }
        }
        
        /// <summary>
        /// 看向目标
        /// </summary>
        public void LookAt(Vector3 target)
        {
            var direction = (target - transform.position).normalized;
            direction.y = 0; // 保持水平
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 5f
                );
            }
        }
        
        private void OnDestroy()
        {
            ClearConversationHistory();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (personality == null) return;
            
            // 绘制触发范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, personality.triggerDistance);
            
            // 绘制对话范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, personality.conversationDistance);
        }
    }
    
    /// <summary>
    /// NPC状态
    /// </summary>
    public enum NPCState
    {
        Idle,           // 空闲
        Patrol,         // 巡逻
        Greeting,       // 问候
        Conversing,     // 对话中
        Farewell,       // 告别
        Thinking        // 思考中
    }
    
    /// <summary>
    /// 对话消息
    /// </summary>
    [Serializable]
    public class ConversationMessage
    {
        public string role;
        public string content;
        public DateTime timestamp;
    }
}
