# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TripMeta is an AI-driven VR tourism platform built with Unity 2021.3.11f1. It integrates multiple AI services (OpenAI GPT, Azure Speech, Computer Vision, Recommendations) with PICO VR headsets to create immersive virtual tourism experiences with intelligent AI tour guides.

## Project Structure

```
Assets/Scripts/
├── AI/              - AI services (GPT, Speech, Vision, Recommendations)
│   ├── Core/        - AI service manager and interfaces
│   ├── Services/    - Concrete AI service implementations
│   └── Models/      - AI data models and DTOs
├── Core/            - Infrastructure (DI, Config, Error Handling, Performance)
│   ├── Bootstrap/   - Application initialization (ApplicationBootstrap)
│   ├── Configuration/ - ScriptableObject-based configuration
│   ├── DependencyInjection/ - Custom DI container
│   └── ErrorHandling/ - Global error handling and logging
├── Features/        - Business features (Tour Guide, Social, Analytics)
├── Infrastructure/  - Cross-cutting concerns (Network, Cache, Resources)
├── Interaction/     - VR input handling and gesture recognition
├── Presentation/    - UI and UX components
├── VR/              - VR-specific functionality (PICO integration)
│   ├── Interaction/ - VR interaction components
│   └── Performance/ - VR performance optimization
├── Tests/           - Unit and integration tests
└── Editor/          - Unity editor tools
```

## Development Commands

### Unity Project
- **Open Project**: Launch Unity Hub and open this directory (requires Unity 2021.3.11f1)
- **Main Scenes**: `Assets/NewYork.unity` or `Assets/Scenes/MainScene.unity`
- **Build**: Use Unity Build Settings (File > Build Settings) - target platform is Android for PICO
- **Package Installation**: Dependencies auto-install via `Packages/manifest.json`

### Testing
- **Run Tests**: Unity Test Runner (Window > General > Test Runner)
- **Unit Tests**: Located in `Assets/Scripts/Tests/`
- **Test Framework**: NUnit via `com.unity.test-framework`

### Key Unity Packages (from manifest.json)
- `com.unity.xr.interaction.toolkit@3.0.0` - VR interactions
- `com.unity.render-pipelines.universal@17.0.3` - URP rendering (URP, not Built-in RP)
- `com.unity.inputsystem@1.7.0` - New Input System
- `com.unity.addressables@1.22.3` - Asset management
- `com.unity.ml-agents@2.0.1` - ML agents framework
- `com.unity.netcode.gameobjects@1.12.0` - Multiplayer networking
- `com.unity.ai.navigation@1.1.6` - AI navigation

## Architecture Patterns

### Application Bootstrap
The application initializes through `ApplicationBootstrap` (in `Core/Bootstrap/`):
1. Validates configuration
2. Initializes service container (DI)
3. Registers core and application services via `ServiceInstaller`
4. Initializes error handling
5. Initializes VR system
6. Initializes AI services via `AIServiceManager`
7. Loads resources
8. Completes initialization

Progress events are fired: `OnInitializationProgress`, `OnInitializationComplete`, `OnInitializationError`

### Dependency Injection
Custom DI container in `Core/DependencyInjection/`:
- `IServiceContainer` - Main container interface with Singleton/Transient/Scoped lifetimes
- `ServiceInstaller` - Centralized service registration
- `ServiceLocator` - Service location pattern for resolving services
- Services registered via `ServiceInstaller.InstallServices(serviceContainer, config)`

### AI Service Architecture
`AIServiceManager` (in `AI/AIServiceManager.cs`) orchestrates all AI services:
- **Service Types**: LLM (OpenAI GPT), Speech (Azure), Vision, Recommendation, Translation
- **Request Queue**: Manages concurrent requests (configurable max concurrent)
- **Health Monitoring**: Individual service status tracking with auto-restart capability
- **Event-Driven**: `OnServiceStatusChanged`, `OnAIResponseReceived`, `OnAIError` events

All AI services implement `IAIService` interface with async lifecycle methods:
```csharp
bool IsAvailable { get; }
Task InitializeAsync();
Task<T> ProcessRequestAsync<T>(AIRequest request);
Task PrewarmAsync();
Task ShutdownAsync();
```

### VR System
- **VRControllerManager** (`Interaction/VRControllerManager.cs`): PICO controller input handling using `Unity.XR.PXR`
- PICO SDK Integration: Uses `PXR_Input`, `PXR_Plugin` for controller tracking, haptic feedback
- Controller Events: `OnLeftTriggerChanged`, `OnRightTriggerChanged`, `OnLeftGripChanged`, `OnRightGripChanged`
- VR Performance optimization in `VR/Performance/`
- VR Interaction components in `VR/Interaction/`

### Event System
Event-driven architecture with central event bus:
- Subscribe to events in `Awake()` or `Start()`
- Always unsubscribe in `OnDestroy()` to prevent memory leaks
- Common events: AI responses, VR interactions, errors

### Error Handling
Global error handler via `GlobalExceptionHandler` in `Core/ErrorHandling/`:
- Severity levels: Debug, Info, Warning, Error, Fatal
- Unity log messages forwarded to error handler via `Application.logMessageReceived`
- Use `AdvancedLogger` for structured logging

## Configuration System

Configuration uses ScriptableObjects in `Assets/Scripts/Core/Configuration/`:
- `TripMetaConfig` - Root configuration
- `AppSettings` - Application settings
- AI-specific configs: `OpenAIConfig`, `AzureSpeechConfig`, `VisionConfig`, `RecommendationConfig`

Configuration is loaded at bootstrap and validated before service initialization.

## Development Guidelines

### C# Coding Standards
```csharp
// Classes: PascalCase
public class AIServiceManager { }

// Private fields: _camelCase
private readonly ILogger _logger;
private IServiceContainer _serviceContainer;

// Public properties: PascalCase
public bool IsInitialized { get; private set; }

// Methods: PascalCase
public async Task<bool> InitializeAsync()

// Local variables: camelCase
var config = new AIConfig();

// Constants: PascalCase
private const int MaxRetries = 3;
```

### Unity-Specific Patterns
- MonoBehaviour singletons: Use `DontDestroyOnLoad` with static `Instance`
- Cache component references in `Awake()` to avoid repeated `GetComponent` calls
- Use `[SerializeField]` for private editor-exposed fields
- Use `[Header]` attributes to organize Inspector
- Avoid expensive operations in `Update()` - use coroutines or async/await instead
- Physics operations go in `FixedUpdate()`

### Async/Await Patterns
- All AI services return `Task<T>` for async operations
- Use `CancellationToken` for cancellable operations (especially AI requests)
- Configure `await` contexts properly - most Unity work should return to main thread
- Implement proper cleanup in `OnDestroy()` or `Dispose()`

### Service Registration Pattern
When adding new services:
1. Create interface in appropriate directory
2. Implement class with constructor injection
3. Register in `ServiceInstaller.InstallServices()`
4. Resolve via `ServiceLocator.Resolve<T>()` or constructor injection

## Important Notes

### Platform Target
- **Primary Target**: PICO 4 VR headset (Android platform)
- **Development Platform**: Windows 10/11
- Unity version: 2021.3.11f1 (planned upgrade to 2022.3 LTS - see docs)

### Performance Targets for VR
- Frame Rate: 90 FPS (PICO 4)
- Motion-to-Photon Latency: <20ms
- Memory: <4GB
- Use foveated rendering for PICO devices
- Implement dynamic LOD and occlusion culling

### Critical Constraints
- **Never commit API keys** - Use environment variables or secure configuration
- **Rendering Pipeline**: Universal Render Pipeline (URP) - NOT Built-in RP
- **Input System**: New Input System - NOT legacy input
- **Language**: C# targeting .NET Standard 2.1

### Common Issues
- The project has an ongoing upgrade plan from Unity 2021.3 to 2022.3 (see `Unity_2021.3_→_2023.3_升级执行计划.md`)
- Some packages may have version conflicts during the upgrade process
- AI services have graceful degradation - app continues if some AI services fail

## Common Workflows

### Adding a New AI Service
1. Create interface in `AI/Interfaces/`
2. Implement in `AI/Services/` (must implement `IAIService`)
3. Add configuration class in `Core/Configuration/`
4. Add initialization in `AIServiceManager.InitializeXXXService()`
5. Add to `AIServiceType` enum if needed
6. Write unit tests in `Tests/`

### Adding VR Interaction
1. Use `VRControllerManager` for PICO controller input
2. Subscribe to controller events (`OnLeftTriggerChanged`, etc.)
3. Use `VibrateController()` for haptic feedback
4. Access controller position/rotation via `GetControllerPosition()` / `GetControllerRotation()`
5. Test on actual PICO hardware for accurate behavior

### Performance Profiling
1. Use Unity Profiler (Window > Analysis > Profiler)
2. Check `PerformanceMonitor` component at runtime
3. Look for GC allocations in Deep Profile
4. Test on target PICO hardware for accurate metrics

### Debugging AI Services
- Check `AIServiceManager.serviceStatus` for service health
- Subscribe to `OnAIError` event for error notifications
- Use `AdvancedLogger` for structured logging
- Each service has independent restart capability via `RestartService()`

## Documentation

Comprehensive documentation is available in the `docs/` directory (in Chinese):
- `ARCHITECTURE.md` - Detailed system architecture with diagrams
- `TECH_STACK.md` - Technology stack overview
- `DEVELOPMENT_STANDARDS.md` - Coding standards and Git workflow
- `AI_INTEGRATION.md` - AI service integration guide
- `TESTING_GUIDE.md` - Testing strategies
- `DEPLOYMENT_GUIDE.md` - Build and deployment procedures
- `CONFIGURATION.md` - Configuration management
- `TROUBLESHOOTING.md` - Common issues and solutions

## Upgrade Planning

The project is planned to upgrade from Unity 2021.3 to 2022.3 LTS. See:
- `docs/Unity_2021.3_→_2023.3_升级执行计划.md`
- `docs/UPGRADE_PLAN_2023.md`
- `docs/UPGRADE_CHANGESET_2023.md`

The upgrade involves package version updates and potential breaking changes in XR Interaction Toolkit and URP.
