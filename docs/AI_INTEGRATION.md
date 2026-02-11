# TripMeta AI服务集成指南

## 📋 目录

- [AI服务概览](#ai服务概览)
- [GPT集成](#gpt集成)
- [语音服务集成](#语音服务集成)
- [计算机视觉集成](#计算机视觉集成)
- [推荐系统](#推荐系统)
- [AI服务管理](#ai服务管理)
- [性能优化](#性能优化)
- [错误处理](#错误处理)

## 🤖 AI服务概览

TripMeta集成了多种AI服务，为用户提供智能化的VR旅游体验：

- **GPT-4**: 智能对话和内容生成
- **Azure Speech**: 语音识别和合成
- **Computer Vision**: 场景理解和物体识别
- **推荐引擎**: 个性化内容推荐
- **情感分析**: 用户情绪识别和响应

### AI服务架构

```
┌─────────────────────────────────────────────────────────┐
│                   AI Service Layer                     │
├─────────────────────────────────────────────────────────┤
│  AI Service Manager                                     │
│  ├── Service Discovery    ├── Load Balancing           │
│  ├── Health Monitoring    ├── Failover Strategy        │
│  ├── Rate Limiting        └── Circuit Breaker          │
├─────────────────────────────────────────────────────────┤
│  Core AI Services                                       │
│  ├── GPT Service          ├── Speech Service           │
│  │   ├── Chat Completion  │   ├── Speech Recognition   │
│  │   ├── Text Generation  │   ├── Speech Synthesis     │
│  │   ├── Context Memory   │   ├── Voice Cloning        │
│  │   └── Fine-tuning      │   └── Real-time STT/TTS    │
│  ├── Vision Service       ├── Recommendation Engine    │
│  │   ├── Scene Analysis   │   ├── Collaborative Filter │
│  │   ├── Object Detection │   ├── Content-based Filter │
│  │   ├── OCR Processing   │   ├── Deep Learning Model  │
│  │   └── Image Generation │   └── Real-time Inference  │
├─────────────────────────────────────────────────────────┤
│  AI Infrastructure                                      │
│  ├── Model Management     ├── Data Pipeline            │
│  │   ├── Model Versioning │   ├── Data Preprocessing   │
│  │   ├── A/B Testing      │   ├── Feature Engineering  │
│  │   ├── Model Deployment │   ├── Data Validation      │
│  │   └── Performance Mon. │   └── ETL Processes        │
│  ├── Caching Layer        └── Security & Privacy       │
│  │   ├── Response Cache   ├── Data Encryption          │
│  │   ├── Model Cache      ├── PII Protection           │
│  │   ├── Embedding Cache  ├── Audit Logging           │
│  │   └── Prediction Cache └── Compliance Monitoring    │
└─────────────────────────────────────────────────────────┘
```

## 🧠 GPT集成

### GPT服务配置

```csharp
// GPT配置类
[Serializable]
public class GPTConfiguration
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4";
    public int MaxTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.7f;
    public float TopP { get; set; } = 1.0f;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableStreaming { get; set; } = true;
    public string SystemPrompt { get; set; }
}

// GPT服务实现
public class GPTService : IGPTService, IDisposable
{
    private readonly GPTConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IRateLimiter _rateLimiter;
    private readonly ICircuitBreaker _circuitBreaker;
    
    public GPTService(GPTConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _httpClient = CreateHttpClient();
        _rateLimiter = new TokenBucketRateLimiter(60, TimeSpan.FromMinutes(1)); // 60 requests per minute
        _circuitBreaker = new CircuitBreaker(5, TimeSpan.FromMinutes(1)); // 5 failures, 1 minute timeout
    }
    
    public async Task<string> GenerateResponseAsync(string prompt, GPTOptions options = null)
    {
        await _rateLimiter.WaitAsync();
        
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            var request = CreateChatRequest(prompt, options);
            var response = await SendRequestAsync(request);
            
            _logger.LogInfo($"GPT response generated. Tokens used: {response.Usage.TotalTokens}");
            
            return response.Choices[0].Message.Content;
        });
    }
    
    public async IAsyncEnumerable<string> GenerateStreamAsync(string prompt, GPTOptions options = null)
    {
        await _rateLimiter.WaitAsync();
        
        var request = CreateChatRequest(prompt, options);
        request.Stream = true;
        
        using var response = await _httpClient.PostAsync("/chat/completions", 
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                if (data == "[DONE]") break;
                
                var chunk = JsonSerializer.Deserialize<GPTStreamChunk>(data);
                if (chunk.Choices?[0]?.Delta?.Content != null)
                {
                    yield return chunk.Choices[0].Delta.Content;
                }
            }
        }
    }
    
    private ChatCompletionRequest CreateChatRequest(string prompt, GPTOptions options)
    {
        var messages = new List<ChatMessage>();
        
        // 添加系统提示
        if (!string.IsNullOrEmpty(_config.SystemPrompt))
        {
            messages.Add(new ChatMessage
            {
                Role = "system",
                Content = _config.SystemPrompt
            });
        }
        
        // 添加用户消息
        messages.Add(new ChatMessage
        {
            Role = "user",
            Content = prompt
        });
        
        return new ChatCompletionRequest
        {
            Model = options?.Model ?? _config.Model,
            Messages = messages,
            MaxTokens = options?.MaxTokens ?? _config.MaxTokens,
            Temperature = options?.Temperature ?? _config.Temperature,
            TopP = options?.TopP ?? _config.TopP,
            Stream = false
        };
    }
}
```

### 上下文管理

```csharp
// 对话上下文管理
public class ConversationContextManager : IConversationContextManager
{
    private readonly Dictionary<string, ConversationContext> _contexts;
    private readonly int _maxContextLength;
    private readonly int _maxTokens;
    
    public ConversationContextManager(int maxContextLength = 10, int maxTokens = 4000)
    {
        _contexts = new Dictionary<string, ConversationContext>();
        _maxContextLength = maxContextLength;
        _maxTokens = maxTokens;
    }
    
    public void AddMessage(string sessionId, string role, string content)
    {
        if (!_contexts.TryGetValue(sessionId, out var context))
        {
            context = new ConversationContext();
            _contexts[sessionId] = context;
        }
        
        context.Messages.Add(new ChatMessage
        {
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        });
        
        // 限制上下文长度
        TrimContext(context);
    }
    
    public List<ChatMessage> GetContext(string sessionId)
    {
        return _contexts.TryGetValue(sessionId, out var context) 
            ? context.Messages.ToList() 
            : new List<ChatMessage>();
    }
    
    private void TrimContext(ConversationContext context)
    {
        // 按消息数量限制
        while (context.Messages.Count > _maxContextLength)
        {
            context.Messages.RemoveAt(0);
        }
        
        // 按Token数量限制
        var totalTokens = EstimateTokenCount(context.Messages);
        while (totalTokens > _maxTokens && context.Messages.Count > 1)
        {
            context.Messages.RemoveAt(0);
            totalTokens = EstimateTokenCount(context.Messages);
        }
    }
    
    private int EstimateTokenCount(List<ChatMessage> messages)
    {
        // 简单的Token估算：1个Token约等于4个字符
        return messages.Sum(m => m.Content.Length / 4);
    }
}
```

### 智能提示工程

```csharp
// 提示模板管理
public class PromptTemplateManager : IPromptTemplateManager
{
    private readonly Dictionary<string, PromptTemplate> _templates;
    
    public PromptTemplateManager()
    {
        _templates = new Dictionary<string, PromptTemplate>
        {
            ["tour_guide"] = new PromptTemplate
            {
                Name = "智能导游",
                Template = @"你是一位专业的虚拟导游，名叫小美。你的任务是为用户提供有趣、准确、个性化的旅游信息。

当前场景：{scene_description}
用户位置：{user_location}
用户兴趣：{user_interests}
天气信息：{weather_info}

请根据以上信息，用友好、专业的语调回答用户的问题：{user_question}

回答要求：
1. 保持友好和专业的语调
2. 提供准确的历史和文化信息
3. 根据用户兴趣个性化回答
4. 如果涉及安全问题，请给出适当提醒
5. 回答长度控制在200字以内",
                Parameters = new[] { "scene_description", "user_location", "user_interests", "weather_info", "user_question" }
            },
            
            ["content_generator"] = new PromptTemplate
            {
                Name = "内容生成器",
                Template = @"请为以下旅游景点生成吸引人的介绍内容：

景点名称：{attraction_name}
景点类型：{attraction_type}
历史背景：{historical_background}
特色亮点：{highlights}
目标受众：{target_audience}

生成要求：
1. 内容要生动有趣，能够吸引游客
2. 突出景点的独特性和价值
3. 包含实用的游览建议
4. 语言要通俗易懂
5. 字数控制在300-500字之间",
                Parameters = new[] { "attraction_name", "attraction_type", "historical_background", "highlights", "target_audience" }
            }
        };
    }
    
    public string GeneratePrompt(string templateName, Dictionary<string, string> parameters)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }
        
        var prompt = template.Template;
        
        foreach (var param in template.Parameters)
        {
            var placeholder = $"{{{param}}}";
            var value = parameters.TryGetValue(param, out var paramValue) ? paramValue : "";
            prompt = prompt.Replace(placeholder, value);
        }
        
        return prompt;
    }
}

// 动态提示优化
public class PromptOptimizer : IPromptOptimizer
{
    private readonly IGPTService _gptService;
    private readonly IAnalyticsService _analytics;
    
    public async Task<string> OptimizePromptAsync(string originalPrompt, string expectedOutput, string actualOutput)
    {
        var optimizationPrompt = $@"
请优化以下AI提示，使其能够生成更接近期望输出的结果：

原始提示：
{originalPrompt}

期望输出：
{expectedOutput}

实际输出：
{actualOutput}

请提供优化后的提示，并说明优化理由：";

        var optimizedPrompt = await _gptService.GenerateResponseAsync(optimizationPrompt);
        
        // 记录优化历史
        await _analytics.TrackPromptOptimizationAsync(originalPrompt, optimizedPrompt, expectedOutput, actualOutput);
        
        return optimizedPrompt;
    }
}
```

## 🎤 语音服务集成

### Azure Speech服务

```csharp
// 语音服务配置
[Serializable]
public class SpeechConfiguration
{
    public string SubscriptionKey { get; set; }
    public string Region { get; set; } = "eastus";
    public string Language { get; set; } = "zh-CN";
    public string VoiceName { get; set; } = "zh-CN-XiaoxiaoNeural";
    public float SpeechRate { get; set; } = 1.0f;
    public float Pitch { get; set; } = 0.0f;
    public int SampleRate { get; set; } = 16000;
    public AudioFormat AudioFormat { get; set; } = AudioFormat.Wav;
}

// 语音服务实现
public class AzureSpeechService : ISpeechService, IDisposable
{
    private readonly SpeechConfiguration _config;
    private readonly SpeechConfig _speechConfig;
    private readonly ILogger _logger;
    private SpeechRecognizer _recognizer;
    private SpeechSynthesizer _synthesizer;
    
    public AzureSpeechService(SpeechConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
        
        _speechConfig = SpeechConfig.FromSubscription(config.SubscriptionKey, config.Region);
        _speechConfig.SpeechRecognitionLanguage = config.Language;
        _speechConfig.SpeechSynthesisVoiceName = config.VoiceName;
        
        InitializeServices();
    }
    
    public async Task<string> RecognizeSpeechAsync(AudioClip audioClip)
    {
        try
        {
            var audioData = ConvertAudioClipToWav(audioClip);
            
            using var audioInputStream = AudioInputStream.CreatePushStream();
            using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
            using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
            
            // 推送音频数据
            audioInputStream.Write(audioData);
            audioInputStream.Close();
            
            var result = await recognizer.RecognizeOnceAsync();
            
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                _logger.LogInfo($"Speech recognized: {result.Text}");
                return result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                _logger.LogWarning("No speech could be recognized");
                return string.Empty;
            }
            else
            {
                _logger.LogError($"Speech recognition failed: {result.Reason}");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Speech recognition error: {ex.Message}");
            throw;
        }
    }
    
    public async Task<AudioClip> SynthesizeSpeechAsync(string text, VoiceSettings voiceSettings = null)
    {
        try
        {
            var ssml = GenerateSSML(text, voiceSettings);
            
            using var synthesizer = new SpeechSynthesizer(_speechConfig);
            var result = await synthesizer.SpeakSsmlAsync(ssml);
            
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                var audioClip = ConvertWavToAudioClip(result.AudioData);
                _logger.LogInfo($"Speech synthesized successfully. Duration: {audioClip.length}s");
                return audioClip;
            }
            else
            {
                _logger.LogError($"Speech synthesis failed: {result.Reason}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Speech synthesis error: {ex.Message}");
            throw;
        }
    }
    
    public async IAsyncEnumerable<string> StartContinuousRecognitionAsync()
    {
        var recognitionQueue = new ConcurrentQueue<string>();
        var recognitionComplete = new TaskCompletionSource<bool>();
        
        _recognizer.Recognized += (sender, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                recognitionQueue.Enqueue(e.Result.Text);
            }
        };
        
        _recognizer.SessionStopped += (sender, e) =>
        {
            recognitionComplete.SetResult(true);
        };
        
        await _recognizer.StartContinuousRecognitionAsync();
        
        while (!recognitionComplete.Task.IsCompleted)
        {
            if (recognitionQueue.TryDequeue(out var recognizedText))
            {
                yield return recognizedText;
            }
            
            await Task.Delay(100); // 避免CPU占用过高
        }
        
        await _recognizer.StopContinuousRecognitionAsync();
    }
    
    private string GenerateSSML(string text, VoiceSettings voiceSettings)
    {
        var settings = voiceSettings ?? new VoiceSettings();
        
        return $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{_config.Language}'>
    <voice name='{settings.VoiceName ?? _config.VoiceName}'>
        <prosody rate='{settings.Rate ?? _config.SpeechRate}' pitch='{settings.Pitch ?? _config.Pitch}'>
            {System.Security.SecurityElement.Escape(text)}
        </prosody>
    </voice>
</speak>";
    }
}
```

### 实时语音处理

```csharp
// 实时语音处理器
public class RealTimeSpeechProcessor : MonoBehaviour
{
    [SerializeField] private float _silenceThreshold = 0.01f;
    [SerializeField] private float _silenceDuration = 2.0f;
    [SerializeField] private int _sampleRate = 16000;
    
    private AudioSource _audioSource;
    private ISpeechService _speechService;
    private bool _isRecording;
    private float _lastSoundTime;
    private List<float> _audioBuffer;
    
    public event Action<string> OnSpeechRecognized;
    public event Action OnRecordingStarted;
    public event Action OnRecordingStopped;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _speechService = ServiceContainer.Instance.GetService<ISpeechService>();
        _audioBuffer = new List<float>();
        
        StartListening();
    }
    
    public void StartListening()
    {
        if (!Microphone.IsRecording(null))
        {
            _audioSource.clip = Microphone.Start(null, true, 10, _sampleRate);
            _audioSource.loop = true;
            
            while (!(Microphone.GetPosition(null) > 0)) { }
            
            _audioSource.Play();
            _isRecording = true;
            
            StartCoroutine(ProcessAudioStream());
        }
    }
    
    public void StopListening()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            _audioSource.Stop();
            _isRecording = false;
        }
    }
    
    private IEnumerator ProcessAudioStream()
    {
        var samples = new float[1024];
        var lastPosition = 0;
        
        while (_isRecording)
        {
            var currentPosition = Microphone.GetPosition(null);
            
            if (currentPosition < lastPosition)
            {
                // 处理环形缓冲区的回绕
                var samplesToEnd = _audioSource.clip.samples - lastPosition;
                _audioSource.clip.GetData(samples, lastPosition);
                ProcessAudioSamples(samples, samplesToEnd);
                
                lastPosition = 0;
            }
            
            if (currentPosition > lastPosition)
            {
                var sampleCount = currentPosition - lastPosition;
                _audioSource.clip.GetData(samples, lastPosition);
                ProcessAudioSamples(samples, sampleCount);
                
                lastPosition = currentPosition;
            }
            
            yield return null;
        }
    }
    
    private void ProcessAudioSamples(float[] samples, int sampleCount)
    {
        var hasSound = false;
        
        for (int i = 0; i < sampleCount; i++)
        {
            var sample = Mathf.Abs(samples[i]);
            _audioBuffer.Add(samples[i]);
            
            if (sample > _silenceThreshold)
            {
                hasSound = true;
                _lastSoundTime = Time.time;
            }
        }
        
        if (hasSound && !_isRecording)
        {
            OnRecordingStarted?.Invoke();
            _isRecording = true;
        }
        else if (_isRecording && Time.time - _lastSoundTime > _silenceDuration)
        {
            ProcessRecordedAudio();
        }
    }
    
    private async void ProcessRecordedAudio()
    {
        OnRecordingStopped?.Invoke();
        
        if (_audioBuffer.Count > 0)
        {
            var audioClip = CreateAudioClipFromBuffer();
            var recognizedText = await _speechService.RecognizeSpeechAsync(audioClip);
            
            if (!string.IsNullOrEmpty(recognizedText))
            {
                OnSpeechRecognized?.Invoke(recognizedText);
            }
        }
        
        _audioBuffer.Clear();
        _isRecording = false;
    }
    
    private AudioClip CreateAudioClipFromBuffer()
    {
        var audioClip = AudioClip.Create("RecordedAudio", _audioBuffer.Count, 1, _sampleRate, false);
        audioClip.SetData(_audioBuffer.ToArray(), 0);
        return audioClip;
    }
}
```

## 👁️ 计算机视觉集成

### Azure Computer Vision

```csharp
// 计算机视觉服务
public class AzureVisionService : IVisionService
{
    private readonly string _subscriptionKey;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    
    public AzureVisionService(string subscriptionKey, string endpoint, ILogger logger)
    {
        _subscriptionKey = subscriptionKey;
        _endpoint = endpoint;
        _logger = logger;
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
    }
    
    public async Task<VisionAnalysisResult> AnalyzeImageAsync(Texture2D image, VisionFeatures features = VisionFeatures.All)
    {
        try
        {
            var imageBytes = image.EncodeToPNG();
            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
            var featuresParam = BuildFeaturesParameter(features);
            var url = $"{_endpoint}/vision/v3.2/analyze?visualFeatures={featuresParam}";
            
            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AzureVisionResponse>(responseJson);
                return ConvertToVisionResult(result);
            }
            else
            {
                _logger.LogError($"Vision API error: {response.StatusCode} - {responseJson}");
                return new VisionAnalysisResult { Success = false, ErrorMessage = responseJson };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Vision analysis error: {ex.Message}");
            return new VisionAnalysisResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    public async Task<List<DetectedObject>> DetectObjectsAsync(Texture2D image)
    {
        try
        {
            var imageBytes = image.EncodeToPNG();
            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
            var url = $"{_endpoint}/vision/v3.2/detect";
            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ObjectDetectionResponse>(responseJson);
                return result.Objects.Select(obj => new DetectedObject
                {
                    Name = obj.ObjectProperty,
                    Confidence = obj.Confidence,
                    BoundingBox = new BoundingBox
                    {
                        X = obj.Rectangle.X,
                        Y = obj.Rectangle.Y,
                        Width = obj.Rectangle.W,
                        Height = obj.Rectangle.H
                    }
                }).ToList();
            }
            else
            {
                _logger.LogError($"Object detection error: {response.StatusCode} - {responseJson}");
                return new List<DetectedObject>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Object detection error: {ex.Message}");
            return new List<DetectedObject>();
        }
    }
    
    public async Task<string> GenerateImageDescriptionAsync(Texture2D image, string language = "zh")
    {
        try
        {
            var imageBytes = image.EncodeToPNG();
            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
            var url = $"{_endpoint}/vision/v3.2/describe?maxCandidates=1&language={language}";
            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ImageDescriptionResponse>(responseJson);
                return result.Description.Captions.FirstOrDefault()?.Text ?? "无法生成描述";
            }
            else
            {
                _logger.LogError($"Image description error: {response.StatusCode} - {responseJson}");
                return "描述生成失败";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Image description error: {ex.Message}");
            return "描述生成异常";
        }
    }
}
```

### 场景理解系统

```csharp
// 场景理解服务
public class SceneUnderstandingService : ISceneUnderstandingService
{
    private readonly IVisionService _visionService;
    private readonly IGPTService _gptService;
    private readonly ILogger _logger;
    
    public async Task<SceneAnalysis> AnalyzeSceneAsync(Texture2D sceneImage, Vector3 userPosition, Vector3 userDirection)
    {
        var analysis = new SceneAnalysis
        {
            Timestamp = DateTime.UtcNow,
            UserPosition = userPosition,
            UserDirection = userDirection
        };
        
        try
        {
            // 1. 基础视觉分析
            var visionResult = await _visionService.AnalyzeImageAsync(sceneImage, 
                VisionFeatures.Objects | VisionFeatures.Categories | VisionFeatures.Description);
            
            analysis.DetectedObjects = visionResult.Objects;
            analysis.Categories = visionResult.Categories;
            analysis.Description = visionResult.Description;
            
            // 2. 深度场景理解
            var contextPrompt = GenerateSceneContextPrompt(visionResult);
            var sceneContext = await _gptService.GenerateResponseAsync(contextPrompt);
            analysis.ContextualDescription = sceneContext;
            
            // 3. 兴趣点识别
            analysis.PointsOfInterest = await IdentifyPointsOfInterestAsync(visionResult, userPosition);
            
            // 4. 导航建议
            analysis.NavigationSuggestions = await GenerateNavigationSuggestionsAsync(analysis);
            
            // 5. 安全评估
            analysis.SafetyAssessment = await AssessSceneSafetyAsync(visionResult);
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Scene analysis error: {ex.Message}");
            analysis.ErrorMessage = ex.Message;
            return analysis;
        }
    }
    
    private string GenerateSceneContextPrompt(VisionAnalysisResult visionResult)
    {
        var objectList = string.Join(", ", visionResult.Objects.Select(o => o.Name));
        var categories = string.Join(", ", visionResult.Categories.Select(c => c.Name));
        
        return $@"
基于以下视觉分析结果，请提供详细的场景理解和旅游导览信息：

场景描述：{visionResult.Description}
检测到的物体：{objectList}
场景分类：{categories}

请提供：
1. 场景的历史文化背景
2. 主要看点和特色
3. 最佳观赏角度和时间
4. 相关的有趣故事或传说
5. 摄影建议

回答要专业且有趣，适合作为虚拟导游的解说内容。";
    }
    
    private async Task<List<PointOfInterest>> IdentifyPointsOfInterestAsync(VisionAnalysisResult visionResult, Vector3 userPosition)
    {
        var pointsOfInterest = new List<PointOfInterest>();
        
        foreach (var obj in visionResult.Objects.Where(o => o.Confidence > 0.7))
        {
            var poi = new PointOfInterest
            {
                Name = obj.Name,
                Position = EstimateWorldPosition(obj.BoundingBox, userPosition),
                Confidence = obj.Confidence,
                Type = ClassifyObjectType(obj.Name),
                Description = await GenerateObjectDescriptionAsync(obj.Name)
            };
            
            pointsOfInterest.Add(poi);
        }
        
        return pointsOfInterest;
    }
    
    private Vector3 EstimateWorldPosition(BoundingBox boundingBox, Vector3 userPosition)
    {
        // 简化的3D位置估算，实际应用中需要更复杂的算法
        var screenCenter = new Vector2(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
        var worldDirection = Camera.main.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, 10f));
        
        return userPosition + worldDirection.normalized * 5f; // 假设距离5米
    }
}
```

## 🎯 推荐系统

### 协同过滤推荐

```csharp
// 推荐引擎
public class RecommendationEngine : IRecommendationEngine
{
    private readonly IUserBehaviorService _behaviorService;
    private readonly IContentService _contentService;
    private readonly IMLModelService _mlService;
    private readonly ILogger _logger;
    
    public async Task<List<Recommendation>> GetRecommendationsAsync(string userId, RecommendationContext context)
    {
        try
        {
            var userProfile = await BuildUserProfileAsync(userId);
            var candidates = await GetCandidateItemsAsync(context);
            
            var recommendations = new List<Recommendation>();
            
            // 1. 协同过滤推荐
            var collaborativeRecs = await GetCollaborativeRecommendationsAsync(userProfile, candidates);
            recommendations.AddRange(collaborativeRecs);
            
            // 2. 基于内容的推荐
            var contentBasedRecs = await GetContentBasedRecommendationsAsync(userProfile, candidates);
            recommendations.AddRange(contentBasedRecs);
            
            // 3. 深度学习推荐
            var deepLearningRecs = await GetDeepLearningRecommendationsAsync(userProfile, candidates, context);
            recommendations.AddRange(deepLearningRecs);
            
            // 4. 混合推荐和排序
            var finalRecommendations = await HybridRankingAsync(recommendations, userProfile, context);
            
            return finalRecommendations.Take(10).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Recommendation generation error: {ex.Message}");
            return new List<Recommendation>();
        }
    }
    
    private async Task<UserProfile> BuildUserProfileAsync(string userId)
    {
        var behaviors = await _behaviorService.GetUserBehaviorsAsync(userId);
        var preferences = ExtractPreferences(behaviors);
        
        return new UserProfile
        {
            UserId = userId,
            Preferences = preferences,
            VisitHistory = behaviors.Where(b => b.Type == BehaviorType.Visit).ToList(),
            Ratings = behaviors.Where(b => b.Type == BehaviorType.Rating).ToList(),
            Demographics = await GetUserDemographicsAsync(userId)
        };
    }
    
    private async Task<List<Recommendation>> GetCollaborativeRecommendationsAsync(UserProfile userProfile, List<ContentItem> candidates)
    {
        // 用户-物品协同过滤
        var similarUsers = await FindSimilarUsersAsync(userProfile);
        var recommendations = new List<Recommendation>();
        
        foreach (var candidate in candidates)
        {
            var score = CalculateCollaborativeScore(candidate, similarUsers);
            if (score > 0.5) // 阈值过滤
            {
                recommendations.Add(new Recommendation
                {
                    ItemId = candidate.Id,
                    Score = score,
                    Reason = "基于相似用户的喜好推荐",
                    Algorithm = "CollaborativeFiltering"
                });
            }
        }
        
        return recommendations;
    }
    
    private async Task<List<Recommendation>> GetContentBasedRecommendationsAsync(UserProfile userProfile, List<ContentItem> candidates)
    {
        var recommendations = new List<Recommendation>();
        
        foreach (var candidate in candidates)
        {
            var score = CalculateContentSimilarity(candidate, userProfile.Preferences);
            if (score > 0.6)
            {
                recommendations.Add(new Recommendation
                {
                    ItemId = candidate.Id,
                    Score = score,
                    Reason = $"因为您喜欢{GetTopPreference(userProfile.Preferences)}",
                    Algorithm = "ContentBased"
                });
            }
        }
        
        return recommendations;
    }
    
    private async Task<List<Recommendation>> GetDeepLearningRecommendationsAsync(UserProfile userProfile, List<ContentItem> candidates, RecommendationContext context)
    {
        // 使用深度学习模型进行推荐
        var features = BuildFeatureVector(userProfile, context);
        var predictions = await _mlService.PredictAsync("recommendation_model", features);
        
        var recommendations = new List<Recommendation>();
        
        for (int i = 0; i < candidates.Count && i < predictions.Length; i++)
        {
            if (predictions[i] > 0.7)
            {
                recommendations.Add(new Recommendation
                {
                    ItemId = candidates[i].Id,
                    Score = predictions[i],
                    Reason = "AI智能推荐",
                    Algorithm = "DeepLearning"
                });
            }
        }
        
        return recommendations;
    }
    
    private async Task<List<Recommendation>> HybridRankingAsync(List<Recommendation> recommendations, UserProfile userProfile, RecommendationContext context)
    {
        // 混合推荐算法权重
        var weights = new Dictionary<string, float>
        {
            ["CollaborativeFiltering"] = 0.4f,
            ["ContentBased"] = 0.3f,
            ["DeepLearning"] = 0.3f
        };
        
        // 按算法分组并计算加权分数
        var groupedRecs = recommendations.GroupBy(r => r.ItemId);
        var hybridRecommendations = new List<Recommendation>();
        
        foreach (var group in groupedRecs)
        {
            var weightedScore = group.Sum(r => r.Score * weights.GetValueOrDefault(r.Algorithm, 0.1f));
            var bestRec = group.OrderByDescending(r => r.Score).First();
            
            bestRec.Score = weightedScore;
            bestRec.Algorithm = "Hybrid";
            
            hybridRecommendations.Add(bestRec);
        }
        
        // 考虑上下文因素调整排序
        foreach (var rec in hybridRecommendations)
        {
            rec.Score *= CalculateContextualBoost(rec, context);
        }
        
        return hybridRecommendations.OrderByDescending(r => r.Score).ToList();
    }
}
```

## 🔧 AI服务管理

### 服务健康监控

```csharp
// AI服务健康监控
public class AIServiceHealthMonitor : MonoBehaviour
{
    [SerializeField] private float _checkInterval = 30f;
    [SerializeField] private int _maxFailures = 3;
    
    private readonly Dictionary<string, ServiceHealthStatus> _serviceStatus;
    private readonly Dictionary<string, int> _failureCounts;
    
    public event Action<string, ServiceHealthStatus> OnServiceStatusChanged;
    
    private void Start()
    {
        _serviceStatus = new Dictionary<string, ServiceHealthStatus>();
        _failureCounts = new Dictionary<string, int>();
        
        InvokeRepeating(nameof(CheckAllServices), 0f, _checkInterval);
    }
    
    private async void CheckAllServices()
    {
        var services = new[]
        {
            "GPTService",
            "SpeechService", 
            "VisionService",
            "RecommendationEngine"
        };
        
        foreach (var serviceName in services)
        {
            await CheckServiceHealthAsync(serviceName);
        }
    }
    
    private async Task CheckServiceHealthAsync(string serviceName)
    {
        try
        {
            var service = ServiceContainer.Instance.GetService<IHealthCheckable>(serviceName);
            var healthResult = await service.CheckHealthAsync();
            
            var previousStatus = _serviceStatus.GetValueOrDefault(serviceName, ServiceHealthStatus.Unknown);
            var currentStatus = healthResult.IsHealthy ? ServiceHealthStatus.Healthy : ServiceHealthStatus.Unhealthy;
            
            if (currentStatus != previousStatus)
            {
                _serviceStatus[serviceName] = currentStatus;
                OnServiceStatusChanged?.Invoke(serviceName, currentStatus);
                
                if (currentStatus == ServiceHealthStatus.Healthy)
                {
                    _failureCounts[serviceName] = 0;
                    Debug.Log($"Service {serviceName} is now healthy");
                }
                else
                {
                    _failureCounts[serviceName] = _failureCounts.GetValueOrDefault(serviceName, 0) + 1;
                    Debug.LogWarning($"Service {serviceName} is unhealthy: {healthResult.ErrorMessage}");
                    
                    if (_failureCounts[serviceName] >= _maxFailures)
                    {
                        await HandleServiceFailureAsync(serviceName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Health check failed for {serviceName}: {ex.Message}");
            _serviceStatus[serviceName] = ServiceHealthStatus.Error;
            OnServiceStatusChanged?.Invoke(serviceName, ServiceHealthStatus.Error);
        }
    }
    
    private async Task HandleServiceFailureAsync(string serviceName)
    {
        Debug.LogError($"Service {serviceName} has failed {_maxFailures} times. Attempting recovery...");
        
        try
        {
            // 尝试重启服务
            var service = ServiceContainer.Instance.GetService<IRestartable>(serviceName);
            await service.RestartAsync();
            
            Debug.Log($"Service {serviceName} restart attempted");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to restart service {serviceName}: {ex.Message}");
            
            // 启用降级模式
            EnableFallbackMode(serviceName);
        }
    }
    
    private void EnableFallbackMode(string serviceName)
    {
        switch (serviceName)
        {
            case "GPTService":
                // 使用本地缓存的回答或简化回复
                EnableGPTFallback();
                break;
            case "SpeechService":
                // 使用文本显示替代语音
                EnableSpeechFallback();
                break;
            case "VisionService":
                // 禁用视觉分析功能
                EnableVisionFallback();
                break;
        }
        
        Debug.Log($"Fallback mode enabled for {serviceName}");
    }
}
```

### 性能优化和缓存

```csharp
// AI响应缓存系统
public class AIResponseCache : IAIResponseCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger _logger;
    
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        // 1. 检查内存缓存
        if (_memoryCache.TryGetValue(key, out T cachedValue))
        {
            return cachedValue;
        }
        
        // 2. 检查分布式缓存
        var distributedValue = await _distributedCache.GetAsync<T>(key);
        if (distributedValue != null)
        {
            // 回填内存缓存
            _memoryCache.Set(key, distributedValue, TimeSpan.FromMinutes(5));
            return distributedValue;
        }
        
        // 3. 执行原始操作
        var result = await factory();
        
        // 4. 缓存结果
        var cacheExpiration = expiration ?? TimeSpan.FromHours(1);
        _memoryCache.Set(key, result, TimeSpan.FromMinutes(5));
        await _distributedCache.SetAsync(key, result, cacheExpiration);
        
        return result;
    }
    
    public string GenerateCacheKey(string operation, params object[] parameters)
    {
        var keyBuilder = new StringBuilder(operation);
        
        foreach (var param in parameters)
        {
            keyBuilder.Append(":");
            keyBuilder.Append(param?.ToString() ?? "null");
        }
        
        // 生成哈希以避免键过长
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyBuilder.ToString()));
        return Convert.ToBase64String(hashBytes);
    }
}

// 批量请求优化
public class BatchRequestOptimizer : IBatchRequestOptimizer
{
    private readonly Dictionary<string, List<BatchRequest>> _pendingRequests;
    private readonly Timer _batchTimer;
    
    public async Task<T> AddToBatchAsync<T>(string batchKey, Func<List<object>, Task<List<T>>> batchProcessor, object request)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        
        var batchRequest = new BatchRequest
        {
            Request = request,
            CompletionSource = taskCompletionSource
        };
        
        if (!_pendingRequests.ContainsKey(batchKey))
        {
            _pendingRequests[batchKey] = new List<BatchRequest>();
        }
        
        _pendingRequests[batchKey].Add(batchRequest);
        
        // 如果批次已满或超时，立即处理
        if (_pendingRequests[batchKey].Count >= 10)
        {
            await ProcessBatchAsync(batchKey, batchProcessor);
        }
        
        return await taskCompletionSource.Task;
    }
    
    private async Task ProcessBatchAsync<T>(string batchKey, Func<List<object>, Task<List<T>>> batchProcessor)
    {
        if (!_pendingRequests.TryGetValue(batchKey, out var requests) || requests.Count == 0)
            return;
            
        _pendingRequests[batchKey] = new List<BatchRequest>();
        
        try
        {
            var requestObjects = requests.Select(r => r.Request).ToList();
            var results = await batchProcessor(requestObjects);
            
            for (int i = 0; i < requests.Count && i < results.Count; i++)
            {
                ((TaskCompletionSource<T>)requests[i].CompletionSource).SetResult(results[i]);
            }
        }
        catch (Exception ex)
        {
            foreach (var request in requests)
            {
                request.CompletionSource.SetException(ex);
            }
        }
    }
}
```

---

*本AI集成指南会随着新技术和服务的引入持续更新。*