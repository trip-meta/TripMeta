using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.AI;
using TripMeta.AI.Services;
using TripMeta.Core.Configuration;

namespace TripMeta.Tests
{
    /// <summary>
    /// 翻译服务单元测试
    /// </summary>
    public class TranslationServiceTests
    {
        private MockTranslationService _translationService;

        [SetUp]
        public void Setup()
        {
            _translationService = new MockTranslationService();
        }

        [TearDown]
        public void Teardown()
        {
            _translationService?.DisposeAsync().Wait();
        }

        [Test]
        public async Task InitializeAsync_ServiceInitialized()
        {
            await _translationService.InitializeAsync();
            Assert.IsTrue(_translationService.IsInitialized);
        }

        [Test]
        public async Task CheckHealthAsync_ReturnsTrue()
        {
            var health = await _translationService.CheckHealthAsync();
            Assert.IsTrue(health);
        }

        [UnityTest]
        public IEnumerator TranslateTextAsync_ValidInput_ReturnsTranslation()
        {
            var task = TranslateAndVerifyAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task TranslateAndVerifyAsync()
        {
            await _translationService.InitializeAsync();

            var result = await _translationService.TranslateTextAsync(
                "你好",
                "zh-Hans",
                "en"
            );

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("你好", result.OriginalText);
            Assert.AreEqual("Hello", result.TranslatedText);
            Assert.AreEqual("zh-Hans", result.SourceLanguage);
            Assert.AreEqual("en", result.TargetLanguage);
            Assert.Greater(result.Confidence, 0);
        }

        [UnityTest]
        public IEnumerator TranslateTextAutoDetectAsync_AutoDetectsSourceLanguage()
        {
            var task = AutoDetectAndTranslateAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task AutoDetectAndTranslateAsync()
        {
            await _translationService.InitializeAsync();

            var result = await _translationService.TranslateTextAutoDetectAsync(
                "欢迎来到 TripMeta",
                "en"
            );

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.SourceLanguage);
            Assert.AreEqual("en", result.TargetLanguage);
        }

        [UnityTest]
        public IEnumerator TranslateBatchAsync_MultipleTexts_ReturnsTranslations()
        {
            var task = BatchTranslateAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task BatchTranslateAsync()
        {
            await _translationService.InitializeAsync();

            var texts = new List<string>
            {
                "你好",
                "谢谢",
                "再见"
            };

            var results = await _translationService.TranslateBatchAsync(
                texts,
                "zh-Hans",
                "en"
            );

            Assert.AreEqual(3, results.Count);
            foreach (var result in results)
            {
                Assert.IsTrue(result.IsSuccess);
            }
        }

        [UnityTest]
        public IEnumerator GetSupportedLanguagesAsync_ReturnsLanguages()
        {
            var task = GetLanguagesAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task GetLanguagesAsync()
        {
            await _translationService.InitializeAsync();

            var languages = await _translationService.GetSupportedLanguagesAsync();

            Assert.IsNotNull(languages);
            Assert.IsNotEmpty(languages);
            Assert.IsTrue(languages.Count >= 10);

            // 验证包含主要语言
            var hasChinese = languages.Exists(l => l.Code == "zh-Hans");
            var hasEnglish = languages.Exists(l => l.Code == "en");
            var hasJapanese = languages.Exists(l => l.Code == "ja");

            Assert.IsTrue(hasChinese, "应该支持中文");
            Assert.IsTrue(hasEnglish, "应该支持英文");
            Assert.IsTrue(hasJapanese, "应该支持日文");
        }

        [UnityTest]
        public IEnumerator TranslationOptions_SetAndGet_WorksCorrectly()
        {
            var task = TestOptionsAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task TestOptionsAsync()
        {
            var options = new TranslationOptions
            {
                EnableFormality = true,
                EnableDomainSpecific = true,
                Domain = "tourism",
                EnableProfanityFilter = false,
                MaxAlternativeTranslations = 5,
                AutoPlayVoiceTranslation = true,
                VoiceSpeed = 1.0f
            };

            _translationService.SetTranslationOptions(options);
            var retrievedOptions = _translationService.GetTranslationOptions();

            Assert.AreEqual(options.EnableFormality, retrievedOptions.EnableFormality);
            Assert.AreEqual(options.EnableDomainSpecific, retrievedOptions.EnableDomainSpecific);
            Assert.AreEqual(options.Domain, retrievedOptions.Domain);
            Assert.AreEqual(options.EnableProfanityFilter, retrievedOptions.EnableProfanityFilter);
            Assert.AreEqual(options.MaxAlternativeTranslations, retrievedOptions.MaxAlternativeTranslations);
            Assert.AreEqual(options.AutoPlayVoiceTranslation, retrievedOptions.AutoPlayVoiceTranslation);
            Assert.AreEqual(options.VoiceSpeed, retrievedOptions.VoiceSpeed);
        }

        [Test]
        public void TranslationConfig_ToTranslationOptions_MapsCorrectly()
        {
            var config = ScriptableObject.CreateInstance<TranslationConfig>();
            config.EnableFormality = true;
            config.EnableDomainSpecific = true;
            config.Domain = TranslationDomain.Tourism;
            config.EnableProfanityFilter = false;
            config.MaxAlternativeTranslations = 5;
            config.AutoPlayVoiceTranslation = false;
            config.VoiceSpeed = 1.5f;

            var options = config.ToTranslationOptions();

            Assert.AreEqual(config.EnableFormality, options.EnableFormality);
            Assert.AreEqual(config.EnableDomainSpecific, options.EnableDomainSpecific);
            Assert.AreEqual("tourism", options.Domain);
            Assert.AreEqual(config.EnableProfanityFilter, options.EnableProfanityFilter);
            Assert.AreEqual(config.MaxAlternativeTranslations, options.MaxAlternativeTranslations);
            Assert.AreEqual(config.AutoPlayVoiceTranslation, options.AutoPlayVoiceTranslation);
            Assert.AreEqual(config.VoiceSpeed, options.VoiceSpeed);
        }

        [Test]
        public void TranslationConfig_IsValid_ValidatesRequiredFields()
        {
            var config = ScriptableObject.CreateInstance<TranslationConfig>();

            // 未配置密钥时应该无效
            Assert.IsFalse(config.IsValid());

            // 配置密钥后应该有效
            config.SubscriptionKey = "test-key";
            config.Region = "eastasia";
            Assert.IsTrue(config.IsValid());
        }

        [UnityTest]
        public IEnumerator TranslateTextAsync_EventsFired()
        {
            var task = TestEventsAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task TestEventsAsync()
        {
            await _translationService.InitializeAsync();

            TranslationResult receivedResult = null;
            _translationService.OnTranslationCompleted += (result) =>
            {
                receivedResult = result;
            };

            await _translationService.TranslateTextAsync("你好", "zh-Hans", "en");

            Assert.IsNotNull(receivedResult);
            Assert.AreEqual("你好", receivedResult.OriginalText);
        }
    }
}
