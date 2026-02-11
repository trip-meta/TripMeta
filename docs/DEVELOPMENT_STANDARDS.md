# TripMeta 开发规范

## 📋 目录

- [代码规范](#代码规范)
- [架构规范](#架构规范)
- [Git工作流](#git工作流)
- [代码审查](#代码审查)
- [测试规范](#测试规范)
- [文档规范](#文档规范)
- [性能规范](#性能规范)
- [安全规范](#安全规范)

## 💻 代码规范

### C# 编码标准

#### 命名约定

```csharp
// ✅ 正确的命名方式
public class AIServiceManager          // 类名：PascalCase
{
    private readonly ILogger _logger;   // 私有字段：_camelCase
    public bool IsInitialized { get; }  // 属性：PascalCase
    
    public async Task InitializeAsync() // 方法：PascalCase
    {
        var config = new AIConfig();    // 局部变量：camelCase
        const int MaxRetries = 3;       // 常量：PascalCase
    }
}

// ❌ 错误的命名方式
public class aiServiceManager          // 类名应该PascalCase
{
    private ILogger logger;            // 私有字段缺少下划线前缀
    public bool isInitialized;         // 属性应该PascalCase
    
    public async Task initialize_async() // 方法名不应该使用下划线
    {
        var Config = new AIConfig();    // 局部变量不应该PascalCase
    }
}
```

#### 代码格式

```csharp
// ✅ 正确的格式
public class ExampleClass : MonoBehaviour, IDisposable
{
    [Header("Configuration")]
    [SerializeField] private float _speed = 5.0f;
    
    private readonly Dictionary<string, object> _cache = new();
    
    public event Action<bool> OnStateChanged;
    
    public async Task<bool> ProcessAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }
        
        try
        {
            var result = await SomeAsyncOperation(input);
            OnStateChanged?.Invoke(result);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Processing failed: {ex.Message}");
            return false;
        }
    }
    
    public void Dispose()
    {
        // 清理资源
        _cache?.Clear();
        OnStateChanged = null;
    }
}
```

### Unity特定规范

#### MonoBehaviour最佳实践

```csharp
// ✅ 正确的MonoBehaviour实现
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    
    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _cameraTransform;
    
    // 缓存组件引用，避免重复GetComponent调用
    private CharacterController _characterController;
    private Animator _animator;
    
    // 使用属性而不是公共字段
    public bool IsGrounded { get; private set; }
    
    private void Awake()
    {
        // 在Awake中获取组件引用
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        // 在Start中进行初始化逻辑
        InitializePlayer();
    }
    
    private void Update()
    {
        // 避免在Update中执行昂贵操作
        HandleInput();
    }
    
    private void FixedUpdate()
    {
        // 物理相关操作放在FixedUpdate中
        HandleMovement();
    }
}
```

## 🏗️ 架构规范

### 依赖注入模式

```csharp
// ✅ 正确的依赖注入实现
public interface IUserService
{
    Task<User> GetUserAsync(string userId);
}

public class UserService : IUserService
{
    private readonly ILogger _logger;
    private readonly IDatabase _database;
    
    // 构造函数注入
    public UserService(ILogger logger, IDatabase database)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }
    
    public async Task<User> GetUserAsync(string userId)
    {
        _logger.LogInfo($"Getting user: {userId}");
        return await _database.GetUserAsync(userId);
    }
}
```

### 事件驱动架构

```csharp
// ✅ 事件驱动的实现
public class GameEvents
{
    public static event Action<Player> OnPlayerSpawned;
    public static event Action<int> OnScoreChanged;
    public static event Action<string> OnGameStateChanged;
    
    public static void PlayerSpawned(Player player) => OnPlayerSpawned?.Invoke(player);
    public static void ScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
    public static void GameStateChanged(string newState) => OnGameStateChanged?.Invoke(newState);
}
```

## 🔄 Git工作流

### 分支策略

```
main (生产环境)
├── develop (开发环境)
│   ├── feature/ai-integration (功能分支)
│   ├── feature/vr-optimization (功能分支)
│   └── feature/user-interface (功能分支)
├── release/v1.2.0 (发布分支)
└── hotfix/critical-bug-fix (热修复分支)
```

### 提交规范

```bash
# 提交消息格式
<type>(<scope>): <subject>

<body>

<footer>

# 示例
feat(ai): add GPT-4 integration for smart tour guide

- Implement GPTService with async/await pattern
- Add configuration for API key management
- Include error handling and retry logic
- Add unit tests for service methods

Closes #123
```

### 提交类型

- `feat`: 新功能
- `fix`: 修复bug
- `docs`: 文档更新
- `style`: 代码格式调整
- `refactor`: 代码重构
- `perf`: 性能优化
- `test`: 测试相关
- `chore`: 构建工具、辅助工具等

## 👀 代码审查

### 审查清单

#### 功能性检查
- [ ] 代码实现了需求规格说明中的所有功能
- [ ] 边界条件和异常情况得到正确处理
- [ ] 代码逻辑清晰，易于理解
- [ ] 没有明显的bug或逻辑错误

#### 代码质量检查
- [ ] 遵循项目编码规范
- [ ] 变量和方法命名清晰有意义
- [ ] 代码复杂度在可接受范围内
- [ ] 没有重复代码或可以提取的公共逻辑

#### 性能检查
- [ ] 没有明显的性能问题
- [ ] 合理使用缓存和对象池
- [ ] 避免在Update中执行昂贵操作
- [ ] 内存使用合理，没有内存泄漏

#### 安全检查
- [ ] 输入验证和清理
- [ ] 没有硬编码的敏感信息
- [ ] 权限检查正确实现
- [ ] 防止常见安全漏洞

## 🧪 测试规范

### 测试策略

#### 单元测试规范

```csharp
[TestFixture]
public class GPTServiceTests
{
    private GPTService _gptService;
    private Mock<IHttpClient> _mockHttpClient;
    private Mock<ILogger> _mockLogger;
    
    [SetUp]
    public void Setup()
    {
        _mockHttpClient = new Mock<IHttpClient>();
        _mockLogger = new Mock<ILogger>();
        _gptService = new GPTService(_mockHttpClient.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task GenerateResponseAsync_ShouldReturnResponse_WhenValidPrompt()
    {
        // Arrange
        var prompt = "Hello, world!";
        var expectedResponse = "Hello! How can I help you?";
        
        _mockHttpClient
            .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponse { Content = expectedResponse });
        
        // Act
        var result = await _gptService.GenerateResponseAsync(prompt);
        
        // Assert
        Assert.AreEqual(expectedResponse, result);
        _mockHttpClient.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
```

### 测试覆盖率要求

- **单元测试覆盖率**: ≥ 80%
- **集成测试覆盖率**: ≥ 60%
- **关键路径覆盖率**: 100%
- **新增代码覆盖率**: ≥ 90%

## ⚡ 性能规范

### 性能目标

#### VR性能指标
- **帧率**: 90 FPS (PICO 4)
- **延迟**: < 20ms (Motion-to-Photon)
- **内存使用**: < 4GB
- **CPU使用率**: < 70%
- **GPU使用率**: < 80%

#### AI服务性能指标
- **响应时间**: < 3秒 (GPT响应)
- **并发用户**: 1000+ 同时在线
- **可用性**: 99.9% 正常运行时间
- **错误率**: < 0.1%

## 🔒 安全规范

### 数据安全
- 用户数据加密存储
- API密钥安全管理
- 网络传输加密
- 访问权限控制

### 隐私保护
- 用户同意机制
- 数据最小化原则
- 匿名化处理
- GDPR合规

---

*本文档会根据项目发展持续更新，请定期查看最新版本。*