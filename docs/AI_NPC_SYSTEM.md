# AI NPC System - Technical Documentation

## Overview

The NPC (Non-Player Character) AI system enables multiple intelligent characters in the TripMeta VR world, each with unique personalities and the ability to engage in natural conversations powered by LLM.

## Architecture

### Core Components

```
TripMeta.AI.NPC/
├── NPCPersonality.cs          # Character configuration (ScriptableObject)
├── NPCAIController.cs         # Per-NPC AI controller
├── NPCDialogueManager.cs      # Global dialogue manager
├── NPCBehaviorTree.cs         # Behavior state machine
└── AIServiceInstaller.cs      # DI service registration
```

### Component Responsibilities

#### 1. NPCPersonality (ScriptableObject)
- **Purpose**: Define NPC character traits and behavior
- **Configuration**:
  - `npcName`: Character name
  - `role`: TourGuide | Merchant | Resident | Scholar | Storyteller | Guardian
  - `systemPrompt`: LLM system instruction
  - `personalityTraits[]`: Character tags (friendly, knowledgeable, etc.)
  - `knowledgeDomains[]`: Expertise areas (history, culture, art, etc.)
  - `voiceId`: Azure TTS voice identifier
  - `triggerDistance`: Player detection radius (meters)
  - `conversationDistance`: Interaction radius
  - `enablePatrol`: Enable autonomous movement
  - `patrolWaypoints[]`: Patrol path points

#### 2. NPCAIController (MonoBehaviour)
- **Purpose**: Manage single NPC's AI behavior and conversations
- **Features**:
  - Independent conversation history per NPC
  - Player proximity detection
  - Auto-greeting when player enters range
  - Streaming LLM responses with callbacks
  - Speech synthesis via Azure TTS
  - State management (Idle, Patrol, Greeting, Conversing, Farewell)

#### 3. NPCDialogueManager (Singleton)
- **Purpose**: Global coordination of all NPC dialogues
- **Features**:
  - Request queue with priority
  - Concurrent request control (max 3 simultaneous)
  - Token usage tracking (per-minute, per-hour)
  - Rate limiting and retry logic
  - Conversation history persistence
  - NPC registration and lifecycle management

#### 4. NPCBehaviorTree
- **Purpose**: Control NPC autonomous behaviors
- **States**:
  - `Idle`: Standing still, random idle animations
  - `Patrol`: Moving between waypoints
  - `Greeting`: Welcoming player with wave animation
  - `Conversing`: Facing player, talking animation
  - `Farewell`: Waving goodbye
  - `Thinking`: Processing LLM request

## Usage

### Creating a New NPC

1. **Create Personality Asset**
   ```
   Unity Menu → Assets → Create → TripMeta → NPC → Personality
   ```

2. **Configure Personality**
   ```csharp
   // Example: Tour Guide
   npcName = "Guide Alice"
   role = NPCRole.TourGuide
   systemPrompt = "You are a friendly tour guide..."
   personalityTraits = ["friendly", "knowledgeable"]
   knowledgeDomains = ["history", "architecture"]
   voiceId = "zh-CN-XiaoxiaoNeural"
   triggerDistance = 3.0f
   autoGreeting = true
   ```

3. **Add to Scene**
   ```csharp
   GameObject npc = new GameObject("NPC_Guide");
   NPCAIController controller = npc.AddComponent<NPCAIController>();
   controller.personality = guidePersonalityAsset;
   ```

### Triggering Conversations

```csharp
// Get NPC controller
var npc = GetComponent<NPCAIController>();

// Start conversation
string response = await npc.StartConversation("Tell me about this place");

// Stream conversation
await npc.StartStreamConversation("What's the history?", (partialResponse) => {
    Debug.Log($"Partial: {partialResponse}");
});
```

### Dialogue Manager API

```csharp
var manager = NPCDialogueManager.Instance;

// Submit dialogue request
var request = new NPCDialogueRequest {
    npcId = "guide_alice",
    message = "Hello!",
    priority = 0
};

var response = await manager.SubmitDialogueRequest(request);

if (response.success) {
    Debug.Log($"NPC replied: {response.response}");
}

// Check token usage
var stats = manager.GetTokenUsageStats();
Debug.Log($"Token usage: {stats.minuteUsagePercent}% (per minute)");
```

## LLM Configuration

### OpenAI GPT-4o (Default)
```csharp
var config = new GPTConfig {
    apiKey = "sk-...",
    model = "gpt-4o",
    maxTokens = 2000,
    temperature = 0.7f,
    enableFallback = true
};
```

### Ollama Fallback
```csharp
var gptService = ServiceLocator.Get<IGPTService>();
gptService.SetOllamaConfig(
    "http://localhost:11434/api/generate",
    "llama3.2"
);

// Enable fallback mode
gptService.SetUseOllama(true);  // Manual switch
// OR automatic failover when OpenAI fails
```

## Streaming Responses

### OpenAI SSE Streaming
```csharp
await gptService.SendStreamChatAsync(
    message: "Tell me a story",
    onPartialResponse: (partial) => {
        UpdateUI(partial);  // Real-time UI update
    },
    conversationId: npcId
);
```

### Ollama Streaming
```csharp
// Automatically uses streaming when Ollama mode enabled
// Same API as OpenAI
```

## Token Management

### Tracking
- Per-minute usage: Default 90,000 tokens
- Per-hour usage: Default 90,000 tokens
- Automatic cleanup of expired tracking data

### Rate Limiting
```csharp
// Check before request
if (tokenTracker.IsLimitExceeded()) {
    return "Rate limit exceeded. Try again later.";
}
```

### Monitoring
```csharp
var stats = manager.GetTokenUsageStats();
// stats.currentMinuteUsage
// stats.minuteUsagePercent
// stats.currentHourUsage
// stats.hourUsagePercent
```

## Behavior Tree

### Custom Behaviors

```csharp
// Add custom behavior node
var customNode = new ActionNode(() => {
    // Custom logic
    return BehaviorStatus.Success;
});

behaviorTree.AddNode(customNode);
```

### Patrol Configuration
```csharp
personality.enablePatrol = true;
personality.patrolSpeed = 1.0f;
personality.waypointWaitTime = 5.0f;
personality.patrolWaypoints = new Vector3[] {
    new Vector3(0, 0, 0),
    new Vector3(10, 0, 0),
    new Vector3(10, 0, 10),
    new Vector3(0, 0, 10)
};
```

## Performance Optimization

### Request Queue
- Max concurrent requests: 3 (configurable)
- Request timeout: 15 seconds
- Auto-retry with exponential backoff

### Conversation History
- Max history length: 20 messages per NPC
- Auto-trim old messages
- Persistence to disk (optional)

### Memory Management
- Conversation cache cleanup
- Token tracker cleanup
- Automatic unregistration on NPC destroy

## Integration with Existing Systems

### DI Container
```csharp
// Install AI services
AIServiceInstaller.InstallServices(container);
AIServiceInstaller.InstallNPCServices(container);
```

### Service Locator
```csharp
var gptService = ServiceLocator.Get<IGPTService>();
var speechService = ServiceLocator.Get<IAzureSpeechService>();
var dialogueManager = NPCDialogueManager.Instance;
```

## Troubleshooting

### NPC Not Responding
1. Check `NPCPersonality` is assigned
2. Verify GPT service is initialized
3. Check token limits not exceeded
4. Review console for errors

### Streaming Not Working
1. Ensure `useStreamingResponse = true` in personality
2. Check OpenAI API supports streaming
3. Verify network connectivity
4. Test Ollama fallback

### Token Limit Exceeded
1. Check `GetTokenUsageStats()`
2. Wait for rate limit window to reset
3. Enable Ollama fallback
4. Reduce max tokens per request

## Future Enhancements

- [ ] Multi-turn conversation context
- [ ] Emotion detection and response
- [ ] Gesture synthesis during speech
- [ ] Long-term memory (RAG integration)
- [ ] Multi-language auto-translation
- [ ] NPC-to-NPC conversations
- [ ] Crowd simulation with AI agents

---

**Documentation Version**: 1.0  
**Last Updated**: 2026-03-16  
**Author**: OpenClaw Coder Agent
