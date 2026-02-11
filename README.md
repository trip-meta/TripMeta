# TripMeta - AI-Powered VR Tourism Platform

<div align="center">

[![Unity](https://img.shields.io/badge/Unity-2021.3.11f1-black?style=for-the-badge&logo=unity)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-PICO%20VR-blue?style=for-the-badge)](https://www.pico-interactive.com/)
[![Status](https://img.shields.io/badge/Status-Alpha-orange?style=for-the-badge)]()

**An immersive VR tourism experience powered by AI**

[English](#english) • [简体中文](#简体中文)

</div>

---

<a name="english"></a>
## English

### Overview

TripMeta is an innovative VR tourism platform that combines AI technology with virtual reality to provide intelligent tour guides and immersive travel experiences. Users can explore virtual attractions worldwide using PICO VR headsets, engage in natural conversations with AI tour guides, and receive personalized travel explanations.

### Features

- **🤖 AI Tour Guide** - GPT-powered intelligent dialogue system with personalized attraction explanations
- **🥽 Immersive VR** - High-quality virtual tourism experience on PICO VR headsets
- **🎯 Multimodal Interaction** - Voice dialogue, VR controller interaction, and spatial UI
- **📚 Knowledge Graph** - Rich tourism attraction knowledge and points of interest
- **🌍 Multiple Scenes** - Cities, nature, history, and various virtual tourism scenarios

### Demo Video

[![Watch VR Demo](https://raw.githubusercontent.com/trip-meta/TripMeta/main/docs/site/poster.jpg)](https://trip-meta.github.io/TripMeta/)

**[▶ Watch the VR Demo Video](https://trip-meta.github.io/TripMeta/)**

> 🎬 Click to watch the AI tour guide in action!

### Quick Start

#### Prerequisites
- Unity 2021.3.11f1
- Windows 10/11
- PICO 4 VR headset (optional)

#### Installation

```bash
git clone https://github.com/trip-meta/TripMeta.git
cd TripMeta
```

1. Open the project in Unity Hub
2. Go to `TripMeta > Create Configuration Assets`
3. Go to `TripMeta > Setup Main Scene`
4. Press Play to run

### Architecture

```
Assets/Scripts/
├── AI/              # AI Services (GPT, Speech, Vision, Recommendations)
├── Core/            # Infrastructure (DI, Config, Error Handling)
├── Features/        # Business features (Tour Guide, Social, Analytics)
├── Interaction/     # VR input handling
├── Presentation/    # UI and UX components
├── VR/              # VR-specific functionality (PICO integration)
└── Editor/          # Unity editor tools
```

### Documentation

- [Quick Start Guide](docs/QUICKSTART.md)
- [Architecture](docs/ARCHITECTURE.md)
- [AI Integration](docs/AI_INTEGRATION.md)
- [Contributing](docs/CONTRIBUTING.md)

### Contributing

Contributions are welcome! Please read our [Contributing Guidelines](docs/CONTRIBUTING.md).

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Acknowledgments

- [Unity Technologies](https://unity.com/) - Game Engine
- [PICO Interactive](https://www.pico-interactive.com/) - VR Device Support
- [OpenAI](https://openai.com/) - AI Services

---

<a name="简体中文"></a>
## 简体中文

### 项目概述

TripMeta 是一个创新的 VR 旅游平台，结合 AI 技术和虚拟现实，为用户提供智能导游和沉浸式旅游体验。用户可以通过 PICO VR 头显探索世界各地的虚拟景点，与 AI 导游进行自然对话，获得个性化的旅游解说。

### 核心特性

- **🤖 AI 智能导游** - 基于 GPT 的智能对话系统，提供个性化的景点讲解
- **🥽 沉浸式 VR 体验** - 支持 PICO VR 头显的高质量虚拟旅游
- **🎯 多模态交互** - 语音对话、VR 控制器交互、空间 UI
- **📚 知识图谱** - 内置丰富的旅游景点知识和兴趣点信息
- **🌍 多场景支持** - 城市、自然、历史等多种虚拟旅游场景

### 演示视频

**[▶ 观看 VR 演示视频](https://trip-meta.github.io/TripMeta/)**

> 🎬 点击观看 AI 智能导游的精彩演示！

### 快速开始

#### 系统要求
- Unity 2021.3.11f1
- Windows 10/11
- PICO 4 VR 头显（可选）

#### 安装步骤

```bash
git clone https://github.com/trip-meta/TripMeta.git
cd TripMeta
```

1. 使用 Unity Hub 打开项目
2. 点击 `TripMeta > Create Configuration Assets`
3. 点击 `TripMeta > Setup Main Scene`
4. 点击 Play 运行

### 项目结构

```
Assets/Scripts/
├── AI/              # AI 服务（GPT、语音、视觉、推荐）
├── Core/            # 基础设施（DI、配置、错误处理）
├── Features/        # 业务功能（导游、社交、分析）
├── Interaction/     # VR 输入处理
├── Presentation/    # UI 和 UX 组件
├── VR/              # VR 专用功能（PICO 集成）
└── Editor/          # Unity 编辑器工具
```

### 文档

- [快速开始指南](docs/QUICKSTART.md)
- [系统架构](docs/ARCHITECTURE.md)
- [AI 集成](docs/AI_INTEGRATION.md)
- [贡献指南](docs/CONTRIBUTING.md)

### 贡献

欢迎贡献代码！请阅读我们的[贡献指南](docs/CONTRIBUTING.md)。

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

### 致谢

- [Unity Technologies](https://unity.com/) - 游戏引擎
- [PICO Interactive](https://www.pico-interactive.com/) - VR 设备支持
- [OpenAI](https://openai.com/) - AI 服务

---

<div align="center">

Made with ❤️ by the TripMeta Team

**[⬆ Back to Top](#tripmeta---ai-powered-vr-tourism-platform)**

</div>
