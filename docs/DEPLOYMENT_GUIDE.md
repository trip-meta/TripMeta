# TripMeta 部署指南

## 📋 目录

- [环境准备](#环境准备)
- [开发环境部署](#开发环境部署)
- [测试环境部署](#测试环境部署)
- [生产环境部署](#生产环境部署)
- [配置管理](#配置管理)
- [监控和维护](#监控和维护)
- [故障排除](#故障排除)

## 🛠️ 环境准备

### 系统要求

#### 开发环境
- **操作系统**: Windows 10/11 x64, macOS 10.15+, Ubuntu 18.04+
- **Unity版本**: 2022.3 LTS 或更高
- **IDE**: Visual Studio 2022, JetBrains Rider, 或 VS Code
- **内存**: 16GB+ RAM
- **存储**: 50GB+ 可用空间
- **显卡**: GTX 1070 / RTX 2060 或更高

#### VR设备要求
- **PICO 4**: 推荐设备
- **PICO 4 Enterprise**: 企业版支持
- **Meta Quest 2/3**: 实验性支持
- **HTC Vive/Valve Index**: 计划支持

#### 服务器要求
- **CPU**: 8核心以上
- **内存**: 32GB+ RAM
- **存储**: SSD 500GB+
- **网络**: 1Gbps+ 带宽
- **GPU**: 支持CUDA的显卡（AI推理）

### 软件依赖

#### Unity包管理器依赖
```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "14.0.8",
    "com.unity.xr.management": "4.4.0",
    "com.unity.addressables": "1.21.14",
    "com.unity.netcode.gameobjects": "1.5.2",
    "com.unity.ai.navigation": "1.1.4",
    "com.unity.cinemachine": "2.9.7",
    "com.unity.inputsystem": "1.7.0"
  }
}
```

#### 第三方SDK
- PICO Unity Integration SDK v2.1.1
- OpenAI API Client
- Azure Cognitive Services SDK
- Firebase Unity SDK (可选)

## 🏠 开发环境部署

### 1. 项目克隆和设置

```bash
# 克隆项目
git clone https://github.com/yourusername/tripmeta.git
cd tripmeta

# 切换到开发分支
git checkout develop

# 安装Git LFS（用于大文件管理）
git lfs install
git lfs pull
```

### 2. Unity项目配置

```bash
# 打开Unity Hub
# 添加项目：选择TripMeta文件夹
# Unity版本：2022.3 LTS

# 或使用命令行（Windows）
"C:\Program Files\Unity\Hub\Editor\2022.3.0f1\Editor\Unity.exe" -projectPath "D:\project\TripMeta\TripMeta"
```

### 3. 环境配置

创建环境配置文件：

```bash
# 复制环境配置模板
cp .env.example .env
```

编辑 `.env` 文件：

```bash
# AI服务配置
OPENAI_API_KEY=your_openai_api_key_here
AZURE_SPEECH_KEY=your_azure_speech_key_here
AZURE_SPEECH_REGION=eastus

# 数据库配置
DATABASE_URL=sqlite://local.db
REDIS_URL=redis://localhost:6379

# 日志配置
LOG_LEVEL=Debug
LOG_OUTPUT=Console,File

# VR配置
VR_TARGET_FRAMERATE=90
VR_EYE_TEXTURE_RESOLUTION=2048
```

### 4. 依赖安装

```bash
# 安装Node.js依赖（构建工具）
npm install

# 安装Python依赖（AI服务）
pip install -r requirements.txt

# 启动本地服务
npm run dev
```

### 5. 数据库初始化

```bash
# 创建本地数据库
npm run db:create

# 运行数据库迁移
npm run db:migrate

# 填充测试数据
npm run db:seed
```

## 🧪 测试环境部署

### 1. Docker容器化部署

创建 `Dockerfile`：

```dockerfile
FROM ubuntu:20.04

# 安装Unity和依赖
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    xvfb \
    libglu1 \
    libxcursor1 \
    libxrandr2

# 安装Unity
WORKDIR /opt/unity
RUN wget -O UnitySetup.tar.xz https://download.unity3d.com/download_unity/...
RUN tar -xf UnitySetup.tar.xz

# 复制项目文件
COPY . /app
WORKDIR /app

# 构建项目
RUN /opt/unity/Editor/Unity \
    -batchmode \
    -quit \
    -projectPath /app/TripMeta \
    -buildTarget Android \
    -executeMethod BuildScript.BuildAndroid

EXPOSE 8080
CMD ["./start.sh"]
```

构建和运行：

```bash
# 构建Docker镜像
docker build -t tripmeta:test .

# 运行容器
docker run -d \
  --name tripmeta-test \
  -p 8080:8080 \
  -e ENVIRONMENT=test \
  tripmeta:test
```

### 2. Kubernetes部署

创建 `k8s-deployment.yaml`：

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: tripmeta-test
  labels:
    app: tripmeta
    env: test
spec:
  replicas: 2
  selector:
    matchLabels:
      app: tripmeta
      env: test
  template:
    metadata:
      labels:
        app: tripmeta
        env: test
    spec:
      containers:
      - name: tripmeta
        image: tripmeta:test
        ports:
        - containerPort: 8080
        env:
        - name: ENVIRONMENT
          value: "test"
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
---
apiVersion: v1
kind: Service
metadata:
  name: tripmeta-service
spec:
  selector:
    app: tripmeta
    env: test
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

部署到Kubernetes：

```bash
# 应用配置
kubectl apply -f k8s-deployment.yaml

# 检查部署状态
kubectl get pods -l app=tripmeta

# 查看服务
kubectl get services
```

## 🚀 生产环境部署

### 1. 云服务部署（Azure）

#### 容器实例部署

```bash
# 创建资源组
az group create --name tripmeta-prod --location eastus

# 创建容器注册表
az acr create --resource-group tripmeta-prod \
  --name tripmetaregistry --sku Basic

# 推送镜像
az acr build --registry tripmetaregistry \
  --image tripmeta:prod .

# 创建容器实例
az container create \
  --resource-group tripmeta-prod \
  --name tripmeta-prod \
  --image tripmetaregistry.azurecr.io/tripmeta:prod \
  --cpu 4 \
  --memory 8 \
  --ports 80 443 \
  --environment-variables \
    ENVIRONMENT=production \
    LOG_LEVEL=Info
```

#### AKS集群部署

```bash
# 创建AKS集群
az aks create \
  --resource-group tripmeta-prod \
  --name tripmeta-cluster \
  --node-count 3 \
  --node-vm-size Standard_D4s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# 获取集群凭据
az aks get-credentials \
  --resource-group tripmeta-prod \
  --name tripmeta-cluster

# 部署应用
kubectl apply -f k8s-production.yaml
```

### 2. CDN和负载均衡配置

#### Azure CDN配置

```bash
# 创建CDN配置文件
az cdn profile create \
  --resource-group tripmeta-prod \
  --name tripmeta-cdn \
  --sku Standard_Microsoft

# 创建CDN端点
az cdn endpoint create \
  --resource-group your-resource-group \
  --profile-name your-cdn-profile \
  --name your-assets-endpoint \
  --origin your-origin-domain.com
```

#### 负载均衡器配置

```yaml
apiVersion: v1
kind: Service
metadata:
  name: tripmeta-lb
  annotations:
    service.beta.kubernetes.io/azure-load-balancer-internal: "false"
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
    name: http
  - port: 443
    targetPort: 8443
    protocol: TCP
    name: https
  selector:
    app: tripmeta
```

### 3. 数据库部署

#### PostgreSQL配置

```bash
# 创建PostgreSQL服务器
az postgres server create \
  --resource-group tripmeta-prod \
  --name tripmeta-db \
  --location eastus \
  --admin-user tripmetaadmin \
  --admin-password YourSecurePassword123! \
  --sku-name GP_Gen5_4 \
  --version 13

# 创建数据库
az postgres db create \
  --resource-group tripmeta-prod \
  --server-name tripmeta-db \
  --name tripmeta_production
```

#### Redis缓存配置

```bash
# 创建Redis缓存
az redis create \
  --resource-group tripmeta-prod \
  --name tripmeta-cache \
  --location eastus \
  --sku Standard \
  --vm-size c1
```

## ⚙️ 配置管理

### 1. 环境变量配置

#### 开发环境 (.env.development)

```bash
# 服务配置
ENVIRONMENT=development
DEBUG=true
LOG_LEVEL=Debug

# API配置
API_BASE_URL=http://localhost:8080
OPENAI_API_KEY=sk-dev-key
AZURE_SPEECH_KEY=dev-speech-key

# 数据库配置
DATABASE_URL=sqlite://dev.db
REDIS_URL=redis://localhost:6379

# VR配置
VR_TARGET_FRAMERATE=72
VR_ENABLE_FOVEATED_RENDERING=false
```

#### 生产环境 (.env.production)

```bash
# 服务配置
ENVIRONMENT=production
DEBUG=false
LOG_LEVEL=Info

# API配置
# API_BASE_URL=https://api.your-domain.com  # Replace with your API URL
OPENAI_API_KEY=${OPENAI_API_KEY}
AZURE_SPEECH_KEY=${AZURE_SPEECH_KEY}

# 数据库配置
DATABASE_URL=${DATABASE_URL}
REDIS_URL=${REDIS_URL}

# VR配置
VR_TARGET_FRAMERATE=90
VR_ENABLE_FOVEATED_RENDERING=true

# 安全配置
JWT_SECRET=${JWT_SECRET}
ENCRYPTION_KEY=${ENCRYPTION_KEY}
```

### 2. Unity配置文件

#### 运行时配置 (StreamingAssets/config.json)

```json
{
  "ai": {
    "gpt": {
      "model": "gpt-4",
      "maxTokens": 2048,
      "temperature": 0.7,
      "timeout": 30
    },
    "speech": {
      "language": "zh-CN",
      "voice": "zh-CN-XiaoxiaoNeural",
      "speechRate": 1.0
    }
  },
  "vr": {
    "targetFrameRate": 90,
    "eyeTextureResolution": 2048,
    "enableFoveatedRendering": true,
    "trackingSpace": "RoomScale"
  },
  "performance": {
    "enableProfiling": false,
    "memoryThreshold": 1024,
    "autoOptimization": true
  }
}
```

### 3. 构建配置

#### Unity构建设置

```csharp
// BuildConfiguration.cs
[CreateAssetMenu(fileName = "BuildConfig", menuName = "TripMeta/Build Configuration")]
public class BuildConfiguration : ScriptableObject
{
    [Header("Build Settings")]
    public BuildTarget targetPlatform = BuildTarget.Android;
    public bool developmentBuild = false;
    public bool autoConnectProfiler = false;
    public bool deepProfilingSupport = false;
    
    [Header("Android Settings")]
    public AndroidArchitecture targetArchitectures = AndroidArchitecture.ARM64;
    public AndroidBuildSystem buildSystem = AndroidBuildSystem.Gradle;
    public int bundleVersionCode = 1;
    
    [Header("Optimization")]
    public bool stripEngineCode = true;
    public ManagedStrippingLevel managedStrippingLevel = ManagedStrippingLevel.High;
    public bool il2CppCodeGeneration = true;
}
```

## 📊 监控和维护

### 1. 应用监控

#### Prometheus配置

```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'tripmeta'
    static_configs:
      - targets: ['tripmeta-service:8080']
    metrics_path: /metrics
    scrape_interval: 5s
```

#### Grafana仪表板

```json
{
  "dashboard": {
    "title": "TripMeta Monitoring",
    "panels": [
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "http_request_duration_seconds{job=\"tripmeta\"}"
          }
        ]
      },
      {
        "title": "Memory Usage",
        "type": "graph",
        "targets": [
          {
            "expr": "process_resident_memory_bytes{job=\"tripmeta\"}"
          }
        ]
      }
    ]
  }
}
```

### 2. 日志管理

#### ELK Stack配置

```yaml
# docker-compose.yml
version: '3.7'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.14.0
    environment:
      - discovery.type=single-node
    ports:
      - "9200:9200"
  
  logstash:
    image: docker.elastic.co/logstash/logstash:7.14.0
    volumes:
      - ./logstash.conf:/usr/share/logstash/pipeline/logstash.conf
    ports:
      - "5044:5044"
  
  kibana:
    image: docker.elastic.co/kibana/kibana:7.14.0
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
```

### 3. 备份策略

#### 数据库备份

```bash
#!/bin/bash
# backup.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups"
DB_NAME="tripmeta_production"

# 创建备份目录
mkdir -p $BACKUP_DIR

# 数据库备份
pg_dump -h $DB_HOST -U $DB_USER $DB_NAME > $BACKUP_DIR/db_backup_$DATE.sql

# 压缩备份文件
gzip $BACKUP_DIR/db_backup_$DATE.sql

# 上传到云存储
aws s3 cp $BACKUP_DIR/db_backup_$DATE.sql.gz s3://tripmeta-backups/

# 清理本地旧备份（保留7天）
find $BACKUP_DIR -name "db_backup_*.sql.gz" -mtime +7 -delete

echo "Backup completed: db_backup_$DATE.sql.gz"
```

#### 自动备份配置

```bash
# 添加到crontab
# 每天凌晨2点执行备份
0 2 * * * /opt/scripts/backup.sh >> /var/log/backup.log 2>&1
```

## 🔧 故障排除

### 1. 常见问题

#### Unity构建失败

```bash
# 问题：构建时出现"Unable to find Unity installation"
# 解决方案：
export UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity"
$UNITY_PATH -batchmode -quit -projectPath ./TripMeta -buildTarget Android

# 问题：Android构建失败，提示SDK路径错误
# 解决方案：在Unity中设置正确的Android SDK路径
# Edit -> Preferences -> External Tools -> Android SDK
```

#### VR设备连接问题

```csharp
// 检查VR设备连接状态
public class VRDeviceChecker : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(CheckVRDevice());
    }
    
    private IEnumerator CheckVRDevice()
    {
        yield return new WaitForSeconds(1f);
        
        if (XRSettings.loadedDeviceName == "")
        {
            Debug.LogError("No VR device detected!");
            // 显示错误提示UI
            ShowVRErrorDialog();
        }
        else
        {
            Debug.Log($"VR Device: {XRSettings.loadedDeviceName}");
        }
    }
}
```

#### AI服务连接问题

```csharp
// AI服务健康检查
public class AIServiceHealthCheck : MonoBehaviour
{
    private async void Start()
    {
        var healthCheck = await CheckAIServicesHealth();
        if (!healthCheck.IsHealthy)
        {
            Debug.LogError($"AI Services unhealthy: {healthCheck.ErrorMessage}");
            // 实施降级策略
            EnableOfflineMode();
        }
    }
    
    private async Task<HealthCheckResult> CheckAIServicesHealth()
    {
        try
        {
            var gptService = ServiceContainer.Instance.GetService<IGPTService>();
            var testResponse = await gptService.GenerateResponseAsync("test", 
                new GPTOptions { MaxTokens = 10, Timeout = 5 });
            
            return new HealthCheckResult { IsHealthy = true };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult 
            { 
                IsHealthy = false, 
                ErrorMessage = ex.Message 
            };
        }
    }
}
```

### 2. 性能问题诊断

#### 内存泄漏检测

```csharp
public class MemoryLeakDetector : MonoBehaviour
{
    private float _lastMemoryCheck;
    private long _baselineMemory;
    
    private void Start()
    {
        _baselineMemory = GC.GetTotalMemory(true);
        InvokeRepeating(nameof(CheckMemoryUsage), 10f, 10f);
    }
    
    private void CheckMemoryUsage()
    {
        var currentMemory = GC.GetTotalMemory(false);
        var memoryIncrease = currentMemory - _baselineMemory;
        
        if (memoryIncrease > 100 * 1024 * 1024) // 100MB增长
        {
            Debug.LogWarning($"Potential memory leak detected: {memoryIncrease / 1024 / 1024}MB increase");
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // 重新设置基线
            _baselineMemory = GC.GetTotalMemory(true);
        }
    }
}
```

#### 帧率优化

```csharp
public class FrameRateOptimizer : MonoBehaviour
{
    [SerializeField] private int _targetFrameRate = 90;
    [SerializeField] private float _frameTimeThreshold = 11.1f; // ms for 90fps
    
    private Queue<float> _frameTimeHistory = new Queue<float>();
    private int _maxHistorySize = 60; // 1秒历史
    
    private void Update()
    {
        var frameTime = Time.unscaledDeltaTime * 1000f;
        
        _frameTimeHistory.Enqueue(frameTime);
        if (_frameTimeHistory.Count > _maxHistorySize)
        {
            _frameTimeHistory.Dequeue();
        }
        
        var averageFrameTime = _frameTimeHistory.Average();
        
        if (averageFrameTime > _frameTimeThreshold)
        {
            // 启用性能优化
            OptimizePerformance();
        }
    }
    
    private void OptimizePerformance()
    {
        // 降低渲染质量
        QualitySettings.DecreaseLevel();
        
        // 减少LOD距离
        var lodGroups = FindObjectsOfType<LODGroup>();
        foreach (var lodGroup in lodGroups)
        {
            var lods = lodGroup.GetLODs();
            for (int i = 0; i < lods.Length; i++)
            {
                lods[i].screenRelativeTransitionHeight *= 0.8f;
            }
            lodGroup.SetLODs(lods);
        }
        
        Debug.Log("Performance optimization applied");
    }
}
```

### 3. 网络问题诊断

#### 网络连接检测

```csharp
public class NetworkDiagnostics : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DiagnoseNetworkIssues());
    }
    
    private IEnumerator DiagnoseNetworkIssues()
    {
        // 检查网络连接
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connection");
            yield break;
        }
        
        // 测试API连接
        yield return StartCoroutine(TestAPIConnection());
        
        // 测试CDN连接
        yield return StartCoroutine(TestCDNConnection());
    }
    
    private IEnumerator TestAPIConnection()
    {
        var request = UnityWebRequest.Get("https://api.your-domain.com/health");
        request.timeout = 10;
        
        yield return request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"API connection failed: {request.error}");
        }
        else
        {
            Debug.Log("API connection successful");
        }
    }
}
```

---

*本部署指南会根据项目发展和技术栈变化持续更新。*