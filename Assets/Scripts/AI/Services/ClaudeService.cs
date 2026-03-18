using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.AI
{
    /// <summary>
    /// Claude服务 - Anthropic Claude-3.5集成
    /// 与GPT服务实现相同的IGPTService接口，实现双引擎架构
    /// </summary>
    public class ClaudeService : IGPTService
    {
        private readonly ClaudeConfig config;
        private readonly Queue<ClaudeRequest> requestQueue = new Queue<ClaudeRequest>();
        private readonly Dictionary<string, ClaudeConversation> conversations = new Dictionary<string, ClaudeConversation>();

        private bool isInitialized = false;
        private bool isPaused = false;
        private int requestCount = 0;
        private DateTime lastRequestTime = DateTime.MinValue;

        public bool IsInitialized => isInitialized;
        public event Action<string, string> OnResponseReceived;
        public event Action<string> OnError;

        public ClaudeService(ClaudeConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 初始化Claude服务
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInfo("初始化Claude服务...", "Claude");

                ValidateConfig();
                await TestConnectionAsync();

                isInitialized = true;
                Logger.LogInfo("Claude服务初始化完成", "Claude");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Claude服务初始化失败");
                throw;
            }
        }

        /// <summary>
        /// 发送聊天请求
        /// </summary>
        public async Task<string> SendChatAsync(string message, string conversationId = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("Claude服务未初始化");

            if (isPaused)
                throw new InvalidOperationException("Claude服务已暂停");

            try
            {
                await CheckRateLimitAsync();

                var conversation = GetOrCreateConversation(conversationId);
                conversation.AddMessage("user", message);

                var request = CreateChatRequest(conversation);
                var response = await SendRequestAsync(request);
                var assistantMessage = ParseChatResponse(response);

                conversation.AddMessage("assistant", assistantMessage);
                OnResponseReceived?.Invoke(message, assistantMessage);

                Logger.LogInfo($"Claude聊天完成: {message.Substring(0, Math.Min(50, message.Length))}...", "Claude");

                return assistantMessage;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Claude聊天请求失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 生成内容
        /// </summary>
        public async Task<string> GenerateContentAsync(string prompt, GPTGenerationOptions options = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("Claude服务未初始化");

            try
            {
                await CheckRateLimitAsync();

                var request = CreateGenerationRequest(prompt, options);
                var response = await SendRequestAsync(request);
                var content = ParseChatResponse(response);

                Logger.LogInfo($"Claude内容生成完成: {prompt.Substring(0, Math.Min(30, prompt.Length))}...", "Claude");

                return content;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Claude内容生成失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 流式聊天
        /// </summary>
        public async Task SendStreamChatAsync(string message, Action<string> onPartialResponse, string conversationId = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("Claude服务未初始化");

            try
            {
                await CheckRateLimitAsync();

                var conversation = GetOrCreateConversation(conversationId);
                conversation.AddMessage("user", message);

                var request = CreateStreamChatRequest(conversation);
                await SendStreamRequestAsync(request, onPartialResponse);

                Logger.LogInfo($"Claude流式聊天完成: {message.Substring(0, Math.Min(50, message.Length))}...", "Claude");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Claude流式聊天失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 获取对话历史
        /// </summary>
        public GPTConversation GetConversation(string conversationId)
        {
            var id = conversationId ?? "default";
            if (conversations.TryGetValue(id, out var claudeConv))
            {
                // 转换为GPTConversation格式
                var gptConv = new GPTConversation(id, config.maxConversationLength);
                foreach (var msg in claudeConv.GetMessages())
                {
                    gptConv.AddMessage(msg.role, msg.content);
                }
                return gptConv;
            }
            return null;
        }

        /// <summary>
        /// 清除对话历史
        /// </summary>
        public void ClearConversation(string conversationId = null)
        {
            var id = conversationId ?? "default";
            if (conversations.ContainsKey(id))
            {
                conversations[id].Clear();
                Logger.LogInfo($"已清除Claude对话历史: {id}", "Claude");
            }
        }

        /// <summary>
        /// 获取或创建对话
        /// </summary>
        private ClaudeConversation GetOrCreateConversation(string conversationId)
        {
            var id = conversationId ?? "default";

            if (!conversations.TryGetValue(id, out var conversation))
            {
                conversation = new ClaudeConversation(id, config.maxConversationLength);
                conversations[id] = conversation;
            }

            return conversation;
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        private void ValidateConfig()
        {
            if (string.IsNullOrEmpty(config.apiKey))
                throw new InvalidOperationException("Claude API密钥未配置");

            if (string.IsNullOrEmpty(config.model))
                throw new InvalidOperationException("Claude模型未配置");

            if (config.maxTokens <= 0)
                throw new InvalidOperationException("最大令牌数必须大于0");
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        private async Task TestConnectionAsync()
        {
            try
            {
                var testRequest = new
                {
                    model = config.model,
                    max_tokens = 10,
                    messages = new[]
                    {
                        new { role = "user", content = "Hello" }
                    }
                };

                await SendRequestAsync(testRequest);
                Logger.LogInfo("Claude连接测试成功", "Claude");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Claude连接测试失败");
                throw;
            }
        }

        /// <summary>
        /// 检查速率限制
        /// </summary>
        private async Task CheckRateLimitAsync()
        {
            var now = DateTime.Now;
            var timeSinceLastRequest = now - lastRequestTime;

            if (timeSinceLastRequest.TotalMinutes >= 1)
            {
                requestCount = 0;
            }

            if (requestCount >= config.maxRequestsPerMinute)
            {
                var waitTime = 60 - (int)timeSinceLastRequest.TotalSeconds;
                if (waitTime > 0)
                {
                    Logger.LogWarning($"达到Claude速率限制，等待 {waitTime} 秒", "Claude");
                    await Task.Delay(waitTime * 1000);
                    requestCount = 0;
                }
            }

            requestCount++;
            lastRequestTime = now;
        }

        /// <summary>
        /// 创建聊天请求
        /// </summary>
        private object CreateChatRequest(ClaudeConversation conversation)
        {
            return new
            {
                model = config.model,
                max_tokens = config.maxTokens,
                temperature = config.temperature,
                top_p = config.topP,
                messages = conversation.GetMessages()
            };
        }

        /// <summary>
        /// 创建内容生成请求
        /// </summary>
        private object CreateGenerationRequest(string prompt, GPTGenerationOptions options)
        {
            options = options ?? new GPTGenerationOptions();

            var messages = new List<object>();
            if (!string.IsNullOrEmpty(options.systemPrompt))
            {
                messages.Add(new { role = "system", content = options.systemPrompt });
            }
            messages.Add(new { role = "user", content = prompt });

            return new
            {
                model = config.model,
                max_tokens = options.maxTokens ?? config.maxTokens,
                temperature = options.temperature ?? config.temperature,
                top_p = options.topP ?? config.topP,
                messages = messages.ToArray()
            };
        }

        /// <summary>
        /// 创建流式聊天请求
        /// </summary>
        private object CreateStreamChatRequest(ClaudeConversation conversation)
        {
            var request = CreateChatRequest(conversation);
            var requestDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(request));
            requestDict["stream"] = true;
            return requestDict;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        private async Task<string> SendRequestAsync(object requestData)
        {
            var json = JsonConvert.SerializeObject(requestData);
            var bytes = Encoding.UTF8.GetBytes(json);

            using (var request = new UnityWebRequest(config.apiEndpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-api-key", config.apiKey);
                request.SetRequestHeader("anthropic-version", config.apiVersion);
                request.timeout = (int)config.requestTimeout;

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"Claude请求失败: {request.error} - {request.downloadHandler.text}");
                }

                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// 发送流式请求
        /// </summary>
        private async Task SendStreamRequestAsync(object requestData, Action<string> onPartialResponse)
        {
            var response = await SendRequestAsync(requestData);
            var content = ParseChatResponse(response);

            var words = content.Split(' ');
            var currentText = "";

            foreach (var word in words)
            {
                currentText += word + " ";
                onPartialResponse?.Invoke(currentText.Trim());
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// 解析聊天响应
        /// </summary>
        private string ParseChatResponse(string response)
        {
            try
            {
                var responseObj = JsonConvert.DeserializeObject<dynamic>(response);
                return responseObj.content[0].text.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "解析Claude响应失败");
                throw new Exception($"解析Claude响应失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查健康状态
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                await TestConnectionAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 重新初始化
        /// </summary>
        public async Task ReinitializeAsync()
        {
            isInitialized = false;
            await InitializeAsync();
        }

        /// <summary>
        /// 暂停服务
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            Logger.LogInfo("Claude服务已暂停", "Claude");
        }

        /// <summary>
        /// 恢复服务
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            Logger.LogInfo("Claude服务已恢复", "Claude");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                conversations.Clear();
                requestQueue.Clear();
                isInitialized = false;

                Logger.LogInfo("Claude服务资源已释放", "Claude");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Claude服务资源释放失败");
            }
        }
    }

    /// <summary>
    /// Claude配置
    /// </summary>
    [Serializable]
    public class ClaudeConfig
    {
        [Header("API设置")]
        public string apiKey = "";
        public string apiEndpoint = "https://api.anthropic.com/v1/messages";
        public string apiVersion = "2023-06-01";
        public string model = "claude-3-5-sonnet-20241022";

        [Header("生成参数")]
        public int maxTokens = 2000;
        public float temperature = 0.7f;
        public float topP = 1.0f;

        [Header("限制设置")]
        public int maxRequestsPerMinute = 60;
        public float requestTimeout = 30f;
        public int maxConversationLength = 20;
    }

    /// <summary>
    /// Claude对话
    /// </summary>
    public class ClaudeConversation
    {
        public string Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdated { get; private set; }

        private readonly List<ClaudeMessage> messages = new List<ClaudeMessage>();
        private readonly int maxLength;

        public ClaudeConversation(string id, int maxLength = 20)
        {
            Id = id;
            CreatedAt = DateTime.Now;
            LastUpdated = DateTime.Now;
            this.maxLength = maxLength;
        }

        public void AddMessage(string role, string content)
        {
            messages.Add(new ClaudeMessage { role = role, content = content });
            LastUpdated = DateTime.Now;

            while (messages.Count > maxLength)
            {
                messages.RemoveAt(0);
            }
        }

        public List<ClaudeMessage> GetMessages()
        {
            return new List<ClaudeMessage>(messages);
        }

        public void Clear()
        {
            messages.Clear();
            LastUpdated = DateTime.Now;
        }

        public int MessageCount => messages.Count;
    }

    /// <summary>
    /// Claude消息
    /// </summary>
    [Serializable]
    public class ClaudeMessage
    {
        public string role;
        public string content;
    }

    /// <summary>
    /// Claude请求
    /// </summary>
    public class ClaudeRequest
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string Message { get; set; }
        public GPTGenerationOptions Options { get; set; }
        public DateTime CreatedAt { get; set; }
        public Action<string> OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }
    }
}
