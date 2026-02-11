# TripMeta 测试指南

## 📋 目录

- [测试概览](#测试概览)
- [测试策略](#测试策略)
- [单元测试](#单元测试)
- [集成测试](#集成测试)
- [VR测试](#vr测试)
- [AI服务测试](#ai服务测试)
- [性能测试](#性能测试)
- [自动化测试](#自动化测试)

## 🧪 测试概览

TripMeta采用全面的测试策略，确保VR旅游平台的质量、性能和可靠性。

### 测试金字塔

```
                    E2E Tests (10%)
                 ┌─────────────────┐
                 │   UI Tests      │
                 │   VR Tests      │
                 │   User Journey  │
                 └─────────────────┘
              Integration Tests (20%)
           ┌─────────────────────────┐
           │   API Tests             │
           │   Service Integration   │
           │   Database Tests        │
           │   AI Service Tests      │
           └─────────────────────────┘
        Unit Tests (70%)
    ┌─────────────────────────────────┐
    │   Component Tests               │
    │   Service Tests                 │
    │   Utility Tests                 │
    │   Mock Tests                    │
    └─────────────────────────────────┘
```

## 📊 测试策略

### 测试环境

- **Unit**: 使用内存数据库和模拟服务
- **Integration**: 使用测试数据库和真实服务
- **Staging**: 接近生产环境的完整测试
- **Performance**: 专门的性能测试环境

### 测试数据管理

```csharp
public class TestDataFactory
{
    public static User CreateTestUser(string role = "User")
    {
        return new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = $"testuser_{Random.Shared.Next(1000, 9999)}",
            Email = $"test{Random.Shared.Next(1000, 9999)}@example.com",
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
    
    public static VRSession CreateTestVRSession(string userId = null)
    {
        return new VRSession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId ?? Guid.NewGuid().ToString(),
            StartTime = DateTime.UtcNow,
            DeviceType = "PICO 4",
            SceneId = "test_scene_paris"
        };
    }
}
```

## 🔬 单元测试

### AI服务测试

```csharp
[Fact]
public async Task GenerateResponseAsync_ValidPrompt_ReturnsResponse()
{
    // Arrange
    var prompt = "What is the history of the Eiffel Tower?";
    
    // Act
    var response = await _gptService.GenerateResponseAsync(prompt);
    
    // Assert
    Assert.NotNull(response);
    Assert.NotEmpty(response);
    Assert.Contains("Eiffel Tower", response);
}
```

### VR交互测试

```csharp
[Fact]
public void ProcessHandGesture_PointingGesture_TriggersRaycast()
{
    // Arrange
    var gesture = new HandGesture
    {
        Type = GestureType.Pointing,
        Confidence = 0.9f
    };
    
    // Act
    _interactionManager.ProcessHandGesture(gesture);
    
    // Assert
    Assert.True(raycastTriggered);
}
```

## 🔗 集成测试

### API集成测试

```csharp
[Fact]
public async Task PostAIRequest_ValidRequest_ReturnsResponse()
{
    // Arrange
    var request = new AIRequestDto
    {
        Input = "Tell me about the Colosseum",
        Type = "TourGuide"
    };
    
    // Act
    var response = await Client.PostAsync("/api/ai/chat", content);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var aiResponse = JsonSerializer.Deserialize<AIResponseDto>(responseContent);
    Assert.NotNull(aiResponse);
}
```

## 🥽 VR测试

### VR功能测试

```csharp
[Fact]
public async Task VRScene_LoadScene_LoadsSuccessfully()
{
    // Arrange
    var sceneId = "paris_eiffel_tower";
    
    // Act
    var loadResult = await _vrTestHarness.LoadSceneAsync(sceneId);
    
    // Assert
    Assert.True(loadResult.Success);
    Assert.True(loadResult.LoadTime < TimeSpan.FromSeconds(10));
}
```

## 🤖 AI服务测试

### GPT服务测试

```csharp
[Theory]
[InlineData("Tell me about Paris")]
[InlineData("What can I do in Tokyo?")]
public async Task GenerateResponseAsync_TourGuidePrompts_ReturnsRelevantResponse(string prompt)
{
    // Act
    var response = await _gptService.GenerateResponseAsync(prompt);
    
    // Assert
    Assert.NotNull(response);
    Assert.True(response.Length > 50);
}
```

## ⚡ 性能测试

### 负载测试

```csharp
[Fact]
public async Task VRRendering_ComplexScene_MaintainsFrameRate()
{
    // Arrange
    var sceneConfig = new VRSceneConfiguration
    {
        ObjectCount = 10000,
        TextureQuality = TextureQuality.High
    };
    
    // Act
    var result = await _performanceRunner.RunPerformanceTestAsync(sceneConfig);
    
    // Assert
    Assert.True(result.AverageFrameRate >= 72);
}
```

## 🤖 自动化测试

### CI/CD集成

```yaml
# .github/workflows/test.yml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    - name: Run Tests
      run: dotnet test --logger trx --results-directory TestResults
    - name: Publish Test Results
      uses: dorny/test-reporter@v1
      if: success() || failure()
      with:
        name: Test Results
        path: TestResults/*.trx
        reporter: dotnet-trx
```

### 测试覆盖率

```bash
# 运行测试并生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## 📊 测试报告

### 测试指标

- **代码覆盖率**: 目标 >80%
- **单元测试**: 目标 >90%
- **集成测试**: 目标 >70%
- **性能测试**: 帧率 >72 FPS

### 质量门禁

- 所有测试必须通过
- 代码覆盖率不得低于80%
- 性能测试不得低于基准值
- 安全扫描不得有高危漏洞

---

*测试是确保软件质量的重要环节，应持续改进测试策略和覆盖率。*