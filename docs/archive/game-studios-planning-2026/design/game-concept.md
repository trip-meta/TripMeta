# Game Concept: TripMeta

> **Status**: Approved (extracted from existing project)
> **Created**: 2026-03-30
> **Source**: TripMeta README, CLAUDE.md, FUTURE_ROADMAP.md

---

## One-Line Pitch

AI 驱动的 VR 元宇宙旅游社区——戴上头显，AI 导游带你沉浸式游览全球景点。

## Core Experience

| Pillar | Description |
|--------|-------------|
| **探索** | 用 PICO VR 头显探索世界各地虚拟景点 |
| **对话** | 与火山方舟 Ark 驱动的 AI 导游进行自然语言对话 |
| **学习** | 通过知识图谱了解历史和文化 |
| **交互** | 语音对话 + VR 控制器直观交互 |

## Technology

- **Engine**: Unity 2021.3.11f1 (LTS) + URP 17.0.3
- **Platform**: PICO 4 VR (Android)
- **AI Model**: Volcengine Ark Agent Plan / Doubao
- **Speech**: TBD (replacing Azure)
- **Networking**: Netcode for GameObjects 2.1.1 (deferred)

## Current State

Alpha — 80 C# files, ~6,817 LOC, core systems implemented with Mock AI services.

## Near-Term Goals

1. Ark model integration (replace GPT-4o)
2. Single-player VR experience polish
3. Speech service integration

## Long-Term Vision

- Multi-user VR sessions
- UGC content creation tools
- Cross-platform (Quest, Vision Pro, WebXR)
- Global localization (50+ languages)

## Performance Targets

| Metric | Target |
|--------|--------|
| Frame Rate | 90 FPS |
| Latency | <20ms motion-to-photon |
| AI Response | <2s |
| Memory | <4GB |
| Draw Calls | <100/frame |
