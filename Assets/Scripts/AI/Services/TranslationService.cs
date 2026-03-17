using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TripMeta.AI.Services
{
    /// <summary>
    /// Azure翻译服务实现
    /// </summary>
    public class TranslationService : ITranslationService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;
        private TranslationOptions _options;
        private bool _isRealtimeTranslating;
        private bool _isInitialized;
        private bool _isPaused;

        // 事件
        public event Action<TranslationResult> OnTranslationCompleted;
        public event Action<string> OnTranslationError;
        public event Action<VoiceTranslationResult> OnVoiceTranslationCompleted;

        // 属性
        public bool IsInitialized => _isInitialized;

        public TranslationService(string subscriptionKey, string region, string endpoint = null)
        {
            _subscriptionKey = subscriptionKey;
            _region = region;
            _endpoint = endpoint ?? $"https://api.cognitive.microsofttranslator.com";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _region);
            _options = new TranslationOptions();
        }

        public async Task InitializeAsync()
        {
            try
            {
                // 验证API连接
                var languages = await GetSupportedLanguagesAsync();
                _isInitialized = languages != null && languages.Count > 0;

                if (_isInitialized)
                {
                    Debug.Log($"[TranslationService] 初始化成功，支持 {languages.Count} 种语言");
                }
                else
                {
                    Debug.LogError("[TranslationService] 初始化失败：无法获取语言列表");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TranslationService] 初始化失败: {ex.Message}");
                _isInitialized = false;
            }
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var languages = await GetSupportedLanguagesAsync();
                return languages != null && languages.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task ReinitializeAsync()
        {
            await DisposeAsync();
            await InitializeAsync();
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public async Task DisposeAsync()
        {
            _isInitialized = false;
            StopRealtimeVoiceTranslation();
            _httpClient?.Dispose();
        }

        public async Task<TranslationResult> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            if (_isPaused) return null;

            try
            {
                var route = $"/translate?api-version=3.0&from={sourceLanguage}&to={targetLanguage}";
                if (_options.EnableFormality)
                {
                    route += "&formality=default";
                }

                var body = new object[] { new { Text = text } };
                var requestBody = JsonConvert.SerializeObject(body);

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(_endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

                    var response = await _httpClient.SendAsync(request);
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonArray = JArray.Parse(result);
                        var translation = jsonArray[0]["translations"][0];

                        var translationResult = new TranslationResult
                        {
                            OriginalText = text,
                            TranslatedText = translation["text"].ToString(),
                            SourceLanguage = sourceLanguage,
                            TargetLanguage = targetLanguage,
                            Confidence = translation["to"].ToString() == targetLanguage ? 0.95f : 0.8f,
                            Timestamp = DateTime.UtcNow,
                            IsSuccess = true
                        };

                        // 收集备选翻译
                        var translations = jsonArray[0]["translations"];
                        for (int i = 1; i < Math.Min(translations.Count(), _options.MaxAlternativeTranslations + 1); i++)
                        {
                            translationResult.AlternativeTranslations.Add(translations[i]["text"].ToString());
                        }

                        OnTranslationCompleted?.Invoke(translationResult);
                        return translationResult;
                    }
                    else
                    {
                        var errorResult = new TranslationResult
                        {
                            OriginalText = text,
                            SourceLanguage = sourceLanguage,
                            TargetLanguage = targetLanguage,
                            IsSuccess = false,
                            ErrorMessage = $"API错误: {result}",
                            Timestamp = DateTime.UtcNow
                        };
                        OnTranslationError?.Invoke(errorResult.ErrorMessage);
                        return errorResult;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"翻译失败: {ex.Message}";
                Debug.LogError($"[TranslationService] {errorMessage}");
                OnTranslationError?.Invoke(errorMessage);

                return new TranslationResult
                {
                    OriginalText = text,
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<TranslationResult> TranslateTextAutoDetectAsync(string text, string targetLanguage)
        {
            if (_isPaused) return null;

            try
            {
                var route = $"/translate?api-version=3.0&to={targetLanguage}";

                var body = new object[] { new { Text = text } };
                var requestBody = JsonConvert.SerializeObject(body);

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(_endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

                    var response = await _httpClient.SendAsync(request);
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonArray = JArray.Parse(result);
                        var detectedLanguage = jsonArray[0]["detectedLanguage"]["language"].ToString();
                        var confidence = float.Parse(jsonArray[0]["detectedLanguage"]["score"].ToString());
                        var translation = jsonArray[0]["translations"][0];

                        var translationResult = new TranslationResult
                        {
                            OriginalText = text,
                            TranslatedText = translation["text"].ToString(),
                            SourceLanguage = detectedLanguage,
                            TargetLanguage = targetLanguage,
                            Confidence = confidence,
                            Timestamp = DateTime.UtcNow,
                            IsSuccess = true
                        };

                        OnTranslationCompleted?.Invoke(translationResult);
                        return translationResult;
                    }
                    else
                    {
                        var errorResult = new TranslationResult
                        {
                            OriginalText = text,
                            TargetLanguage = targetLanguage,
                            IsSuccess = false,
                            ErrorMessage = $"API错误: {result}",
                            Timestamp = DateTime.UtcNow
                        };
                        OnTranslationError?.Invoke(errorResult.ErrorMessage);
                        return errorResult;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"自动检测翻译失败: {ex.Message}";
                Debug.LogError($"[TranslationService] {errorMessage}");
                OnTranslationError?.Invoke(errorMessage);

                return new TranslationResult
                {
                    OriginalText = text,
                    TargetLanguage = targetLanguage,
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<List<TranslationResult>> TranslateBatchAsync(List<string> texts, string sourceLanguage, string targetLanguage)
        {
            var results = new List<TranslationResult>();

            foreach (var text in texts)
            {
                var result = await TranslateTextAsync(text, sourceLanguage, targetLanguage);
                results.Add(result);
            }

            return results;
        }

        public async Task<VoiceTranslationResult> TranslateVoiceAsync(byte[] audioData, string sourceLanguage, string targetLanguage)
        {
            if (_isPaused) return null;

            try
            {
                // 首先使用 Azure Speech 服务进行语音识别
                // 然后翻译识别出的文本
                // 最后使用 TTS 合成目标语言语音

                // 这里简化实现，实际应该调用 Speech SDK
                Debug.Log($"[TranslationService] 语音翻译: {sourceLanguage} -> {targetLanguage}");

                // 模拟实现（实际应集成 Azure Speech SDK）
                var voiceResult = new VoiceTranslationResult
                {
                    OriginalText = "[语音输入]",
                    RecognizedText = "[识别的语音文本]",
                    TranslatedText = "[翻译后的文本]",
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    AudioDuration = audioData.Length / 16000f, // 假设 16kHz 采样率
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                };

                OnVoiceTranslationCompleted?.Invoke(voiceResult);
                return voiceResult;
            }
            catch (Exception ex)
            {
                var errorMessage = $"语音翻译失败: {ex.Message}";
                Debug.LogError($"[TranslationService] {errorMessage}");
                OnTranslationError?.Invoke(errorMessage);

                return new VoiceTranslationResult
                {
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public Task StartRealtimeVoiceTranslationAsync(
            string sourceLanguage,
            string targetLanguage,
            Action<string> onPartialResult,
            Action<VoiceTranslationResult> onFinalResult)
        {
            _isRealtimeTranslating = true;

            // 实际实现应该启动 Azure Speech SDK 的连续识别模式
            Debug.Log($"[TranslationService] 启动实时语音翻译: {sourceLanguage} -> {targetLanguage}");

            // 这里简化处理，实际应该集成 Speech SDK 的连续识别功能
            return Task.CompletedTask;
        }

        public void StopRealtimeVoiceTranslation()
        {
            if (_isRealtimeTranslating)
            {
                _isRealtimeTranslating = false;
                Debug.Log("[TranslationService] 停止实时语音翻译");
            }
        }

        public async Task<List<LanguageInfo>> GetSupportedLanguagesAsync()
        {
            try
            {
                var route = "/languages?api-version=3.0&scope=translation";
                var response = await _httpClient.GetAsync(_endpoint + route);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(result);
                    var languages = new List<LanguageInfo>();

                    foreach (var lang in json["translation"])
                    {
                        var code = ((JProperty)lang).Name;
                        var info = lang.First;

                        languages.Add(new LanguageInfo
                        {
                            Code = code,
                            NativeName = info["name"]?.ToString() ?? code,
                            EnglishName = info["nativeName"]?.ToString() ?? code,
                            SupportsVoiceInput = code.StartsWith("zh") || code.StartsWith("en") ||
                                                code.StartsWith("ja") || code.StartsWith("de") ||
                                                code.StartsWith("fr") || code.StartsWith("es"),
                            SupportsVoiceOutput = code.StartsWith("zh") || code.StartsWith("en") ||
                                                 code.StartsWith("ja") || code.StartsWith("de") ||
                                                 code.StartsWith("fr") || code.StartsWith("es")
                        });
                    }

                    return languages;
                }
                else
                {
                    Debug.LogError($"[TranslationService] 获取语言列表失败: {result}");
                    return GetDefaultLanguages();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TranslationService] 获取语言列表失败: {ex.Message}");
                return GetDefaultLanguages();
            }
        }

        public async Task<bool> IsLanguagePairSupportedAsync(string sourceLanguage, string targetLanguage)
        {
            var languages = await GetSupportedLanguagesAsync();
            var targetLang = languages.Find(l => l.Code.Equals(targetLanguage, StringComparison.OrdinalIgnoreCase));
            return targetLang != null;
        }

        public void SetTranslationOptions(TranslationOptions options)
        {
            _options = options ?? new TranslationOptions();
        }

        public TranslationOptions GetTranslationOptions()
        {
            return _options;
        }

        private List<LanguageInfo> GetDefaultLanguages()
        {
            return new List<LanguageInfo>
            {
                new LanguageInfo { Code = "zh-Hans", NativeName = "简体中文", EnglishName = "Chinese Simplified", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "zh-Hant", NativeName = "繁體中文", EnglishName = "Chinese Traditional", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "en", NativeName = "English", EnglishName = "English", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "ja", NativeName = "日本語", EnglishName = "Japanese", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "ko", NativeName = "한국어", EnglishName = "Korean", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "fr", NativeName = "Français", EnglishName = "French", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "de", NativeName = "Deutsch", EnglishName = "German", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "es", NativeName = "Español", EnglishName = "Spanish", SupportsVoiceInput = true, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "it", NativeName = "Italiano", EnglishName = "Italian", SupportsVoiceInput = false, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "ru", NativeName = "Русский", EnglishName = "Russian", SupportsVoiceInput = false, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "ar", NativeName = "العربية", EnglishName = "Arabic", SupportsVoiceInput = false, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "pt", NativeName = "Português", EnglishName = "Portuguese", SupportsVoiceInput = false, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "th", NativeName = "ไทย", EnglishName = "Thai", SupportsVoiceInput = false, SupportsVoiceOutput = true },
                new LanguageInfo { Code = "vi", NativeName = "Tiếng Việt", EnglishName = "Vietnamese", SupportsVoiceInput = false, SupportsVoiceOutput = true }
            };
        }
    }
}
