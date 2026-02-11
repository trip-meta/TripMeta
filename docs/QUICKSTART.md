# TripMeta 快速启动指南

## 概述

这是一个AI驱动的VR旅游平台项目，使用Unity 2021.3.11f1开发，目标平台为PICO VR头显。

## 环境准备

### 必需软件

1. **Unity Hub** 和 **Unity 2021.3.11f1**
   - 从 Unity 官网下载 Unity Hub
   - 安装 Unity 2021.3.11f1 版本
   - 需要的模块:
     - Android Build Support
     - OpenJDK
     - Android SDK & NDK Tools

2. **Visual Studio 2022** 或 **JetBrains Rider**
   - 用于 C# 脚本开发

3. **Git**
   - 用于版本控制

### Unity包依赖

项目使用以下Unity包 (在 Packages/manifest.json 中配置):
- Unity XR Interaction Toolkit 3.0.0
- Universal Render Pipeline (URP) 17.0.3
- Input System 1.7.0
- Addressables 1.22.3
- ML-Agents 2.0.1
- Netcode for GameObjects 1.12.0

## 首次设置

### 1. 克隆/打开项目

```bash
cd D:\project\TripMeta\TripMeta
```

使用 Unity Hub 打开 `TripMeta` 目录。

### 2. 创建配置资产

在 Unity 编辑器中:

1. 打开菜单: `TripMeta > Create Configuration Assets`
2. 配置资产将自动创建在 `Assets/Resources/Config/` 目录下

### 3. 设置主场景

**选项A: 自动创建**
1. 打开菜单: `TripMeta > Setup Main Scene`
2. 点击 "Create Main Scene" 按钮
3. 场景将自动创建并配置基础对象

**选项B: 手动创建**
1. 创建新场景: `File > New Scene`
2. 保存为 `Assets/Scenes/MainScene.unity`
3. 添加以下对象到场景:
   - ApplicationBootstrap (带有配置引用)
   - VRManager
   - AIServiceManager
   - AITourGuide
   - Directional Light
   - Ground (Plane)

### 4. 配置 XR 设置

1. 打开 `Edit > Project Settings > XR Plug-in Management`
2. 在 Android 标签页:
   - 启用 PICO 插件

### 5. 运行项目

**在编辑器中测试:**
1. 打开 MainScene
2. 点击 Play 按钮
3. 检查 Console 查看初始化日志

**构建到PICO设备:**
1. `File > Build Settings`
2. 添加 MainScene 到构建
3. 平台选择 Android
4. 点击 "Build and Run"

## 项目结构

```
TripMeta/Assets/Scripts/
├── AI/                    # AI服务
│   ├── AIServiceManager.cs
│   ├── AITourGuide.cs
│   ├── Models/            # 数据模型
│   └── Services/          # AI服务实现
├── Core/                  # 核心系统
│   ├── Bootstrap/         # 启动初始化
│   ├── Configuration/     # 配置管理
│   ├── DependencyInjection/
│   └── ErrorHandling/
├── Interaction/           # VR交互
│   └── VRControllerManager.cs
└── VR/                    # VR功能
    ├── Interaction/
    └── Performance/
```

## 核心功能

### AI导游系统

- **AIServiceManager**: 管理所有AI服务
- **AITourGuide**: 智能导游，提供个性化旅游解说
- **ConversationHistory**: 记录对话历史
- **KnowledgeGraph**: 存储旅游知识

### VR系统

- **VRManager**: VR系统初始化和管理
- **VRControllerManager**: PICO控制器输入处理

### 配置系统

- **TripMetaConfig**: 主配置文件 (ScriptableObject)
- **AppSettings**: 应用设置
- **ConfigurationLoader**: 运行时配置加载

## 开发工作流

### 1. 编写代码

使用 Visual Studio 或 Rider 编辑 C# 脚本。

### 2. 测试更改

在 Unity 编辑器中运行场景测试更改。

### 3. 提交更改

```bash
git add .
git commit -m "描述更改"
git push
```

## 常见问题

### Q: 项目无法编译

**A**: 确保所有必要的包已安装:
- 打开 Window > Package Manager
- 检查所有必需的包是否已安装

### Q: VR无法在编辑器中工作

**A**: 编辑器中的VR支持有限:
- 大多数VR功能需要在实际硬件上测试
- 可以使用 Unity 的 XR 模拟模式进行基本测试

### Q: AI服务不工作

**A**: 当前使用模拟服务:
- MockAIServices 提供模拟响应
- 要使用真实AI服务，需要配置API密钥

## 调试技巧

### 查看日志

所有系统都使用 Debug.Log 输出日志:
- 打开 Console 窗口查看
- 搜索 "[AIServiceManager]", "[VRManager]" 等查看特定系统日志

### 测试AI导游

在 Play 模式下:
```csharp
// 通过脚本测试
var tourGuide = FindObjectOfType<AITourGuide>();
var response = await tourGuide.ProcessUserQuery("介绍一下纽约");
Debug.Log(response);
```

### 检查VR状态

```csharp
// 检查VR初始化状态
var vrManager = VRManager.Instance;
Debug.Log($"VR Ready: {vrManager?.IsVRReady()}");
```

## 下一步

1. 探索场景内容 (`Assets/NewYork.unity`)
2. 查看 AI 配置选项
3. 测试 VR 控制器交互
4. 添加自定义旅游内容

## 技术支持

- 查看项目文档: `docs/` 目录
- 查看 CLAUDE.md 了解代码架构
- 提交问题到项目仓库
