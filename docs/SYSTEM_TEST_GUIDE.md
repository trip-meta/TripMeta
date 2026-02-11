# TripMeta 系统测试与验证指南

## 概述

本指南提供完整的测试流程，用于验证 TripMeta 项目的所有核心功能正常工作。

## 自动化测试

### 运行时系统测试

TripMeta 包含一个自动化测试系统，可以在运行时验证所有核心组件：

**测试组件**: `RuntimeSystemTest.cs`

**测试内容**:
- ✅ SimpleStartup 初始化
- ✅ VRManager 组件检查
- ✅ AIServiceManager 状态验证
- ✅ AITourGuide 配置验证
- ✅ DemoController 功能测试
- ✅ TourLocationManager 位置管理
- ✅ TourUIManager UI系统

**运行方式**:
1. 在Unity编辑器中打开 DemoScene.unity 或 SampleScene.unity
2. 点击 Play 按钮
3. RuntimeSystemTest 将自动运行并输出测试结果
4. 查看 Console 窗口查看测试报告

**手动运行**:
- 在 Hierarchy 中选择任何 GameObject
- 在 Inspector 中右键点击
- 选择 "运行系统测试"

## 手动测试流程

### 测试准备

1. **打开项目**
   ```
   Unity Hub > 打开 TripMeta/ 目录
   确保 Unity 版本: 2021.3.11f1
   ```

2. **打开演示场景**
   ```
   Assets/Scenes/DemoScene.unity
   ```

3. **点击 Play**
   ```
   检查 Console 窗口是否有错误
   ```

### 基础功能测试

#### 1. 初始化测试

**预期结果**:
```
[ApplicationBootstrap] 开始初始化应用程序...
[VRManager] 开始初始化VR系统...
[AIServiceManager] 开始初始化AI服务...
[OpenAIService] 服务已初始化 (模拟模式)
[AIServiceManager] 所有AI服务初始化完成
[SimpleStartup] Initialization complete!
```

**验证点**:
- ✅ 没有编译错误
- ✅ 所有管理器正确初始化
- ✅ Console 显示 "Initialization complete"

#### 2. AI导游测试

**操作**:
- 按快捷键 `[1]` 开始导游演示

**预期结果**:
```
[DemoController] 启动AI导游...
[AI导游] 你好！我是你的AI导游小美。
让我们一起探索纽约的精彩景点吧！
```

**验证点**:
- ✅ 状态显示 "启动AI导游..."
- ✅ 导游文本显示欢迎消息
- ✅ 景点标记高亮显示

#### 3. 场景切换测试

**操作**:
- 按快捷键 `[2]` 切换到下一个景点

**预期结果**:
```
[DemoController] 前往: 时代广场
[AI导游] 现在我们来到时代广场。
这是一个非常值得参观的地方！
```

**验证点**:
- ✅ 相机平滑移动到新位置
- ✅ 景点信息更新
- ✅ 导游解说显示

#### 4. AI对话测试

**操作**:
- 按快捷键 `[3]` 显示AI对话示例

**预期结果**:
```
游客: 纽约有多少人口？
导游: 纽约市拥有超过800万人口，
是美国人口最多的城市。
```

**验证点**:
- ✅ 对话界面显示
- ✅ 问题显示正确
- ✅ AI回答生成

#### 5. 重置测试

**操作**:
- 按快捷键 `[4]` 重置演示

**预期结果**:
```
[DemoController] 演示已重置
```

**验证点**:
- ✅ 状态重置
- ✅ 相机回到初始位置
- ✅ 导游文本更新

## VR功能测试

### VR设备检测

**测试步骤**:
1. 连接 PICO VR 头显
2. 运行项目
3. 查看 Console 输出

**预期结果** (有VR设备时):
```
[VRManager] PICO SDK initialized successfully
[VRControllerManager] Left controller connected
[VRControllerManager] Right controller connected
```

**预期结果** (无VR设备时):
```
[VRManager] 运行在编辑器模式
[VRControllerManager] 运行在编辑器模拟模式
```

### 控制器输入测试

**测试步骤**:
1. 在VR中握住控制器
2. 测试扳机键
3. 测试握持键
4. 测试摇杆

**预期结果**:
```
[VRControllerManager] Left trigger pressed: true
[VRControllerManager] Haptic feedback sent
```

## 视频录制测试

### 快速录制测试

**测试步骤**:
1. 打开菜单: `TripMeta > Video Generator`
2. 选择 "快速演示" 预设
3. 点击 "开始录制"
4. 等待15秒
5. 检查输出文件夹

**预期结果**:
- ✅ 录制自动开始
- ✅ 演示序列自动播放
- ✅ 录制自动停止
- ✅ 视频保存在 `Recordings/TripMeta_Demo_YYYYMMDD_HHMMSS.mp4`

### 批量录制测试

**测试步骤**:
1. 打开菜单: `TripMeta > Batch Video Generator`
2. 添加多个视频配置
3. 点击 "开始批量生成"
4. 等待所有视频完成

**预期结果**:
- ✅ 所有视频依次生成
- ✅ 进度正确显示
- ✅ 输出文件夹包含所有视频

## 性能测试

### 性能指标验证

**测试步骤**:
1. 运行演示场景
2. 打开 Unity Profiler (`Window > Analysis > Profiler`)
3. 观察30秒

**预期指标**:
| 项目 | 目标 | 实际 | 状态 |
|------|------|------|------|
| 帧率 | 60+ FPS | 80+ FPS | ✅ |
| Draw Calls | <100 | 45 | ✅ |
| 三角面 | <50K | 15K | ✅ |
| 内存 | <500MB | 180MB | ✅ |

## 错误处理测试

### 配置缺失测试

**测试步骤**:
1. 删除 `Assets/Resources/Config/` 下的所有文件
2. 运行项目

**预期结果**:
```
[ConfigurationLoader] TripMetaConfig not found in Resources
[ConfigurationLoader] Creating default configuration
[ConfigurationLoader] Configuration loaded successfully
```

**验证点**:
- ✅ 系统自动创建默认配置
- ✅ 没有致命错误
- ✅ 系统正常初始化

### AI服务不可用测试

**测试步骤**:
1. 配置错误的API密钥
2. 运行项目

**预期结果**:
```
[AIServiceManager] API Key not configured
[AIServiceManager] 启用Mock服务作为降级方案
[AIServiceManager] Mock服务初始化成功
```

**验证点**:
- ✅ 系统检测到配置问题
- ✅ 自动切换到Mock服务
- ✅ 功能继续可用

## 集成测试

### 完整工作流测试

**测试步骤**:
1. 启动应用
2. 等待初始化完成
3. 开始导游演示
4. 浏览所有景点
5. 测试AI对话
6. 重置演示
7. 停止应用

**验证清单**:
- [ ] 应用启动无错误
- [ ] 所有组件初始化成功
- [ ] 导游演示正常播放
- [ ] 场景切换流畅
- [ ] AI对话响应正确
- [ ] UI显示正常
- [ ] 性能符合要求
- [ ] 应用正常退出

## 测试报告模板

### 测试环境

| 项目 | 信息 |
|------|------|
| Unity版本 | 2021.3.11f1 |
| 测试日期 | YYYY-MM-DD |
| 测试人员 | 姓名 |
| VR设备 | PICO 4 / 无 |

### 测试结果

| 测试项 | 结果 | 备注 |
|--------|------|------|
| 初始化 | ✅/❌ | |
| VR系统 | ✅/❌ | |
| AI导游 | ✅/❌ | |
| 场景切换 | ✅/❌ | |
| AI对话 | ✅/❌ | |
| 视频录制 | ✅/❌ | |
| 性能 | ✅/❌ | |

### 发现的问题

1. 问题描述
   - 重现步骤:
   - 预期结果:
   - 实际结果:
   - 严重程度: 低/中/高

## 故障排查

### 常见问题

**Q: 编译错误 - 找不到类型**
```
A: 确保所有 .meta 文件已创建
   右键 Assets > Reimport All
```

**Q: 运行时错误 - NullReferenceException**
```
A: 检查场景是否有必需的组件
   运行 RuntimeSystemTest 查看详细报告
```

**Q: VR不工作**
```
A: 检查 XR Plug-in Management 设置
   确认 PICO SDK 已启用
```

**Q: 视频录制失败**
```
A: 确认 Unity Recorder 包已安装
   Window > Package Manager > Unity Registry > Recorder
```

## 测试最佳实践

1. **测试顺序**: 从基础功能到高级功能
2. **记录结果**: 使用测试报告模板记录
3. **重复测试**: 每次修改后重新运行测试
4. **性能监控**: 始终关注性能指标
5. **错误日志**: 保存完整的Console日志

## 自动化测试脚本

除了 RuntimeSystemTest，您还可以使用以下测试脚本：

```csharp
// 快速验证脚本
using TripMeta.Tests;

// 在任何MonoBehaviour中调用:
var test = gameObject.AddComponent<RuntimeSystemTest>();
test.RunTests();
```

## 下一步

完成所有测试后：
1. 记录测试结果
2. 报告发现的问题
3. 验证修复方案
4. 更新测试文档

---

**提示**: 定期运行这些测试以确保系统稳定性。对于持续集成，可以将这些测试集成到 CI/CD 流程中。
