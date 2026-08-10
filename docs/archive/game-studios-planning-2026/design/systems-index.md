# Systems Index: TripMeta

> **Status**: Approved
> **Created**: 2026-03-30
> **Last Updated**: 2026-06-27
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

TripMeta 是一个 AI 驱动的 VR 旅游平台，需要三类核心系统：(1) 基础设施层——DI 容器、
配置、错误处理、性能监控；(2) AI 服务层——火山方舟 Ark 模型接入、语音交互、NPC 行为；
(3) VR 体验层——设备管理、交互、空间 UI、场景管理。当前 Alpha 阶段的核心任务是
用 Ark 替换 Mock AI 服务，完善单人 VR 体验。

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Core Infrastructure | Foundation | Done | Implemented | — | — |
| 2 | Error Handling & Logging | Foundation | Done | Implemented | — | — |
| 3 | Performance Monitoring | Foundation | Done | Implemented | — | — |
| 4 | AI Tour Guide | AI | P1 | Implemented (needs Ark adapt) | — | 6, 16 |
| 5 | AI NPC System | AI | P1 | Implemented (needs Ark adapt) | — | 6, 16, 7 |
| 6 | AI Service Layer | AI | Done | Implemented | — | 1, 2, 15 |
| 7 | VR Interaction | VR | Done | Implemented | — | 8 |
| 8 | VR Manager | VR | Done | Implemented | — | 1, 2 |
| 9 | Spatial UI | UI | P2 | Implemented (needs enhancement) | — | 7, 4, 10 |
| 10 | Tour Guide Feature | Gameplay | Done | Implemented | — | 4, 18 |
| 11 | Demo System | Tools | Done | Implemented | — | 4, 7, 9 |
| 12 | Video Recording | Tools | Done | Implemented | — | 11 |
| 13 | Editor Tools | Tools | Done | Implemented | — | — |
| 14 | Test Framework | Quality | Done | Implemented | — | — |
| 15 | Infrastructure (Network/Cache) | Foundation | Done | Implemented (interfaces) | — | 1 |
| 16 | Ark Service Integration | AI | **P0** | Designed | [design/gdd/ark-service-integration.md](ark-service-integration.md) | 6, 15 |
| 17 | Speech Service (国产化) (inferred) | AI | P1 | Not Started | — | 6, 15 |
| 18 | Scene/World Management (inferred) | Core | P2 | Not Started | — | 1, 3 |
| 19 | Save/Persistence (inferred) | Persistence | P3 | Not Started | — | 1, 2 |
| 20 | Localization (inferred) | Meta | P3 | Not Started | — | 1 |
| 21 | Analytics (inferred) | Meta | P3 | Not Started | — | 6, 19 |
| 22 | Audio System (inferred) | Audio | P2 | Not Started | — | 1, 18 |

---

## Categories

| Category | Description |
|----------|-------------|
| **Foundation** | DI, configuration, error handling, networking interfaces, performance monitoring |
| **AI** | LLM integration, NPC behavior, dialogue management, speech services |
| **VR** | Device management, controller input, gesture recognition, haptic feedback |
| **Gameplay** | Tour guide logic, scene navigation, location data |
| **UI** | Spatial panels, HUD, dialogue UI, VR-specific UI |
| **Persistence** | User preferences, conversation history, tour progress |
| **Audio** | Spatial audio, ambient sound, voice playback |
| **Meta** | Analytics, localization, accessibility |
| **Tools** | Demo, video recording, editor extensions, testing |
| **Quality** | Test framework, CI/CD |

---

## Priority Tiers

| Tier | Definition | Systems |
|------|------------|---------|
| **P0 — Immediate** | Core blocker: Ark integration unlocks all AI features | 16 |
| **P1 — Sprint 1** | Adapt existing AI features to Ark + speech | 4, 5, 17 |
| **P2 — Sprint 2** | Scene management, UI enhancement, audio | 9, 18, 22 |
| **P3 — Alpha Complete** | Persistence, localization, analytics | 19, 20, 21 |
| **Done** | Already implemented, maintenance only | 1, 2, 3, 6, 7, 8, 10, 11, 12, 13, 14, 15 |

---

## Dependency Map

### Layer 0 — Foundation (no dependencies)

1. **[1] Core Infrastructure** — DI container, bootstrap, configuration loading
2. **[2] Error Handling & Logging** — Global exception handler, structured logging
3. **[3] Performance Monitoring** — Frame rate, rendering optimizer, VR perf

### Layer 1 — Core (depends on Foundation)

1. **[15] Infrastructure** — Network/cache/resource interfaces → [1]
2. **[8] VR Manager** — PICO SDK, XR loader → [1][2]
3. **[19] Save/Persistence** — User prefs, progress → [1][2]
4. **[18] Scene/World Management** — Location loading, transitions → [1][3]

### Layer 2 — Services (depends on Core)

1. **[6] AI Service Layer** — Service orchestration, health monitoring → [1][2][15]
2. **[7] VR Interaction** — Controllers, gestures, haptics → [8]
3. **[22] Audio System** — Spatial audio, ambient, voice → [1][18]

### Layer 3 — Features (depends on Services)

1. **[16] Ark Service Integration** ★P0 → [6][15]
2. **[17] Speech Service** → [6][15]
3. **[4] AI Tour Guide** → [6][16]
4. **[5] AI NPC System** → [6][16][7]
5. **[10] Tour Guide Feature** → [4][18]
6. **[20] Localization** → [1]

### Layer 4 — Presentation (depends on Features)

1. **[9] Spatial UI** → [7][4][10]
2. **[21] Analytics** → [6][19]

### Layer 5 — Tools (independent)

1. **[11] Demo System** → [4][7][9]
2. **[12] Video Recording** → [11]
3. **[13] Editor Tools** — no runtime dependency
4. **[14] Test Framework** — no runtime dependency

---

## Recommended Design Order

| Order | System | Priority | Layer | Agent(s) | Est. Effort |
|-------|--------|----------|-------|----------|-------------|
| 1 | [16] Ark Service Integration | P0 | Feature | ai-programmer, unity-specialist | M |
| 2 | [4] AI Tour Guide (Ark adapt) | P1 | Feature | ai-programmer, gameplay-programmer | M |
| 3 | [5] AI NPC System (Ark adapt) | P1 | Feature | ai-programmer, gameplay-programmer | M |
| 4 | [17] Speech Service | P1 | Feature | ai-programmer | M |
| 5 | [18] Scene/World Management | P2 | Core | gameplay-programmer, unity-specialist | L |
| 6 | [22] Audio System | P2 | Service | audio-director, sound-designer | M |
| 7 | [9] Spatial UI Enhancement | P2 | Presentation | unity-ui-specialist, ux-designer | M |
| 8 | [19] Save/Persistence | P3 | Core | gameplay-programmer | S |
| 9 | [20] Localization | P3 | Feature | localization-lead | M |
| 10 | [21] Analytics | P3 | Presentation | analytics-engineer | S |

Effort: S = 1 session, M = 2-3 sessions, L = 4+ sessions.

---

## Circular Dependencies

- None found. All dependencies flow downward through the layer hierarchy.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| [16] Ark Service Integration | Technical | Ark OpenAI-compatible 接口的流式响应兼容性 | 先调研 Ark Agent Plan 文档，建 adapter 层 |
| [17] Speech Service | Technical | 国产语音服务选型未定，延迟/质量不确定 | 保留接口抽象，支持多后端切换 |
| [18] Scene/World Management | Scope | 景点数量和复杂度可能膨胀 | 先支持 2-3 个景点，验证流程 |

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 22 |
| Systems implemented | 15 |
| Systems needing adaptation | 2 (AI Tour Guide, AI NPC) |
| Systems not started | 6 (Speech, Scene, Save, Localization, Analytics, Audio) |
| Design docs completed | 1 (Ark Service Integration) |
| P0 systems designed | **1/1** ✅ |
| P1 systems designed | 0/3 |

---

## Next Steps

- [ ] Design [16] Ark Service Integration — `/design-system Ark Service Integration`
- [ ] Plan Sprint 1 around P0 + P1 systems — `/sprint-plan new`
- [ ] Research Ark Agent Plan API documentation before design
- [ ] Design [4] AI Tour Guide adaptation after Ark design
- [ ] Design [5] AI NPC System adaptation after Ark design
- [ ] Run `/gate-check` when P0-P1 systems are designed and implemented
