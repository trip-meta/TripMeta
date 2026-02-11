# TripMeta 故障排除指南

## 📋 目录

- [常见问题](#常见问题)
- [Unity相关问题](#unity相关问题)
- [VR设备问题](#vr设备问题)
- [AI服务问题](#ai服务问题)
- [性能问题](#性能问题)
- [网络连接问题](#网络连接问题)
- [构建和部署问题](#构建和部署问题)
- [调试工具](#调试工具)

## ❓ 常见问题

### Q: 项目无法在Unity中打开

**症状**: Unity Hub显示项目版本不兼容或无法加载

**解决方案**:
1. 确认Unity版本为2022.3 LTS或更高
2. 检查项目路径中是否包含中文字符
3. 清除Unity缓存

```bash
# Windows
rmdir /s "%APPDATA%\Unity"
rmdir /s "%LOCALAPPDATA%\Unity"

# macOS
rm -rf ~/Library/Unity
rm -rf ~/Library/Preferences/Unity

# Linux
rm -rf ~/.config/unity3d
```

### Q: 编译错误 "The type or namespace name 'XXX' could not be found"

**症状**: 脚本编译失败，提示找不到类型或命名空间

**解决方案**:
1. 检查Package Manager中的依赖包是否正确安装
2. 重新导入所有资源: `Assets -> Reimport All`
3. 清除脚本缓存: `Assets -> Refresh`
4. 检查Assembly Definition文件配置

```csharp
// 检查Assembly Definition References
// TripMeta.Core.asmdef 应该包含:
{
    "name": "TripMeta.Core",
    "references": [
        "Unity.XR.Management",
        "Unity.InputSystem",
        "Unity.Addressables"
    ]
}
```

### Q: 运行时出现 "ServiceContainer not initialized" 错误

**症状**: 游戏启动时服务容器相关错误

**解决方案**:
1. 确保场景中有ServiceInstaller组件
2. 检查服务注册顺序
3. 验证依赖注入配置

```csharp
// 在场景的某个GameObject上添加ServiceInstaller
public class ServiceInstaller : MonoBehaviour
{
    private void Awake()
    {
        // 确保在其他组件之前初始化
        var container = ServiceContainer.Instance;
        
        // 注册核心服务
        container.RegisterSingleton<ILogger, UnityLogger>();
        container.RegisterSingleton<IConfigManager, ConfigManager>();
        
        Debug.Log("Services initialized successfully");
    }
}
```

## 🎮 Unity相关问题

### 性能问题

#### 帧率低于预期

**诊断步骤**:
1. 打开Profiler窗口 (`Window -> Analysis -> Profiler`)
2. 检查CPU和GPU使用情况
3. 查看内存分配

**常见原因和解决方案**:

```csharp
// 1. Update中的昂贵操作
// ❌ 错误做法
void Update()
{
    GameObject.Find("Player"); // 每帧查找
    GetComponent<Rigidbody>(); // 每帧获取组件
}

// ✅ 正确做法
private GameObject _player;
private Rigidbody _rigidbody;

void Start()
{
    _player = GameObject.Find("Player");
    _rigidbody = GetComponent<Rigidbody>();
}

void Update()
{
    // 使用缓存的引用
}
```

```csharp
// 2. 字符串拼接性能问题
// ❌ 错误做法
string result = "";
for (int i = 0; i < 1000; i++)
{
    result += i.ToString();
}

// ✅ 正确做法
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.Append(i);
}
string result = sb.ToString();
```

#### 内存泄漏

**检测方法**:
```csharp
public class MemoryMonitor : MonoBehaviour
{
    private void Start()
    {
        InvokeRepeating(nameof(LogMemoryUsage), 1f, 5f);
    }
    
    private void LogMemoryUsage()
    {
        var totalMemory = GC.GetTotalMemory(false);
        var unityMemory = Profiler.GetTotalAllocatedMemory(Profiler.Area.All);
        
        Debug.Log($"GC Memory: {totalMemory / 1024 / 1024}MB, Unity Memory: {unityMemory / 1024 / 1024}MB");
        
        if (totalMemory > 500 * 1024 * 1024) // 500MB阈值
        {
            Debug.LogWarning("High memory usage detected!");
            // 触发内存清理
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }
}
```

**常见内存泄漏原因**:
1. 事件订阅未取消
2. 静态引用未清理
3. 协程未正确停止

```csharp
// 正确的事件管理
public class EventManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDeath;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDeath; // 重要：取消订阅
    }
}
```

### 资源加载问题

#### Addressables资源加载失败

**症状**: 资源加载返回null或抛出异常

**解决方案**:
```csharp
public class SafeAssetLoader : MonoBehaviour
{
    public async Task<T> LoadAssetSafelyAsync<T>(string address) where T : UnityEngine.Object
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var result = await handle.Task;
            
            if (result == null)
            {
                Debug.LogError($"Failed to load asset: {address}");
                return null;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Asset loading exception for {address}: {ex.Message}");
            return null;
        }
    }
}
```

## 🥽 VR设备问题

### PICO设备连接问题

#### 设备未被识别

**检查步骤**:
1. 确认设备开发者模式已启用
2. 检查USB连接和驱动程序
3. 验证PICO SDK配置

```csharp
// VR设备检测脚本
public class VRDeviceDetector : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DetectVRDevice());
    }
    
    private IEnumerator DetectVRDevice()
    {
        yield return new WaitForSeconds(2f);
        
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        
        if (xrDisplaySubsystems.Count == 0)
        {
            Debug.LogError("No VR display subsystem found!");
            ShowVRErrorDialog("VR设备未检测到，请检查设备连接和驱动程序");
            return;
        }
        
        foreach (var subsystem in xrDisplaySubsystems)
        {
            Debug.Log($"VR Display: {subsystem.SubsystemDescriptor.id}");
        }
        
        // 检查输入系统
        var xrInputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(xrInputSubsystems);
        
        if (xrInputSubsystems.Count == 0)
        {
            Debug.LogWarning("No VR input subsystem found!");
        }
    }
    
    private void ShowVRErrorDialog(string message)
    {
        // 显示错误对话框的实现
    }
}
```

#### 追踪丢失问题

**症状**: 头显或手柄追踪不稳定

**解决方案**:
```csharp
public class TrackingMonitor : MonoBehaviour
{
    private XRNode[] _trackedNodes = { XRNode.Head, XRNode.LeftHand, XRNode.RightHand };
    
    private void Update()
    {
        foreach (var node in _trackedNodes)
        {
            if (InputDevices.GetDeviceAtXRNode(node).isValid)
            {
                var device = InputDevices.GetDeviceAtXRNode(node);
                
                if (device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked))
                {
                    if (!isTracked)
                    {
                        Debug.LogWarning($"Tracking lost for {node}");
                        HandleTrackingLoss(node);
                    }
                }
            }
        }
    }
    
    private void HandleTrackingLoss(XRNode node)
    {
        switch (node)
        {
            case XRNode.Head:
                // 头显追踪丢失处理
                ShowTrackingLossWarning("请确保头显在追踪范围内");
                break;
            case XRNode.LeftHand:
            case XRNode.RightHand:
                // 手柄追踪丢失处理
                ShowControllerTrackingLoss(node);
                break;
        }
    }
}
```

### 渲染问题

#### 画面模糊或重影

**可能原因**:
1. IPD设置不正确
2. 渲染分辨率过低
3. 抗锯齿设置问题

**解决方案**:
```csharp
public class VRRenderingOptimizer : MonoBehaviour
{
    [SerializeField] private float _renderScale = 1.2f;
    [SerializeField] private int _eyeTextureResolution = 2048;
    
    private void Start()
    {
        OptimizeVRRendering();
    }
    
    private void OptimizeVRRendering()
    {
        // 设置渲染分辨率
        XRSettings.eyeTextureResolutionScale = _renderScale;
        
        // 设置目标帧率
        Application.targetFrameRate = 90;
        
        // 禁用垂直同步（VR中由运行时处理）
        QualitySettings.vSyncCount = 0;
        
        // 优化渲染管线设置
        var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            // 启用SRP Batcher
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            
            Debug.Log("VR rendering optimized");
        }
    }
}
```

## 🤖 AI服务问题

### OpenAI API连接问题

#### API密钥无效

**症状**: 返回401 Unauthorized错误

**解决方案**:
```csharp
public class APIKeyValidator : MonoBehaviour
{
    public async Task<bool> ValidateAPIKeyAsync(string apiKey)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            
            var response = await client.GetAsync("https://api.openai.com/v1/models");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Debug.LogError("Invalid OpenAI API key");
                return false;
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.LogError($"API key validation failed: {ex.Message}");
            return false;
        }
    }
}
```

#### 请求超时

**症状**: 请求长时间无响应

**解决方案**:
```csharp
public class RobustGPTService : MonoBehaviour
{
    private readonly int _maxRetries = 3;
    private readonly float _baseDelay = 1f;
    
    public async Task<string> GenerateResponseWithRetryAsync(string prompt)
    {
        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                var response = await CallGPTAPIAsync(prompt, cts.Token);
                return response;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Request timeout, attempt {attempt + 1}/{_maxRetries}");
                
                if (attempt < _maxRetries - 1)
                {
                    var delay = _baseDelay * Mathf.Pow(2, attempt); // 指数退避
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GPT request failed: {ex.Message}");
                
                if (attempt == _maxRetries - 1)
                    throw;
                    
                await Task.Delay(TimeSpan.FromSeconds(_baseDelay));
            }
        }
        
        throw new Exception("All retry attempts failed");
    }
}
```

### 语音服务问题

#### 麦克风权限问题

**症状**: 无法录制音频

**解决方案**:
```csharp
public class MicrophonePermissionChecker : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(CheckMicrophonePermission());
    }
    
    private IEnumerator CheckMicrophonePermission()
    {
        // 请求麦克风权限
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.LogError("Microphone permission denied");
            ShowPermissionDialog();
            yield break;
        }
        
        // 检查可用的麦克风设备
        var devices = Microphone.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No microphone devices found");
            ShowNoMicrophoneDialog();
            yield break;
        }
        
        Debug.Log($"Found {devices.Length} microphone devices");
        foreach (var device in devices)
        {
            Debug.Log($"Microphone: {device}");
        }
    }
}
```

## ⚡ 性能问题

### 帧率优化

#### 动态质量调整

```csharp
public class DynamicQualityManager : MonoBehaviour
{
    [SerializeField] private float _targetFrameTime = 11.1f; // 90 FPS
    [SerializeField] private int _sampleSize = 60;
    
    private Queue<float> _frameTimeHistory = new Queue<float>();
    private float _lastQualityAdjustment = 0f;
    
    private void Update()
    {
        var frameTime = Time.unscaledDeltaTime * 1000f;
        
        _frameTimeHistory.Enqueue(frameTime);
        if (_frameTimeHistory.Count > _sampleSize)
        {
            _frameTimeHistory.Dequeue();
        }
        
        // 每秒检查一次性能
        if (Time.time - _lastQualityAdjustment > 1f)
        {
            AdjustQualityBasedOnPerformance();
            _lastQualityAdjustment = Time.time;
        }
    }
    
    private void AdjustQualityBasedOnPerformance()
    {
        if (_frameTimeHistory.Count < _sampleSize) return;
        
        var averageFrameTime = _frameTimeHistory.Average();
        var currentQuality = QualitySettings.GetQualityLevel();
        
        if (averageFrameTime > _targetFrameTime * 1.2f && currentQuality > 0)
        {
            // 降低质量
            QualitySettings.DecreaseLevel();
            Debug.Log($"Quality decreased to level {QualitySettings.GetQualityLevel()}");
        }
        else if (averageFrameTime < _targetFrameTime * 0.8f && currentQuality < QualitySettings.names.Length - 1)
        {
            // 提高质量
            QualitySettings.IncreaseLevel();
            Debug.Log($"Quality increased to level {QualitySettings.GetQualityLevel()}");
        }
    }
}
```

### 内存优化

#### 资源清理

```csharp
public class ResourceCleaner : MonoBehaviour
{
    [SerializeField] private float _cleanupInterval = 30f;
    [SerializeField] private long _memoryThreshold = 500 * 1024 * 1024; // 500MB
    
    private void Start()
    {
        InvokeRepeating(nameof(CleanupResources), _cleanupInterval, _cleanupInterval);
    }
    
    private void CleanupResources()
    {
        var currentMemory = GC.GetTotalMemory(false);
        
        if (currentMemory > _memoryThreshold)
        {
            Debug.Log("Starting resource cleanup...");
            
            // 卸载未使用的资源
            Resources.UnloadUnusedAssets();
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var newMemory = GC.GetTotalMemory(true);
            var freed = currentMemory - newMemory;
            
            Debug.Log($"Cleanup completed. Freed {freed / 1024 / 1024}MB memory");
        }
    }
}
```

## 🌐 网络连接问题

### API连接诊断

```csharp
public class NetworkDiagnostics : MonoBehaviour
{
    public async Task<NetworkDiagnosticResult> DiagnoseNetworkIssuesAsync()
    {
        var result = new NetworkDiagnosticResult();
        
        // 检查基本网络连接
        result.HasInternetConnection = Application.internetReachability != NetworkReachability.NotReachable;
        
        if (!result.HasInternetConnection)
        {
            result.ErrorMessage = "No internet connection";
            return result;
        }
        
        // 测试DNS解析
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync("api.openai.com");
            result.DNSResolutionWorking = true;
        }
        catch (Exception ex)
        {
            result.DNSResolutionWorking = false;
            result.ErrorMessage += $"DNS resolution failed: {ex.Message}; ";
        }
        
        // 测试API连接
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            
            var response = await client.GetAsync("https://api.openai.com/v1/models");
            result.APIReachable = response.IsSuccessStatusCode;
            result.ResponseTime = response.Headers.Date?.Subtract(DateTime.UtcNow).TotalMilliseconds ?? 0;
        }
        catch (Exception ex)
        {
            result.APIReachable = false;
            result.ErrorMessage += $"API connection failed: {ex.Message}; ";
        }
        
        return result;
    }
}

[Serializable]
public class NetworkDiagnosticResult
{
    public bool HasInternetConnection;
    public bool DNSResolutionWorking;
    public bool APIReachable;
    public double ResponseTime;
    public string ErrorMessage = "";
}
```

## 🔨 调试工具

### 内置调试面板

```csharp
public class DebugPanel : MonoBehaviour
{
    [SerializeField] private bool _showDebugPanel = true;
    [SerializeField] private KeyCode _toggleKey = KeyCode.F1;
    
    private bool _isVisible = false;
    private Vector2 _scrollPosition;
    
    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            _isVisible = !_isVisible;
        }
    }
    
    private void OnGUI()
    {
        if (!_showDebugPanel || !_isVisible) return;
        
        var rect = new Rect(10, 10, 400, 600);
        GUILayout.BeginArea(rect, GUI.skin.box);
        
        GUILayout.Label("TripMeta Debug Panel", GUI.skin.label);
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        
        // 系统信息
        GUILayout.Label("=== System Info ===");
        GUILayout.Label($"FPS: {1f / Time.unscaledDeltaTime:F1}");
        GUILayout.Label($"Memory: {GC.GetTotalMemory(false) / 1024 / 1024}MB");
        GUILayout.Label($"VR Device: {XRSettings.loadedDeviceName}");
        
        // AI服务状态
        GUILayout.Label("=== AI Services ===");
        var aiManager = FindObjectOfType<AIServiceManager>();
        if (aiManager != null)
        {
            GUILayout.Label($"GPT Status: {aiManager.GetGPTStatus()}");
            GUILayout.Label($"Speech Status: {aiManager.GetSpeechStatus()}");
        }
        
        // 性能监控
        GUILayout.Label("=== Performance ===");
        var perfMonitor = FindObjectOfType<PerformanceMonitor>();
        if (perfMonitor != null)
        {
            var report = perfMonitor.GetPerformanceReport();
            GUILayout.Label($"CPU Usage: {report.CPUUsage:F1}%");
            GUILayout.Label($"GPU Usage: {report.GPUUsage:F1}%");
            GUILayout.Label($"Draw Calls: {report.DrawCalls}");
        }
        
        // 调试按钮
        GUILayout.Label("=== Debug Actions ===");
        if (GUILayout.Button("Force GC"))
        {
            GC.Collect();
        }
        
        if (GUILayout.Button("Reload Scene"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        if (GUILayout.Button("Test AI Service"))
        {
            StartCoroutine(TestAIService());
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private IEnumerator TestAIService()
    {
        Debug.Log("Testing AI service...");
        
        var gptService = ServiceContainer.Instance.GetService<IGPTService>();
        if (gptService != null)
        {
            var testPrompt = "Hello, this is a test message.";
            
            try
            {
                var response = gptService.GenerateResponseAsync(testPrompt);
                yield return new WaitUntil(() => response.IsCompleted);
                
                Debug.Log($"AI Test Result: {response.Result}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"AI Test Failed: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("GPT Service not found");
        }
    }
}
```

### 日志收集器

```csharp
public class LogCollector : MonoBehaviour
{
    private List<LogEntry> _logs = new List<LogEntry>();
    private int _maxLogs = 1000;
    
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    
    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        var entry = new LogEntry
        {
            Message = logString,
            StackTrace = stackTrace,
            Type = type,
            Timestamp = DateTime.Now
        };
        
        _logs.Add(entry);
        
        if (_logs.Count > _maxLogs)
        {
            _logs.RemoveAt(0);
        }
    }
    
    public void ExportLogs()
    {
        var logPath = Path.Combine(Application.persistentDataPath, "debug_logs.txt");
        
        using var writer = new StreamWriter(logPath);
        foreach (var log in _logs)
        {
            writer.WriteLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Type}] {log.Message}");
            if (!string.IsNullOrEmpty(log.StackTrace))
            {
                writer.WriteLine(log.StackTrace);
            }
            writer.WriteLine();
        }
        
        Debug.Log($"Logs exported to: {logPath}");
    }
}

[Serializable]
public class LogEntry
{
    public string Message;
    public string StackTrace;
    public LogType Type;
    public DateTime Timestamp;
}
```

---

*如果遇到本指南未涵盖的问题，请在GitHub Issues中报告，我们会及时更新文档。*