# TripMeta - AI 驱动的 VR 旅游平台

<div align="center">

<!-- Banner image will be added later -->

# ![Unity](https://img.shields.io/badge/Unity-2021.3.11f1-black?style=for-the-badge&logo=unity)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
[![Platform](https://img.shields.io/badge/Platform-PICO%20VR-blue?style=for-the-badge)
[![Status](https://img.shields.io/badge/Status-Alpha-orange?style=for-the-badge)

**由 AI 驱动的沉浸式 VR 旅游体验**

[English](README.md) • **简体中文**

[在线演示](https://trip-meta.github.io/TripMeta/site/) • [问题反馈](../../issues) • [参与贡献](#贡献)

</div>

---

## 快速链接

| 资源 | 链接 |
|------|------|
| 🌐 **在线演示** | [trip-meta.github.io/TripMeta/site](https://trip-meta.github.io/TripMeta/site/) |
| 📖 **文档** | [TripMeta/docs](./docs) |
| 📹 **源码** | [github.com/trip-meta/TripMeta](https://github.com/trip-meta/TripMeta) |
| 🎬 **演示视频** | [观看 VR 演示](https://trip-meta.github.io/TripMeta/site/vr.mp4) |
| 🐛 **问题追踪** | [GitHub Issues](../../issues) |
| 💬 **讨论区** | [GitHub Discussions](../../discussions) |
| 📜 **更新日志** | [Releases](../../releases) |

## 演示视频

https://trip-meta.github.io/TripMeta/site/vr.mp4

---

## 项目概述

TripMeta 是一个创新的 VR 旅游平台，结合 AI 技术和虚拟现实，提供智能导游和沉浸式旅游体验。用户可以：

- 🌍 **探索** - 使用 PICO VR 头显探索世界各地的虚拟景点
- 🤖 **对话** - 与基于 GPT 的 AI 导游进行自然对话
- 📚 **学习** - 通过丰富的知识图谱了解历史和文化
- 🎯 **交互** - 使用语音对话和 VR 控制器直观交互

## 系统架构

![TripMeta 系统架构图](./docs/architecture-diagram.png)

**架构亮点：**

| 指标 | 目标 | 状态 |
|------|------|--------|
| **帧率** | 90 FPS | ✅ PICO 4 就绪 |
| **延迟** | <20ms | ✅ 动作到光子 |
| **AI 响应** | <2s | ✅ GPT-4 优化 |
| **内存预算** | <4GB | ✅ 已优化 |

## 核心特性

### 🤖 AI 智能导游
- **GPT 驱动对话**：自然语言理解和生成
- **个性化响应**：基于用户兴趣的上下文解说
- **多语言支持**：英语、中文、日语等
- **丰富知识库**：历史事实、文化洞察和旅行建议

### 🥽 沉浸式 VR 体验
- **PICO 4 支持**：专为 PICO VR 头显优化
- **90 FPS 性能**：流畅舒适的 VR 体验
- **低延迟**：<20ms 动作到光子延迟
- **高质量图形**：通用渲染管线 (URP)

### 🎯 自然交互
- **语音命令**：自然说话与 AI 导游交互
- **VR 控制器**：直观的手部追踪和手势识别
- **空间 UI**：悬浮在 3D 空间中的界面
- **触觉反馈**：真实的触摸感觉

## 技术栈

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

## 项目结构

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
├── docs/                    # 文档
├── README.md                # 英文文档
└── README_CN.md             # 中文文档
```

## 快速开始

### 系统要求

| 要求 | 版本/平台 |
|-------------|----------|
| **Unity** | 2021.3.11f1 或更高 |
| **操作系统** | Windows 10/11 |
| **VR 头显** | PICO 4（可选） |
| **Git** | 最新版本 |

### 安装步骤

```bash
# 克隆仓库
git clone https://github.com/trip-meta/TripMeta.git
cd TripMeta

# 在 Unity Hub 中打开
# 1. 从 unity.com 安装 Unity Hub
# 2. 点击"添加" → 选择此文件夹
# 3. 使用 Unity 2021.3.11f1 打开
```

### 首次运行设置

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

## 文档

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

## 开发工作流

1. **Fork** 本仓库
2. **创建** 功能分支：
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **提交** 更改：
   ```bash
   git commit -m "feat: 添加某个功能"
   ```
4. **推送** 到分支：
   ```bash
   git push origin feature/your-feature-name
   ```
5. 在 GitHub 上 **打开** Pull Request

详细指南请参阅 [CONTRIBUTING.md](./docs/CONTRIBUTING.md)。

## 持续集成

项目使用 GitHub Actions 进行自动化测试和构建：

| 工作流 | 用途 |
|--------|--------|
| **Unity 构建和测试** | 自动化 Unity 构建和单元测试 |
| **代码质量检查** | 静态分析和样式检查 |
| **性能测试** | FPS 和延迟验证 |

**设置说明**：参阅 [GitHub Actions 设置指南](./docs/GITHUB_ACTIONS_SETUP.md)

## 性能目标

| 指标 | 目标 | 说明 |
|--------|--------|-------|
| **帧率** | 90 FPS | PICO 4 要求 |
| **延迟** | <20ms | 动作到光子 |
| **内存** | <4GB | 总预算 |
| **Draw Calls** | <100 | 每帧优化 |
| **三角形** | <50K | 每场景限制 |

## 贡献

欢迎贡献！请阅读我们的[贡献指南](./docs/CONTRIBUTING.md)。

**贡献领域**：
- 🐛 错误修复
- ✨ 新功能
- 📖 文档改进
- 🎨 UI/UX 增强
- ⚡ 性能优化
- 🌍 多语言支持

## 许可证

本项目采用 **MIT 许可证**。

```
MIT 许可证

版权所有 (c) 2025 TripMeta 贡献者

特此免费授予任何获得本软件及其相关文档文件（"软件"）的
人不受限制地处理本软件的权利，包括但不限于使用、复制、
修改、合并、发布、分发、再许可和/或销售软件副本的权利。
```

完整文本请参阅 [LICENSE](LICENSE) 文件。

## 致谢

基于优秀的开源技术构建：

- [Unity Technologies](https://unity.com/) - 游戏引擎
- [PICO Interactive](https://www.pico-interactive.com/) - VR 平台
- [OpenAI](https://openai.com/) - AI 服务
- [Microsoft Azure](https://azure.microsoft.com/) - 云服务

## 开发路线图

- [x] 初始版本，包含 AI 导游
- [x] PICO 4 VR 支持
- [ ] 多用户 VR 会话
- [ ] AR 景点叠加
- [ ] 实时翻译
- [ ] 移动应用伴侣
- [ ] Unity 2022.3 升级

---

<div align="center">

**[⬆ 回到顶部](#tripmeta---ai-驱动的-vr-旅游平台)**

由 [TripMeta 团队](../../graphs/contributors) 用 ❤️ 打造

**[⭐ 在 GitHub 上 Star 我们！](../../stargazers)**

</div>
