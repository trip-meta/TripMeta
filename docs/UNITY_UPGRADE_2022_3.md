# Unity 2022.3 LTS 升级指南

## 升级概述

从 Unity 2021.3.11f1 升级到 Unity 2022.3 LTS

## 主要变更

### 1. Unity 版本更新
- **原版本**: 2021.3.11f1
- **新版本**: 2022.3.x LTS

### 2. 包版本更新

| 包名 | 旧版本 | 新版本 | 备注 |
|------|--------|--------|------|
| Universal Render Pipeline | 17.0.3 | 14.0.11 | Unity 2022.3 内置版本 |
| XR Interaction Toolkit | 3.0.0 | 2.6.3 | 降级到兼容版本 |
| Input System | 1.7.0 | 1.6.3 | 兼容版本 |
| Netcode for GameObjects | 1.12.0 | 1.8.1 | 兼容版本 |
| ML Agents | 2.0.1 | 2.0.1 | 保持不变 |
| AI Navigation | 1.1.6 | 1.1.5 | 兼容版本 |
| TextMeshPro | 3.0.7 | 3.0.9 | 更新版本 |

### 3. API 变更

#### AR Foundation
- `ARSessionOrigin` 重命名为 `XROrigin`
- 需要更新所有引用

#### XR Interaction Toolkit
- `XRRig` 被 `XROrigin` 替代
- 输入 Action 配置格式变更

#### URP
- 渲染管线配置可能需要重新设置
- 一些渲染特性可能已弃用

## 升级步骤

### 步骤 1: 备份项目
```bash
git add .
git commit -m "backup: Unity 2021.3 升级前备份"
```

### 步骤 2: 更新 ProjectVersion.txt
修改 `ProjectSettings/ProjectVersion.txt`:
```
m_EditorVersion: 2022.3.45f1
m_EditorVersionWithRevision: 2022.3.45f1 (d11192b3c6f4)
```

### 步骤 3: 更新 Packages/manifest.json
更新所有包版本到 Unity 2022.3 兼容版本

### 步骤 4: 打开项目
使用 Unity 2022.3 打开项目，让 Unity 自动更新元数据

### 步骤 5: 修复编译错误
- 更新 API 调用
- 修复弃用警告
- 更新预制体引用

### 步骤 6: 测试
- 运行所有单元测试
- 验证VR功能
- 验证网络功能
- 验证AR功能

## 已知问题及解决方案

### 问题 1: AR Foundation API 变更
**解决方案**: 将所有 `ARSessionOrigin` 替换为 `XROrigin`

### 问题 2: XR Interaction Toolkit 版本
**解决方案**: 降级到 2.6.3 或更新输入 Action 配置

### 问题 3: URP 配置
**解决方案**: 重新生成 URP 管线配置

## 回滚计划

如果升级失败，执行以下命令回滚:
```bash
git reset --hard HEAD~1
```

## 验证清单

- [ ] 项目能在 Unity 2022.3 中打开
- [ ] 无编译错误
- [ ] 所有单元测试通过
- [ ] VR 功能正常
- [ ] 多人游戏功能正常
- [ ] AR 功能正常
- [ ] 移动伴侣功能正常
