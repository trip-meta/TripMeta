# TripMeta - AI-Powered VR Tourism Platform

<div align="center">

<!-- Banner image will be added later -->

# ![Unity](https://img.shields.io/badge/Unity-2021.3.11f1-black?style=for-the-badge&logo=unity)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
[![Platform](https://img.shields.io/badge/Platform-PICO%20VR-blue?style=for-the-badge)
[![Status](https://img.shields.io/badge/Status-Alpha-orange?style=for-the-badge)

**An immersive VR tourism experience powered by AI**

[English](#english) • [简体中文](#简体中文)

[Live Demo](https://trip-meta.github.io/TripMeta/site/) • [Report Issue](../../issues) • [Contribute](#contributing)

</div>

---

## Quick Links

| Resource | Link |
|----------|------|
| 🌐 **Live Demo** | [trip-meta.github.io/TripMeta/site](https://trip-meta.github.io/TripMeta/site/) |
| 📖 **Documentation** | [TripMeta/docs](./docs) |
| 📹 **Source Code** | [github.com/trip-meta/TripMeta](https://github.com/trip-meta/TripMeta) |
| 🎬 **Demo Video** | [Watch VR Demo](https://trip-meta.github.io/TripMeta/site/vr.mp4) |
| 🐛 **Issue Tracker** | [GitHub Issues](../../issues) |
| 💬 **Discussions** | [GitHub Discussions](../../discussions) |
| 📜 **Changelog** | [Releases](../../releases) |

## Demo Video

https://trip-meta.github.io/TripMeta/site/vr.mp4

---

<a name="english"></a>
## English

### Overview

TripMeta is an innovative VR tourism platform combining AI technology with virtual reality. It provides intelligent tour guides and immersive travel experiences where users can:

- 🌍 **Explore** virtual attractions worldwide using PICO VR headsets
- 🤖 **Converse** naturally with AI tour guides powered by GPT
- 📚 **Learn** about history and culture through rich knowledge graphs
- 🎯 **Interact** intuitively using voice dialogue and VR controllers

### System Architecture

```mermaid
flowchart TB
    subgraph AI["🤖 AI Services Layer"]
        GPT["GPT-4 API\nConversational AI"]
        AzureSpeech["Azure Speech Services\nVoice Recognition & TTS"]
        AzureVision["Azure Computer Vision\nObject Detection & AR"]
        KG["Knowledge Graph\nHistorical & Cultural Data"]
    end

    subgraph VR["🥽 VR Platform Layer"]
        Unity["Unity 2021.3.11f1\nGame Engine"]
        PICO["PICO 4 SDK\nVR Headset Integration"]
        URP["Universal Render Pipeline\n90 FPS Graphics"]
        XR["XR Interaction Toolkit\nHand Tracking"]
    end

    subgraph Core["⚙️ Core Systems Layer"]
        DI["Dependency Injection\nService Locator"]
        Config["Configuration Manager\nScriptableObjects"]
        Event["Event Bus\nMessage Broker"]
        Error["Error Handling\nException Management"]
    end

    subgraph Features["✨ Feature Modules"]
        Guide["AI Tour Guide\nConversation & Personalization"]
        Social["Social Module\nMulti-user Sessions"]
        Nav["Navigation System\nWaypoints & POI"]
        Video["Video Recording\nCapture & Export"]
    end

    subgraph Interaction["🎯 Interaction Layer"]
        Voice["Voice Commands\nNatural Language Input"]
        Controller["VR Controllers\nHand Tracking"]
        UI["Spatial UI\nFloating Interfaces"]
        Haptic["Haptic Feedback\nTouch Sensations"]
    end

    subgraph Presentation["🎨 Presentation Layer"]
        Scene["Scene Management\nLevel Streaming"]
        Assets["3D Assets\nPOLYGON City, Sci-Fi Pack"]
        Audio["Audio System\nSpatial Audio"]
    end

    AI --> Core
    VR --> Core
    Core --> Features
    Features --> Interaction
    Interaction --> Presentation

    style AI fill:#e1bee7,stroke:#8e24aa,stroke-width:2px
    style VR fill:#bbdefb,stroke:#1976d2,stroke-width:2px
    style Core fill:#c8e6c9,stroke:#388e3c,stroke-width:2px
    style Features fill:#ffe0b2,stroke:#f57c00,stroke-width:2px
    style Interaction fill:#b2dfdb,stroke:#00796b,stroke-width:2px
    style Presentation fill:#f8bbd9,stroke:#c2185b,stroke-width:2px
```

**Architecture Highlights:**

| Metric | Target | Status |
|--------|--------|--------|
| **Frame Rate** | 90 FPS | ✅ PICO 4 Ready |
| **Latency** | <20ms | ✅ Motion-to-photon |
| **AI Response** | <2s | ✅ GPT-4 Optimized |
| **Memory Budget** | <4GB | ✅ Optimized |

### Key Features

#### 🤖 AI Tour Guide
- **GPT-Powered Conversations**: Natural language understanding and generation
- **Personalized Responses**: Context-aware explanations based on user interests
- **Multi-language Support**: English, Chinese, Japanese, and more
- **Rich Knowledge Base**: Historical facts, cultural insights, and travel tips

#### 🥽 Immersive VR Experience
- **PICO 4 Support**: Optimized for PICO VR headsets
- **90 FPS Performance**: Smooth, comfortable VR experience
- **Low Latency**: <20ms motion-to-photon latency
- **High-Quality Graphics**: Universal Render Pipeline (URP)

#### 🎯 Natural Interactions
- **Voice Commands**: Speak naturally to interact with the AI guide
- **VR Controllers**: Intuitive hand tracking and gesture recognition
- **Spatial UI**: Floating interfaces positioned in 3D space
- **Haptic Feedback**: Realistic touch sensations

### Technology Stack

| Component | Technology | Purpose |
|-----------|----------|---------|
| **Game Engine** | Unity 2021.3.11f1 | Core development platform |
| **VR Platform** | PICO 4 | Target VR headset |
| **Render Pipeline** | Universal Render Pipeline (URP) | High-performance graphics |
| **Input System** | Unity Input System | Modern input handling |
| **AI Engine** | GPT-4 via OpenAI API | Conversational AI |
| **Speech** | Azure Cognitive Services | Voice recognition & TTS |
| **Vision** | Azure Computer Vision | Object detection & AR |
| **Networking** | Unity Netcode for GameObjects | Multiplayer support |

### Project Architecture

```
TripMeta/
├── Assets/
│   ├── Scripts/
│   │   ├── AI/              # AI Services (GPT, Speech, Vision)
│   │   ├── Core/            # Infrastructure (DI, Config, Errors)
│   │   ├── Features/        # Business Logic (Tour Guide, Social)
│   │   ├── Interaction/     # Input Handling (VR Controllers)
│   │   ├── Presentation/    # UI/UX Components
│   │   ├── VR/              # PICO Integration
│   │   └── Editor/          # Unity Editor Tools
│   ├── Scenes/              # Unity Scene Files
│   └── Packages/            # Unity Package Manager
├── docs/                  # Documentation
└── README.md
```

### Quick Start

#### Prerequisites

| Requirement | Version/Platform |
|-------------|------------------|
| **Unity** | 2021.3.11f1 or later |
| **OS** | Windows 10/11 |
| **VR Headset** | PICO 4 (optional) |
| **Git** | Latest version |

#### Installation

```bash
# Clone the repository
git clone https://github.com/trip-meta/TripMeta.git
cd TripMeta

# Open in Unity Hub
# 1. Install Unity Hub from unity.com
# 2. Click "Add" → Select this folder
# 3. Open with Unity 2021.3.11f1
```

#### First Run Setup

```
Unity Editor Menu:
├── TripMeta
│   ├── Create Configuration Assets  ← Run this first (creates ScriptableObjects)
│   └── Setup Main Scene           ← Run this after config creation
```

1. **Create Configuration Assets**
   - Go to `TripMeta > Create Configuration Assets`
   - Creates ScriptableObject configurations in `Assets/Resources/Config/`
   - Required before first run

2. **Setup Main Scene**
   - Go to `TripMeta > Setup Main Scene`
   - Configures the main scene with required systems
   - Registers all services and initializes the application

3. **Press Play**
   - The application will boot up and show the VR scene
   - Use VR headset or editor preview to explore

### Documentation

| Document | Description |
|----------|-------------|
| [Quick Start Guide](./docs/QUICKSTART.md) | Detailed setup instructions |
| [Architecture](./docs/ARCHITECTURE.md) | System design and patterns |
| [AI Integration](./docs/AI_INTEGRATION.md) | AI services setup |
| [Tech Stack](./docs/TECH_STACK.md) | Complete technology overview |
| [Development Standards](./docs/DEVELOPMENT_STANDARDS.md) | Coding conventions |
| [Testing Guide](./docs/TESTING_GUIDE.md) | Testing strategies |
| [Deployment Guide](./docs/DEPLOYMENT_GUIDE.md) | Build and deploy |
| [Troubleshooting](./docs/TROUBLESHOOTING.md) | Common issues |

### Development Workflow

1. **Fork** the repository
2. **Create** a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Commit** your changes:
   ```bash
   git commit -m "feat: add some amazing feature"
   ```
4. **Push** to your branch:
   ```bash
   git push origin feature/your-feature-name
   ```
5. **Open** a Pull Request on GitHub

See [CONTRIBUTING.md](./docs/CONTRIBUTING.md) for detailed guidelines.

### CI/CD

The project uses GitHub Actions for automated testing and building:

| Workflow | Purpose |
|----------|---------|
| **Unity Build and Test** | Automated Unity builds and unit tests |
| **Code Quality Check** | Static analysis and style checks |
| **Performance Tests** | FPS and latency validation |

**Setup Instructions**: See [GitHub Actions Setup Guide](./docs/GITHUB_ACTIONS_SETUP.md)

### Performance Targets

| Metric | Target | Notes |
|---------|--------|-------|
| **Frame Rate** | 90 FPS | PICO 4 requirement |
| **Latency** | <20ms | Motion-to-photon |
| **Memory** | <4GB | Total budget |
| **Draw Calls** | <100 | Per frame optimization |
| **Triangles** | <50K | Per scene limit |

### Contributing

We welcome contributions! Please read our [Contributing Guidelines](./docs/CONTRIBUTING.md).

**Areas of Contribution**:
- 🐛 Bug fixes
- ✨ New features
- 📖 Documentation improvements
- 🎨 UI/UX enhancements
- ⚡ Performance optimizations
- 🌍 Multi-language support

### License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 TripMeta Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

See [LICENSE](LICENSE) file for full text.

### Acknowledgments

Built with amazing open-source technologies:

- [Unity Technologies](https://unity.com/) - Game Engine
- [PICO Interactive](https://www.pico-interactive.com/) - VR Platform
- [OpenAI](https://openai.com/) - AI Services
- [Microsoft Azure](https://azure.microsoft.com/) - Cloud Services

### Roadmap

- [x] Initial release with AI tour guide
- [x] PICO 4 VR support
- [ ] Multi-user VR sessions
- [ ] AR attraction overlays
- [ ] Real-time translation
- [ ] Mobile app companion
- [ ] Unity 2022.3 upgrade

---

<a name="简体中文"></a>
## 简体中文

### 项目概述

TripMeta 是一个创新的 VR 旅游平台，结合 AI 技术和虚拟现实，提供智能导游和沉浸式旅游体验。用户可以：

- 🌍 **探索** - 使用 PICO VR 头显探索世界各地的虚拟景点
- 🤖 **对话** - 与基于 GPT 的 AI 导游进行自然对话
- 📚 **学习** - 通过丰富的知识图谱了解历史和文化
- 🎯 **交互** - 使用语音对话和 VR 控制器直观交互

### 系统架构

```mermaid
flowchart TB
    subgraph AI["🤖 AI 服务层"]
        GPT["GPT-4 API\n对话式 AI"]
        AzureSpeech["Azure 语音服务\n语音识别与合成"]
        AzureVision["Azure 计算机视觉\n物体检测与 AR"]
        KG["知识图谱\n历史文化数据"]
    end

    subgraph VR["🥽 VR 平台层"]
        Unity["Unity 2021.3.11f1\n游戏引擎"]
        PICO["PICO 4 SDK\nVR 头显集成"]
        URP["通用渲染管线 URP\n90 FPS 图形"]
        XR["XR 交互工具包\n手部追踪"]
    end

    subgraph Core["⚙️ 核心系统层"]
        DI["依赖注入\n服务定位器"]
        Config["配置管理\nScriptableObjects"]
        Event["事件总线\n消息代理"]
        Error["错误处理\n异常管理"]
    end

    subgraph Features["✨ 功能模块"]
        Guide["AI 智能导游\n对话与个性化"]
        Social["社交模块\n多用户会话"]
        Nav["导航系统\n路径点与兴趣点"]
        Video["视频录制\n捕获与导出"]
    end

    subgraph Interaction["🎯 交互层"]
        Voice["语音命令\n自然语言输入"]
        Controller["VR 控制器\n手部追踪"]
        UI["空间 UI\n悬浮界面"]
        Haptic["触觉反馈\n触摸感知"]
    end

    subgraph Presentation["🎨 表现层"]
        Scene["场景管理\n关卡流式加载"]
        Assets["3D 资源\nPOLYGON 城市, Sci-Fi 包"]
        Audio["音频系统\n空间音频"]
    end

    AI --> Core
    VR --> Core
    Core --> Features
    Features --> Interaction
    Interaction --> Presentation

    style AI fill:#e1bee7,stroke:#8e24aa,stroke-width:2px
    style VR fill:#bbdefb,stroke:#1976d2,stroke-width:2px
    style Core fill:#c8e6c9,stroke:#388e3c,stroke-width:2px
    style Features fill:#ffe0b2,stroke:#f57c00,stroke-width:2px
    style Interaction fill:#b2dfdb,stroke:#00796b,stroke-width:2px
    style Presentation fill:#f8bbd9,stroke:#c2185b,stroke-width:2px
```

**架构亮点:**

| 指标 | 目标 | 状态 |
|------|------|--------|
| **帧率** | 90 FPS | ✅ PICO 4 就绪 |
| **延迟** | <20ms | ✅ 动作到光子 |
| **AI 响应** | <2s | ✅ GPT-4 优化 |
| **内存预算** | <4GB | ✅ 已优化 |

### 核心特性

#### 🤖 AI 智能导游
- **GPT 驱动对话**: 自然语言理解和生成
- **个性化响应**: 基于用户兴趣的上下文解说
- **多语言支持**: 英语、中文、日语等
- **丰富知识库**: 历史事实、文化洞察和旅行建议

#### 🥽 沉浸式 VR 体验
- **PICO 4 支持**: 专为 PICO VR 头显优化
- **90 FPS 性能**: 流畅舒适的 VR 体验
- **低延迟**: <20ms 动作到光子延迟
- **高质量图形**: 通用渲染管线 (URP)

#### 🎯 自然交互
- **语音命令**: 自然说话与 AI 导游交互
- **VR 控制器**: 直观的手部追踪和手势识别
- **空间 UI**: 悬浮在 3D 空间中的界面
- **触觉反馈**: 真实的触摸感觉

### 技术栈

| 组件 | 技术 | 用途 |
|------|--------|------|
| **游戏引擎** | Unity 2021.3.11f1 | 核心开发平台 |
| **VR 平台** | PICO 4 | 目标 VR 头显 |
| **渲染管线** | Universal Render Pipeline (URP) | 高性能图形 |
| **输入系统** | Unity Input System | 现代输入处理 |
| **AI 引擎** | GPT-4 via OpenAI API | 对话式 AI |
| **语音** | Azure Cognitive Services | 语音识别和 TTS |
| **视觉** | Azure Computer Vision | 物体检测和 AR |
| **网络** | Unity Netcode for GameObjects | 多人支持 |

### 项目结构

```
TripMeta/
├── Assets/
│   ├── Scripts/
│   │   ├── AI/              # AI 服务（GPT、语音、视觉）
│   │   ├── Core/            # 基础设施（DI、配置、错误）
│   │   ├── Features/        # 业务逻辑（导游、社交）
│   │   ├── Interaction/     # 输入处理（VR 控制器）
│   │   ├── Presentation/    # UI/UX 组件
│   │   ├── VR/              # PICO 集成
│   │   └── Editor/          # Unity 编辑器工具
│   ├── Scenes/              # Unity 场景文件
│   └── Packages/            # Unity 包管理器
├── docs/                  # 文档
└── README.md
```

### 快速开始

#### 系统要求

| 要求 | 版本/平台 |
|-------------|----------|
| **Unity** | 2021.3.11f1 或更高 |
| **操作系统** | Windows 10/11 |
| **VR 头显** | PICO 4（可选） |
| **Git** | 最新版本 |

#### 安装步骤

```bash
# 克隆仓库
git clone https://github.com/trip-meta/TripMeta.git
cd TripMeta

# 在 Unity Hub 中打开
# 1. 从 unity.com 安装 Unity Hub
# 2. 点击"添加" → 选择此文件夹
# 3. 使用 Unity 2021.3.11f1 打开
```

#### 首次运行设置

```
Unity 编辑器菜单:
├── TripMeta
│   ├── Create Configuration Assets  ← 首先运行此选项（创建 ScriptableObjects）
│   └── Setup Main Scene           ← 配置创建后运行此选项
```

1. **创建配置资源**
   - 进入 `TripMeta > Create Configuration Assets`
   - 在 `Assets/Resources/Config/` 创建 ScriptableObject 配置
   - 首次运行前必须执行

2. **设置主场景**
   - 进入 `TripMeta > Setup Main Scene`
   - 使用所需系统配置主场景
   - 注册所有服务并初始化应用

3. **按 Play 运行**
   - 应用程序将启动并显示 VR 场景
   - 使用 VR 头显或编辑器预览进行探索

### 文档

| 文档 | 描述 |
|------|--------|
| [快速开始指南](./docs/QUICKSTART.md) | 详细设置说明 |
| [系统架构](./docs/ARCHITECTURE.md) | 系统设计和模式 |
| [AI 集成](./docs/AI_INTEGRATION.md) | AI 服务设置 |
| [技术栈](./docs/TECH_STACK.md) | 完整技术概述 |
| [开发标准](./docs/DEVELOPMENT_STANDARDS.md) | 编码规范 |
| [测试指南](./docs/TESTING_GUIDE.md) | 测试策略 |
| [部署指南](./docs/DEPLOYMENT_GUIDE.md) | 构建和部署 |
| [故障排除](./docs/TROUBLESHOOTING.md) | 常见问题 |

### 开发工作流

1. **Fork** 本仓库
2. **创建** 功能分支：
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **提交** 更改：
   ```bash
   git commit -m "feat: add some amazing feature"
   ```
4. **推送** 到分支：
   ```bash
   git push origin feature/your-feature-name
   ```
5. 在 GitHub 上 **打开** Pull Request

详细指南请参阅 [CONTRIBUTING.md](./docs/CONTRIBUTING.md)。

### 持续集成

项目使用 GitHub Actions 进行自动化测试和构建：

| 工作流 | 用途 |
|--------|--------|
| **Unity 构建和测试** | 自动化 Unity 构建和单元测试 |
| **代码质量检查** | 静态分析和样式检查 |
| **性能测试** | FPS 和延迟验证 |

**设置说明**: 参阅 [GitHub Actions 设置指南](./docs/GITHUB_ACTIONS_SETUP.md)

### 性能目标

| 指标 | 目标 | 说明 |
|--------|--------|-------|
| **帧率** | 90 FPS | PICO 4 要求 |
| **延迟** | <20ms | 动作到光子 |
| **内存** | <4GB | 总预算 |
| **Draw Calls** | <100 | 每帧优化 |
| **三角形** | <50K | 每场景限制 |

### 贡献

欢迎贡献！请阅读我们的[贡献指南](./docs/CONTRIBUTING.md)。

**贡献领域**：
- 🐛 错误修复
- ✨ 新功能
- 📖 文档改进
- 🎨 UI/UX 增强
- ⚡ 性能优化
- 🌍 多语言支持

### 许可证

本项目采用 **MIT 许可证**。

```
MIT 许可证

版权所有 (c) 2025 TripMeta 贡献者

特此免费授予任何获得本软件及其相关文档文件（"软件"）的
人不受限制地处理...
```

完整文本请参阅 [LICENSE](LICENSE) 文件。

### 致谢

基于优秀的开源技术构建：

- [Unity Technologies](https://unity.com/) - 游戏引擎
- [PICO Interactive](https://www.pico-interactive.com/) - VR 平台
- [OpenAI](https://openai.com/) - AI 服务
- [Microsoft Azure](https://azure.microsoft.com/) - 云服务

### 开发路线图

- [x] 初始版本，包含 AI 导游
- [x] PICO 4 VR 支持
- [ ] 多用户 VR 会话
- [ ] AR 景点叠加
- [ ] 实时翻译
- [ ] 移动应用伴侣
- [ ] Unity 2022.3 升级

---

<div align="center">

**[⬆ Back to Top](#tripmeta---ai-powered-vr-tourism-platform)**

Made with ❤️ by the [TripMeta Team](../../graphs/contributors)

**[⭐ Star us on GitHub!](../../stargazers)**

</div>
