# Unity 2021.3 → 2023.3 升级执行计划

## Core Features

- 安全备份

- manifest版本升级

- 依赖重新解析

- XR/URP迁移

- PICO SDK对齐

- 构建验证

## Tech Stack

{
  "VR": "Unity 2023.3 LTS + XR Interaction Toolkit 3.x + URP 17.x",
  "AI核心": "保留现有AI实现，后续验证兼容",
  "架构": "保留现有DI与分层架构",
  "优化工具": "Unity Profiler/Frame Debugger + API Updater"
}

## Design

最小改动平滑升级，先通编译与运行，再做渲染与交互优化

## Plan

Note: 

- [ ] is holding
- [/] is doing
- [X] is done

---

[X] 准备与备份

[X] 生成升级清单与manifest草案

[/] 应用manifest并删除lock

[ ] 用Unity 2023打开并解析依赖

[ ] XR/URP迁移与PICO对齐

[ ] 构建验证与文档更新
