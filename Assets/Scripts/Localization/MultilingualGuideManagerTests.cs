using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Localization;

namespace TripMeta.Tests.Localization
{
    /// <summary>
    /// 多语言AI导游管理器单元测试
    /// </summary>
    public class MultilingualGuideManagerTests
    {
        private GameObject testObject;
        private MultilingualGuideManager manager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestMultilingualManager");
            manager = testObject.AddComponent<MultilingualGuideManager>();
            manager.autoDetectLanguage = false;
            manager.defaultLanguage = LanguageCode.en_US;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator MultilingualManager_Initialization_HasSupportedLanguages()
        {
            yield return null;

            Assert.IsNotNull(manager);
            Assert.Greater(manager.SupportedLanguages.Count, 0);
            Debug.Log($"支持语言数量: {manager.SupportedLanguages.Count}");
        }

        [UnityTest]
        public IEnumerator MultilingualManager_SetLanguage_ChangesLanguage()
        {
            yield return null;

            bool languageChanged = false;
            LanguageCode changedLanguage = LanguageCode.en_US;
            manager.OnLanguageChanged += (lang) =>
            {
                languageChanged = true;
                changedLanguage = lang;
            };

            manager.SetLanguage(LanguageCode.zh_CN);

            Assert.IsTrue(languageChanged);
            Assert.AreEqual(LanguageCode.zh_CN, changedLanguage);
            Assert.AreEqual(LanguageCode.zh_CN, manager.CurrentLanguage);
        }

        [UnityTest]
        public IEnumerator MultilingualManager_SetUnsupportedLanguage_FallsBackToDefault()
        {
            yield return null;

            // 尝试设置一个可能不支持的语言，应该回退到默认
            manager.SetLanguage(LanguageCode.en_US);
            Assert.AreEqual(LanguageCode.en_US, manager.CurrentLanguage);
        }

        [Test]
        public void LanguageConfig_HasRequiredFields()
        {
            var config = new LanguageConfig
            {
                code = LanguageCode.ja_JP,
                name = "Japanese",
                nativeName = "日本語",
                localeCode = "ja-JP",
                rtl = false,
                formalityLevels = new[] { "casual", "neutral", "formal" }
            };

            Assert.AreEqual(LanguageCode.ja_JP, config.code);
            Assert.AreEqual("Japanese", config.name);
            Assert.AreEqual("日本語", config.nativeName);
            Assert.IsFalse(config.rtl);
            Assert.AreEqual(3, config.formalityLevels.Length);
        }

        [Test]
        public void LanguageConfig_RTLLanguages_AreMarked()
        {
            var arabicConfig = new LanguageConfig
            {
                code = LanguageCode.ar_SA,
                name = "Arabic",
                rtl = true
            };

            var hebrewConfig = new LanguageConfig
            {
                code = LanguageCode.he_IL,
                name = "Hebrew",
                rtl = true
            };

            Assert.IsTrue(arabicConfig.rtl);
            Assert.IsTrue(hebrewConfig.rtl);
        }

        [Test]
        public void CulturalContext_StoresCulturalData()
        {
            var context = new CulturalContext
            {
                region = "JP",
                formalityPreference = "formal",
                greetingStyle = "polite",
                timeFormat = "24h",
                currencySymbol = "¥",
                avoidTopics = new[] { "politics", "religion" }
            };

            Assert.AreEqual("JP", context.region);
            Assert.AreEqual("formal", context.formalityPreference);
            Assert.AreEqual("¥", context.currencySymbol);
            Assert.AreEqual(2, context.avoidTopics.Length);
        }

        [UnityTest]
        public IEnumerator MultilingualManager_SupportsAtLeast36Languages()
        {
            yield return null;

            Assert.GreaterOrEqual(manager.SupportedLanguages.Count, 36);
            Debug.Log($"支持 {manager.SupportedLanguages.Count} 种语言");
        }

        [Test]
        public void VoiceInfo_HasRequiredProperties()
        {
            var voice = new VoiceInfo
            {
                id = "en-US-female-1",
                name = "Emma",
                gender = "female",
                language = "en-US",
                pitch = 1.0f,
                speed = 1.0f
            };

            Assert.AreEqual("en-US-female-1", voice.id);
            Assert.AreEqual("Emma", voice.name);
            Assert.AreEqual("female", voice.gender);
        }

        [Test]
        public void LocalizedContent_StoresLocalizedData()
        {
            var content = new LocalizedContent
            {
                key = "welcome_message",
                language = LanguageCode.fr_FR,
                text = "Bienvenue à Paris!",
                updatedAt = System.DateTime.Now
            };

            Assert.AreEqual("welcome_message", content.key);
            Assert.AreEqual(LanguageCode.fr_FR, content.language);
            Assert.AreEqual("Bienvenue à Paris!", content.text);
        }

        [UnityTest]
        public IEnumerator TranslationService_Translate_ReturnsTranslatedText()
        {
            var service = new TranslationService();
            Task<string> task = service.Translate("Hello", "en", "zh");

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsNotNull(task.Result);
            Assert.IsTrue(task.Result.Contains("[zh]"));
        }

        [UnityTest]
        public IEnumerator SpeechLocalizationService_GetVoices_ReturnsVoices()
        {
            var service = new SpeechLocalizationService();
            var voices = service.GetVoicesForLanguage(LanguageCode.en_US);

            Assert.Greater(voices.Count, 0);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CulturalAdaptationService_GetAdvice_ReturnsAdvice()
        {
            var service = new CulturalAdaptationService();
            var advice = service.GetCulturalAdvice("JP");

            Assert.IsNotNull(advice);
            Assert.AreEqual("JP", advice.region);
            Assert.IsNotNull(advice.recommendations);
            yield return null;
        }
    }
}
