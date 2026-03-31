# GLM Service Setup Guide

TripMeta 使用智谱 AI 的 GLM 系列模型作为 AI 对话后端。

## 1. 获取 API Key

1. 注册智谱 AI 开放平台：https://open.bigmodel.cn/
2. 进入控制台 → API Keys → 创建新 Key
3. 复制 API Key（仅显示一次，请妥善保存）

## 2. 配置项目

### 方式 A：通过 Unity Editor 配置

1. 打开 Unity → Assets/Resources/Config/
2. 选择 `AppSettings` ScriptableObject
3. 在 Inspector 中填入：
   - `AI Settings > OpenAI Api Key` → 粘贴你的 GLM API Key
   - `AI Settings > GPT Model` → `glm-4-flash-250414`（免费）或 `glm-4.7`（旗舰）

### 方式 B：通过代码默认值

`GPTConfig` 类（`Assets/Scripts/AI/Models/AIModels.cs`）已配置默认值：
- Endpoint: `https://open.bigmodel.cn/api/paas/v4/chat/completions`
- Model: `glm-4-flash-250414`

只需配置 API Key 即可运行。

## 3. 可用模型

| Model ID | Context | Max Output | Price | Use Case |
|----------|---------|------------|-------|----------|
| `glm-4-flash-250414` | 128K | 16K | 免费 | 开发测试 |
| `glm-4.7` | 200K | 128K | 5元/百万tokens | 生产环境 |
| `glm-4-plus-250414` | 128K | 16K | 50元/百万tokens | 高质量场景 |

推荐：开发阶段使用 `glm-4-flash-250414`（免费），上线后切换 `glm-4.7`。

## 4. Fallback 机制

GLMService 支持三级降级：

```
GLM API (主) → Ollama 本地 (备) → Mock 预设回复 (兜底)
```

- GLM 连续失败 3 次 → 自动切换 Ollama
- Ollama 不可用 → 自动切换 Mock
- 后台健康检查恢复后自动切回 GLM

### 配置 Ollama 本地备用

1. 安装 Ollama：https://ollama.ai
2. 拉取模型：`ollama pull llama3.2`
3. 确保运行在默认端口：`http://localhost:11434`

无需额外配置，GLMService 会自动发现 Ollama。

## 5. 配置参数

| Parameter | Default | Description |
|-----------|---------|-------------|
| apiKey | "" | 智谱 AI API Key |
| apiEndpoint | `https://open.bigmodel.cn/api/paas/v4/chat/completions` | API 端点 |
| model | `glm-4-flash-250414` | 模型 ID |
| maxTokens | 2048 | 最大生成长度 |
| temperature | 0.7 | 创意度 (0=确定性, 1.5=高随机) |
| maxRequestsPerMinute | 30 | 速率限制 |
| maxConcurrentRequests | 3 | 最大并发 |
| requestTimeout | 30s | 请求超时 |
| maxConversationLength | 20 | 对话历史保留条数 |
| enableFallback | true | 是否启用降级 |
| fallbackRetryCount | 3 | 失败重试次数 |

## 6. 验证

在 Unity Editor 中进入 Play Mode，触发 AI 导游对话。Console 中应看到：

```
[GLM] GLM连接测试成功
[GLM] GLM服务初始化完成 (model: glm-4-flash-250414)
[AIServiceManager] LLM服务初始化成功 (backend: GLM)
```

如果看到 `已降级到Ollama后端`，说明 GLM API Key 配置有误或网络不通。

## 7. Troubleshooting

| 问题 | 原因 | 解决方案 |
|------|------|---------|
| "GLM API密钥未配置" | API Key 为空 | 检查 AppSettings 中的配置 |
| 429 Rate Limited | 请求过于频繁 | 自动指数退避重试，或降低 maxRequestsPerMinute |
| 连接超时 | 网络问题 | 确认可访问 open.bigmodel.cn |
| 响应乱码 | 编码问题 | 确认 Content-Type: application/json + UTF-8 |
