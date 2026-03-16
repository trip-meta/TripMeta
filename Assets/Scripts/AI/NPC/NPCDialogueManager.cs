using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.ErrorHandling;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.AI.NPC
{
    /// <summary>
    /// NPC对话管理器 - 全局管理所有NPC的对话
    /// 负责请求队列、并发控制、Token追踪和对话持久化
    /// </summary>
    public class NPCDialogueManager : MonoBehaviour, IService
    {
        [Header("并发配置")]
        [SerializeField] private int maxConcurrentRequests = 3;
        [SerializeField] private float requestTimeout = 15f;
        [SerializeField] private int maxRetries = 3;
        [SerializeField] private float retryDelay = 1f;
        
        [Header("Token追踪")]
        [SerializeField] private bool enableTokenTracking = true;
        [SerializeField] private int maxTokensPerMinute = 90000; // GPT-4限制
        [SerializeField] private int maxTokensPerHour = 90000;
        
        [Header("持久化")]
        [SerializeField] private bool enablePersistence = true;
        [SerializeField] private string persistencePath = "NPCConversations";
        
        // 请求队列
        private Queue<NPCDialogueRequest> requestQueue;
        private Dictionary<string, NPCDialogueRequest> activeRequests;
        private int currentConcurrentRequests = 0;
        
        // Token使用追踪
        private TokenUsageTracker tokenTracker;
        
        // NPC注册表
        private Dictionary<string, NPCAIController> registeredNPCs;
        
        // 对话历史缓存
        private Dictionary<string, List<ConversationMessage>> conversationCache;
        
        // 状态
        private bool isInitialized = false;
        
        // 事件
        public event Action<NPCDialogueRequest> OnRequestQueued;
        public event Action<NPCDialogueRequest> OnRequestStarted;
        public event Action<NPCDialogueRequest, string> OnRequestCompleted;
        public event Action<NPCDialogueRequest, Exception> OnRequestFailed;
        public event Action<int, int> OnTokenUsageUpdated; // (current, limit)
        
        // 属性
        public bool IsInitialized => isInitialized;
        public int QueueLength => requestQueue?.Count ?? 0;
        public int ActiveRequestsCount => currentConcurrentRequests;
        
        public static NPCDialogueManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            try
            {
                Logger.LogInfo("Initializing NPC Dialogue Manager...", "NPCDialogue");
                
                requestQueue = new Queue<NPCDialogueRequest>();
                activeRequests = new Dictionary<string, NPCDialogueRequest>();
                registeredNPCs = new Dictionary<string, NPCAIController>();
                conversationCache = new Dictionary<string, List<ConversationMessage>>();
                
                if (enableTokenTracking)
                {
                    tokenTracker = new TokenUsageTracker(maxTokensPerMinute, maxTokensPerHour);
                }
                
                // 加载持久化的对话
                if (enablePersistence)
                {
                    LoadPersistedConversations();
                }
                
                isInitialized = true;
                
                Logger.LogInfo("NPC Dialogue Manager initialized", "NPCDialogue");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to initialize NPC Dialogue Manager");
            }
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            ProcessQueue();
            UpdateTokenTracker();
        }
        
        /// <summary>
        /// 注册NPC
        /// </summary>
        public void RegisterNPC(NPCAIController npc)
        {
            if (npc == null || npc.Personality == null) return;
            
            var npcId = npc.Personality.npcId;
            if (!registeredNPCs.ContainsKey(npcId))
            {
                registeredNPCs[npcId] = npc;
                Logger.LogInfo($"[NPCDialogueManager] Registered NPC: {npc.Personality.npcName}", "NPCDialogue");
            }
        }
        
        /// <summary>
        /// 注销NPC
        /// </summary>
        public void UnregisterNPC(string npcId)
        {
            if (registeredNPCs.Remove(npcId))
            {
                Logger.LogInfo($"[NPCDialogueManager] Unregistered NPC: {npcId}", "NPCDialogue");
            }
        }
        
        /// <summary>
        /// 提交对话请求
        /// </summary>
        public async Task<NPCDialogueResponse> SubmitDialogueRequest(NPCDialogueRequest request)
        {
            if (!isInitialized)
            {
                return new NPCDialogueResponse
                {
                    success = false,
                    error = "Dialogue manager not initialized"
                };
            }
            
            // 检查Token限制
            if (enableTokenTracking && tokenTracker.IsLimitExceeded())
            {
                return new NPCDialogueResponse
                {
                    success = false,
                    error = "Token limit exceeded. Please try again later."
                };
            }
            
            // 如果并发数未满，立即处理
            if (currentConcurrentRequests < maxConcurrentRequests)
            {
                return await ProcessRequestImmediately(request);
            }
            
            // 否则加入队列
            requestQueue.Enqueue(request);
            OnRequestQueued?.Invoke(request);
            
            Logger.LogInfo($"[NPCDialogueManager] Request queued: {request.npcName} (Queue: {requestQueue.Count})", "NPCDialogue");
            
            // 等待处理完成
            return await WaitForRequestCompletion(request);
        }
        
        /// <summary>
        /// 立即处理请求
        /// </summary>
        private async Task<NPCDialogueResponse> ProcessRequestImmediately(NPCDialogueRequest request)
        {
            currentConcurrentRequests++;
            activeRequests[request.requestId] = request;
            OnRequestStarted?.Invoke(request);
            
            try
            {
                var response = await ExecuteRequestWithRetry(request);
                
                // 追踪Token使用
                if (enableTokenTracking && response.tokensUsed > 0)
                {
                    tokenTracker.AddUsage(response.tokensUsed);
                    OnTokenUsageUpdated?.Invoke(tokenTracker.CurrentMinuteUsage, maxTokensPerMinute);
                }
                
                OnRequestCompleted?.Invoke(request, response.response);
                
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"[NPCDialogueManager] Request failed: {request.requestId}");
                OnRequestFailed?.Invoke(request, ex);
                
                return new NPCDialogueResponse
                {
                    success = false,
                    error = ex.Message
                };
            }
            finally
            {
                currentConcurrentRequests--;
                activeRequests.Remove(request.requestId);
            }
        }
        
        /// <summary>
        /// 带重试的请求执行
        /// </summary>
        private async Task<NPCDialogueResponse> ExecuteRequestWithRetry(NPCDialogueRequest request)
        {
            Exception lastException = null;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // 获取NPC控制器
                    if (!registeredNPCs.TryGetValue(request.npcId, out var npc))
                    {
                        throw new InvalidOperationException($"NPC not found: {request.npcId}");
                    }
                    
                    // 执行对话
                    var response = await npc.StartConversation(request.message);
                    
                    // 保存对话历史
                    if (enablePersistence)
                    {
                        SaveConversationHistory(request.conversationId);
                    }
                    
                    return new NPCDialogueResponse
                    {
                        success = true,
                        response = response,
                        requestId = request.requestId,
                        tokensUsed = EstimateTokenCount(request.message + response)
                    };
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Logger.LogWarning($"[NPCDialogueManager] Request attempt {attempt + 1} failed: {ex.Message}", "NPCDialogue");
                    
                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(retryDelay * (attempt + 1)));
                    }
                }
            }
            
            throw lastException ?? new Exception("Request failed after all retries");
        }
        
        /// <summary>
        /// 等待请求完成
        /// </summary>
        private async Task<NPCDialogueResponse> WaitForRequestCompletion(NPCDialogueRequest request)
        {
            var tcs = new TaskCompletionSource<NPCDialogueResponse>();
            
            Action<NPCDialogueRequest, string> onComplete = null;
            Action<NPCDialogueRequest, Exception> onFail = null;
            
            onComplete = (req, response) =>
            {
                if (req.requestId == request.requestId)
                {
                    OnRequestCompleted -= onComplete;
                    OnRequestFailed -= onFail;
                    tcs.TrySetResult(new NPCDialogueResponse
                    {
                        success = true,
                        response = response,
                        requestId = request.requestId
                    });
                }
            };
            
            onFail = (req, exception) =>
            {
                if (req.requestId == request.requestId)
                {
                    OnRequestCompleted -= onComplete;
                    OnRequestFailed -= onFail;
                    tcs.TrySetResult(new NPCDialogueResponse
                    {
                        success = false,
                        error = exception.Message,
                        requestId = request.requestId
                    });
                }
            };
            
            OnRequestCompleted += onComplete;
            OnRequestFailed += onFail;
            
            // 超时处理
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(requestTimeout));
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                OnRequestCompleted -= onComplete;
                OnRequestFailed -= onFail;
                return new NPCDialogueResponse
                {
                    success = false,
                    error = "Request timeout",
                    requestId = request.requestId
                };
            }
            
            return await tcs.Task;
        }
        
        /// <summary>
        /// 处理队列
        /// </summary>
        private async void ProcessQueue()
        {
            if (requestQueue.Count == 0 || currentConcurrentRequests >= maxConcurrentRequests)
            {
                return;
            }
            
            var request = requestQueue.Dequeue();
            _ = ProcessRequestImmediately(request);
        }
        
        /// <summary>
        /// 更新Token追踪器
        /// </summary>
        private void UpdateTokenTracker()
        {
            if (enableTokenTracking)
            {
                tokenTracker?.Update();
            }
        }
        
        /// <summary>
        /// 估算Token数量（粗略估算）
        /// </summary>
        private int EstimateTokenCount(string text)
        {
            // 粗略估算：英文约4字符=1 token，中文约1.5字符=1 token
            int charCount = text.Length;
            int chineseCount = 0;
            
            foreach (char c in text)
            {
                if (c > 0x4E00 && c < 0x9FFF)
                {
                    chineseCount++;
                }
            }
            
            int englishCount = charCount - chineseCount;
            return (int)((englishCount / 4.0) + (chineseCount / 1.5)) + 10;
        }
        
        /// <summary>
        /// 保存对话历史
        /// </summary>
        private void SaveConversationHistory(string conversationId)
        {
            if (!enablePersistence || !conversationCache.ContainsKey(conversationId))
            {
                return;
            }
            
            try
            {
                var history = conversationCache[conversationId];
                var json = JsonUtility.ToJson(new ConversationHistoryData
                {
                    conversationId = conversationId,
                    messages = history.ToArray(),
                    lastUpdated = DateTime.Now.Ticks
                });
                
                var path = System.IO.Path.Combine(Application.persistentDataPath, persistencePath);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                
                var filePath = System.IO.Path.Combine(path, $"{conversationId}.json");
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to save conversation history");
            }
        }
        
        /// <summary>
        /// 加载持久化的对话
        /// </summary>
        private void LoadPersistedConversations()
        {
            try
            {
                var path = System.IO.Path.Combine(Application.persistentDataPath, persistencePath);
                if (!System.IO.Directory.Exists(path))
                {
                    return;
                }
                
                var files = System.IO.Directory.GetFiles(path, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var json = System.IO.File.ReadAllText(file);
                        var data = JsonUtility.FromJson<ConversationHistoryData>(json);
                        
                        conversationCache[data.conversationId] = new List<ConversationMessage>(data.messages);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, $"Failed to load conversation: {file}");
                    }
                }
                
                Logger.LogInfo($"[NPCDialogueManager] Loaded {conversationCache.Count} persisted conversations", "NPCDialogue");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to load persisted conversations");
            }
        }
        
        /// <summary>
        /// 获取对话历史
        /// </summary>
        public List<ConversationMessage> GetConversationHistory(string conversationId)
        {
            if (conversationCache.TryGetValue(conversationId, out var history))
            {
                return new List<ConversationMessage>(history);
            }
            return new List<ConversationMessage>();
        }
        
        /// <summary>
        /// 清除对话历史
        /// </summary>
        public void ClearConversationHistory(string conversationId)
        {
            conversationCache.Remove(conversationId);
            
            if (enablePersistence)
            {
                try
                {
                    var path = System.IO.Path.Combine(Application.persistentDataPath, persistencePath);
                    var file = System.IO.Path.Combine(path, $"{conversationId}.json");
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed to delete conversation file");
                }
            }
        }
        
        /// <summary>
        /// 获取Token使用统计
        /// </summary>
        public TokenUsageStats GetTokenUsageStats()
        {
            if (!enableTokenTracking || tokenTracker == null)
            {
                return null;
            }
            
            return tokenTracker.GetStats();
        }
        
        public void Cleanup()
        {
            requestQueue?.Clear();
            activeRequests?.Clear();
            registeredNPCs?.Clear();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
    }
    
    /// <summary>
    /// NPC对话请求
    /// </summary>
    public class NPCDialogueRequest
    {
        public string requestId;
        public string npcId;
        public string npcName;
        public string conversationId;
        public string message;
        public DateTime createdAt;
        public int priority;
        
        public NPCDialogueRequest()
        {
            requestId = Guid.NewGuid().ToString();
            createdAt = DateTime.Now;
            priority = 0;
        }
    }
    
    /// <summary>
    /// NPC对话响应
    /// </summary>
    [Serializable]
    public class NPCDialogueResponse
    {
        public bool success;
        public string response;
        public string error;
        public string requestId;
        public int tokensUsed;
    }
    
    /// <summary>
    /// 对话历史数据
    /// </summary>
    [Serializable]
    public class ConversationHistoryData
    {
        public string conversationId;
        public ConversationMessage[] messages;
        public long lastUpdated;
    }
    
    /// <summary>
    /// Token使用追踪器
    /// </summary>
    public class TokenUsageTracker
    {
        private int maxTokensPerMinute;
        private int maxTokensPerHour;
        private Dictionary<long, int> minuteUsage;
        private Dictionary<long, int> hourUsage;
        
        public int CurrentMinuteUsage { get; private set; }
        public int CurrentHourUsage { get; private set; }
        
        public TokenUsageTracker(int maxPerMinute, int maxPerHour)
        {
            maxTokensPerMinute = maxPerMinute;
            maxTokensPerHour = maxPerHour;
            minuteUsage = new Dictionary<long, int>();
            hourUsage = new Dictionary<long, int>();
        }
        
        public void AddUsage(int tokens)
        {
            var minute = DateTime.Now.Ticks / TimeSpan.TicksPerMinute;
            var hour = DateTime.Now.Ticks / TimeSpan.TicksPerHour;
            
            if (!minuteUsage.ContainsKey(minute))
                minuteUsage[minute] = 0;
            minuteUsage[minute] += tokens;
            
            if (!hourUsage.ContainsKey(hour))
                hourUsage[hour] = 0;
            hourUsage[hour] += tokens;
            
            CurrentMinuteUsage = minuteUsage[minute];
            CurrentHourUsage = hourUsage[hour];
        }
        
        public bool IsLimitExceeded()
        {
            return CurrentMinuteUsage >= maxTokensPerMinute || CurrentHourUsage >= maxTokensPerHour;
        }
        
        public void Update()
        {
            // 清理过期数据
            var currentMinute = DateTime.Now.Ticks / TimeSpan.TicksPerMinute;
            var currentHour = DateTime.Now.Ticks / TimeSpan.TicksPerHour;
            
            var expiredMinutes = new List<long>();
            foreach (var kvp in minuteUsage)
            {
                if (currentMinute - kvp.Key > 1)
                    expiredMinutes.Add(kvp.Key);
            }
            foreach (var minute in expiredMinutes)
            {
                minuteUsage.Remove(minute);
            }
            
            var expiredHours = new List<long>();
            foreach (var kvp in hourUsage)
            {
                if (currentHour - kvp.Key > 1)
                    expiredHours.Add(kvp.Key);
            }
            foreach (var hour in expiredHours)
            {
                hourUsage.Remove(hour);
            }
        }
        
        public TokenUsageStats GetStats()
        {
            return new TokenUsageStats
            {
                currentMinuteUsage = CurrentMinuteUsage,
                currentHourUsage = CurrentHourUsage,
                maxTokensPerMinute = maxTokensPerMinute,
                maxTokensPerHour = maxTokensPerHour,
                minuteUsagePercent = (float)CurrentMinuteUsage / maxTokensPerMinute * 100f,
                hourUsagePercent = (float)CurrentHourUsage / maxTokensPerHour * 100f
            };
        }
    }
    
    /// <summary>
    /// Token使用统计
    /// </summary>
    [Serializable]
    public class TokenUsageStats
    {
        public int currentMinuteUsage;
        public int currentHourUsage;
        public int maxTokensPerMinute;
        public int maxTokensPerHour;
        public float minuteUsagePercent;
        public float hourUsagePercent;
    }
}
