# Ark Service Setup Guide

TripMeta uses Volcengine Ark CodingPlan as the primary OpenAI-compatible LLM backend for AI tour guide conversations.

## 1. Required Secret

Use the saved local Ark key only through local secret files or platform secret managers. Do not commit a real key.

For local Unity development, create or update the gitignored `secrets.json` file at the repository root:

```json
{
  "ark_api_key": "ark-your-key"
}
```

Legacy provider-specific key fields are no longer read by runtime code.

## 2. Unity Editor Configuration

Open `Assets/Resources/Config/AppSettings` in the Unity Inspector and set:

- `AI Settings > Ark Api Key`: your Ark API key, or leave empty to load `secrets.json`
- `AI Settings > Ark Base Url`: `https://ark.cn-beijing.volces.com/api/coding/v3`
- `AI Settings > Ark Chat Model`: `doubao-seed-2-0-code-preview-260215`

The existing `OpenAI Api Key` field is kept only for older serialized settings. Prefer the Ark fields for new configuration.

## 3. Code Defaults

`GPTConfig` in `Assets/Scripts/AI/Models/AIModels.cs` defaults to:

- Endpoint: `https://ark.cn-beijing.volces.com/api/coding/v3/chat/completions`
- Model: `doubao-seed-2-0-code-preview-260215`

Only the API key is required for the default Ark path.

## 4. Fallback

`ArkService` uses a three-tier fallback:

```text
Ark API (primary) -> Ollama local (fallback) -> Mock response
```

- Ark consecutive failures trigger Ollama fallback.
- Ollama failures trigger Mock fallback.
- Health checks switch the primary backend back to Ark after recovery.

### Ollama Local Fallback

1. Install Ollama: `https://ollama.ai`
2. Pull a model: `ollama pull llama3.2`
3. Ensure Ollama runs at `http://localhost:11434`

## 5. Parameters

| Parameter | Default | Description |
| --- | --- | --- |
| `apiKey` | `""` | Ark API key, loaded from Inspector or `secrets.json` |
| `apiEndpoint` | `https://ark.cn-beijing.volces.com/api/coding/v3/chat/completions` | OpenAI-compatible chat endpoint |
| `model` | `doubao-seed-2-0-code-preview-260215` | Ark CodingPlan chat model |
| `maxTokens` | `2048` | Maximum generated tokens |
| `temperature` | `0.7` | Generation randomness |
| `maxRequestsPerMinute` | `30` | Rate limit |
| `maxConcurrentRequests` | `3` | Maximum concurrent requests |
| `requestTimeout` | `30s` | Request timeout |
| `maxConversationLength` | `20` | Conversation history length |
| `enableFallback` | `true` | Enable fallback chain |
| `fallbackRetryCount` | `3` | Retry count before backend degradation |

## 6. Verification

Enter Play Mode in Unity Editor and trigger an AI tour guide conversation. The Console should show:

```text
[Ark] Ark连接测试成功
[Ark] Ark服务初始化完成 (model: doubao-seed-2-0-code-preview-260215)
[AIServiceManager] LLM服务初始化成功 (backend: Ark)
```

If the service degrades to Ollama, verify that `ark_api_key` exists in `secrets.json` or that `Ark Api Key` is set in `AppSettings`.

## 7. Troubleshooting

| Problem | Cause | Solution |
| --- | --- | --- |
| `Ark API密钥未配置` | Missing API key | Set `ark_api_key` in `secrets.json` or `Ark Api Key` in `AppSettings` |
| `429 Rate Limited` | Too many requests | Exponential backoff is automatic; lower `maxRequestsPerMinute` if needed |
| Connection timeout | Network or endpoint issue | Verify access to `ark.cn-beijing.volces.com` |
| Garbled response | Encoding issue | Confirm `Content-Type: application/json` and UTF-8 request body |
