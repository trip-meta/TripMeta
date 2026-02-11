# TripMeta Unity 2021.3 → 2023.3 LTS 升级计划

本计划在不修改现有工程的前提下给出可执行步骤，确认后可进入实际变更阶段。

## 目标
- Unity 2023.3 LTS
- XR Interaction Toolkit 3.x + XROrigin
- URP（可选）配置资产绑定
- PICO Integration SDK 与Unity 2023兼容版本
- Android: IL2CPP + .NETStd2.1 + ARM64

## 准备与备份（已完成）
- 备份 Packages、ProjectSettings、PXR资产列表
- 生成本升级计划与 manifest 草案

## 执行步骤（建议顺序）
1. 分支与清理
   - 新建分支 feat/upgrade-2023.3
   - 关闭Unity，删除 Library/（可选），删除 Packages/packages-lock.json（建议）
2. 包版本调整
   - 按 manifest.upgrade.draft.json 更新 manifest.json
   - 打开Unity 2023.3，等待包解析与API Updater
3. 渲染与URP（如采用URP）
   - 创建并绑定 UniversalRenderPipelineAsset
   - 运行 Render Pipeline Converter 迁移材质/后处理
4. XR与PICO
   - 升级 XR Interaction Toolkit → 3.x，场景中替换 XRRig → XROrigin
   - 导入匹配Unity 2023的 PICO SDK，检查 Resources/PXR_* 配置
   - 启用 XR Plug-in Management（PICO/Android）
5. 平台设置
   - Android: IL2CPP / .NET Standard 2.1 / ARM64 / Vulkan+GLES3（按PICO建议）
6. 代码迁移
   - 适配XR 3.x 事件与Provider接口
   - 适配PICO命名空间/签名变更
7. 验证与构建
   - Editor运行、PC构建、Android（PICO）构建
8. 文档与CI
   - 更新README、记录改动与注意事项

## 风险与回滚
- 若出现包冲突：回退 manifest.json 到备份版本并重新打开Unity
- 若URP材质异常：在副本场景中测试Converter，必要时回退到Built-in
- 若XR/PICO API变更大：先封装适配层，再逐步替换调用点