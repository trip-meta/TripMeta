using UnityEngine;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// 翻译服务配置
    /// </summary>
    [CreateAssetMenu(fileName = "TranslationConfig", menuName = "TripMeta/Config/Translation Config")]
    public class TranslationConfig : ScriptableObject
    {
        [Header("Azure Translator")]
        [Tooltip("Azure Translator 订阅密钥")]
        public string SubscriptionKey = "";

        [Tooltip("Azure Translator 区域")]
        public string Region = "eastasia";

        [Tooltip("Azure Translator API 端点")]
        public string Endpoint = "https://api.cognitive.microsofttranslator.com";

        [Header("默认设置")]
        [Tooltip("默认源语言")]
        public string DefaultSourceLanguage = "zh-Hans";

        [Tooltip("默认目标语言")]
        public string DefaultTargetLanguage = "en";

        [Header("翻译选项")]
        [Tooltip("启用正式/非正式语气区分")]
        public bool EnableFormality = false;

        [Tooltip("启用领域特定翻译")]
        public bool EnableDomainSpecific = false;

        [Tooltip("翻译领域")]
        public TranslationDomain Domain = TranslationDomain.General;

        [Tooltip("启用不当内容过滤")]
        public bool EnableProfanityFilter = true;

        [Tooltip("最大备选翻译数量")]
        [Range(0, 5)]
        public int MaxAlternativeTranslations = 3;

        [Tooltip("翻译超时时间（秒）")]
        [Range(5, 60)]
        public int TimeoutSeconds = 30;

        [Tooltip("自动播放语音翻译结果")]
        public bool AutoPlayVoiceTranslation = true;

        [Tooltip("语音语速 (-2 到 2)")]
        [Range(-2f, 2f)]
        public float VoiceSpeed = 0f;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SubscriptionKey) &&
                   !string.IsNullOrEmpty(Region);
        }

        /// <summary>
        /// 转换为翻译选项
        /// </summary>
        public AI.TranslationOptions ToTranslationOptions()
        {
            return new AI.TranslationOptions
            {
                EnableFormality = EnableFormality,
                EnableDomainSpecific = EnableDomainSpecific,
                Domain = Domain.ToString().ToLower(),
                EnableProfanityFilter = EnableProfanityFilter,
                MaxAlternativeTranslations = MaxAlternativeTranslations,
                TimeoutSeconds = TimeoutSeconds,
                AutoPlayVoiceTranslation = AutoPlayVoiceTranslation,
                VoiceSpeed = VoiceSpeed
            };
        }
    }

    /// <summary>
    /// 翻译领域
    /// </summary>
    public enum TranslationDomain
    {
        General,
        Tourism,
        Medical,
        Technical,
        Legal,
        Business,
        Academic
    }
}
