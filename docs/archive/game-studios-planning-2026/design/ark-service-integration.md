# Ark Service Integration

> **Status**: Implemented
> **Author**: user + ai-programmer
> **Last Updated**: 2026-06-27
> **Implements Pillar**: 对话 — 与火山方舟 Ark 驱动的 AI 导游进行自然语言对话

## Overview

Ark Service Integration 是 TripMeta 的大语言模型接入层，负责将火山方舟 Ark Agent Plan 的 OpenAI-compatible chat completion 接入到现有的 AI 服务架构中，替代旧的 OpenAI/第三方直连实现。该系统实现 `IGPTService` 接口，为 AI 导游和 NPC 对话提供多轮对话、流式响应、内容生成能力。用户不直接感知该系统——他们感知到的是导游能"说话"了，NPC 能"回应"了。没有该系统，所有 AI 功能处于 Mock 状态，无法产生真实对话。

## Player Fantasy

这是一个用户"不应该注意到"的基础设施系统。当它正常工作时，用户感受到的是：AI 导游的回答自然流畅、知识丰富、响应快速（<2 秒出首个 token）。流式输出让文字像人类打字一样逐步出现，而不是等待一大段文字突然弹出。失败时系统静默降级，用户看到预设回复而不是错误信息。

## Detailed Design

### Core Rules

1. ArkService 实现 `IGPTService` 接口，作为 GPTService 的替代
2. API 端点：`https://ark.cn-beijing.volces.com/api/plan/v3/chat/completions`
3. 认证方式：Bearer Token（API Key 从配置读取，不硬编码）
4. 默认模型：`doubao-seed-2-0-code-preview-260215`
5. 生产模型：通过 `ARK_CHAT_MODEL` 配置，默认沿用 Agent Plan 模型
6. 请求格式与 OpenAI 兼容：`{ model, messages[], temperature, max_tokens, stream }`
7. 非流式响应解析：`response.choices[0].message.content`
8. 流式响应解析：SSE 格式，`data: {json}\n\n`，`delta.content` 逐 chunk 拼接
9. 流式结束标志：`data: [DONE]`
10. 每个对话维护独立的 messages 历史（conversationId 索引）
11. 速率限制：可配置 `maxRequestsPerMinute`，超限后排队等待
12. Fallback 策略：Ark 主服务 → Ollama 本地 → Mock 服务（三级降级）

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| Uninitialized | 服务创建 | `InitializeAsync()` 成功 | 拒绝所有请求 |
| Ready | 初始化成功 / 恢复 | 请求到达 / `Pause()` / 故障 | 接受并处理请求 |
| Processing | 收到请求 | 响应完成 / 超时 / 错误 | 发送 HTTP 请求，解析响应 |
| Streaming | 收到流式请求 | SSE `[DONE]` / 超时 / 错误 | 逐 chunk 回调 `onPartialResponse` |
| Paused | `Pause()` 调用 | `Resume()` 调用 | 拒绝新请求，不影响进行中请求 |
| Degraded | Ark API 连续失败 N 次 | 健康检查恢复 | 自动切换到 Ollama fallback |
| Failed | 所有后端不可用 | `ReinitializeAsync()` | 返回 Mock 预设回复或抛异常 |

### Interactions with Other Systems

| System | Direction | Interface |
|--------|-----------|-----------|
| AI Service Layer [6] | 上游 | `AIServiceManager` 通过 `services[AIServiceType.LLM]` 持有 ArkService 引用 |
| AI Tour Guide [4] | 下游 | 调用 `SendChatAsync()` / `SendStreamChatAsync()` 获取导游回复 |
| AI NPC System [5] | 下游 | `NPCDialogueManager` 调用 `SendStreamChatAsync()` 获取 NPC 对话 |
| Infrastructure [15] | 上游 | 使用 `UnityWebRequest` 发送 HTTP 请求 |
| Core Config [1] | 上游 | 从 `ArkConfig` ScriptableObject 或 AppSettings 读取 API Key、模型、端点 |
| Error Handling [2] | 上游 | 使用 `Logger` 记录请求/响应/错误日志 |

## Formulas

### Rate Limit

```
canSendRequest = (requestCount < maxRequestsPerMinute)
                 OR (now - windowStart >= 60s)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| requestCount | int | 0-∞ | runtime | 当前窗口内已发请求数 |
| maxRequestsPerMinute | int | 1-120 | config | 每分钟最大请求数 |
| windowStart | DateTime | — | runtime | 当前限速窗口起始时间 |

### Timeout Calculation

```
requestTimeout = baseTimeout + (estimatedTokens / tokensPerSecond)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| baseTimeout | float | 5-30s | config | 基础超时（网络 + 首 token） |
| estimatedTokens | int | 10-4096 | request | 预估输出 token 数 |
| tokensPerSecond | float | 20-100 | measured | Ark 实测吐 token 速度 |

**Expected output range**: requestTimeout 在 5s-70s 之间
**Edge case**: 流式请求使用固定 baseTimeout（无需预估 token 总量）

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| API Key 为空或无效 | 初始化时抛异常，记录日志，fallback 到 Ollama | 快速失败，不浪费请求 |
| Ark API 返回 429 (Rate Limited) | 指数退避重试（1s → 2s → 4s），最多 3 次 | 避免雪崩，尊重服务端限制 |
| 流式响应中途断开 | 返回已接收的部分内容，标记 `partial: true` | 部分回答优于无回答 |
| 网络完全不可用 | 切换 Ollama → 若也不可用，返回 Mock 预设回复 | 三级降级保证体验不崩溃 |
| 对话历史超过 context window | 截断最早的消息，保留 system prompt + 最近 N 轮 | 防止请求失败 |
| 并发请求超过 maxConcurrentRequests | 排队等待，FIFO 顺序处理 | 防止 API 过载 |
| temperature = 0 | 允许，产出确定性回复（适合事实类问答） | 导游讲历史事实时需要确定性 |
| 空消息输入 | 拒绝请求，返回错误，不消耗 API 配额 | 防止浪费 |
| Ark 返回空内容 | 重试 1 次，仍空则返回 "抱歉，请再说一遍" | 避免 UI 显示空白 |

## Dependencies

| System | Direction | Nature |
|--------|-----------|--------|
| [6] AI Service Layer | This depends on | 硬依赖 — 通过 AIServiceManager 注册和管理生命周期 |
| [15] Infrastructure | This depends on | 硬依赖 — HTTP 网络请求能力 |
| [1] Core Infrastructure | This depends on | 硬依赖 — DI 容器注册、配置加载 |
| [2] Error Handling | This depends on | 软依赖 — 日志记录（无日志也能工作） |
| [4] AI Tour Guide | Depends on this | 硬依赖 — 导游对话的 LLM 后端 |
| [5] AI NPC System | Depends on this | 硬依赖 — NPC 对话的 LLM 后端 |

## Tuning Knobs

| Parameter | Default | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|---------|------------|-------------------|-------------------|
| model | doubao-seed-2-0-code-preview-260215 | 见 Ark 控制台/Agent Plan 模型列表 | 更强能力，更高成本 | 更快更便宜，能力下降 |
| temperature | 0.7 | 0.0-1.5 | 更有创意/随机 | 更确定/可预测 |
| maxTokens | 2048 | 64-16384 | 更长回复，更慢 | 更短更快 |
| maxRequestsPerMinute | 30 | 1-120 | 更高吞吐 | 更低成本 |
| maxConcurrentRequests | 3 | 1-10 | 更快并发 | 更稳定 |
| requestTimeout | 30s | 5-120s | 容忍慢响应 | 快速失败 |
| maxConversationLength | 20 | 5-50 | 更长上下文记忆 | 更少 token 消耗 |
| enableFallback | true | bool | 有降级保护 | 失败即报错 |
| fallbackRetryCount | 3 | 0-5 | 更多重试机会 | 更快失败 |

## Acceptance Criteria

- [ ] ArkService 实现 `IGPTService` 完整接口
- [ ] `SendChatAsync` 正确发送请求到 Ark API 并返回文本
- [ ] `SendStreamChatAsync` 通过 SSE 逐 chunk 回调
- [ ] 流式首 token 延迟 < 2s（正常网络条件下）
- [ ] API Key 从配置文件读取，代码中无硬编码密钥
- [ ] Ark 不可用时自动降级到 Ollama
- [ ] Ollama 也不可用时降级到 Mock 服务
- [ ] 对话历史正确维护（多轮上下文连贯）
- [ ] 速率限制正常工作（超限排队，不丢弃）
- [ ] 超时机制工作（30s 内无响应则中止）
- [ ] `AIServiceManager.InitializeLLMService()` 使用 ArkService 替代 OpenAIService
- [ ] 所有日志正确输出到 Logger（无 `Debug.Log` 直接调用）
- [ ] 无 `[DONE]` 之后的多余解析尝试
- [ ] Performance: 单次非流式请求处理（不含网络）< 5ms

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| 语音服务替代方案：是否用讯飞/百度替换 Azure Speech？ | user | Sprint 1 结束前 | 待定 |
| Ark API Key 的获取和管理流程？ | user | 开发开始前 | 需要在火山方舟控制台创建并通过 `ARK_API_KEY` 注入 |
| 是否需要特殊认证协议？ | ai-programmer | Sprint 1 | OpenAI-compatible chat endpoint 使用 Bearer Token |
