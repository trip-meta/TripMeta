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
    /// GLM服务 — 智谱AI GLM系列模型集成（OpenAI兼容格式）
    /// 支持流式SSE响应，三级降级：GLM → Ollama → Mock
    /// </summary>
    public class GLMService : IGPTService
    {
        private readonly GPTConfig _config;
        private readonly Dictionary<string, GPTConversation> _conversations = new Dictionary<string, GPTConversation>();

        private bool _isInitialized;
        private bool _isPaused;
        private int _requestCount;
        private DateTime _windowStart = DateTime.UtcNow;

        // Fallback state
        private LLMBackend _activeBackend = LLMBackend.GLM;
        private int _consecutiveFailures;

        public bool IsInitialized => _isInitialized;
        public LLMBackend ActiveBackend => _activeBackend;

        public event Action<string, string> OnResponseReceived;
        public event Action<string> OnError;
        public event Action<string, string> OnStreamChunk;

        private enum LLMBackend { GLM, Ollama, Mock }

        public GLMService(GPTConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInfo("初始化GLM服务...", "GLM");
                ValidateConfig();
                await TestConnectionAsync();
                _isInitialized = true;
                _activeBackend = LLMBackend.GLM;
                _consecutiveFailures = 0;
                Logger.LogInfo($"GLM服务初始化完成 (model: {_config.model})", "GLM");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"GLM主服务初始化失败: {ex.Message}，尝试fallback", "GLM");

                if (_config.enableFallback)
                {
                    _activeBackend = LLMBackend.Ollama;
                    _isInitialized = true;
                    Logger.LogInfo("已降级到Ollama后端", "GLM");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<string> SendChatAsync(string message, string conversationId = null)
        {
            EnsureReady();
            await WaitForRateLimit();

            var conversation = GetOrCreateConversation(conversationId);
            conversation.AddMessage("user", message);

            try
            {
                var response = _activeBackend switch
                {
                    LLMBackend.GLM => await SendGLMRequestAsync(conversation),
                    LLMBackend.Ollama => await SendOllamaRequestAsync(conversation),
                    _ => GetMockResponse(message)
                };

                conversation.AddMessage("assistant", response);
                OnResponseReceived?.Invoke(message, response);
                ResetFailureCount();
                return response;
            }
            catch (Exception ex)
            {
                return await HandleFailureWithFallback(ex, message, conversation,
                    (conv) => SendChatInternalAsync(conv));
            }
        }

        public async Task<string> GenerateContentAsync(string prompt, GPTGenerationOptions options = null)
        {
            EnsureReady();
            await WaitForRateLimit();

            options ??= new GPTGenerationOptions();
            var tempConversation = new GPTConversation("gen_" + Guid.NewGuid().ToString("N"), 2);

            if (!string.IsNullOrEmpty(options.systemPrompt))
                tempConversation.AddMessage("system", options.systemPrompt);
            tempConversation.AddMessage("user", prompt);

            try
            {
                var response = _activeBackend switch
                {
                    LLMBackend.GLM => await SendGLMRequestAsync(tempConversation, options),
                    LLMBackend.Ollama => await SendOllamaRequestAsync(tempConversation),
                    _ => GetMockResponse(prompt)
                };

                ResetFailureCount();
                return response;
            }
            catch (Exception ex)
            {
                return await HandleFailureWithFallback(ex, prompt, tempConversation,
                    (conv) => SendChatInternalAsync(conv));
            }
        }

        public async Task SendStreamChatAsync(string message, Action<string> onPartialResponse, string conversationId = null)
        {
            EnsureReady();
            await WaitForRateLimit();

            var conversation = GetOrCreateConversation(conversationId);
            conversation.AddMessage("user", message);

            try
            {
                string fullResponse;
                if (_activeBackend == LLMBackend.GLM)
                    fullResponse = await SendGLMStreamRequestAsync(conversation, onPartialResponse);
                else if (_activeBackend == LLMBackend.Ollama)
                    fullResponse = await SendOllamaStreamRequestAsync(conversation, onPartialResponse);
                else
                    fullResponse = SimulateMockStream(message, onPartialResponse);

                conversation.AddMessage("assistant", fullResponse);
                OnResponseReceived?.Invoke(message, fullResponse);
                ResetFailureCount();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"流式请求失败 (backend: {_activeBackend})");

                if (_config.enableFallback && TryDegradeBackend())
                {
                    Logger.LogWarning($"降级到 {_activeBackend}，重试流式请求", "GLM");
                    await SendStreamChatAsync(message, onPartialResponse, conversationId);
                    return;
                }

                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        #region GLM API (OpenAI-compatible)

        private async Task<string> SendGLMRequestAsync(GPTConversation conversation, GPTGenerationOptions options = null)
        {
            var requestBody = BuildChatRequestBody(conversation, stream: false, options: options);
            var responseText = await SendHttpPostAsync(_config.apiEndpoint, requestBody, _config.apiKey);
            return ParseChatResponse(responseText);
        }

        private async Task<string> SendGLMStreamRequestAsync(GPTConversation conversation, Action<string> onPartialResponse, System.Threading.CancellationToken cancellationToken = default)
        {
            var requestBody = BuildChatRequestBody(conversation, stream: true);
            var json = JsonConvert.SerializeObject(requestBody);
            var bytes = Encoding.UTF8.GetBytes(json);

            using var webRequest = new UnityWebRequest(_config.apiEndpoint, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(bytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {_config.apiKey}");
            webRequest.timeout = (int)_config.requestTimeout;

            var operation = webRequest.SendWebRequest();
            var fullContent = new StringBuilder(1024);
            var lastLength = 0;

            while (!operation.isDone)
            {
                await Task.Delay(50, cancellationToken);

                if (webRequest.downloadHandler == null) continue;
                var currentText = webRequest.downloadHandler.text;
                if (currentText.Length <= lastLength) continue;

                var newData = currentText.Substring(lastLength);
                lastLength = currentText.Length;

                ParseSSEChunks(newData, fullContent, onPartialResponse, conversation.Id);
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
                throw new Exception($"GLM stream request failed: {webRequest.error}");

            // Parse any remaining data after completion
            var finalText = webRequest.downloadHandler?.text;
            if (!string.IsNullOrEmpty(finalText) && finalText.Length > lastLength)
            {
                var remaining = finalText.Substring(lastLength);
                ParseSSEChunks(remaining, fullContent, onPartialResponse, conversation.Id);
            }

            // Explicitly dispose handlers to prevent memory leaks
            webRequest.uploadHandler?.Dispose();
            webRequest.downloadHandler?.Dispose();

            return fullContent.ToString();
        }

        private void ParseSSEChunks(string data, StringBuilder fullContent, Action<string> onPartialResponse, string conversationId)
        {
            var lines = data.Split('\n');
            foreach (var line in lines)
            {
                if (!line.StartsWith("data: ")) continue;

                var jsonLine = line.Substring(6).Trim();
                if (jsonLine == "[DONE]") break;

                try
                {
                    var chunk = JsonConvert.DeserializeObject<dynamic>(jsonLine);
                    string content = chunk?.choices?[0]?.delta?.content?.ToString();
                    if (string.IsNullOrEmpty(content)) continue;

                    fullContent.Append(content);
                    onPartialResponse?.Invoke(fullContent.ToString());
                    OnStreamChunk?.Invoke(conversationId, content);
                }
                catch (JsonException)
                {
                    // Incomplete JSON chunk — skip, will be completed in next read
                }
            }
        }

        #endregion

        #region Ollama Fallback

        private async Task<string> SendOllamaRequestAsync(GPTConversation conversation)
        {
            var prompt = BuildPromptFromMessages(conversation.GetMessages());
            var requestBody = new { model = _config.ollamaModel, prompt, stream = false };
            var json = JsonConvert.SerializeObject(requestBody);
            var bytes = Encoding.UTF8.GetBytes(json);

            using var webRequest = new UnityWebRequest(_config.ollamaEndpoint, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(bytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = (int)_config.requestTimeout;

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (webRequest.result != UnityWebRequest.Result.Success)
                throw new Exception($"Ollama request failed: {webRequest.error}");

            var response = JsonConvert.DeserializeObject<dynamic>(webRequest.downloadHandler.text);
            return response?.response?.ToString() ?? "";
        }

        private async Task<string> SendOllamaStreamRequestAsync(GPTConversation conversation, Action<string> onPartialResponse)
        {
            var prompt = BuildPromptFromMessages(conversation.GetMessages());
            var requestBody = new { model = _config.ollamaModel, prompt, stream = true };
            var json = JsonConvert.SerializeObject(requestBody);
            var bytes = Encoding.UTF8.GetBytes(json);

            using var webRequest = new UnityWebRequest(_config.ollamaEndpoint, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(bytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.timeout = (int)_config.requestTimeout;

            var operation = webRequest.SendWebRequest();
            var fullContent = new StringBuilder();
            var lastLength = 0;

            while (!operation.isDone)
            {
                await Task.Delay(50);

                if (webRequest.downloadHandler == null) continue;
                var currentText = webRequest.downloadHandler.text;
                if (currentText.Length <= lastLength) continue;

                var newData = currentText.Substring(lastLength);
                lastLength = currentText.Length;

                var lines = newData.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var chunk = JsonConvert.DeserializeObject<dynamic>(line);
                        string content = chunk?.response?.ToString();
                        if (string.IsNullOrEmpty(content)) continue;

                        fullContent.Append(content);
                        onPartialResponse?.Invoke(fullContent.ToString());
                        OnStreamChunk?.Invoke(conversation.Id, content);
                    }
                    catch (JsonException) { }
                }
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
                throw new Exception($"Ollama stream failed: {webRequest.error}");

            return fullContent.ToString();
        }

        #endregion

        #region Mock Fallback

        private string GetMockResponse(string message)
        {
            Logger.LogWarning("使用Mock响应（所有LLM后端不可用）", "GLM");
            return $"[Mock] 感谢您的提问。当前AI服务暂时不可用，请稍后再试。您说的是：\"{message.Substring(0, Math.Min(30, message.Length))}...\"";
        }

        private string SimulateMockStream(string message, Action<string> onPartialResponse)
        {
            var response = GetMockResponse(message);
            onPartialResponse?.Invoke(response);
            return response;
        }

        #endregion

        #region Fallback & Recovery

        private bool TryDegradeBackend()
        {
            _consecutiveFailures++;

            if (_consecutiveFailures >= _config.fallbackRetryCount)
            {
                switch (_activeBackend)
                {
                    case LLMBackend.GLM:
                        _activeBackend = LLMBackend.Ollama;
                        _consecutiveFailures = 0;
                        Logger.LogWarning("GLM连续失败，降级到Ollama", "GLM");
                        return true;
                    case LLMBackend.Ollama:
                        _activeBackend = LLMBackend.Mock;
                        _consecutiveFailures = 0;
                        Logger.LogWarning("Ollama连续失败，降级到Mock", "GLM");
                        return true;
                    case LLMBackend.Mock:
                        return false;
                }
            }

            return true; // retry same backend
        }

        private const int MAX_FALLBACK_DEPTH = 3;

        private async Task<string> HandleFailureWithFallback(
            Exception ex, string message, GPTConversation conversation,
            Func<GPTConversation, Task<string>> retryFunc, int retryCount = 0)
        {
            Logger.LogException(ex, $"LLM请求失败 (backend: {_activeBackend}, retry: {retryCount})");

            if (retryCount >= MAX_FALLBACK_DEPTH)
            {
                Logger.LogError($"达到最大降级深度 {MAX_FALLBACK_DEPTH}，放弃重试", "GLM");
                OnError?.Invoke("服务暂时不可用，请稍后重试");
                throw new InvalidOperationException("All LLM backends failed after maximum retries", ex);
            }

            if (_config.enableFallback && TryDegradeBackend())
            {
                Logger.LogWarning($"降级到 {_activeBackend}，重试 (attempt: {retryCount + 1})", "GLM");
                try
                {
                    return await retryFunc(conversation);
                }
                catch (Exception retryEx)
                {
                    return await HandleFailureWithFallback(retryEx, message, conversation, retryFunc, retryCount + 1);
                }
            }

            OnError?.Invoke(ex.Message);
            throw ex;
        }

        private async Task<string> SendChatInternalAsync(GPTConversation conversation)
        {
            return _activeBackend switch
            {
                LLMBackend.GLM => await SendGLMRequestAsync(conversation),
                LLMBackend.Ollama => await SendOllamaRequestAsync(conversation),
                _ => GetMockResponse(conversation.GetMessages()[^1].content)
            };
        }

        private void ResetFailureCount()
        {
            _consecutiveFailures = 0;
        }

        #endregion

        #region Health & Lifecycle

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                await TestConnectionAsync();

                // If we were degraded, try to recover to GLM
                if (_activeBackend != LLMBackend.GLM)
                {
                    _activeBackend = LLMBackend.GLM;
                    _consecutiveFailures = 0;
                    Logger.LogInfo("GLM服务恢复，切回主后端", "GLM");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ReinitializeAsync()
        {
            _isInitialized = false;
            _activeBackend = LLMBackend.GLM;
            _consecutiveFailures = 0;
            await InitializeAsync();
        }

        public void Pause()
        {
            _isPaused = true;
            Logger.LogInfo("GLM服务已暂停", "GLM");
        }

        public void Resume()
        {
            _isPaused = false;
            Logger.LogInfo("GLM服务已恢复", "GLM");
        }

        public async Task DisposeAsync()
        {
            _conversations.Clear();
            _isInitialized = false;
            Logger.LogInfo("GLM服务资源已释放", "GLM");
            await Task.CompletedTask;
        }

        #endregion

        #region Conversation Management

        public GPTConversation GetConversation(string conversationId)
        {
            return _conversations.TryGetValue(conversationId ?? "default", out var conv) ? conv : null;
        }

        public void ClearConversation(string conversationId = null)
        {
            var id = conversationId ?? "default";
            if (_conversations.ContainsKey(id))
            {
                _conversations[id].Clear();
                Logger.LogInfo($"已清除对话历史: {id}", "GLM");
            }
        }

        private GPTConversation GetOrCreateConversation(string conversationId)
        {
            var id = conversationId ?? "default";
            if (!_conversations.TryGetValue(id, out var conv))
            {
                conv = new GPTConversation(id, _config.maxConversationLength);
                _conversations[id] = conv;
            }
            return conv;
        }

        #endregion

        #region Helpers

        private void EnsureReady()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("GLM服务未初始化");
            if (_isPaused)
                throw new InvalidOperationException("GLM服务已暂停");
        }

        private void ValidateConfig()
        {
            if (string.IsNullOrEmpty(_config.apiKey))
                throw new InvalidOperationException("GLM API密钥未配置");
            if (string.IsNullOrEmpty(_config.model))
                throw new InvalidOperationException("GLM模型未配置");
            if (_config.maxTokens <= 0)
                throw new InvalidOperationException("最大令牌数必须大于0");
        }

        private async Task TestConnectionAsync()
        {
            var testBody = new
            {
                model = _config.model,
                messages = new[] { new { role = "user", content = "Hello" } },
                max_tokens = 10
            };

            await SendHttpPostAsync(_config.apiEndpoint, testBody, _config.apiKey);
            Logger.LogInfo("GLM连接测试成功", "GLM");
        }

        private async Task WaitForRateLimit()
        {
            var now = DateTime.UtcNow;
            if ((now - _windowStart).TotalSeconds >= 60)
            {
                _requestCount = 0;
                _windowStart = now;
            }

            if (_requestCount >= _config.maxRequestsPerMinute)
            {
                var waitSeconds = 60 - (int)(now - _windowStart).TotalSeconds;
                if (waitSeconds > 0)
                {
                    Logger.LogWarning($"达到速率限制，等待 {waitSeconds}s", "GLM");
                    await Task.Delay(waitSeconds * 1000);
                    _requestCount = 0;
                    _windowStart = DateTime.UtcNow;
                }
            }

            _requestCount++;
        }

        private object BuildChatRequestBody(GPTConversation conversation, bool stream, GPTGenerationOptions options = null)
        {
            return new
            {
                model = _config.model,
                messages = conversation.GetMessages(),
                max_tokens = options?.maxTokens ?? _config.maxTokens,
                temperature = options?.temperature ?? _config.temperature,
                top_p = options?.topP ?? _config.topP,
                stream
            };
        }

        private async Task<string> SendHttpPostAsync(string url, object body, string apiKey)
        {
            var json = JsonConvert.SerializeObject(body);
            var maxRetries = _config.fallbackRetryCount;
            var delayMs = 1000;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                var bytes = Encoding.UTF8.GetBytes(json);

                using var request = new UnityWebRequest(url, "POST");
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                request.timeout = (int)_config.requestTimeout;

                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                    return request.downloadHandler.text;

                // 429 Rate Limited — exponential backoff retry
                if (request.responseCode == 429 && attempt < maxRetries)
                {
                    Logger.LogWarning($"GLM 429 Rate Limited, retry {attempt + 1}/{maxRetries} after {delayMs}ms", "GLM");
                    await Task.Delay(delayMs);
                    delayMs *= 2; // 1s → 2s → 4s
                    continue;
                }

                throw new Exception($"HTTP request failed ({request.responseCode}): {request.error} - {request.downloadHandler.text}");
            }

            throw new Exception("Max retries exceeded");
        }

        private static string ParseChatResponse(string responseJson)
        {
            var response = JsonConvert.DeserializeObject<dynamic>(responseJson);
            return response?.choices?[0]?.message?.content?.ToString()
                ?? throw new Exception("GLM响应格式异常：无法解析content字段");
        }

        private static string BuildPromptFromMessages(List<GPTMessage> messages)
        {
            var sb = new StringBuilder();
            foreach (var msg in messages)
                sb.AppendLine($"{msg.role}: {msg.content}");
            sb.Append("assistant: ");
            return sb.ToString();
        }

        #endregion
    }
}
