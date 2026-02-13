# AI Capabilities in TripMeta

Complete guide to AI features and integration in TripMeta VR tourism platform.

---

## Overview

TripMeta integrates multiple AI services to create an intelligent, conversational tour guide that enhances the virtual tourism experience.

---

## AI Services Architecture

```
┌─────────────────────────────────────────────────────────┐
│                                              │
│  ┌───────────────┐     ┌───────────────┐     │
│  │ GPT-4         │     │ Azure Speech  │     │ Azure Vision  │     │
│  │ (OpenAI)       │     │ (Cognitive)    │     │ (Computer)   │
│  │               │     │               │     │             │
│  │ Conversational │     │ Speech Recognition  │     │ Object        │
│  │ AI             │     │ & TTS          │     │ Detection     │
│  └───────────────┘     └───────────────┘     │
└─────────────────────────────────────────────────────────┘
```

### Service Descriptions

| Service | Provider | Purpose | Integration Point |
|---------|----------|----------------|------------------|
| **GPT-4** | OpenAI | Conversational AI | `AIServiceManager` | Generates natural language responses |
| **Azure Speech** | Microsoft | Speech & Voice | `AzureSpeechService` | Voice recognition and text-to-speech |
| **Azure Vision** | Microsoft | Computer Vision | `VisionService` | Object detection and scene analysis |
| **Recommendations** | Custom | AI Engine | `RecommendationService` | Suggests attractions based on preferences |

---

## Core AI Features

### 1. Natural Language Understanding (NLU)

**Implementation:** `GPT-4 Integration`

The tour guide system can:
- **Understand user intent** - Parse questions like "What's special here?" or "Tell me about history"
- **Provide context-aware responses** - Access knowledge graph for relevant information
- **Handle follow-up questions** - Maintain conversation flow across multiple exchanges
- **Multi-language support** - Process queries in English, Chinese, Japanese

**Example Interactions:**
```
User: "What's the history of this place?"
AI: "Times Square was a gift from France to the US in 1886..."
```

### 2. Knowledge Graph

**Data Structure:**
- Historical facts and dates
- Cultural information and traditions
- Architectural details
- Fun facts and trivia
- Travel tips and recommendations

**Query Types:**
```csharp
// Location-based query
await knowledgeGraph.QueryAsync(locationId, "history");

// Topic-based query
await knowledgeGraph.QueryAsync("architecture");

// Open-ended query
await knowledgeGraph.QueryAsync("interesting facts");
```

**Knowledge Sources:**
- Wikipedia API integration
- Custom tourism database
- Static trip metadata
- Cultural heritage databases

### 3. Voice Interaction

**Azure Speech Service Integration**

**Features:**
- **Speech Recognition**: Convert user speech to text
- **Text-to-Speech**: AI responses spoken aloud
- **Multi-language**: Supported languages for international tourists

**Configuration:**
```csharp
// Azure Speech configuration
public class AzureSpeechConfig : ScriptableObject
{
    public string subscriptionKey;
    public string region = "eastus";
    public Language voiceLanguage = "zh-CN"; // or "en-US"
}
```

**Usage Flow:**
1. User presses VR controller trigger
2. Speech recognition captures voice command
3. Text sent to GPT-4 for processing
4. GPT-4 response returned
5. Response sent to Azure Speech for TTS
6. Audio played through VR headset

### 4. Computer Vision

**Azure Vision Service Integration**

**Use Cases:**
- **Object Detection**: Identify objects in VR environment
- **Scene Analysis**: Understand user's surroundings
- **Text Recognition**: Read signs or displays in VR
- **AR Overlays**: Display information on real-world view

**Implementation:**
```csharp
// Vision service initialization
var visionService = new VisionService(apiKey, region);

// Analyze VR scene
var objects = await visionService.DetectObjectsAsync(sceneImage);

// Recognize text from image
var text = await visionService.RecognizeTextAsync(imageData);
```

### 5. Personalization

**User Preference Storage:**
- Language preference
- Interest areas (history, architecture, food, nature)
- Interaction style (detailed, concise, humorous)
- Accessibility needs

**Implementation:**
```csharp
// UserProfile component
[Serializable]
public class UserProfile
{
    public string UserId;
    public string PreferredLanguage;
    public List<string> InterestTags;
    public ConversationStyle Style;
    public AccessibilitySettings Accessibility;
}
```

**How AI Uses Profile:**
- Adjust response complexity based on `ConversationStyle`
- Suggest content aligned with `InterestTags`
- Provide explanations in `PreferredLanguage`

---

## Integration Patterns

### Adding New AI Service

1. **Create interface** in `AI/Interfaces/`
   ```csharp
   public interface IAIService { ... }
   ```

2. **Implement service class** in `AI/Services/`
   ```csharp
   public class GPTService : IAIService { ... }
   ```

3. **Register in DI container**
   ```csharp
   serviceContainer.Register<IAIService>(new GPTService(config));
   ```

4. **Initialize in `AIServiceManager****
   ```csharp
   await aiServiceManager.InitializeServiceAsync(AIServiceType.GPT);
   ```

### Configuration

**Required Settings:**
- OpenAI API Key (for GPT-4)
- Azure Subscription Keys (for Speech & Vision)
- API endpoints and models
- Rate limits and quotas

**Security:**
```csharp
// NEVER hardcode API keys
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
```

---

## Technical Implementation

### AI Tour Guide Component

**Location:** `Assets/Scripts/Features/TourGuide/AITourGuide.cs`

**Key Methods:**
- `ProcessUserQuery(string query)` - Main entry point
- `GetContextInformation()` - Retrieve location context
- `GenerateResponse(string userInput)` - Create AI response

**Data Flow:**
```
User Input (Voice/Controller)
       ↓
AIServiceManager.ProcessQueryAsync(query)
       ↓
    GPT-4 generates response
       ↓
Azure Speech synthesizes to audio
       ↓
Audio played through VR headset
```

---

## Extending AI Capabilities

### Future Enhancements

| Feature | Description | Priority | Complexity |
|---------|-------------|------------|----------|
| **Sentiment Analysis** | Detect user emotions | Medium | Add emotion detection to responses |
| **Proactive Suggestions** | Anticipate user needs | High | Suggest attractions based on context |
| **Multi-turn Conversations** | Memory across session | Medium | Track conversation history |
| **Image Understanding** | Process user photos | High | Describe visual content |
| **Translation** | Real-time translation | Low | Translate between languages |

### Performance Considerations

- **Latency**: Keep AI responses under 500ms
- **Cost Management**: Optimize API calls to reduce costs
- **Caching**: Cache frequently asked questions
- **Fallback**: Provide cached responses if API fails

---

## Related Documentation

- [Architecture](ARCHITECTURE.md) - System design overview
- [AI_INTEGRATION.md](AI_INTEGRATION.md) - Integration guide
- [Configuration](CONFIGURATION.md) - Settings management

---

**Last Updated:** 2025-02-12
