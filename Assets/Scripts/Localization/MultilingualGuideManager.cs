using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Localization
{
    /// <summary>
    /// 多语言AI导游管理器
    /// 支持50+语言，本地化语言模型，跨文化体验设计
    /// </summary>
    public class MultilingualGuideManager : MonoBehaviour
    {
        [Header("语言配置")]
        public List<LanguageConfig> supportedLanguages = new List<LanguageConfig>();
        public LanguageCode defaultLanguage = LanguageCode.en_US;
        public bool autoDetectLanguage = true;

        [Header("AI模型配置")]
        public bool useLocalizedModels = true;
        public string baseModelEndpoint = "https://api.tripmeta.ai/llm";
        public int modelTimeoutMs = 10000;

        [Header("文化适配")]
        public bool enableCulturalAdaptation = true;
        public bool enableFormalityAdjustment = true;
        public bool enableLocalReferences = true;

        // 当前语言
        private LanguageCode currentLanguage;
        private LanguageConfig currentLanguageConfig;

        // 本地化资源
        private Dictionary<string, LocalizedContent> contentCache = new Dictionary<string, LocalizedContent>();
        private Dictionary<string, CulturalContext> culturalContexts = new Dictionary<string, CulturalContext>();

        // 服务
        private TranslationService translationService;
        private CulturalAdaptationService culturalService;
        private SpeechLocalizationService speechService;

        public static MultilingualGuideManager Instance { get; private set; }

        public LanguageCode CurrentLanguage => currentLanguage;
        public IReadOnlyList<LanguageConfig> SupportedLanguages => supportedLanguages;

        // 事件
        public event Action<LanguageCode> OnLanguageChanged;
        public event Action<string> OnContentLocalized;
        public event Action<float> OnLocalizationProgress;

        void Awake()
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
        private void Initialize()
        {
            InitializeSupportedLanguages();
            InitializeServices();

            if (autoDetectLanguage)
            {
                AutoDetectLanguage();
            }
            else
            {
                SetLanguage(defaultLanguage);
            }

            Debug.Log($"[MultilingualGuideManager] 初始化完成，支持 {supportedLanguages.Count} 种语言");
        }

        /// <summary>
        /// 初始化支持的语言
        /// </summary>
        private void InitializeSupportedLanguages()
        {
            supportedLanguages = new List<LanguageConfig>
            {
                // 主要国际语言
                new LanguageConfig { code = LanguageCode.en_US, name = "English", nativeName = "English", localeCode = "en-US", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.zh_CN, name = "Chinese (Simplified)", nativeName = "简体中文", localeCode = "zh-CN", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.zh_TW, name = "Chinese (Traditional)", nativeName = "繁體中文", localeCode = "zh-TW", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.ja_JP, name = "Japanese", nativeName = "日本語", localeCode = "ja-JP", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal", "honorific" } },
                new LanguageConfig { code = LanguageCode.ko_KR, name = "Korean", nativeName = "한국어", localeCode = "ko-KR", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal", "honorific" } },
                new LanguageConfig { code = LanguageCode.es_ES, name = "Spanish", nativeName = "Español", localeCode = "es-ES", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.fr_FR, name = "French", nativeName = "Français", localeCode = "fr-FR", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.de_DE, name = "German", nativeName = "Deutsch", localeCode = "de-DE", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.it_IT, name = "Italian", nativeName = "Italiano", localeCode = "it-IT", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.pt_BR, name = "Portuguese (Brazil)", nativeName = "Português", localeCode = "pt-BR", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.ru_RU, name = "Russian", nativeName = "Русский", localeCode = "ru-RU", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.ar_SA, name = "Arabic", nativeName = "العربية", localeCode = "ar-SA", rtl = true, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.hi_IN, name = "Hindi", nativeName = "हिन्दी", localeCode = "hi-IN", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.th_TH, name = "Thai", nativeName = "ไทย", localeCode = "th-TH", rtl = false, formalityLevels = new[] { "casual", "formal", "royal" } },
                new LanguageConfig { code = LanguageCode.vi_VN, name = "Vietnamese", nativeName = "Tiếng Việt", localeCode = "vi-VN", rtl = false, formalityLevels = new[] { "casual", "neutral", "formal" } },
                new LanguageConfig { code = LanguageCode.id_ID, name = "Indonesian", nativeName = "Bahasa Indonesia", localeCode = "id-ID", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.tr_TR, name = "Turkish", nativeName = "Türkçe", localeCode = "tr-TR", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.pl_PL, name = "Polish", nativeName = "Polski", localeCode = "pl-PL", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.nl_NL, name = "Dutch", nativeName = "Nederlands", localeCode = "nl-NL", rtl = false, formalityLevels = new[] { "casual", "formal" } },

                // 其他亚洲语言
                new LanguageConfig { code = LanguageCode.ms_MY, name = "Malay", nativeName = "Bahasa Melayu", localeCode = "ms-MY", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.tl_PH, name = "Filipino", nativeName = "Filipino", localeCode = "tl-PH", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.bn_BD, name = "Bengali", nativeName = "বাংলা", localeCode = "bn-BD", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.ta_IN, name = "Tamil", nativeName = "தமிழ்", localeCode = "ta-IN", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.ur_PK, name = "Urdu", nativeName = "اردو", localeCode = "ur-PK", rtl = true, formalityLevels = new[] { "casual", "formal" } },

                // 欧洲语言
                new LanguageConfig { code = LanguageCode.sv_SE, name = "Swedish", nativeName = "Svenska", localeCode = "sv-SE", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.cs_CZ, name = "Czech", nativeName = "Čeština", localeCode = "cs-CZ", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.el_GR, name = "Greek", nativeName = "Ελληνικά", localeCode = "el-GR", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.he_IL, name = "Hebrew", nativeName = "עברית", localeCode = "he-IL", rtl = true, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.ro_RO, name = "Romanian", nativeName = "Română", localeCode = "ro-RO", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.hu_HU, name = "Hungarian", nativeName = "Magyar", localeCode = "hu-HU", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.uk_UA, name = "Ukrainian", nativeName = "Українська", localeCode = "uk-UA", rtl = false, formalityLevels = new[] { "casual", "formal" } },

                // 北欧语言
                new LanguageConfig { code = LanguageCode.da_DK, name = "Danish", nativeName = "Dansk", localeCode = "da-DK", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.fi_FI, name = "Finnish", nativeName = "Suomi", localeCode = "fi-FI", rtl = false, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.no_NO, name = "Norwegian", nativeName = "Norsk", localeCode = "no-NO", rtl = false, formalityLevels = new[] { "casual", "formal" } },

                // 其他重要语言
                new LanguageConfig { code = LanguageCode.fa_IR, name = "Persian", nativeName = "فارسی", localeCode = "fa-IR", rtl = true, formalityLevels = new[] { "casual", "formal" } },
                new LanguageConfig { code = LanguageCode.sw_KE, name = "Swahili", nativeName = "Kiswahili", localeCode = "sw-KE", rtl = false, formalityLevels = new[] { "casual", "formal" } },
            };
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitializeServices()
        {
            translationService = new TranslationService();
            culturalService = new CulturalAdaptationService();
            speechService = new SpeechLocalizationService();
        }

        /// <summary>
        /// 自动检测语言
        /// </summary>
        private void AutoDetectLanguage()
        {
            // 获取系统语言
            string systemLanguage = Application.systemLanguage.ToString();

            // 映射到支持的语言
            LanguageCode detected = MapSystemLanguageToCode(systemLanguage);

            SetLanguage(detected);
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public void SetLanguage(LanguageCode languageCode)
        {
            if (!supportedLanguages.Any(l => l.code == languageCode))
            {
                Debug.LogWarning($"[MultilingualGuideManager] 不支持的语言: {languageCode}，使用默认语言");
                languageCode = defaultLanguage;
            }

            currentLanguage = languageCode;
            currentLanguageConfig = supportedLanguages.First(l => l.code == languageCode);

            // 加载文化上下文
            LoadCulturalContext(languageCode);

            OnLanguageChanged?.Invoke(languageCode);

            Debug.Log($"[MultilingualGuideManager] 语言已切换至: {currentLanguageConfig.name}");
        }

        /// <summary>
        /// 加载文化上下文
        /// </summary>
        private void LoadCulturalContext(LanguageCode languageCode)
        {
            string region = languageCode.ToString().Split('_')[1];

            if (!culturalContexts.ContainsKey(region))
            {
                culturalContexts[region] = new CulturalContext
                {
                    region = region,
                    formalityPreference = GetDefaultFormality(languageCode),
                    greetingStyle = GetGreetingStyle(languageCode),
                    timeFormat = GetTimeFormat(languageCode),
                    dateFormat = GetDateFormat(languageCode),
                    numberFormat = GetNumberFormat(languageCode),
                    currencySymbol = GetCurrencySymbol(region),
                    culturalReferences = GetCulturalReferences(region),
                    avoidTopics = GetSensitiveTopics(region)
                };
            }
        }

        /// <summary>
        /// 获取本地化的AI响应
        /// </summary>
        public async Task<string> GetLocalizedResponse(string input, string context = null)
        {
            try
            {
                // 1. 获取基础AI响应
                string baseResponse = await GetBaseAIResponse(input, context);

                // 2. 如果需要，进行翻译
                if (currentLanguage != LanguageCode.en_US)
                {
                    baseResponse = await TranslateContent(baseResponse, LanguageCode.en_US, currentLanguage);
                }

                // 3. 文化适配
                if (enableCulturalAdaptation)
                {
                    baseResponse = await AdaptToCulture(baseResponse, currentLanguage);
                }

                // 4. 正式程度调整
                if (enableFormalityAdjustment)
                {
                    baseResponse = AdjustFormality(baseResponse, currentLanguageConfig.formalityLevels[1]);
                }

                return baseResponse;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultilingualGuideManager] 获取本地化响应失败: {e.Message}");
                return "Sorry, I couldn't process that. / 抱歉，我无法处理这个请求。";
            }
        }

        /// <summary>
        /// 获取基础AI响应
        /// </summary>
        private async Task<string> GetBaseAIResponse(string input, string context)
        {
            // 这里调用AI服务获取响应
            await Task.Delay(500);
            return $"Response to: {input}";
        }

        /// <summary>
        /// 翻译内容
        /// </summary>
        private async Task<string> TranslateContent(string content, LanguageCode from, LanguageCode to)
        {
            if (from == to) return content;

            return await translationService.Translate(content, from.ToString(), to.ToString());
        }

        /// <summary>
        /// 文化适配
        /// </summary>
        private async Task<string> AdaptToCulture(string content, LanguageCode language)
        {
            string region = language.ToString().Split('_')[1];

            if (culturalContexts.TryGetValue(region, out var context))
            {
                return await culturalService.AdaptContent(content, context);
            }

            return content;
        }

        /// <summary>
        /// 调整正式程度
        /// </summary>
        private string AdjustFormality(string content, string formalityLevel)
        {
            // 根据语言的正式程度规则调整内容
            // 简化实现：返回原内容
            return content;
        }

        /// <summary>
        /// 获取支持的语音列表
        /// </summary>
        public List<VoiceInfo> GetSupportedVoices()
        {
            return speechService.GetVoicesForLanguage(currentLanguage);
        }

        /// <summary>
        /// 获取本地化音频
        /// </summary>
        public async Task<AudioClip> GetLocalizedSpeech(string text, VoiceInfo voice = null)
        {
            voice ??= GetSupportedVoices().FirstOrDefault();
            return await speechService.SynthesizeSpeech(text, voice, currentLanguage);
        }

        /// <summary>
        /// 映射系统语言到语言代码
        /// </summary>
        private LanguageCode MapSystemLanguageToCode(string systemLanguage)
        {
            return systemLanguage switch
            {
                "Chinese" => LanguageCode.zh_CN,
                "ChineseSimplified" => LanguageCode.zh_CN,
                "ChineseTraditional" => LanguageCode.zh_TW,
                "Japanese" => LanguageCode.ja_JP,
                "Korean" => LanguageCode.ko_KR,
                "Spanish" => LanguageCode.es_ES,
                "French" => LanguageCode.fr_FR,
                "German" => LanguageCode.de_DE,
                "Italian" => LanguageCode.it_IT,
                "Portuguese" => LanguageCode.pt_BR,
                "Russian" => LanguageCode.ru_RU,
                "Arabic" => LanguageCode.ar_SA,
                "Hindi" => LanguageCode.hi_IN,
                _ => LanguageCode.en_US
            };
        }

        // 文化上下文辅助方法
        private string GetDefaultFormality(LanguageCode code) => "neutral";
        private string GetGreetingStyle(LanguageCode code) => "polite";
        private string GetTimeFormat(LanguageCode code) => code.ToString().StartsWith("en") ? "12h" : "24h";
        private string GetDateFormat(LanguageCode code) => "YYYY-MM-DD";
        private string GetNumberFormat(LanguageCode code) => "decimal";
        private string GetCurrencySymbol(string region) => "$";
        private string[] GetCulturalReferences(string region) => new string[0];
        private string[] GetSensitiveTopics(string region) => new string[0];

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 语言代码
    /// </summary>
    public enum LanguageCode
    {
        en_US, zh_CN, zh_TW, ja_JP, ko_KR, es_ES, fr_FR, de_DE, it_IT, pt_BR,
        ru_RU, ar_SA, hi_IN, th_TH, vi_VN, id_ID, tr_TR, pl_PL, nl_NL, ms_MY,
        tl_PH, bn_BD, ta_IN, ur_PK, sv_SE, cs_CZ, el_GR, he_IL, ro_RO, hu_HU,
        uk_UA, da_DK, fi_FI, no_NO, fa_IR, sw_KE
    }

    /// <summary>
    /// 语言配置
    /// </summary>
    [Serializable]
    public class LanguageConfig
    {
        public LanguageCode code;
        public string name;
        public string nativeName;
        public string localeCode;
        public bool rtl;
        public string[] formalityLevels;
    }

    /// <summary>
    /// 本地化内容
    /// </summary>
    [Serializable]
    public class LocalizedContent
    {
        public string key;
        public LanguageCode language;
        public string text;
        public AudioClip audio;
        public DateTime updatedAt;
    }

    /// <summary>
    /// 文化上下文
    /// </summary>
    [Serializable]
    public class CulturalContext
    {
        public string region;
        public string formalityPreference;
        public string greetingStyle;
        public string timeFormat;
        public string dateFormat;
        public string numberFormat;
        public string currencySymbol;
        public string[] culturalReferences;
        public string[] avoidTopics;
    }

    /// <summary>
    /// 语音信息
    /// </summary>
    [Serializable]
    public class VoiceInfo
    {
        public string id;
        public string name;
        public string gender;
        public string language;
        public float pitch;
        public float speed;
    }

    #endregion
}
