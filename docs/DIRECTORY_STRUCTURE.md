# TripMeta 目录结构说明文档

## 📋 目录

- [项目根目录](#项目根目录)
- [Unity项目结构](#unity项目结构)
- [文档目录](#文档目录)
- [配置文件](#配置文件)
- [开发工具](#开发工具)

## 🏗️ 项目根目录

```
TripMeta/
├── 📁 .codebuddy/              # CodeBuddy分析结果存储
├── 📁 .github/                 # GitHub Actions CI/CD配置
├── 📁 docs/                    # 项目文档目录
├── 📁 MetaTrip/               # 产品设计和规划文档
├── 📁 TripMeta/               # Unity主项目目录
├── 📄 .gitignore              # Git忽略文件配置
├── 📄 CHANGELOG.md            # 版本更新历史
├── 📄 LICENSE                 # MIT开源许可证
├── 📄 README.md               # 项目主文档
├── 📄 TripMeta.pptx          # 项目演示文稿
└── 📄 Trip-Meta*.md          # 项目分析和设计文档
```

## 🎮 Unity项目结构 (TripMeta/)

### 核心目录
```
TripMeta/
├── 📁 Assets/                  # Unity资源目录
│   ├── 📁 Geopipe/            # 地理数据和3D重建资源
│   ├── 📁 POLYGON city pack/   # 城市场景资源包
│   ├── 📁 PolygonStarter/     # 基础多边形资源
│   ├── 📁 Resources/          # Unity Resources目录
│   ├── 📁 Samples/            # 示例和测试场景
│   ├── 📁 Scenes/             # Unity场景文件
│   ├── 📁 Sci-Fi Styled Modular Pack/ # 科幻风格模块化资源
│   ├── 📁 Scripts/            # C#脚本代码
│   ├── 📁 TripMetaImages/     # 项目图片资源
│   ├── 📁 XR/                 # XR相关资源
│   └── 📁 XRI/                # XR Interaction Toolkit资源
├── 📁 Packages/               # Unity包管理器配置
├── 📁 ProjectSettings/        # Unity项目设置
└── 📁 UserSettings/           # 用户个人设置
```

### Scripts目录详细结构
```
Scripts/
├── 📁 Core/                   # 核心系统
│   ├── 📁 Architecture/       # 架构模式
│   │   ├── 📄 ServiceContainer.cs
│   │   ├── 📄 ServiceLocator.cs
│   │   └── 📄 IService.cs
│   ├── 📁 DependencyInjection/ # 依赖注入
│   │   ├── 📄 DIContainer.cs
│   │   ├── 📄 ServiceLifetime.cs
│   │   └── 📄 ServiceDescriptor.cs
│   ├── 📁 Configuration/      # 配置管理
│   │   ├── 📄 ConfigManager.cs
│   │   ├── 📄 GameSettings.cs
│   │   └── 📄 EnvironmentConfig.cs
│   └── 📁 Events/            # 事件系统
│       ├── 📄 EventBus.cs
│       ├── 📄 IEventHandler.cs
│       └── 📄 GameEvents.cs
├── 📁 Features/              # 功能模块
│   ├── 📁 AI/               # AI服务
│   │   ├── 📁 Services/     # AI服务实现
│   │   │   ├── 📄 AIServiceManager.cs
│   │   │   ├── 📄 GPTService.cs
│   │   │   ├── 📄 SpeechService.cs
│   │   │   ├── 📄 VisionService.cs
│   │   │   └── 📄 RecommendationService.cs
│   │   ├── 📁 Models/       # AI数据模型
│   │   │   ├── 📄 ChatMessage.cs
│   │   │   ├── 📄 SpeechResult.cs
│   │   │   └── 📄 VisionResult.cs
│   │   └── 📁 Interfaces/   # AI接口定义
│   │       ├── 📄 IAIService.cs
│   │       ├── 📄 IGPTService.cs
│   │       └── 📄 ISpeechService.cs
│   ├── 📁 VR/              # VR交互系统
│   │   ├── 📁 Interaction/ # 交互组件
│   │   │   ├── 📄 VRInteractionManager.cs
│   │   │   ├── 📄 HandTracker.cs
│   │   │   ├── 📄 GestureRecognizer.cs
│   │   │   └── 📄 SpatialUI.cs
│   │   ├── 📁 Performance/ # 性能优化
│   │   │   ├── 📄 VRPerformanceOptimizer.cs
│   │   │   ├── 📄 LODManager.cs
│   │   │   └── 📄 OcclusionCulling.cs
│   │   └── 📁 Audio/       # 空间音频
│   │       ├── 📄 SpatialAudioManager.cs
│   │       └── 📄 VoiceChatManager.cs
│   ├── 📁 Tourism/         # 旅游功能
│   │   ├── 📁 Destinations/ # 目的地管理
│   │   │   ├── 📄 DestinationManager.cs
│   │   │   ├── 📄 LocationData.cs
│   │   │   └── 📄 POIManager.cs
│   │   ├── 📁 Guide/       # 导游系统
│   │   │   ├── 📄 AIGuide.cs
│   │   │   ├── 📄 TourManager.cs
│   │   │   └── 📄 NarrativeSystem.cs
│   │   └── 📁 Social/      # 社交功能
│   │       ├── 📄 MultiplayerManager.cs
│   │       ├── 📄 UserProfile.cs
│   │       └── 📄 SocialInteraction.cs
│   └── 📁 Content/         # 内容管理
│       ├── 📁 Generation/  # 内容生成
│       │   ├── 📄 AIContentGenerator.cs
│       │   ├── 📄 SceneGenerator.cs
│       │   └── 📄 TextureGenerator.cs
│       ├── 📁 Loading/     # 资源加载
│       │   ├── 📄 AddressableManager.cs
│       │   ├── 📄 AssetLoader.cs
│       │   └── 📄 StreamingManager.cs
│       └── 📁 Management/  # 内容管理
│           ├── 📄 ContentManager.cs
│           ├── 📄 VersionControl.cs
│           └── 📄 CacheManager.cs
├── 📁 Infrastructure/        # 基础设施
│   ├── 📁 Networking/       # 网络通信
│   │   ├── 📄 NetworkManager.cs
│   │   ├── 📄 APIClient.cs
│   │   ├── 📄 WebSocketClient.cs
│   │   └── 📄 HttpService.cs
│   ├── 📁 Data/            # 数据访问
│   │   ├── 📄 DatabaseManager.cs
│   │   ├── 📄 LocalStorage.cs
│   │   ├── 📄 CloudStorage.cs
│   │   └── 📄 DataRepository.cs
│   ├── 📁 Security/        # 安全模块
│   │   ├── 📄 AuthenticationManager.cs
│   │   ├── 📄 EncryptionService.cs
│   │   └── 📄 SecurityValidator.cs
│   └── 📁 Monitoring/      # 监控系统
│       ├── 📄 PerformanceMonitor.cs
│       ├── 📄 AnalyticsManager.cs
│       ├── 📄 ErrorReporter.cs
│       └── 📄 MetricsCollector.cs
├── 📁 Presentation/         # 表现层
│   ├── 📁 UI/              # 用户界面
│   │   ├── 📁 Panels/      # UI面板
│   │   │   ├── 📄 MainMenuPanel.cs
│   │   │   ├── 📄 SettingsPanel.cs
│   │   │   └── 📄 TourPanel.cs
│   │   ├── 📁 Components/  # UI组件
│   │   │   ├── 📄 CustomButton.cs
│   │   │   ├── 📄 ProgressBar.cs
│   │   │   └── 📄 NotificationSystem.cs
│   │   └── 📁 Controllers/ # UI控制器
│   │       ├── 📄 UIManager.cs
│   │       ├── 📄 MenuController.cs
│   │       └── 📄 HUDController.cs
│   ├── 📁 Audio/           # 音频系统
│   │   ├── 📄 AudioManager.cs
│   │   ├── 📄 MusicPlayer.cs
│   │   ├── 📄 SoundEffects.cs
│   │   └── 📄 VoiceManager.cs
│   └── 📁 Visual/          # 视觉效果
│       ├── 📄 EffectsManager.cs
│       ├── 📄 ParticleController.cs
│       ├── 📄 LightingManager.cs
│       └── 📄 PostProcessing.cs
├── 📁 Utilities/           # 工具类
│   ├── 📁 Extensions/      # 扩展方法
│   │   ├── 📄 UnityExtensions.cs
│   │   ├── 📄 CollectionExtensions.cs
│   │   └── 📄 StringExtensions.cs
│   ├── 📁 Helpers/         # 辅助类
│   │   ├── 📄 MathHelper.cs
│   │   ├── 📄 FileHelper.cs
│   │   ├── 📄 JsonHelper.cs
│   │   └── 📄 DebugHelper.cs
│   └── 📁 Constants/       # 常量定义
│       ├── 📄 GameConstants.cs
│       ├── 📄 APIConstants.cs
│       └── 📄 UIConstants.cs
└── 📁 Editor/              # Unity编辑器扩展
    ├── 📁 Tools/           # 编辑器工具
    │   ├── 📄 BuildManager.cs
    │   ├── 📄 AssetProcessor.cs
    │   └── 📄 SceneValidator.cs
    ├── 📁 CodeQuality/     # 代码质量工具
    │   ├── 📄 CodeAnalyzer.cs
    │   ├── 📄 PerformanceAnalyzer.cs
    │   └── 📄 StyleChecker.cs
    ├── 📁 Testing/         # 测试工具
    │   ├── 📄 TestFramework.cs
    │   ├── 📄 MockDataGenerator.cs
    │   └── 📄 PerformanceTester.cs
    └── 📁 Windows/         # 编辑器窗口
        ├── 📄 ProjectDashboard.cs
        ├── 📄 AIServiceWindow.cs
        └── 📄 PerformanceWindow.cs
```

## 📚 文档目录 (docs/)

```
docs/
├── 📄 AI_INTEGRATION.md       # AI服务集成指南
├── 📄 API_REFERENCE.md        # API接口文档
├── 📄 ARCHITECTURE.md         # 系统架构设计
├── 📄 CONFIGURATION.md        # 配置参考手册
├── 📄 CONTRIBUTING.md         # 贡献指南
├── 📄 DEPLOYMENT_GUIDE.md     # 部署指南
├── 📄 DEVELOPMENT_STANDARDS.md # 开发规范
├── 📄 DIRECTORY_STRUCTURE.md  # 目录结构说明(本文档)
├── 📄 FAQ.md                  # 常见问题解答
├── 📄 SECURITY.md             # 安全指南
├── 📄 TECH_STACK.md           # 技术栈说明
├── 📄 TESTING_GUIDE.md        # 测试指南
├── 📄 TROUBLESHOOTING.md      # 故障排除
└── 📄 USER_MANUAL.md          # 用户使用手册
```

## ⚙️ 配置文件

### Unity项目配置
```
ProjectSettings/
├── 📄 AudioManager.asset      # 音频管理器设置
├── 📄 ClusterInputManager.asset # 集群输入管理
├── 📄 DynamicsManager.asset   # 物理动力学设置
├── 📄 EditorBuildSettings.asset # 构建设置
├── 📄 EditorSettings.asset    # 编辑器设置
├── 📄 GraphicsSettings.asset  # 图形设置
├── 📄 InputManager.asset      # 输入管理器
├── 📄 NavMeshAreas.asset      # 导航网格区域
├── 📄 NetworkManager.asset    # 网络管理器
├── 📄 Physics2DSettings.asset # 2D物理设置
├── 📄 PresetManager.asset     # 预设管理器
├── 📄 ProjectSettings.asset   # 项目设置
├── 📄 QualitySettings.asset   # 质量设置
├── 📄 TagManager.asset        # 标签管理器
├── 📄 TimeManager.asset       # 时间管理器
├── 📄 UnityConnectSettings.asset # Unity云服务设置
├── 📄 VFXManager.asset        # 视觉效果管理器
└── 📄 XRSettings.asset        # XR设置
```

### 包管理配置
```
Packages/
├── 📄 manifest.json          # 包依赖清单
└── 📄 packages-lock.json     # 包版本锁定文件
```

## 🔧 开发工具目录

### GitHub Actions配置
```
.github/
├── 📁 workflows/             # CI/CD工作流
│   ├── 📄 build.yml          # 构建流程
│   ├── 📄 test.yml           # 测试流程
│   ├── 📄 deploy.yml         # 部署流程
│   └── 📄 code-quality.yml   # 代码质量检查
├── 📄 ISSUE_TEMPLATE.md      # Issue模板
└── 📄 PULL_REQUEST_TEMPLATE.md # PR模板
```

### CodeBuddy分析结果
```
.codebuddy/
├── 📄 analysis-summary.json  # 分析摘要
├── 📄 project-context.json   # 项目上下文
└── 📄 optimization-history.json # 优化历史
```

## 📁 资源目录详解

### 3D资源
- **Geopipe/**: 真实地理数据的3D重建模型
- **POLYGON city pack/**: 低多边形风格的城市场景资源
- **Sci-Fi Styled Modular Pack/**: 科幻风格的模块化建筑资源

### 场景文件
- **Scenes/**: Unity场景文件，包含不同的游戏关卡和测试场景

### 脚本组织原则
- **Core/**: 核心系统，提供基础架构和服务
- **Features/**: 功能模块，按业务领域划分
- **Infrastructure/**: 基础设施，提供技术支撑
- **Presentation/**: 表现层，处理UI和用户交互
- **Utilities/**: 工具类，提供通用功能
- **Editor/**: 编辑器扩展，提升开发效率

## 🎯 目录设计原则

### 1. 分层架构
- **表现层** (Presentation): UI、音频、视觉效果
- **业务层** (Features): 核心业务逻辑
- **服务层** (Infrastructure): 技术服务
- **数据层** (Data): 数据访问和存储

### 2. 模块化设计
- 按功能域划分模块
- 每个模块职责单一
- 模块间低耦合高内聚
- 支持独立开发和测试

### 3. 可扩展性
- 预留扩展接口
- 支持插件化架构
- 配置驱动的功能开关
- 版本兼容性考虑

### 4. 开发效率
- 清晰的命名规范
- 统一的代码组织
- 完善的工具支持
- 自动化的构建流程

## 📝 文件命名规范

### C#脚本
- **类名**: PascalCase (如: `AIServiceManager`)
- **接口**: I前缀 + PascalCase (如: `IAIService`)
- **枚举**: PascalCase (如: `ServiceLifetime`)
- **常量**: UPPER_CASE (如: `MAX_RETRY_COUNT`)

### Unity资源
- **场景**: PascalCase (如: `MainMenu.unity`)
- **预制体**: PascalCase (如: `PlayerController.prefab`)
- **材质**: PascalCase + _Mat (如: `Ground_Mat.mat`)
- **纹理**: PascalCase + _Tex (如: `Wall_Tex.png`)

### 文档文件
- **Markdown**: UPPER_CASE (如: `README.md`)
- **配置文件**: lowercase (如: `package.json`)

## 🔍 快速导航

### 常用目录快捷方式
- **核心代码**: `TripMeta/Assets/Scripts/Core/`
- **AI服务**: `TripMeta/Assets/Scripts/Features/AI/`
- **VR交互**: `TripMeta/Assets/Scripts/Features/VR/`
- **UI系统**: `TripMeta/Assets/Scripts/Presentation/UI/`
- **编辑器工具**: `TripMeta/Assets/Scripts/Editor/`
- **项目文档**: `docs/`
- **构建配置**: `.github/workflows/`

### 重要配置文件
- **项目设置**: `TripMeta/ProjectSettings/ProjectSettings.asset`
- **包依赖**: `TripMeta/Packages/manifest.json`
- **构建设置**: `TripMeta/ProjectSettings/EditorBuildSettings.asset`
- **质量设置**: `TripMeta/ProjectSettings/QualitySettings.asset`

---

## 📞 相关文档

- [技术栈说明](./TECH_STACK.md)
- [开发规范](./DEVELOPMENT_STANDARDS.md)
- [架构设计](./ARCHITECTURE.md)
- [API文档](./API_REFERENCE.md)

---

*最后更新: 2024年12月*