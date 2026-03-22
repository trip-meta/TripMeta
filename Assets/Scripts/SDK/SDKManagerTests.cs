using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.SDK;

namespace TripMeta.Tests.SDK
{
    /// <summary>
    /// SDK管理器单元测试
    /// </summary>
    public class SDKManagerTests
    {
        private GameObject testObject;
        private PluginManager pluginManager;
        private APIManager apiManager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestSDKManager");
            pluginManager = testObject.AddComponent<PluginManager>();
            apiManager = testObject.AddComponent<APIManager>();
            pluginManager.sandboxPlugins = true;
            apiManager.maxRequestsPerMinute = 1000;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator PluginManager_Initialization_CreatesPluginsDirectory()
        {
            yield return null;

            Assert.IsNotNull(pluginManager);
            string expectedPath = Path.Combine(Application.persistentDataPath, "Plugins/");
            Assert.IsTrue(Directory.Exists(expectedPath));
        }

        [UnityTest]
        public IEnumerator PluginManager_LoadedPlugins_IsInitialized()
        {
            yield return null;

            Assert.IsNotNull(pluginManager.LoadedPlugins);
        }

        [Test]
        public void PluginManifest_HasRequiredFields()
        {
            var manifest = new PluginManifest
            {
                pluginId = "test.plugin",
                name = "Test Plugin",
                version = "1.0.0",
                description = "A test plugin",
                author = "Test Author",
                entryClass = "TestPlugin.Entry",
                autoStart = true
            };

            Assert.AreEqual("test.plugin", manifest.pluginId);
            Assert.AreEqual("Test Plugin", manifest.name);
            Assert.AreEqual("1.0.0", manifest.version);
        }

        [Test]
        public void LoadedPlugin_TracksState()
        {
            var plugin = new LoadedPlugin
            {
                manifest = new PluginManifest { pluginId = "test", name = "Test" },
                directoryPath = "/test/path",
                isActive = false
            };

            Assert.IsFalse(plugin.isActive);
            Assert.AreEqual("test", plugin.manifest.pluginId);
        }

        [UnityTest]
        public IEnumerator PluginManager_RegisterAPI_AddsAPI()
        {
            yield return null;

            var api = new PluginAPI
            {
                apiName = "TestAPI",
                version = "1.0",
                description = "Test API"
            };

            pluginManager.RegisterAPI(api);

            var retrievedAPI = pluginManager.GetAPI("TestAPI");
            Assert.IsNotNull(retrievedAPI);
            Assert.AreEqual("TestAPI", retrievedAPI.apiName);
        }

        [UnityTest]
        public IEnumerator PluginManager_GetAllAPIs_ReturnsList()
        {
            yield return null;

            var apis = pluginManager.GetAllAPIs();
            Assert.IsNotNull(apis);
        }

        [Test]
        public void PluginAPI_DefinesContract()
        {
            var api = new PluginAPI
            {
                apiName = "GetUserData",
                version = "1.0",
                description = "Gets user data",
                returnType = typeof(string)
            };

            Assert.AreEqual("GetUserData", api.apiName);
            Assert.AreEqual(typeof(string), api.returnType);
        }

        [UnityTest]
        public IEnumerator APIManager_SetApiKey_UpdatesKey()
        {
            yield return null;

            apiManager.SetApiKey("test_api_key_123");
            Assert.IsTrue(apiManager.IsAuthenticated);
        }

        [Test]
        public void APIResponse_TracksSuccess()
        {
            var response = new APIResponse
            {
                success = true,
                statusCode = 200
            };

            Assert.IsTrue(response.success);
            Assert.AreEqual(200, response.statusCode);
        }

        [Test]
        public void APIResponseGeneric_ContainsData()
        {
            var response = new APIResponse<string>
            {
                success = true,
                data = "test data",
                statusCode = 200
            };

            Assert.AreEqual("test data", response.data);
        }

        [UnityTest]
        public IEnumerator PluginManager_ActivateNonExistentPlugin_ReturnsFalse()
        {
            yield return null;

            bool result = pluginManager.ActivatePlugin("nonexistent.plugin");
            Assert.IsFalse(result);
        }

        [UnityTest]
        public IEnumerator PluginManager_DeactivateNonExistentPlugin_ReturnsFalse()
        {
            yield return null;

            bool result = pluginManager.DeactivatePlugin("nonexistent.plugin");
            Assert.IsFalse(result);
        }

        [Test]
        public void APIManager_DefaultConfiguration_IsValid()
        {
            Assert.AreEqual(30, apiManager.requestTimeout);
            Assert.AreEqual(3, apiManager.maxRetries);
            Assert.AreEqual(60, apiManager.maxRequestsPerMinute);
        }

        [UnityTest]
        public IEnumerator SDK_Components_Exist()
        {
            yield return null;

            Assert.IsNotNull(PluginManager.Instance);
            Assert.IsNotNull(APIManager.Instance);
        }
    }
}
