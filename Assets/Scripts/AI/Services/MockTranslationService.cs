using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.AI.Services
{
    /// <summary>
    /// 翻译服务模拟实现 - 用于开发和测试
    /// </summary>
    public class MockTranslationService : ITranslationService
    {
        private bool _isInitialized;
        private TranslationOptions _options;
        private Dictionary<string, Dictionary<string, string>> _mockTranslations;

        public bool IsInitialized => _isInitialized;

        public event Action<TranslationResult> OnTranslationCompleted;
        public event Action<string> OnTranslationError;
        public event Action<VoiceTranslationResult> OnVoiceTranslationCompleted;

        public MockTranslationService()
        {
            _options = new TranslationOptions();
            InitializeMockTranslations();
        }

        private void InitializeMockTranslations()
        {
            _mockTranslations = new Dictionary<string, Dictionary<string, string>>
            {
                // 简体中文 -> 英文
                ["zh-Hans"] = new Dictionary<string, string>
                {
                    ["欢迎来到 TripMeta"] = "Welcome to TripMeta",
                    ["你好"] = "Hello",
                    ["谢谢"] = "Thank you",
                    ["再见"] = "Goodbye",
                    ["今天天气很好"] = "The weather is nice today",
                    ["这个景点很漂亮"] = "This attraction is beautiful",
                    ["我需要帮助"] = "I need help",
                    ["请问洗手间在哪里"] = "Where is the restroom?",
                    ["这个多少钱"] = "How much is this?",
                    ["我想要一杯咖啡"] = "I would like a cup of coffee"
                },
                // 英文 -> 简体中文
                ["en"] = new Dictionary<string, string>
                {
                    ["Welcome to TripMeta"] = "欢迎来到 TripMeta",
                    ["Hello"] = "你好",
                    ["Thank you"] = "谢谢",
                    ["Goodbye"] = "再见",
                    ["The weather is nice today"] = "今天天气很好",
                    ["This attraction is beautiful"] = "这个景点很漂亮",
                    ["I need help"] = "我需要帮助",
                    ["Where is the restroom?"] = "请问洗手间在哪里",
                    ["How much is this?"] = "这个多少钱",
                    ["I would like a cup of coffee"] = "我想要一杯咖啡"
                },
                // 日文
                ["ja"] = new Dictionary<string, string>
                {
                    ["Welcome to TripMeta"] = "TripMetaへようこそ",
                    ["Hello"] = "こんにちは",
                    ["Thank you"] = "ありがとう"
                }
            };
        }

        public Task InitializeAsync()
        {
            _isInitialized = true;
            Debug.Log("[MockTranslationService] 模拟翻译服务初始化完成");
            return Task.CompletedTask;
        }

        public Task<bool> CheckHealthAsync()
        {
            return Task.FromResult(true);
        }

        public Task ReinitializeAsync()
        {
            return InitializeAsync();
        }

        public void Pause() { }

        public void Resume() { }

        public Task DisposeAsync()
        {
            _isInitialized = false;
            return Task.CompletedTask;
        }

        public async Task<TranslationResult> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            await Task.Delay(100); // 模拟网络延迟

            string translatedText = text;
            float confidence = 0.95f;

            // 尝试从模拟字典中查找翻译
            if (_mockTranslations.TryGetValue(sourceLanguage, out var translations) &&
                translations.TryGetValue(text, out var translation))
            {
                translatedText = translation;
                confidence = 1.0f;
            }
            else if (sourceLanguage != targetLanguage)
            {
                // 如果没有找到翻译，生成一个模拟翻译
                translatedText = $"[{targetLanguage}] {text}";
                confidence = 0.7f;
            }

            var result = new TranslationResult
            {
                OriginalText = text,
                TranslatedText = translatedText,
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage,
                Confidence = confidence,
                Timestamp = DateTime.UtcNow,
                IsSuccess = true,
                AlternativeTranslations = new List<string> { translatedText + " (alt 1)" }
            };

            OnTranslationCompleted?.Invoke(result);
            return result;
        }

        public async Task<TranslationResult> TranslateTextAutoDetectAsync(string text, string targetLanguage)
        {
            // 模拟语言检测
            string detectedLanguage = DetectLanguage(text);
            return await TranslateTextAsync(text, detectedLanguage, targetLanguage);
        }

        public async Task<List<TranslationResult>> TranslateBatchAsync(List<string> texts, string sourceLanguage, string targetLanguage)
        {
            var results = new List<TranslationResult>();
            foreach (var text in texts)
            {
                results.Add(await TranslateTextAsync(text, sourceLanguage, targetLanguage));
            }
            return results;
        }

        public async Task<VoiceTranslationResult> TranslateVoiceAsync(byte[] audioData, string sourceLanguage, string targetLanguage)
        {
            await Task.Delay(200); // 模拟处理时间

            // 模拟语音识别和翻译
            var recognizedText = $"[模拟识别: {audioData.Length} bytes]";
            var translatedText = await TranslateTextAsync(recognizedText, sourceLanguage, targetLanguage);

            var result = new VoiceTranslationResult
            {
                OriginalText = audioData.ToString(),
                RecognizedText = recognizedText,
                TranslatedText = translatedText.TranslatedText,
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage,
                AudioDuration = audioData.Length / 16000f,
                RecognitionConfidence = 0.85f,
                IsSuccess = true,
                Timestamp = DateTime.UtcNow
            };

            OnVoiceTranslationCompleted?.Invoke(result);
            return result;
        }

        public Task StartRealtimeVoiceTranslationAsync(
            string sourceLanguage,
            string targetLanguage,
            Action<string> onPartialResult,
            Action<VoiceTranslationResult> onFinalResult)
        {
            Debug.Log($"[MockTranslationService] 启动实时语音翻译模拟: {sourceLanguage} -> {targetLanguage}");

            // 模拟部分结果
            Task.Run(async () =>
            {
                await Task.Delay(500);
                onPartialResult?.Invoke("[模拟部分识别...]");

                await Task.Delay(500);
                onPartialResult?.Invoke("[模拟部分识别继续...]");

                await Task.Delay(500);
                var result = new VoiceTranslationResult
                {
                    RecognizedText = "[模拟实时识别文本]",
                    TranslatedText = "[Simulated translation]",
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                };
                onFinalResult?.Invoke(result);
            });

            return Task.CompletedTask;
        }

        public void StopRealtimeVoiceTranslation()
        {
            Debug.Log("[MockTranslationService] 停止实时语音翻译模拟");
        }

        public Task<List<LanguageInfo>> GetSupportedLanguagesAsync()
        {
            var languages = new List<LanguageInfo>
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
                new LanguageInfo { Code = "pt", NativeName = "Português", EnglishName = "Portuguese", SupportsVoiceInput = false, SupportsVoiceOutput = true }
            };

            return Task.FromResult(languages);
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

        private string DetectLanguage(string text)
        {
            // 简单的语言检测逻辑
            if (text.Contains("你好") || text.Contains("谢谢") || text.Contains("再见"))
                return "zh-Hans";
            if (text.Contains("こんにちは") || text.Contains("ありがとう"))
                return "ja";
            if (text.Contains("안녕하세요") || text.Contains("감사합니다"))
                return "ko";

            return "en"; // 默认为英文
        }
    }
}
