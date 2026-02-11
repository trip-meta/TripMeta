# TripMeta 视频录制指南

## 概述

TripMeta 项目包含完整的视频录制系统，支持自动生成演示视频。本指南介绍如何使用这些工具创建高质量的项目演示视频。

## 功能特性

### 自动视频录制
- **Unity Recorder API**: 使用 Unity 官方 Recorder 包进行高质量视频录制
- **Screen Capture**: 使用 Screen Capture API 录制截图序列
- **自动化演示**: 完全自动的演示序列，无需人工操作
- **批量生成**: 支持批量生成多个不同配置的视频

### 录制选项
- 多种分辨率支持 (720p, 1080p, 1440p, 4K)
- 可调节帧率 (30-120 FPS)
- 自动相机移动和场景切换
- 预设演示模板

## 安装设置

### 方法1: Unity Recorder API (推荐)

#### 1. 安装 Recorder 包

1. 打开 Unity Package Manager (`Window > Package Manager`)
2. 选择 "Unity Registry"
3. 找到并安装 "Recorder" 包

#### 2. 设置录制组件

1. 打开菜单 `TripMeta > Setup Video Recording`
2. 选择 "Unity Recorder API"
3. 点击 "设置 Unity Recorder"

### 方法2: Screen Capture (简单)

1. 打开菜单 `TripMeta > Setup Video Recording`
2. 选择 "Screen Capture"
3. 点击 "设置 Screen Capture"

## 使用方法

### 快速开始

#### 使用视频生成器窗口

1. 打开菜单 `TripMeta > Video Generator`
2. 选择视频预设:
   - **快速演示**: 720p, 30fps, 15秒
   - **完整演示**: 1080p, 60fps, 45秒
   - **高质量**: 4K, 60fps, 60秒
3. 点击 "开始录制"
4. 等待录制完成

#### 使用快捷键 (Play模式)

在 Unity Play 模式下，可以使用以下快捷键:

```
[R]  - 开始/停止录制
[1]  - 开始导游演示
[2]  - 下一个景点
[3]  - AI对话演示
[4]  - 重置演示
```

### 高级使用

#### 自定义演示序列

编辑 `AutomatedDemoDirector` 组件来自定义演示序列:

```csharp
// 在Inspector中配置:
- demoDuration: 演示总时长
- startDelay: 开始延迟时间
- cameraWaypoints: 相机路径点
- cameraMoveSpeed: 相机移动速度
```

#### 编程控制

```csharp
// 获取VideoRecorder组件
var recorder = FindObjectOfType<VideoRecorder>();

// 开始录制
recorder.StartRecording();

// 停止录制
recorder.StopRecording();
```

### 使用 Screen Capture

Screen Capture 模式会保存一系列截图，可以用视频编辑软件合成:

1. 运行场景
2. 按 `[R]` 开始录制
3. 演示完成后按 `[R]` 停止
4. 截图保存在 `Recordings/Frames_YYYYMMDD_HHMMSS/`
5. 使用视频编辑软件合成 (如 Adobe Premiere, FFmpeg)

## 输出位置

录制的视频默认保存在:

```
TripMeta/Recordings/
├── TripMeta_Demo_YYYYMMDD_HHMMSS.mp4  # Unity Recorder 输出
└── Frames_YYYYMMDD_HHMMSS/            # Screen Capture 输出
    └── TripMeta_00000.png
    └── TripMeta_00001.png
    └── ...
```

## 组件说明

### VideoRecorder

主要录制组件，使用 Unity Recorder API。

**配置选项:**
- `outputFolder`: 输出文件夹
- `fileNamePrefix`: 文件名前缀
- `frameRate`: 录制帧率
- `resolutionWidth/Height`: 视频分辨率
- `autoRecordOnStart`: 自动开始录制
- `autoRecordDuration`: 自动录制时长

### AutomatedDemoDirector

自动演示导演，控制演示序列。

**配置选项:**
- `demoDuration`: 演示总时长
- `autoStart`: 自动开始演示
- `cameraWaypoints`: 相机路径点数组
- `cameraMoveSpeed`: 相机移动速度

### SimpleVideoRecorder

简化录制组件，使用 Screen Capture API。

**配置选项:**
- `captureFrameRate`: 截图帧率
- `superSampleSize`: 超采样大小
- `outputFolder`: 输出文件夹

## FFmpeg 视频合成

如果你使用 Screen Capture 模式，可以用 FFmpeg 合成视频:

```bash
# 基本合成
ffmpeg -framerate 60 -i "Frames/TripMeta_%05d.png" -c:v libx264 -preset slow -crf 20 output.mp4

# 高质量合成
ffmpeg -framerate 60 -i "Frames/TripMeta_%05d.png" \
  -c:v libx264 -preset veryslow -crf 18 \
  -pix_fmt yuv420p -movflags +faststart \
  output_high_quality.mp4

# 添加水印
ffmpeg -i output.mp4 -i watermark.png \
  -filter_complex "overlay=10:10" \
  output_with_watermark.mp4
```

## 常见问题

### Q: 录制的视频没有声音?

A: Unity Recorder 默认不录制音频。要启用音频:
1. 选择 Recorder Window
2. 在 "Added Recorders" 中找到 "Audio"
3. 启用 Audio Recorder

### Q: 视频文件太大?

A: 调整视频设置:
- 降低分辨率
- 降低帧率
- 调整视频比特率模式

### Q: 录制性能问题?

A: 优化建议:
- 降低录制分辨率
- 使用更低的比特率
- 关闭不必要的 Unity 编辑器窗口
- 使用独立构建版本录制

### Q: Screen Capture 截图不完整?

A: 检查设置:
- 确保 `captureFrameRate` 设置正确
- 不要在帧率很低时录制
- 检查磁盘空间是否充足

## 示例场景

### 场景1: 产品演示

创建一个 30 秒的产品演示视频:

```
1. 打开 Video Generator 窗口
2. 选择 "快速演示" 预设
3. 设置时长为 30 秒
4. 点击 "开始录制"
```

### 场景2: 高质量展示

创建 4K 高质量演示视频:

```
1. 打开 Video Generator 窗口
2. 设置分辨率为 3840x2160 (4K)
3. 设置帧率为 60 FPS
4. 设置时长为 60 秒
5. 点击 "开始录制"
```

### 场景3: 批量生成

生成多个不同配置的视频:

```
1. 打开 Video Generator 窗口
2. 选择 "快速演示" -> 录制
3. 选择 "完整演示" -> 录制
4. 选择 "高质量" -> 录制
```

## 自动化脚本

### 创建自动化录制脚本

```csharp
using UnityEngine;
using TripMeta.Video;

public class AutoRecordDemo : MonoBehaviour
{
    private VideoRecorder recorder;
    private AutomatedDemoDirector director;

    void Start()
    {
        // 获取组件
        recorder = FindObjectOfType<VideoRecorder>();
        director = FindObjectOfType<AutomatedDemoDirector>();

        // 自动开始录制和演示
        StartCoroutine(AutoRecordSequence());
    }

    IEnumerator AutoRecordSequence()
    {
        // 等待2秒
        yield return new WaitForSeconds(2f);

        // 开始录制
        recorder.StartRecording();

        // 开始演示
        director.StartDemo();

        // 等待演示完成
        yield return new WaitForSeconds(director.demoDuration);

        // 停止录制
        recorder.StopRecording();
    }
}
```

## 最佳实践

1. **录制前准备**
   - 保存场景
   - 关闭不必要的编辑器窗口
   - 清空输出文件夹

2. **录制设置**
   - 使用稳定的帧率 (30 或 60 FPS)
   - 选择合适的分辨率 (1080p 适合大多数用途)
   - 预留足够的磁盘空间

3. **演示设计**
   - 保持演示简洁明了
   - 突出核心功能
   - 控制时长在 1-3 分钟

4. **后期处理**
   - 添加字幕和说明
   - 调整音频
   - 添加背景音乐

## 技术参考

### Unity Recorder API

- 官方文档: https://docs.unity3d.com/Packages/com.unity.recorder@latest
- 支持的格式: MP4, WebM, MOV, GIF
- 支持的输入: Game View, Camera, Render Texture

### Screen Capture API

- `ScreenCapture.CaptureScreenshot()`: 截取单帧
- `Time.captureFramerate`: 控制截图帧率

## 相关文档

- [README.md](README.md) - 项目总览
- [DEMO_GUIDE.md](DEMO_GUIDE.md) - 演示系统指南
- [ACTUAL_EFFECTS.md](ACTUAL_EFFECTS.md) - 实际效果说明

## 更新日志

### v1.0.0 (2024)
- 初始版本
- Unity Recorder API 集成
- Screen Capture 支持
- 自动化演示系统
- 视频生成器窗口

---

**提示**: 如有问题或建议，请提交 Issue 或 Pull Request。
