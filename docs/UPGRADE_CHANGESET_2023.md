# UPGRADE CHANGESET (Proposed) — Unity 2021.3 → 2023.3

本文件列出“建议的包版本变更”（不直接修改工程）。确认后将实际应用到 Packages/manifest.json，并删除 Packages/packages-lock.json 以便Unity 2023重新解析依赖。

更新要点
- XR Interaction Toolkit: 2.6.4 → 3.0.0（需要场景从 XRRig 迁移到 XROrigin）
- URP: 14.0.12 → 17.0.3（需要创建并绑定 UniversalRenderPipelineAsset，运行材质转换）
- 输入系统：显式添加 com.unity.inputsystem 1.7.0（当前由依赖隐式拉取）
- Timeline: 1.7.7 → 1.8.5（随Unity 2023更兼容）
- 移除 com.unity.ui.toolkit 1.0.0-preview.18（Unity 2023中 UI Toolkit 为内置，无需预览包）

原始 manifest.json（关键项）
- com.unity.render-pipelines.universal: 14.0.12
- com.unity.xr.interaction.toolkit: 2.6.4
- com.unity.timeline: 1.7.7
- 未显式声明：com.unity.inputsystem（lock中为1.7.0）
- 存在：com.unity.ui.toolkit: 1.0.0-preview.18（将移除）

目标版本（建议）
- com.unity.xr.interaction.toolkit: 3.0.0
- com.unity.render-pipelines.universal: 17.0.3
- com.unity.timeline: 1.8.5
- com.unity.inputsystem: 1.7.0（新增显式依赖）
- 移除 com.unity.ui.toolkit

后续操作（待审批）
1) 应用 docs/manifest.updated.proposed.json 到 Packages/manifest.json
2) 删除 Packages/packages-lock.json
3) 打开 Unity 2023.3，等待包解析与API Updater
4) 场景迁移：XRRig → XROrigin；绑定 URP Asset；运行材质转换
5) PICO SDK 更新到与Unity 2023兼容版本，校验 Assets/Resources/PXR_* 资产