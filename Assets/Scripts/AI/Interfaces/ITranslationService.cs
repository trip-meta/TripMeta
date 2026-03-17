using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TripMeta.AI
{
    /// <summary>
    /// 翻译服务接口 - 提供实时翻译功能
    /// </summary>
    public interface ITranslationService : IAIService
    {
        /// <summary>
        /// 翻译完成事件
        /// </summary>
        event Action<TranslationResult> OnTranslationCompleted;

        /// <summary>
        /// 翻译错误事件
        /// </summary>
        event Action<string> OnTranslationError;

        /// <summary>
        /// 语音翻译完成事件
        /// </summary>
        event Action<VoiceTranslationResult> OnVoiceTranslationCompleted;

        /// <summary>
        /// 文本翻译
        /// </summary>
        /// <param name="text">待翻译文本</param>
        /// <param name="sourceLanguage">源语言代码 (如 "zh", "en")</param>
        /// <param name="targetLanguage">目标语言代码</param>
        /// <returns>翻译结果</returns>
        Task<TranslationResult> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);

        /// <summary>
        /// 自动检测语言并翻译
        /// </summary>
        /// <param name="text">待翻译文本</param>
        /// <param name="targetLanguage">目标语言代码</param>
        /// <returns>翻译结果（包含检测到的源语言）</returns>
        Task<TranslationResult> TranslateTextAutoDetectAsync(string text, string targetLanguage);

        /// <summary>
        /// 批量文本翻译
        /// </summary>
        /// <param name="texts">待翻译文本数组</param>
        /// <param name="sourceLanguage">源语言代码</param>
        /// <param name="targetLanguage">目标语言代码</param>
        /// <returns>翻译结果数组</returns>
        Task<List<TranslationResult>> TranslateBatchAsync(List<string> texts, string sourceLanguage, string targetLanguage);

        /// <summary>
        /// 语音翻译 - 将语音转换为目标语言文本
        /// </summary>
        /// <param name="audioData">音频数据</param>
        /// <param name="sourceLanguage">源语言代码</param>
        /// <param name="targetLanguage">目标语言代码</param>
        /// <returns>语音翻译结果</returns>
        Task<VoiceTranslationResult> TranslateVoiceAsync(byte[] audioData, string sourceLanguage, string targetLanguage);

        /// <summary>
        /// 实时语音翻译（流式）
        /// </summary>
        /// <param name="sourceLanguage">源语言代码</param>
        /// <param name="targetLanguage">目标语言代码</param>
        /// <param name="onPartialResult">部分结果回调</param>
        /// <param name="onFinalResult">最终结果回调</param>
        Task StartRealtimeVoiceTranslationAsync(
            string sourceLanguage,
            string targetLanguage,
            Action<string> onPartialResult,
            Action<VoiceTranslationResult> onFinalResult);

        /// <summary>
        /// 停止实时语音翻译
        /// </summary>
        void StopRealtimeVoiceTranslation();

        /// <summary>
        /// 获取支持的语言列表
        /// </summary>
        /// <returns>支持的语言列表</returns>
        Task<List<LanguageInfo>> GetSupportedLanguagesAsync();

        /// <summary>
        /// 检查语言对是否支持
        /// </summary>
        /// <param name="sourceLanguage">源语言</param>
        /// <param name="targetLanguage">目标语言</param>
        /// <returns>是否支持</returns>
        Task<bool> IsLanguagePairSupportedAsync(string sourceLanguage, string targetLanguage);

        /// <summary>
        /// 设置翻译选项
        /// </summary>
        /// <param name="options">翻译选项</param>
        void SetTranslationOptions(TranslationOptions options);

        /// <summary>
        /// 获取翻译选项
        /// </summary>
        /// <returns>当前翻译选项</returns>
        TranslationOptions GetTranslationOptions();
    }

    /// <summary>
    /// 翻译结果
    /// </summary>
    public class TranslationResult
    {
        /// <summary>
        /// 原始文本
        /// </summary>
        public string OriginalText { get; set; }

        /// <summary>
        /// 翻译后的文本
        /// </summary>
        public string TranslatedText { get; set; }

        /// <summary>
        /// 源语言代码
        /// </summary>
        public string SourceLanguage { get; set; }

        /// <summary>
        /// 目标语言代码
        /// </summary>
        public string TargetLanguage { get; set; }

        /// <summary>
        /// 置信度分数 (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// 翻译时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 备选翻译
        /// </summary>
        public List<string> AlternativeTranslations { get; set; } = new List<string>();

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息（如果失败）
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 语音翻译结果
    /// </summary>
    public class VoiceTranslationResult : TranslationResult
    {
        /// <summary>
        /// 识别的原始文本
        /// </summary>
        public string RecognizedText { get; set; }

        /// <summary>
        /// 翻译后的语音数据
        /// </summary>
        public byte[] TranslatedAudioData { get; set; }

        /// <summary>
        /// 音频时长（秒）
        /// </summary>
        public float AudioDuration { get; set; }

        /// <summary>
        /// 识别置信度
        /// </summary>
        public float RecognitionConfidence { get; set; }
    }

    /// <summary>
    /// 语言信息
    /// </summary>
    public class LanguageInfo
    {
        /// <summary>
        /// 语言代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 语言名称（本地）
        /// </summary>
        public string NativeName { get; set; }

        /// <summary>
        /// 语言名称（英文）
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 是否支持语音输入
        /// </summary>
        public bool SupportsVoiceInput { get; set; }

        /// <summary>
        /// 是否支持语音输出
        /// </summary>
        public bool SupportsVoiceOutput { get; set; }

        /// <summary>
        /// 支持的语音名称列表
        /// </summary>
        public List<string> AvailableVoices { get; set; } = new List<string>();
    }

    /// <summary>
    /// 翻译选项
    /// </summary>
    public class TranslationOptions
    {
        /// <summary>
        /// 是否启用正式/非正式语气区分
        /// </summary>
        public bool EnableFormality { get; set; } = false;

        /// <summary>
        /// 是否启用领域特定翻译
        /// </summary>
        public bool EnableDomainSpecific { get; set; } = false;

        /// <summary>
        /// 翻译领域（如 "tourism", "medical", "technical"）
        /// </summary>
        public string Domain { get; set; } = "general";

        /// <summary>
        /// 是否启用 profanity 过滤
        /// </summary>
        public bool EnableProfanityFilter { get; set; } = true;

        /// <summary>
        /// 最大备选翻译数量
        /// </summary>
        public int MaxAlternativeTranslations { get; set; } = 3;

        /// <summary>
        /// 翻译超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否自动播放语音翻译结果
        /// </summary>
        public bool AutoPlayVoiceTranslation { get; set; } = true;

        /// <summary>
        /// 语音语速 (-2 到 2, 0为正常)
        /// </summary>
        public float VoiceSpeed { get; set; } = 0f;
    }
}
