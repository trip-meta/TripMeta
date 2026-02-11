# TripMeta 配置参考手册

## 📋 目录

- [配置概览](#配置概览)
- [环境配置](#环境配置)
- [AI服务配置](#ai服务配置)
- [VR设备配置](#vr设备配置)
- [性能配置](#性能配置)
- [网络配置](#网络配置)
- [安全配置](#安全配置)
- [日志配置](#日志配置)

## ⚙️ 配置概览

TripMeta使用分层配置系统，支持多种配置源和环境特定配置。

### 配置优先级

```
1. 命令行参数 (最高优先级)
2. 环境变量
3. appsettings.{Environment}.json
4. appsettings.json
5. 默认配置 (最低优先级)
```

### 配置文件结构

```json
{
  "Environment": "Development",
  "Logging": { ... },
  "AI": { ... },
  "VR": { ... },
  "Performance": { ... },
  "Network": { ... },
  "Security": { ... }
}
```

## 🌍 环境配置

### 环境类型

#### Development（开发环境）
```json
{
  "Environment": "Development",
  "Debug": {
    "EnableDebugUI": true,
    "ShowPerformanceMetrics": true,
    "EnableHotReload": true,
    "LogLevel": "Debug"
  },
  "AI": {
    "UseMockServices": true,
    "EnableCaching": false
  }
}
```

#### Staging（测试环境）
```json
{
  "Environment": "Staging",
  "Debug": {
    "EnableDebugUI": false,
    "ShowPerformanceMetrics": true,
    "EnableHotReload": false,
    "LogLevel": "Information"
  },
  "AI": {
    "UseMockServices": false,
    "EnableCaching": true
  }
}
```

#### Production（生产环境）
```json
{
  "Environment": "Production",
  "Debug": {
    "EnableDebugUI": false,
    "ShowPerformanceMetrics": false,
    "EnableHotReload": false,
    "LogLevel": "Warning"
  },
  "AI": {
    "UseMockServices": false,
    "EnableCaching": true,
    "EnableRateLimiting": true
  }
}
```

### 环境变量

```bash
# 基础环境配置
TRIPMETA_ENVIRONMENT=Production
TRIPMETA_LOG_LEVEL=Information
TRIPMETA_DEBUG_MODE=false

# AI服务配置
OPENAI_API_KEY=your_openai_api_key
AZURE_SPEECH_KEY=your_azure_speech_key
AZURE_SPEECH_REGION=eastus
AZURE_VISION_KEY=your_azure_vision_key
AZURE_VISION_ENDPOINT=https://your-vision-service.cognitiveservices.azure.com/

# 数据库配置
DATABASE_CONNECTION_STRING=your_database_connection_string
REDIS_CONNECTION_STRING=your_redis_connection_string

# 网络配置
# API_BASE_URL=https://api.your-domain.com  # Replace with your API URL
# CDN_BASE_URL=https://cdn.your-domain.com   # Replace with your CDN URL
```

## 🤖 AI服务配置

### GPT服务配置

```json
{
  "AI": {
    "GPT": {
      "ApiKey": "${OPENAI_API_KEY}",
      "BaseUrl": "https://api.openai.com/v1",
      "Model": "gpt-4",
      "MaxTokens": 2048,
      "Temperature": 0.7,
      "TopP": 1.0,
      "FrequencyPenalty": 0.0,
      "PresencePenalty": 0.0,
      "MaxRetries": 3,
      "TimeoutSeconds": 30,
      "EnableStreaming": true,
      "RateLimiting": {
        "RequestsPerMinute": 60,
        "TokensPerMinute": 90000
      },
      "SystemPrompts": {
        "TourGuide": "你是一位专业的虚拟导游...",
        "ContentGenerator": "请为以下旅游景点生成..."
      }
    }
  }
}
```

### 语音服务配置

```json
{
  "AI": {
    "Speech": {
      "Azure": {
        "SubscriptionKey": "${AZURE_SPEECH_KEY}",
        "Region": "${AZURE_SPEECH_REGION}",
        "Language": "zh-CN",
        "VoiceName": "zh-CN-XiaoxiaoNeural",
        "SpeechRate": 1.0,
        "Pitch": 0.0,
        "SampleRate": 16000,
        "AudioFormat": "Wav"
      },
      "Recognition": {
        "ContinuousRecognition": true,
        "SilenceTimeout": 2000,
        "NoiseReduction": true,
        "AutoLanguageDetection": true
      },
      "Synthesis": {
        "EnableSSML": true,
        "CacheResponses": true,
        "CompressionEnabled": true
      }
    }
  }
}
```

### 计算机视觉配置

```json
{
  "AI": {
    "Vision": {
      "Azure": {
        "SubscriptionKey": "${AZURE_VISION_KEY}",
        "Endpoint": "${AZURE_VISION_ENDPOINT}",
        "ApiVersion": "v3.2"
      },
      "Features": {
        "ObjectDetection": true,
        "SceneAnalysis": true,
        "TextRecognition": true,
        "FaceDetection": false,
        "ImageDescription": true
      },
      "Processing": {
        "MaxImageSize": 4194304,
        "SupportedFormats": ["jpg", "png", "bmp"],
        "CompressionQuality": 85,
        "BatchProcessing": true
      }
    }
  }
}
```

### 推荐系统配置

```json
{
  "AI": {
    "Recommendation": {
      "Algorithms": {
        "CollaborativeFiltering": {
          "Enabled": true,
          "Weight": 0.4,
          "MinSimilarUsers": 5,
          "MaxRecommendations": 20
        },
        "ContentBased": {
          "Enabled": true,
          "Weight": 0.3,
          "SimilarityThreshold": 0.6
        },
        "DeepLearning": {
          "Enabled": true,
          "Weight": 0.3,
          "ModelPath": "models/recommendation_model.onnx",
          "ConfidenceThreshold": 0.7
        }
      },
      "Caching": {
        "UserProfileCacheDuration": "01:00:00",
        "RecommendationCacheDuration": "00:30:00",
        "MaxCacheSize": 10000
      }
    }
  }
}
```

## 🥽 VR设备配置

### PICO设备配置

```json
{
  "VR": {
    "PICO": {
      "TrackingMode": "6DOF",
      "RenderScale": 1.0,
      "RefreshRate": 90,
      "IPDRange": {
        "Min": 58.0,
        "Max": 72.0,
        "Default": 64.0
      },
      "Controllers": {
        "TrackingPrediction": true,
        "HapticFeedback": true,
        "BatteryMonitoring": true
      },
      "Comfort": {
        "VignetteEnabled": true,
        "SnapTurning": true,
        "TeleportMovement": true,
        "ComfortSettings": "Medium"
      }
    }
  }
}
```

### 渲染配置

```json
{
  "VR": {
    "Rendering": {
      "Pipeline": "URP",
      "RenderScale": 1.0,
      "MSAALevel": 4,
      "TextureQuality": "High",
      "ShadowQuality": "Medium",
      "AnisotropicFiltering": 8,
      "VSync": false,
      "TargetFrameRate": 90,
      "AdaptivePerformance": {
        "Enabled": true,
        "TargetFrameRate": 90,
        "MinFrameRate": 72,
        "ThermalThrottling": true
      }
    }
  }
}
```

### 交互配置

```json
{
  "VR": {
    "Interaction": {
      "HandTracking": {
        "Enabled": true,
        "Confidence": 0.8,
        "GestureRecognition": true
      },
      "EyeTracking": {
        "Enabled": false,
        "FoveatedRendering": false
      },
      "SpatialUI": {
        "DefaultDistance": 2.0,
        "MinDistance": 1.0,
        "MaxDistance": 5.0,
        "FollowUser": true
      },
      "Locomotion": {
        "DefaultMode": "Teleport",
        "SmoothTurning": false,
        "TurnSpeed": 90.0,
        "MovementSpeed": 3.0
      }
    }
  }
}
```

## ⚡ 性能配置

### 性能监控配置

```json
{
  "Performance": {
    "Monitoring": {
      "Enabled": true,
      "SampleInterval": 1000,
      "MetricsRetention": "01:00:00",
      "AlertThresholds": {
        "FrameRate": 72,
        "MemoryUsage": 2048,
        "CPUUsage": 80,
        "GPUUsage": 85,
        "Temperature": 70
      }
    },
    "Optimization": {
      "AutoOptimization": true,
      "DynamicResolution": true,
      "LODSystem": true,
      "Culling": {
        "FrustumCulling": true,
        "OcclusionCulling": true,
        "DistanceCulling": true,
        "MaxDistance": 100.0
      }
    }
  }
}
```

### 内存管理配置

```json
{
  "Performance": {
    "Memory": {
      "GarbageCollection": {
        "Mode": "Incremental",
        "MaxTimeSlice": 2.0,
        "TargetFrameRate": 90
      },
      "ObjectPooling": {
        "Enabled": true,
        "InitialPoolSize": 100,
        "MaxPoolSize": 1000,
        "PrewarmPools": true
      },
      "AssetManagement": {
        "UnloadUnusedAssets": true,
        "UnloadInterval": 300,
        "MemoryThreshold": 1536
      }
    }
  }
}
```

## 🌐 网络配置

### API配置

```json
{
  "Network": {
    "API": {
      "BaseUrl": "${API_BASE_URL}",
      "Timeout": 30000,
      "MaxRetries": 3,
      "RetryDelay": 1000,
      "EnableCompression": true,
      "UserAgent": "TripMeta/1.0",
      "Headers": {
        "Accept": "application/json",
        "Content-Type": "application/json"
      }
    },
    "CDN": {
      "BaseUrl": "${CDN_BASE_URL}",
      "CacheControl": "max-age=3600",
      "EnableCaching": true,
      "CompressionEnabled": true
    }
  }
}
```

### 连接配置

```json
{
  "Network": {
    "Connection": {
      "MaxConcurrentConnections": 10,
      "KeepAliveTimeout": 30,
      "ConnectionTimeout": 10,
      "ReadTimeout": 30,
      "WriteTimeout": 30,
      "EnableTcpKeepAlive": true
    },
    "Proxy": {
      "Enabled": false,
      "Host": "",
      "Port": 0,
      "Username": "",
      "Password": ""
    }
  }
}
```

## 🔒 安全配置

### 认证配置

```json
{
  "Security": {
    "Authentication": {
      "JWT": {
        "SecretKey": "${JWT_SECRET_KEY}",
        "Issuer": "TripMeta",
        "Audience": "TripMeta.Users",
        "ExpirationMinutes": 1440,
        "RefreshTokenExpirationDays": 30,
        "RequireHttps": true
      },
      "OAuth": {
        "Google": {
          "ClientId": "${GOOGLE_CLIENT_ID}",
          "ClientSecret": "${GOOGLE_CLIENT_SECRET}"
        },
        "Facebook": {
          "AppId": "${FACEBOOK_APP_ID}",
          "AppSecret": "${FACEBOOK_APP_SECRET}"
        }
      }
    }
  }
}
```

### 数据保护配置

```json
{
  "Security": {
    "DataProtection": {
      "Encryption": {
        "Algorithm": "AES-256-GCM",
        "KeyRotationDays": 90,
        "EnableAtRest": true,
        "EnableInTransit": true
      },
      "Privacy": {
        "DataRetentionDays": 365,
        "AnonymizeAfterDays": 730,
        "EnableGDPRCompliance": true,
        "ConsentRequired": true
      }
    }
  }
}
```

## 📝 日志配置

### 日志级别配置

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "TripMeta.AI": "Debug",
      "TripMeta.VR": "Information",
      "TripMeta.Performance": "Warning",
      "Microsoft": "Warning",
      "System": "Error"
    },
    "Console": {
      "Enabled": true,
      "LogLevel": "Information",
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss"
    },
    "File": {
      "Enabled": true,
      "Path": "logs/tripmeta-{Date}.log",
      "MaxFileSize": "10MB",
      "MaxFiles": 30,
      "LogLevel": "Information"
    },
    "EventLog": {
      "Enabled": false,
      "Source": "TripMeta",
      "LogLevel": "Error"
    }
  }
}
```

### 结构化日志配置

```json
{
  "Logging": {
    "Structured": {
      "Enabled": true,
      "Format": "JSON",
      "IncludeFields": [
        "Timestamp",
        "Level",
        "Message",
        "Exception",
        "Properties",
        "UserId",
        "SessionId",
        "RequestId"
      ],
      "Enrichers": [
        "Environment",
        "Machine",
        "Thread",
        "Process"
      ]
    }
  }
}
```

## 🔧 配置管理最佳实践

### 配置验证

```csharp
// 配置验证示例
public class AIConfigurationValidator : IConfigurationValidator
{
    public ValidationResult Validate(AIConfiguration config)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(config.GPT.ApiKey))
            errors.Add("GPT API Key is required");
            
        if (config.GPT.MaxTokens <= 0)
            errors.Add("GPT MaxTokens must be greater than 0");
            
        if (config.GPT.Temperature < 0 || config.GPT.Temperature > 2)
            errors.Add("GPT Temperature must be between 0 and 2");
            
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
```

### 配置热更新

```csharp
// 配置热更新示例
public class ConfigurationHotReload : IConfigurationChangeHandler
{
    public async Task HandleConfigurationChangeAsync(string configPath, object newConfig)
    {
        switch (configPath)
        {
            case "AI.GPT":
                await UpdateGPTConfigurationAsync((GPTConfiguration)newConfig);
                break;
            case "Performance":
                await UpdatePerformanceConfigurationAsync((PerformanceConfiguration)newConfig);
                break;
        }
    }
}
```

---

*配置文件应根据实际部署环境进行调整，确保安全性和性能的平衡。*