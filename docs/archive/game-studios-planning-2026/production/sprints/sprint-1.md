# Sprint 1 — 2026-03-30 to 2026-04-13

## Sprint Goal

用火山方舟 Ark 替换 Mock AI 服务，让 AI 导游和 NPC 能真实对话。

## Capacity

- Total days: 14 (2 weeks)
- Buffer (20%): 3 days reserved for unplanned work
- Available: 11 days

## Tasks

### Must Have (Critical Path)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S1-01 | 创建 `ArkConfig` ScriptableObject | gameplay-programmer | 0.5 | — | 包含 apiKey, model, endpoint, temperature, maxTokens, fallback 开关 |
| S1-02 | 实现 `ArkService.cs`（实现 `IGPTService`） | ai-programmer | 2 | S1-01 | SendChatAsync + SendStreamChatAsync + GenerateContentAsync 通过测试 |
| S1-03 | 实现 SSE 流式解析 | ai-programmer | 1 | S1-02 | 逐 chunk 回调正确，`[DONE]` 后不再解析 |
| S1-04 | 实现三级 Fallback（Ark → Ollama → Mock） | ai-programmer | 1 | S1-02 | Ark 断开后 5s 内自动切换 Ollama |
| S1-05 | 修改 `AIServiceManager.InitializeLLMService()` | gameplay-programmer | 0.5 | S1-02 | Manager 启动后 `services[LLM]` 为 ArkService 实例 |
| S1-06 | AI Tour Guide 适配 Ark | ai-programmer | 1 | S1-05 | 导游对话使用 Ark 返回真实回复，多轮上下文连贯 |
| S1-07 | AI NPC System 适配 Ark | ai-programmer | 1 | S1-05 | NPC 对话流式输出，行为树正常触发 |

### Should Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S1-08 | 速率限制 + 指数退避重试 | ai-programmer | 0.5 | S1-02 | 429 响应后退避重试，不超过 3 次 |
| S1-09 | 对话历史上下文窗口截断 | ai-programmer | 0.5 | S1-02 | 超 maxConversationLength 时截断旧消息，保留 system prompt |
| S1-10 | Ark 配置文档 + API Key 获取指南 | — | 0.5 | S1-01 | docs/ARK_SETUP.md 完成 |

### Nice to Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S1-11 | Ark 健康检查 + 自动恢复 | ai-programmer | 0.5 | S1-04 | Degraded 状态定期探测，恢复后自动切回 Ark |
| S1-12 | Token 用量统计日志 | ai-programmer | 0.5 | S1-02 | 每次请求记录 input/output token 数 |

## Carryover from Previous Sprint

N/A — First sprint.

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Ark API 响应延迟超标 (>2s 首 token) | Medium | High | 先用 CodingPlan 模型测试延迟基线 |
| 火山方舟控制台/API Key 获取受阻 | Low | High | 备选：保留 Ollama/Mock fallback，不在客户端硬编码密钥 |
| SSE 流式解析在 Unity WebRequest 中兼容性 | Medium | Medium | 参考现有 OpenAI SSE 解析逻辑，已验证可行 |

## Dependencies on External Factors

- 火山方舟 Ark API Key，通过 `ARK_API_KEY` 注入
- 网络环境可访问 `ark.cn-beijing.volces.com`

## Definition of Done for this Sprint

- [ ] All Must Have tasks (S1-01 ~ S1-07) completed
- [ ] AI 导游在 Editor Play Mode 下产生真实 Ark 回复
- [ ] NPC 流式对话正常工作
- [ ] Fallback 到 Ollama/Mock 验证通过
- [ ] 代码中无硬编码 API Key
- [ ] GDD Acceptance Criteria 全部通过
- [ ] design/gdd/ark-service-integration.md 状态更新为 Implemented
