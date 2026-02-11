# TripMeta 系统架构设计

## 📋 目录

- [架构概览](#架构概览)
- [分层架构](#分层架构)
- [核心模块](#核心模块)
- [数据流设计](#数据流设计)
- [服务架构](#服务架构)
- [部署架构](#部署架构)
- [扩展性设计](#扩展性设计)
- [安全架构](#安全架构)

## 🏗️ 架构概览

TripMeta采用现代化的分层架构设计，结合依赖注入、事件驱动和微服务模式，构建了一个可扩展、可维护的AI驱动VR旅游平台。

### 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                        TripMeta Architecture                    │
├─────────────────────────────────────────────────────────────────┤
│  Presentation Layer (表现层)                                   │
│  ├── VR Interface          ├── Mobile Companion                │
│  │   ├── Spatial UI        │   ├── Control Panel               │
│  │   ├── Voice Interface   │   ├── Settings                    │
│  │   └── Gesture Control   │   └── Social Features             │
│  ├── Web Dashboard         └── Admin Panel                     │
│  │   ├── Analytics         ├── User Management                 │
│  │   ├── Content Mgmt      ├── System Monitor                  │
│  │   └── User Profile      └── Configuration                   │
├─────────────────────────────────────────────────────────────────┤
│  Application Layer (应用层)                                    │
│  ├── AI Services           ├── VR Services                     │
│  │   ├── GPT Integration   │   ├── Interaction Manager         │
│  │   ├── Speech Service    │   ├── Gesture Recognition         │
│  │   ├── Vision Service    │   ├── Spatial UI Manager          │
│  │   └── Recommendation    │   └── Performance Optimizer       │
│  ├── User Management       ├── Content Management              │
│  │   ├── Authentication    │   ├── Scene Manager               │
│  │   ├── Profile Service   │   ├── Asset Manager               │
│  │   ├── Preference Mgmt   │   ├── Content Generator           │
│  │   └── Social Features   │   └── Version Control             │
│  ├── Analytics Service     └── Notification Service            │
│  │   ├── User Behavior     ├── Real-time Alerts               │
│  │   ├── Performance       ├── System Events                   │
│  │   └── Business Metrics  └── User Notifications              │
├─────────────────────────────────────────────────────────────────┤
│  Infrastructure Layer (基础设施层)                             │
│  ├── Core Framework        ├── Configuration                   │
│  │   ├── Dependency Inject │   ├── Environment Config          │
│  │   ├── Service Container │   ├── Feature Flags               │
│  │   ├── Event Bus         │   ├── API Keys Management         │
│  │   └── Service Locator   │   └── Runtime Settings            │
│  ├── Error Handling        ├── Performance Monitoring          │
│  │   ├── Global Exception  │   ├── Metrics Collection          │
│  │   ├── Logging System    │   ├── Performance Profiler       │
│  │   ├── Recovery Strategy │   ├── Memory Monitor              │
│  │   └── Health Check      │   └── Network Monitor             │
│  ├── Security Framework    └── Caching System                  │
│  │   ├── Authentication    ├── Memory Cache                    │
│  │   ├── Authorization     ├── Distributed Cache               │
│  │   ├── Data Encryption   ├── Asset Cache                     │
│  │   └── Audit Logging     └── Query Cache                     │
├─────────────────────────────────────────────────────────────────┤
│  Data Layer (数据层)                                           │
│  ├── Local Storage         ├── Cloud Storage                   │
│  │   ├── SQLite Database   │   ├── PostgreSQL                  │
│  │   ├── File System       │   ├── Redis Cache                 │
│  │   ├── Player Prefs      │   ├── Object Storage              │
│  │   └── Streaming Assets  │   └── CDN Assets                  │
│  ├── External APIs         ├── Message Queue                   │
│  │   ├── OpenAI API        │   ├── Event Streaming             │
│  │   ├── Azure Services    │   ├── Task Queue                  │
│  │   ├── Analytics APIs    │   ├── Notification Queue          │
│  │   └── Third-party APIs  │   └── Background Jobs             │
└─────────────────────────────────────────────────────────────────┘
```

## 🏛️ 分层架构

### 表现层 (Presentation Layer)

负责用户界面和交互体验，包括VR界面、移动端配套应用、Web管理后台等。

```csharp
namespace TripMeta.Presentation
{
    // VR用户界面
    public interface IVRUserInterface
    {
        void ShowSpatialUI(UIPanel panel, Vector3 position);
        void HandleVoiceCommand(string command);
        void ProcessGesture(GestureData gesture);
    }
    
    // 移动端界面
    public interface IMobileInterface
    {
        void ShowControlPanel();
        void SyncWithVRSession();
        void ManageSettings();
    }
    
    // Web管理界面
    public interface IWebDashboard
    {
        void DisplayAnalytics();
        void ManageContent();
        void MonitorSystem();
    }
}
```

### 应用层 (Application Layer)

包含业务逻辑和应用服务，协调各个领域服务完成具体的业务功能。

```csharp
namespace TripMeta.Application
{
    // 应用服务接口
    public interface IApplicationService
    {
        Task<Result<T>> ExecuteAsync<T>(ICommand<T> command);
        Task<T> QueryAsync<T>(IQuery<T> query);
    }
    
    // AI服务协调器
    public class AIServiceOrchestrator : IApplicationService
    {
        private readonly IGPTService _gptService;
        private readonly ISpeechService _speechService;
        private readonly IVisionService _visionService;
        
        public async Task<TourGuideResponse> ProcessTourRequestAsync(TourRequest request)
        {
            // 协调多个AI服务完成旅游导览
            var sceneAnalysis = await _visionService.AnalyzeSceneAsync(request.SceneImage);
            var tourContent = await _gptService.GenerateTourContentAsync(request.UserQuery, sceneAnalysis);
            var audioResponse = await _speechService.SynthesizeAsync(tourContent);
            
            return new TourGuideResponse
            {
                TextContent = tourContent,
                AudioContent = audioResponse,
                SceneInsights = sceneAnalysis
            };
        }
    }
}
```

### 基础设施层 (Infrastructure Layer)

提供技术基础设施支持，包括依赖注入、配置管理、错误处理、性能监控等。

```csharp
namespace TripMeta.Infrastructure
{
    // 服务容器
    public class ServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services;
        private readonly Dictionary<Type, object> _singletonInstances;
        
        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Singleton
            };
        }
        
        public T GetService<T>()
        {
            var serviceType = typeof(T);
            if (!_services.ContainsKey(serviceType))
                throw new ServiceNotFoundException($"Service {serviceType.Name} not registered");
                
            return (T)CreateInstance(_services[serviceType]);
        }
    }
    
    // 配置管理
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly Dictionary<string, object> _configurations;
        
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_configurations.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return defaultValue;
        }
    }
}
```

### 数据层 (Data Layer)

负责数据存储和外部服务集成，提供统一的数据访问接口。

```csharp
namespace TripMeta.Data
{
    // 数据仓储模式
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(string id);
    }
    
    // 用户数据仓储
    public class UserRepository : IRepository<User>
    {
        private readonly IDatabase _database;
        
        public async Task<User> GetByIdAsync(string id)
        {
            return await _database.QuerySingleAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }
    }
    
    // 外部API客户端
    public class OpenAIClient : IOpenAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        
        public async Task<GPTResponse> GenerateCompletionAsync(GPTRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/v1/chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<GPTResponse>(responseJson);
        }
    }
}
```

## 🔧 核心模块

### 依赖注入容器

```csharp
// 服务生命周期管理
public enum ServiceLifetime
{
    Transient,  // 每次请求创建新实例
    Scoped,     // 在作用域内单例
    Singleton   // 全局单例
}

// 服务描述符
public class ServiceDescriptor
{
    public Type ServiceType { get; set; }
    public Type ImplementationType { get; set; }
    public ServiceLifetime Lifetime { get; set; }
    public Func<IServiceProvider, object> Factory { get; set; }
}

// 高级服务容器
public class AdvancedServiceContainer : IServiceContainer, IDisposable
{
    private readonly ConcurrentDictionary<Type, ServiceDescriptor> _services;
    private readonly ConcurrentDictionary<Type, object> _singletonInstances;
    private readonly ThreadLocal<Dictionary<Type, object>> _scopedInstances;
    
    public void RegisterFactory<T>(Func<IServiceProvider, T> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        _services[typeof(T)] = new ServiceDescriptor
        {
            ServiceType = typeof(T),
            Factory = provider => factory(provider),
            Lifetime = lifetime
        };
    }
    
    public T GetRequiredService<T>()
    {
        var service = GetService<T>();
        if (service == null)
            throw new InvalidOperationException($"Required service {typeof(T).Name} not found");
        return service;
    }
}
```

### 事件驱动架构

```csharp
// 事件总线
public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<IEventHandler>> _handlers;
    private readonly ILogger _logger;
    
    public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
    {
        var eventType = typeof(T);
        _handlers.AddOrUpdate(eventType, 
            new List<IEventHandler> { handler },
            (key, existing) => { existing.Add(handler); return existing; });
    }
    
    public async Task PublishAsync<T>(T eventData) where T : IEvent
    {
        var eventType = typeof(T);
        if (!_handlers.TryGetValue(eventType, out var handlers))
            return;
            
        var tasks = handlers.Cast<IEventHandler<T>>()
            .Select(handler => HandleEventSafelyAsync(handler, eventData));
            
        await Task.WhenAll(tasks);
    }
    
    private async Task HandleEventSafelyAsync<T>(IEventHandler<T> handler, T eventData) where T : IEvent
    {
        try
        {
            await handler.HandleAsync(eventData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling event {typeof(T).Name}");
        }
    }
}

// 事件处理器
public interface IEventHandler<T> where T : IEvent
{
    Task HandleAsync(T eventData);
}

// 领域事件
public class UserLoginEvent : IEvent
{
    public string UserId { get; set; }
    public DateTime LoginTime { get; set; }
    public string DeviceInfo { get; set; }
    public string IPAddress { get; set; }
}
```

### 配置管理系统

```csharp
// 分层配置系统
public class HierarchicalConfigurationManager : IConfigurationManager
{
    private readonly List<IConfigurationSource> _sources;
    
    public HierarchicalConfigurationManager()
    {
        _sources = new List<IConfigurationSource>
        {
            new EnvironmentVariableSource(),
            new JsonFileSource("appsettings.json"),
            new JsonFileSource($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json"),
            new CommandLineSource(),
            new RemoteConfigSource()
        };
    }
    
    public T GetValue<T>(string key, T defaultValue = default)
    {
        foreach (var source in _sources)
        {
            if (source.TryGetValue(key, out var value))
            {
                return ConvertValue<T>(value);
            }
        }
        return defaultValue;
    }
    
    public void WatchForChanges(string key, Action<object> callback)
    {
        foreach (var source in _sources.OfType<IWatchableConfigurationSource>())
        {
            source.Watch(key, callback);
        }
    }
}

// 配置源接口
public interface IConfigurationSource
{
    bool TryGetValue(string key, out object value);
}

public interface IWatchableConfigurationSource : IConfigurationSource
{
    void Watch(string key, Action<object> callback);
}
```

## 📊 数据流设计

### 用户交互数据流

```
用户VR交互 → 输入处理 → 事件总线 → 应用服务 → AI服务 → 响应生成 → UI更新
     ↓           ↓          ↓         ↓        ↓        ↓         ↓
  手势/语音   → 标准化输入 → 事件分发 → 业务逻辑 → AI处理 → 结果封装 → 界面渲染
```

### AI服务数据流

```csharp
// AI服务管道
public class AIServicePipeline
{
    private readonly List<IAIProcessor> _processors;
    
    public async Task<AIResponse> ProcessAsync(AIRequest request)
    {
        var context = new AIProcessingContext(request);
        
        foreach (var processor in _processors)
        {
            context = await processor.ProcessAsync(context);
            
            if (context.ShouldTerminate)
                break;
        }
        
        return context.Response;
    }
}

// AI处理器接口
public interface IAIProcessor
{
    Task<AIProcessingContext> ProcessAsync(AIProcessingContext context);
}

// 具体处理器实现
public class InputValidationProcessor : IAIProcessor
{
    public async Task<AIProcessingContext> ProcessAsync(AIProcessingContext context)
    {
        if (string.IsNullOrEmpty(context.Request.Input))
        {
            context.Response.Error = "Input cannot be empty";
            context.ShouldTerminate = true;
        }
        
        return context;
    }
}

public class ContentFilterProcessor : IAIProcessor
{
    public async Task<AIProcessingContext> ProcessAsync(AIProcessingContext context)
    {
        // 内容过滤逻辑
        if (ContainsInappropriateContent(context.Request.Input))
        {
            context.Response.Error = "Content filtered";
            context.ShouldTerminate = true;
        }
        
        return context;
    }
}
```

## 🌐 服务架构

### 微服务通信

```csharp
// 服务间通信接口
public interface IServiceCommunication
{
    Task<TResponse> CallAsync<TRequest, TResponse>(string serviceName, string method, TRequest request);
    Task PublishEventAsync<T>(T eventData) where T : IEvent;
    void Subscribe<T>(Func<T, Task> handler) where T : IEvent;
}

// HTTP服务通信
public class HttpServiceCommunication : IServiceCommunication
{
    private readonly HttpClient _httpClient;
    private readonly IServiceDiscovery _serviceDiscovery;
    
    public async Task<TResponse> CallAsync<TRequest, TResponse>(string serviceName, string method, TRequest request)
    {
        var serviceUrl = await _serviceDiscovery.GetServiceUrlAsync(serviceName);
        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{serviceUrl}/{method}", content);
        var responseJson = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<TResponse>(responseJson);
    }
}

// 服务发现
public interface IServiceDiscovery
{
    Task<string> GetServiceUrlAsync(string serviceName);
    Task RegisterServiceAsync(string serviceName, string url);
    Task<bool> IsServiceHealthyAsync(string serviceName);
}
```

### 负载均衡和容错

```csharp
// 断路器模式
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitBreakerState _state;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitBreakerState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = CircuitBreakerState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }
        
        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception)
        {
            OnFailure();
            throw;
        }
    }
    
    private void OnSuccess()
    {
        _failureCount = 0;
        _state = CircuitBreakerState.Closed;
    }
    
    private void OnFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
        
        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitBreakerState.Open;
        }
    }
}

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}
```

## 🚀 部署架构

### 容器化部署

```yaml
# docker-compose.yml
version: '3.8'
services:
  tripmeta-app:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ENVIRONMENT=production
      - DATABASE_URL=${DATABASE_URL}
      - REDIS_URL=${REDIS_URL}
    depends_on:
      - database
      - redis
      - ai-service
    
  ai-service:
    image: tripmeta/ai-service:latest
    ports:
      - "8081:8081"
    environment:
      - OPENAI_API_KEY=${OPENAI_API_KEY}
      - AZURE_SPEECH_KEY=${AZURE_SPEECH_KEY}
    
  database:
    image: postgres:13
    environment:
      - POSTGRES_DB=tripmeta
      - POSTGRES_USER=${DB_USER}
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    
  redis:
    image: redis:6-alpine
    ports:
      - "6379:6379"
    
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - tripmeta-app

volumes:
  postgres_data:
```

### Kubernetes部署

```yaml
# k8s-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: tripmeta-deployment
spec:
  replicas: 3
  selector:
    matchLabels:
      app: tripmeta
  template:
    metadata:
      labels:
        app: tripmeta
    spec:
      containers:
      - name: tripmeta
        image: tripmeta:latest
        ports:
        - containerPort: 8080
        env:
        - name: DATABASE_URL
          valueFrom:
            secretKeyRef:
              name: tripmeta-secrets
              key: database-url
        resources:
          requests:
            memory: "2Gi"
            cpu: "1000m"
          limits:
            memory: "4Gi"
            cpu: "2000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: tripmeta-service
spec:
  selector:
    app: tripmeta
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

## 📈 扩展性设计

### 水平扩展策略

```csharp
// 分片策略
public class ShardingStrategy
{
    private readonly List<string> _shards;
    
    public string GetShardForUser(string userId)
    {
        var hash = userId.GetHashCode();
        var shardIndex = Math.Abs(hash) % _shards.Count;
        return _shards[shardIndex];
    }
    
    public string GetShardForData(string dataKey)
    {
        // 一致性哈希算法
        return ConsistentHash.GetShard(dataKey, _shards);
    }
}

// 缓存分层
public class TieredCacheManager
{
    private readonly IMemoryCache _l1Cache;      // 内存缓存
    private readonly IDistributedCache _l2Cache; // Redis缓存
    private readonly IObjectStorage _l3Cache;    // 对象存储
    
    public async Task<T> GetAsync<T>(string key)
    {
        // L1缓存
        if (_l1Cache.TryGetValue(key, out T value))
            return value;
            
        // L2缓存
        var l2Value = await _l2Cache.GetAsync<T>(key);
        if (l2Value != null)
        {
            _l1Cache.Set(key, l2Value, TimeSpan.FromMinutes(5));
            return l2Value;
        }
        
        // L3缓存
        var l3Value = await _l3Cache.GetAsync<T>(key);
        if (l3Value != null)
        {
            await _l2Cache.SetAsync(key, l3Value, TimeSpan.FromHours(1));
            _l1Cache.Set(key, l3Value, TimeSpan.FromMinutes(5));
            return l3Value;
        }
        
        return default(T);
    }
}
```

## 🔒 安全架构

### 认证和授权

```csharp
// JWT认证
public class JwtAuthenticationService : IAuthenticationService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    
    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await ValidateCredentialsAsync(request.Username, request.Password);
        if (user == null)
            return AuthenticationResult.Failed("Invalid credentials");
            
        var token = GenerateJwtToken(user);
        return AuthenticationResult.Success(token, user);
    }
    
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// 基于角色的访问控制
public class RoleBasedAuthorizationService : IAuthorizationService
{
    private readonly Dictionary<string, List<string>> _rolePermissions;
    
    public async Task<bool> AuthorizeAsync(string userId, string resource, string action)
    {
        var user = await GetUserAsync(userId);
        var requiredPermission = $"{resource}:{action}";
        
        if (_rolePermissions.TryGetValue(user.Role, out var permissions))
        {
            return permissions.Contains(requiredPermission) || permissions.Contains("*");
        }
        
        return false;
    }
}
```

### 数据加密

```csharp
// 数据加密服务
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        
        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);
        
        swEncrypt.Write(plainText);
        return Convert.ToBase64String(msEncrypt.ToArray());
    }
    
    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        
        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}
```

---

*本架构文档会随着系统演进持续更新，确保架构设计与实际实现保持一致。*