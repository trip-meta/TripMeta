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
    /// GPT服务 - OpenAI GPT-4集成
    /// </summary>
    public class GPTService : IGPTService
    {
        private readonly GPTConfig config;
        private readonly Queue<GPTRequest> requestQueue = new Queue<GPTRequest>();
        private readonly Dictionary<string, GPTConversation> conversations = new Dictionary<string, GPTConversation>();
        
        private bool isInitialized = false;
        private bool isPaused = false;
        private int requestCount = 0;
        private DateTime lastRequestTime = DateTime.MinValue;
        
        public bool IsInitialized => isInitialized;
        public event Action<string, string> OnResponseReceived;
        public event Action<string> OnError;
        
        public GPTService(GPTConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// 初始化GPT服务
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInfo("初始化GPT服务...", "GPT");
                
                // 验证配置
                ValidateConfig();
                
                // 测试连接
                await TestConnectionAsync();
                
                isInitialized = true;
                Logger.LogInfo("GPT服务初始化完成", "GPT");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT服务初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 发送聊天请求
        /// </summary>
        public async Task<string> SendChatAsync(string message, string conversationId = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("GPT服务未初始化");
            
            if (isPaused)
                throw new InvalidOperationException("GPT服务已暂停");
            
            try
            {
                // 检查速率限制
                await CheckRateLimitAsync();
                
                // 获取或创建对话
                var conversation = GetOrCreateConversation(conversationId);
                
                // 添加用户消息
                conversation.AddMessage("user", message);
                
                // 构建请求
                var request = CreateChatRequest(conversation);
                
                // 发送请求
                var response = await SendRequestAsync(request);
                
                // 解析响应
                var assistantMessage = ParseChatResponse(response);
                
                // 添加助手回复到对话
                conversation.AddMessage("assistant", assistantMessage);
                
                // 触发事件
                OnResponseReceived?.Invoke(message, assistantMessage);
                
                Logger.LogInfo($"GPT聊天完成: {message.Substring(0, Math.Min(50, message.Length))}...", "GPT");
                
                return assistantMessage;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT聊天请求失败");
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
                throw new InvalidOperationException("GPT服务未初始化");
            
            try
            {
                await CheckRateLimitAsync();
                
                var request = CreateGenerationRequest(prompt, options);
                var response = await SendRequestAsync(request);
                var content = ParseGenerationResponse(response);
                
                Logger.LogInfo($"GPT内容生成完成: {prompt.Substring(0, Math.Min(30, prompt.Length))}...", "GPT");
                
                return content;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT内容生成失败");
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
                throw new InvalidOperationException("GPT服务未初始化");
            
            try
            {
                await CheckRateLimitAsync();
                
                var conversation = GetOrCreateConversation(conversationId);
                conversation.AddMessage("user", message);
                
                var request = CreateStreamChatRequest(conversation);
                
                await SendStreamRequestAsync(request, onPartialResponse);
                
                Logger.LogInfo($"GPT流式聊天完成: {message.Substring(0, Math.Min(50, message.Length))}...", "GPT");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT流式聊天失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// 获取对话历史
        /// </summary>
        public GPTConversation GetConversation(string conversationId)
        {
            return conversations.TryGetValue(conversationId ?? "default", out var conversation) 
                ? conversation 
                : null;
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
                Logger.LogInfo($"已清除对话历史: {id}", "GPT");
            }
        }
        
        /// <summary>
        /// 获取或创建对话
        /// </summary>
        private GPTConversation GetOrCreateConversation(string conversationId)
        {
            var id = conversationId ?? "default";
            
            if (!conversations.TryGetValue(id, out var conversation))
            {
                conversation = new GPTConversation(id, config.maxConversationLength);
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
                throw new InvalidOperationException("GPT API密钥未配置");
            
            if (string.IsNullOrEmpty(config.model))
                throw new InvalidOperationException("GPT模型未配置");
            
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
                    messages = new[]
                    {
                        new { role = "user", content = "Hello" }
                    },
                    max_tokens = 10
                };
                
                await SendRequestAsync(testRequest);
                Logger.LogInfo("GPT连接测试成功", "GPT");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT连接测试失败");
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
            
            // 重置计数器（每分钟）
            if (timeSinceLastRequest.TotalMinutes >= 1)
            {
                requestCount = 0;
            }
            
            // 检查速率限制
            if (requestCount >= config.maxRequestsPerMinute)
            {
                var waitTime = 60 - (int)timeSinceLastRequest.TotalSeconds;
                if (waitTime > 0)
                {
                    Logger.LogWarning($"达到速率限制，等待 {waitTime} 秒", "GPT");
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
        private object CreateChatRequest(GPTConversation conversation)
        {
            return new
            {
                model = config.model,
                messages = conversation.GetMessages(),
                max_tokens = config.maxTokens,
                temperature = config.temperature,
                top_p = config.topP,
                frequency_penalty = config.frequencyPenalty,
                presence_penalty = config.presencePenalty
            };
        }
        
        /// <summary>
        /// 创建内容生成请求
        /// </summary>
        private object CreateGenerationRequest(string prompt, GPTGenerationOptions options)
        {
            options = options ?? new GPTGenerationOptions();
            
            return new
            {
                model = config.model,
                messages = new[]
                {
                    new { role = "system", content = options.systemPrompt ?? "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                },
                max_tokens = options.maxTokens ?? config.maxTokens,
                temperature = options.temperature ?? config.temperature,
                top_p = options.topP ?? config.topP
            };
        }
        
        /// <summary>
        /// 创建流式聊天请求
        /// </summary>
        private object CreateStreamChatRequest(GPTConversation conversation)
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
                request.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");
                request.timeout = (int)config.requestTimeout;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"GPT请求失败: {request.error} - {request.downloadHandler.text}");
                }
                
                return request.downloadHandler.text;
            }
        }
        
        /// <summary>
        /// 发送流式请求
        /// </summary>
        private async Task SendStreamRequestAsync(object requestData, Action<string> onPartialResponse)
        {
            // 简化实现，实际应该使用Server-Sent Events
            var response = await SendRequestAsync(requestData);
            var content = ParseChatResponse(response);
            
            // 模拟流式响应
            var words = content.Split(' ');
            var currentText = "";
            
            foreach (var word in words)
            {
                currentText += word + " ";
                onPartialResponse?.Invoke(currentText.Trim());
                await Task.Delay(50); // 模拟延迟
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
                return responseObj.choices[0].message.content.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "解析GPT响应失败");
                throw new Exception($"解析GPT响应失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 解析内容生成响应
        /// </summary>
        private string ParseGenerationResponse(string response)
        {
            return ParseChatResponse(response);
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
            Logger.LogInfo("GPT服务已暂停", "GPT");
        }
        
        /// <summary>
        /// 恢复服务
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            Logger.LogInfo("GPT服务已恢复", "GPT");
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
                
                Logger.LogInfo("GPT服务资源已释放", "GPT");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT服务资源释放失败");
            }
        }
    }
}