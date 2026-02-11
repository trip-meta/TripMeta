# TripMeta API 参考文档

## 📋 目录

- [AI服务API](#ai服务api)
- [VR交互API](#vr交互api)
- [用户管理API](#用户管理api)
- [内容管理API](#内容管理api)
- [性能监控API](#性能监控api)
- [事件系统API](#事件系统api)

## 🤖 AI服务API

### GPTService

AI对话服务，提供智能导游功能。

#### 初始化

```csharp
public class GPTService : IDisposable
{
    /// <summary>
    /// 初始化GPT服务
    /// </summary>
    /// <param name="config">GPT配置</param>
    /// <returns>初始化是否成功</returns>
    public async Task<bool> InitializeAsync(GPTConfig config)
    
    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <returns>服务状态</returns>
    public ServiceStatus GetStatus()
}
```

#### 对话生成

```csharp
/// <summary>
/// 生成AI响应
/// </summary>
/// <param name="prompt">用户输入</param>
/// <param name="options">生成选项</param>
/// <returns>AI响应内容</returns>
public async Task<string> GenerateResponseAsync(string prompt, GPTOptions options = null)

/// <summary>
/// 流式生成响应
/// </summary>
/// <param name="prompt">用户输入</param>
/// <returns>响应流</returns>
public IAsyncEnumerable<string> GenerateStreamAsync(string prompt)
```

#### 使用示例

```csharp
// 初始化服务
var gptService = ServiceContainer.Instance.GetService<IGPTService>();
await gptService.InitializeAsync(new GPTConfig 
{ 
    ApiKey = "your-api-key",
    Model = "gpt-4",
    MaxTokens = 2048
});

// 生成响应
var response = await gptService.GenerateResponseAsync("介绍一下埃菲尔铁塔");
Debug.Log($"AI回答: {response}");

// 流式响应
await foreach (var chunk in gptService.GenerateStreamAsync("讲个故事"))
{
    Debug.Log($"接收到: {chunk}");
}
```

### SpeechService

语音识别和合成服务。

```csharp
public class SpeechService : IDisposable
{
    /// <summary>
    /// 开始语音识别
    /// </summary>
    /// <param name="config">识别配置</param>
    /// <returns>识别任务</returns>
    public async Task<string> StartRecognitionAsync(SpeechConfig config)
    
    /// <summary>
    /// 语音合成
    /// </summary>
    /// <param name="text">要合成的文本</param>
    /// <param name="voice">语音配置</param>
    /// <returns>音频数据</returns>
    public async Task<AudioClip> SynthesizeAsync(string text, VoiceConfig voice)
    
    /// <summary>
    /// 实时语音识别
    /// </summary>
    /// <returns>识别结果流</returns>
    public IAsyncEnumerable<SpeechResult> StartContinuousRecognitionAsync()
}
```

### VisionService

计算机视觉服务。

```csharp
public class VisionService : IDisposable
{
    /// <summary>
    /// 分析图像内容
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <returns>分析结果</returns>
    public async Task<VisionResult> AnalyzeImageAsync(Texture2D image)
    
    /// <summary>
    /// 物体检测
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <returns>检测到的物体列表</returns>
    public async Task<List<DetectedObject>> DetectObjectsAsync(Texture2D image)
    
    /// <summary>
    /// 场景理解
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <returns>场景描述</returns>
    public async Task<SceneDescription> UnderstandSceneAsync(Texture2D image)
}
```

## 🥽 VR交互API

### VRInteractionManager

VR交互管理器，统一处理所有VR交互。

```csharp
public class VRInteractionManager : MonoBehaviour
{
    /// <summary>
    /// 注册可交互对象
    /// </summary>
    /// <param name="interactable">可交互对象</param>
    public void RegisterInteractable(IVRInteractable interactable)
    
    /// <summary>
    /// 注销可交互对象
    /// </summary>
    /// <param name="interactable">可交互对象</param>
    public void UnregisterInteractable(IVRInteractable interactable)
    
    /// <summary>
    /// 处理交互输入
    /// </summary>
    /// <param name="inputData">输入数据</param>
    public void ProcessInteraction(VRInputData inputData)
    
    /// <summary>
    /// 设置交互模式
    /// </summary>
    /// <param name="mode">交互模式</param>
    public void SetInteractionMode(InteractionMode mode)
}
```

### GestureRecognizer

手势识别系统。

```csharp
public class GestureRecognizer : MonoBehaviour
{
    /// <summary>
    /// 开始手势识别
    /// </summary>
    public void StartRecognition()
    
    /// <summary>
    /// 停止手势识别
    /// </summary>
    public void StopRecognition()
    
    /// <summary>
    /// 注册手势
    /// </summary>
    /// <param name="gesture">手势定义</param>
    public void RegisterGesture(GestureDefinition gesture)
    
    /// <summary>
    /// 手势识别事件
    /// </summary>
    public event Action<GestureResult> OnGestureRecognized;
}
```

### SpatialUIManager

空间UI管理器。

```csharp
public class SpatialUIManager : MonoBehaviour
{
    /// <summary>
    /// 创建空间UI面板
    /// </summary>
    /// <param name="prefab">UI预制体</param>
    /// <param name="position">世界坐标位置</param>
    /// <returns>创建的UI实例</returns>
    public GameObject CreateSpatialPanel(GameObject prefab, Vector3 position)
    
    /// <summary>
    /// 显示上下文菜单
    /// </summary>
    /// <param name="position">显示位置</param>
    /// <param name="options">菜单选项</param>
    public void ShowContextMenu(Vector3 position, List<MenuOption> options)
    
    /// <summary>
    /// 隐藏所有UI
    /// </summary>
    public void HideAllUI()
}
```

## 👤 用户管理API

### UserManager

用户管理服务。

```csharp
public class UserManager : MonoBehaviour
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="credentials">登录凭据</param>
    /// <returns>登录结果</returns>
    public async Task<LoginResult> LoginAsync(UserCredentials credentials)
    
    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="userInfo">用户信息</param>
    /// <returns>注册结果</returns>
    public async Task<RegisterResult> RegisterAsync(UserInfo userInfo)
    
    /// <summary>
    /// 获取用户配置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户配置</returns>
    public async Task<UserProfile> GetUserProfileAsync(string userId)
    
    /// <summary>
    /// 更新用户配置
    /// </summary>
    /// <param name="profile">用户配置</param>
    /// <returns>更新是否成功</returns>
    public async Task<bool> UpdateUserProfileAsync(UserProfile profile)
}
```

### PreferenceManager

用户偏好管理。

```csharp
public class PreferenceManager : MonoBehaviour
{
    /// <summary>
    /// 获取用户偏好
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户偏好</returns>
    public async Task<UserPreferences> GetPreferencesAsync(string userId)
    
    /// <summary>
    /// 更新偏好设置
    /// </summary>
    /// <param name="preferences">偏好设置</param>
    /// <returns>更新是否成功</returns>
    public async Task<bool> UpdatePreferencesAsync(UserPreferences preferences)
    
    /// <summary>
    /// 重置为默认偏好
    /// </summary>
    /// <param name="userId">用户ID</param>
    public async Task ResetToDefaultAsync(string userId)
}
```

## 🌍 内容管理API

### SceneManager

场景管理器。

```csharp
public class SceneManager : MonoBehaviour
{
    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="loadMode">加载模式</param>
    /// <returns>加载任务</returns>
    public async Task<Scene> LoadSceneAsync(string sceneId, SceneLoadMode loadMode = SceneLoadMode.Additive)
    
    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <returns>卸载任务</returns>
    public async Task UnloadSceneAsync(string sceneId)
    
    /// <summary>
    /// 获取当前场景信息
    /// </summary>
    /// <returns>场景信息</returns>
    public SceneInfo GetCurrentSceneInfo()
    
    /// <summary>
    /// 场景加载进度事件
    /// </summary>
    public event Action<string, float> OnSceneLoadProgress;
}
```

### AssetManager

资源管理器。

```csharp
public class AssetManager : MonoBehaviour
{
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetPath">资源路径</param>
    /// <returns>加载的资源</returns>
    public async Task<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    
    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="assetPaths">资源路径列表</param>
    /// <returns>预加载任务</returns>
    public async Task PreloadAssetsAsync(List<string> assetPaths)
    
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="asset">要释放的资源</param>
    public void ReleaseAsset(UnityEngine.Object asset)
    
    /// <summary>
    /// 获取内存使用情况
    /// </summary>
    /// <returns>内存使用信息</returns>
    public MemoryUsageInfo GetMemoryUsage()
}
```

## 📊 性能监控API

### PerformanceMonitor

性能监控器。

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    /// <summary>
    /// 开始性能监控
    /// </summary>
    /// <param name="config">监控配置</param>
    public void StartMonitoring(PerformanceConfig config)
    
    /// <summary>
    /// 停止性能监控
    /// </summary>
    public void StopMonitoring()
    
    /// <summary>
    /// 获取性能报告
    /// </summary>
    /// <returns>性能报告</returns>
    public PerformanceReport GetPerformanceReport()
    
    /// <summary>
    /// 性能警告事件
    /// </summary>
    public event Action<PerformanceWarning> OnPerformanceWarning;
}
```

### MemoryProfiler

内存分析器。

```csharp
public class MemoryProfiler : MonoBehaviour
{
    /// <summary>
    /// 开始内存分析
    /// </summary>
    public void StartProfiling()
    
    /// <summary>
    /// 停止内存分析
    /// </summary>
    public void StopProfiling()
    
    /// <summary>
    /// 获取内存快照
    /// </summary>
    /// <returns>内存快照</returns>
    public MemorySnapshot TakeSnapshot()
    
    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    public void ForceGarbageCollection()
}
```

## 📡 事件系统API

### EventBus

全局事件总线。

```csharp
public static class EventBus
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    public static void Subscribe<T>(Action<T> handler) where T : IEvent
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
    
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    public static void Publish<T>(T eventData) where T : IEvent
    
    /// <summary>
    /// 清除所有订阅
    /// </summary>
    public static void Clear()
}
```

### 事件类型定义

```csharp
// AI事件
public class AIResponseEvent : IEvent
{
    public string Prompt { get; set; }
    public string Response { get; set; }
    public float ResponseTime { get; set; }
}

// VR交互事件
public class VRInteractionEvent : IEvent
{
    public InteractionType Type { get; set; }
    public GameObject Target { get; set; }
    public Vector3 Position { get; set; }
    public float Timestamp { get; set; }
}

// 用户事件
public class UserLoginEvent : IEvent
{
    public string UserId { get; set; }
    public DateTime LoginTime { get; set; }
    public string DeviceInfo { get; set; }
}

// 性能事件
public class PerformanceWarningEvent : IEvent
{
    public PerformanceMetric Metric { get; set; }
    public float CurrentValue { get; set; }
    public float ThresholdValue { get; set; }
    public string Description { get; set; }
}
```

## 🔧 配置类型

### GPTConfig

```csharp
[Serializable]
public class GPTConfig
{
    public string ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4";
    public int MaxTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.7f;
    public string SystemPrompt { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
```

### VRConfig

```csharp
[Serializable]
public class VRConfig
{
    public int TargetFrameRate { get; set; } = 90;
    public int EyeTextureResolution { get; set; } = 2048;
    public bool EnableFoveatedRendering { get; set; } = true;
    public float IPD { get; set; } = 0.064f;
    public TrackingSpace TrackingSpace { get; set; } = TrackingSpace.RoomScale;
}
```

### PerformanceConfig

```csharp
[Serializable]
public class PerformanceConfig
{
    public bool EnableProfiling { get; set; } = true;
    public float UpdateInterval { get; set; } = 1.0f;
    public int MemoryThresholdMB { get; set; } = 1024;
    public float FrameTimeThresholdMs { get; set; } = 11.1f; // 90 FPS
    public bool EnableAutoOptimization { get; set; } = true;
}
```

## 📝 使用示例

### 完整的AI对话流程

```csharp
public class TourGuideExample : MonoBehaviour
{
    private IGPTService _gptService;
    private ISpeechService _speechService;
    
    private async void Start()
    {
        // 获取服务
        _gptService = ServiceContainer.Instance.GetService<IGPTService>();
        _speechService = ServiceContainer.Instance.GetService<ISpeechService>();
        
        // 订阅事件
        EventBus.Subscribe<VRInteractionEvent>(OnVRInteraction);
        
        // 初始化服务
        await InitializeServicesAsync();
    }
    
    private async Task InitializeServicesAsync()
    {
        var gptConfig = new GPTConfig
        {
            ApiKey = ConfigManager.GetString("OpenAI.ApiKey"),
            Model = "gpt-4",
            SystemPrompt = "你是一个专业的虚拟导游..."
        };
        
        await _gptService.InitializeAsync(gptConfig);
        await _speechService.InitializeAsync(new SpeechConfig());
    }
    
    private async void OnVRInteraction(VRInteractionEvent interactionEvent)
    {
        if (interactionEvent.Type == InteractionType.Voice)
        {
            // 语音识别
            var userInput = await _speechService.StartRecognitionAsync();
            
            // AI生成回答
            var response = await _gptService.GenerateResponseAsync(userInput);
            
            // 语音合成
            var audioClip = await _speechService.SynthesizeAsync(response);
            
            // 播放音频
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
            
            // 发布事件
            EventBus.Publish(new AIResponseEvent 
            { 
                Prompt = userInput, 
                Response = response 
            });
        }
    }
    
    private void OnDestroy()
    {
        EventBus.Unsubscribe<VRInteractionEvent>(OnVRInteraction);
    }
}
```

---

*本API文档会随着项目发展持续更新，请关注最新版本。*