# TripMeta 安全指南

## 📋 目录

- [安全概览](#安全概览)
- [认证与授权](#认证与授权)
- [数据保护](#数据保护)
- [网络安全](#网络安全)
- [VR安全](#vr安全)
- [AI安全](#ai安全)
- [隐私保护](#隐私保护)
- [安全监控](#安全监控)
- [事件响应](#事件响应)

## 🛡️ 安全概览

TripMeta采用多层安全架构，确保用户数据、AI服务和VR体验的全面安全保护。

### 安全架构

```
┌─────────────────────────────────────────────────────────┐
│                    Security Architecture                │
├─────────────────────────────────────────────────────────┤
│  Application Security Layer                             │
│  ├── Authentication      ├── Authorization             │
│  ├── Input Validation    ├── Output Encoding           │
│  ├── Session Management  └── CSRF Protection            │
├─────────────────────────────────────────────────────────┤
│  Data Security Layer                                    │
│  ├── Encryption at Rest  ├── Encryption in Transit     │
│  ├── Data Classification ├── Access Control            │
│  ├── Backup Security     └── Data Retention            │
├─────────────────────────────────────────────────────────┤
│  Network Security Layer                                 │
│  ├── TLS/SSL             ├── API Security              │
│  ├── Rate Limiting       ├── DDoS Protection           │
│  ├── Firewall Rules      └── VPN Access                │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Security Layer                          │
│  ├── Container Security  ├── Host Security             │
│  ├── Cloud Security      ├── Monitoring & Logging      │
│  ├── Patch Management    └── Incident Response         │
└─────────────────────────────────────────────────────────┘
```

### 安全原则

1. **最小权限原则**：用户和服务只获得完成任务所需的最小权限
2. **深度防御**：多层安全控制，单点失效不会导致整体安全失效
3. **零信任架构**：不信任任何网络位置，验证所有访问请求
4. **数据保护**：全生命周期数据保护，包括收集、存储、处理和销毁
5. **透明度**：清晰的隐私政策和数据使用说明

## 🔐 认证与授权

### 多因素认证（MFA）

```csharp
// MFA实现
public class MultiFactorAuthenticationService : IMFAService
{
    private readonly ITOTPService _totpService;
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    
    public async Task<MFASetupResult> SetupMFAAsync(string userId, MFAMethod method)
    {
        switch (method)
        {
            case MFAMethod.TOTP:
                return await SetupTOTPAsync(userId);
            case MFAMethod.SMS:
                return await SetupSMSAsync(userId);
            case MFAMethod.Email:
                return await SetupEmailAsync(userId);
            default:
                throw new NotSupportedException($"MFA method {method} not supported");
        }
    }
    
    public async Task<bool> VerifyMFAAsync(string userId, string code, MFAMethod method)
    {
        var user = await GetUserAsync(userId);
        if (!user.MFAEnabled) return false;
        
        switch (method)
        {
            case MFAMethod.TOTP:
                return _totpService.VerifyCode(user.TOTPSecret, code);
            case MFAMethod.SMS:
                return await VerifySMSCodeAsync(userId, code);
            case MFAMethod.Email:
                return await VerifyEmailCodeAsync(userId, code);
            default:
                return false;
        }
    }
    
    private async Task<MFASetupResult> SetupTOTPAsync(string userId)
    {
        var secret = _totpService.GenerateSecret();
        var qrCode = _totpService.GenerateQRCode(userId, secret, "TripMeta");
        
        await SaveMFASecretAsync(userId, secret, MFAMethod.TOTP);
        
        return new MFASetupResult
        {
            Success = true,
            Secret = secret,
            QRCode = qrCode,
            BackupCodes = GenerateBackupCodes()
        };
    }
}
```

### JWT令牌管理

```csharp
// JWT服务实现
public class JWTService : IJWTService
{
    private readonly JWTConfiguration _config;
    private readonly IKeyManagementService _keyService;
    
    public async Task<TokenResult> GenerateTokenAsync(User user, string[] scopes = null)
    {
        var key = await _keyService.GetCurrentSigningKeyAsync();
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("role", user.Role),
            new Claim("mfa_verified", user.MFAVerified.ToString())
        };
        
        // 添加作用域声明
        if (scopes != null)
        {
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_config.ExpirationMinutes),
            Issuer = _config.Issuer,
            Audience = _config.Audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);
        
        return new TokenResult
        {
            AccessToken = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken,
            ExpiresIn = _config.ExpirationMinutes * 60,
            TokenType = "Bearer"
        };
    }
    
    public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = await GetValidationParametersAsync();
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // 验证令牌类型
            if (!(validatedToken is JwtSecurityToken jwtToken) ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            
            return principal;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }
}
```

### 基于角色的访问控制（RBAC）

```csharp
// RBAC实现
public class RoleBasedAccessControl : IAccessControl
{
    private readonly Dictionary<string, Role> _roles;
    private readonly Dictionary<string, Permission> _permissions;
    
    public RoleBasedAccessControl()
    {
        InitializeRolesAndPermissions();
    }
    
    private void InitializeRolesAndPermissions()
    {
        // 定义权限
        _permissions = new Dictionary<string, Permission>
        {
            ["user.read"] = new Permission("user.read", "读取用户信息"),
            ["user.write"] = new Permission("user.write", "修改用户信息"),
            ["content.read"] = new Permission("content.read", "访问内容"),
            ["content.create"] = new Permission("content.create", "创建内容"),
            ["admin.system"] = new Permission("admin.system", "系统管理"),
            ["ai.access"] = new Permission("ai.access", "访问AI服务")
        };
        
        // 定义角色
        _roles = new Dictionary<string, Role>
        {
            ["Guest"] = new Role("Guest", "访客", new[] { "content.read" }),
            ["User"] = new Role("User", "普通用户", new[] { "user.read", "user.write", "content.read", "ai.access" }),
            ["Premium"] = new Role("Premium", "高级用户", new[] { "user.read", "user.write", "content.read", "content.create", "ai.access" }),
            ["Admin"] = new Role("Admin", "管理员", _permissions.Keys.ToArray())
        };
    }
    
    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var user = await GetUserAsync(userId);
        if (user == null) return false;
        
        var role = _roles.GetValueOrDefault(user.Role);
        if (role == null) return false;
        
        return role.Permissions.Contains(permission);
    }
    
    public async Task<bool> HasAnyPermissionAsync(string userId, params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (await HasPermissionAsync(userId, permission))
                return true;
        }
        return false;
    }
}
```

## 🔒 数据保护

### 数据加密

```csharp
// 数据加密服务
public class DataEncryptionService : IDataEncryptionService
{
    private readonly IKeyManagementService _keyService;
    private readonly EncryptionConfiguration _config;
    
    public async Task<EncryptedData> EncryptAsync(byte[] data, string keyId = null)
    {
        keyId ??= await _keyService.GetCurrentKeyIdAsync();
        var key = await _keyService.GetKeyAsync(keyId);
        
        using var aes = Aes.Create();
        aes.Key = key.KeyBytes;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        
        await csEncrypt.WriteAsync(data, 0, data.Length);
        csEncrypt.FlushFinalBlock();
        
        var encryptedBytes = msEncrypt.ToArray();
        var hmac = ComputeHMAC(encryptedBytes, key.HMACKey);
        
        return new EncryptedData
        {
            KeyId = keyId,
            IV = aes.IV,
            Data = encryptedBytes,
            HMAC = hmac,
            Algorithm = "AES-256-GCM"
        };
    }
    
    public async Task<byte[]> DecryptAsync(EncryptedData encryptedData)
    {
        var key = await _keyService.GetKeyAsync(encryptedData.KeyId);
        
        // 验证HMAC
        var computedHMAC = ComputeHMAC(encryptedData.Data, key.HMACKey);
        if (!computedHMAC.SequenceEqual(encryptedData.HMAC))
        {
            throw new SecurityException("Data integrity check failed");
        }
        
        using var aes = Aes.Create();
        aes.Key = key.KeyBytes;
        aes.IV = encryptedData.IV;
        
        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(encryptedData.Data);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msPlain = new MemoryStream();
        
        await csDecrypt.CopyToAsync(msPlain);
        return msPlain.ToArray();
    }
    
    private byte[] ComputeHMAC(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }
}
```

### 密钥管理

```csharp
// 密钥管理服务
public class KeyManagementService : IKeyManagementService
{
    private readonly IKeyVault _keyVault;
    private readonly KeyRotationPolicy _rotationPolicy;
    
    public async Task<EncryptionKey> GenerateKeyAsync(string keyId, KeyType keyType)
    {
        var keySize = keyType switch
        {
            KeyType.AES256 => 32,
            KeyType.AES128 => 16,
            _ => throw new ArgumentException("Unsupported key type")
        };
        
        var keyBytes = new byte[keySize];
        var hmacKey = new byte[32]; // 256-bit HMAC key
        
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        rng.GetBytes(hmacKey);
        
        var key = new EncryptionKey
        {
            Id = keyId,
            KeyBytes = keyBytes,
            HMACKey = hmacKey,
            KeyType = keyType,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_rotationPolicy.KeyLifetime),
            Status = KeyStatus.Active
        };
        
        await _keyVault.StoreKeyAsync(key);
        return key;
    }
    
    public async Task RotateKeysAsync()
    {
        var activeKeys = await _keyVault.GetActiveKeysAsync();
        
        foreach (var key in activeKeys.Where(k => k.ExpiresAt <= DateTime.UtcNow.AddDays(7)))
        {
            // 生成新密钥
            var newKeyId = $"{key.Id}_v{DateTime.UtcNow:yyyyMMdd}";
            await GenerateKeyAsync(newKeyId, key.KeyType);
            
            // 标记旧密钥为即将过期
            key.Status = KeyStatus.Expiring;
            await _keyVault.UpdateKeyAsync(key);
            
            // 通知应用程序密钥轮换
            await NotifyKeyRotationAsync(key.Id, newKeyId);
        }
    }
}
```

### 数据分类和标记

```csharp
// 数据分类服务
public class DataClassificationService : IDataClassificationService
{
    public DataClassification ClassifyData(object data, Type dataType)
    {
        var classification = new DataClassification
        {
            DataType = dataType.Name,
            SensitivityLevel = DetermineSensitivityLevel(dataType),
            RetentionPeriod = DetermineRetentionPeriod(dataType),
            EncryptionRequired = RequiresEncryption(dataType),
            AccessRestrictions = DetermineAccessRestrictions(dataType)
        };
        
        // 基于内容的分类
        if (data is string stringData)
        {
            if (ContainsPII(stringData))
            {
                classification.SensitivityLevel = SensitivityLevel.High;
                classification.EncryptionRequired = true;
                classification.AccessRestrictions.Add("PII_ACCESS");
            }
            
            if (ContainsFinancialData(stringData))
            {
                classification.SensitivityLevel = SensitivityLevel.Critical;
                classification.EncryptionRequired = true;
                classification.AccessRestrictions.Add("FINANCIAL_ACCESS");
            }
        }
        
        return classification;
    }
    
    private SensitivityLevel DetermineSensitivityLevel(Type dataType)
    {
        var sensitiveTypes = new Dictionary<Type, SensitivityLevel>
        {
            [typeof(UserProfile)] = SensitivityLevel.High,
            [typeof(PaymentInfo)] = SensitivityLevel.Critical,
            [typeof(LocationData)] = SensitivityLevel.Medium,
            [typeof(PreferenceData)] = SensitivityLevel.Low
        };
        
        return sensitiveTypes.GetValueOrDefault(dataType, SensitivityLevel.Public);
    }
    
    private bool ContainsPII(string data)
    {
        // 检测个人身份信息
        var piiPatterns = new[]
        {
            @"\b\d{3}-\d{2}-\d{4}\b", // SSN
            @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", // Credit Card
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", // Email
            @"\b\d{3}[\s-]?\d{3}[\s-]?\d{4}\b" // Phone
        };
        
        return piiPatterns.Any(pattern => Regex.IsMatch(data, pattern));
    }
}
```

## 🌐 网络安全

### TLS/SSL配置

```csharp
// TLS配置
public class TLSConfiguration
{
    public static void ConfigureTLS(IServiceCollection services)
    {
        services.Configure<HttpsRedirectionOptions>(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            options.HttpsPort = 443;
        });
        
        services.Configure<HstsOptions>(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
        
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        });
    }
}
```

### API安全

```csharp
// API安全中间件
public class APISecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly APISecurityOptions _options;
    private readonly IRateLimiter _rateLimiter;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. 速率限制
        var clientId = GetClientIdentifier(context);
        if (!await _rateLimiter.IsAllowedAsync(clientId))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        // 2. API密钥验证
        if (!ValidateAPIKey(context))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }
        
        // 3. 请求签名验证
        if (_options.RequireSignature && !ValidateSignature(context))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid signature");
            return;
        }
        
        // 4. 输入验证
        if (!await ValidateInputAsync(context))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid input");
            return;
        }
        
        // 5. 添加安全头
        AddSecurityHeaders(context);
        
        await _next(context);
    }
    
    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;
        
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["X-XSS-Protection"] = "1; mode=block";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'";
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    }
}
```

### DDoS防护

```csharp
// DDoS防护服务
public class DDoSProtectionService : IDDoSProtectionService
{
    private readonly IMemoryCache _cache;
    private readonly DDoSConfiguration _config;
    
    public async Task<bool> IsRequestAllowedAsync(string clientIP, string endpoint)
    {
        var key = $"ddos:{clientIP}:{endpoint}";
        var requestCount = _cache.Get<int>(key);
        
        if (requestCount >= _config.MaxRequestsPerMinute)
        {
            // 检查是否为恶意IP
            if (await IsKnownMaliciousIPAsync(clientIP))
            {
                await BlockIPAsync(clientIP, TimeSpan.FromHours(24));
                return false;
            }
            
            // 临时限制
            await BlockIPAsync(clientIP, TimeSpan.FromMinutes(15));
            return false;
        }
        
        // 增加请求计数
        _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
        
        return true;
    }
    
    private async Task<bool> IsKnownMaliciousIPAsync(string ip)
    {
        // 检查恶意IP数据库
        var maliciousIPs = await GetMaliciousIPListAsync();
        return maliciousIPs.Contains(ip);
    }
    
    private async Task BlockIPAsync(string ip, TimeSpan duration)
    {
        var blockKey = $"blocked:{ip}";
        _cache.Set(blockKey, true, duration);
        
        // 记录安全事件
        await LogSecurityEventAsync(new SecurityEvent
        {
            Type = SecurityEventType.IPBlocked,
            Source = ip,
            Timestamp = DateTime.UtcNow,
            Details = $"IP blocked for {duration.TotalMinutes} minutes"
        });
    }
}
```

## 🥽 VR安全

### VR数据保护

```csharp
// VR数据保护服务
public class VRDataProtectionService : IVRDataProtectionService
{
    private readonly IDataEncryptionService _encryption;
    private readonly IPrivacyService _privacy;
    
    public async Task<SecureVRSession> CreateSecureSessionAsync(string userId)
    {
        var session = new SecureVRSession
        {
            SessionId = Guid.NewGuid().ToString(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            EncryptionKey = await GenerateSessionKeyAsync()
        };
        
        // 启用隐私保护模式
        await _privacy.EnablePrivacyModeAsync(userId);
        
        return session;
    }
    
    public async Task<EncryptedVRData> SecureVRDataAsync(VRTrackingData data, string sessionId)
    {
        // 数据匿名化
        var anonymizedData = AnonymizeTrackingData(data);
        
        // 数据加密
        var encryptedData = await _encryption.EncryptAsync(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(anonymizedData)),
            sessionId
        );
        
        return new EncryptedVRData
        {
            SessionId = sessionId,
            EncryptedPayload = encryptedData,
            DataType = VRDataType.Tracking,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private VRTrackingData AnonymizeTrackingData(VRTrackingData data)
    {
        // 移除或模糊化敏感信息
        return new VRTrackingData
        {
            HeadPosition = FuzzPosition(data.HeadPosition),
            HeadRotation = data.HeadRotation,
            LeftHandPosition = FuzzPosition(data.LeftHandPosition),
            RightHandPosition = FuzzPosition(data.RightHandPosition),
            EyeTracking = null, // 移除眼动追踪数据
            BiometricData = null, // 移除生物特征数据
            Timestamp = data.Timestamp
        };
    }
    
    private Vector3 FuzzPosition(Vector3 position)
    {
        // 添加随机噪声以保护隐私
        var noise = UnityEngine.Random.Range(-0.01f, 0.01f);
        return new Vector3(
            position.x + noise,
            position.y + noise,
            position.z + noise
        );
    }
}
```

### VR内容安全

```csharp
// VR内容安全过滤
public class VRContentSecurityFilter : IVRContentFilter
{
    private readonly IContentModerationService _moderation;
    private readonly IAgeVerificationService _ageVerification;
    
    public async Task<ContentFilterResult> FilterContentAsync(VRContent content, string userId)
    {
        var user = await GetUserAsync(userId);
        var result = new ContentFilterResult { Content = content };
        
        // 1. 年龄适宜性检查
        if (!await _ageVerification.IsContentAppropriateAsync(content.AgeRating, user.Age))
        {
            result.Blocked = true;
            result.Reason = "Content not appropriate for user age";
            return result;
        }
        
        // 2. 内容审核
        var moderationResult = await _moderation.ModerateContentAsync(content);
        if (moderationResult.HasViolations)
        {
            result.Blocked = true;
            result.Reason = $"Content policy violation: {string.Join(", ", moderationResult.Violations)}";
            return result;
        }
        
        // 3. 地理限制检查
        if (content.GeoRestrictions.Any() && !IsContentAvailableInRegion(content, user.Region))
        {
            result.Blocked = true;
            result.Reason = "Content not available in user region";
            return result;
        }
        
        // 4. 用户偏好过滤
        if (user.ContentFilters.Any(filter => content.Tags.Contains(filter)))
        {
            result.Filtered = true;
            result.Reason = "Content filtered based on user preferences";
        }
        
        return result;
    }
    
    public async Task<bool> ValidateUserGeneratedContentAsync(VRUserContent content)
    {
        // 检查用户生成内容
        var checks = new[]
        {
            CheckForInappropriateContent(content),
            CheckForCopyrightViolation(content),
            CheckForMaliciousCode(content),
            CheckForPrivacyViolation(content)
        };
        
        var results = await Task.WhenAll(checks);
        return results.All(r => r);
    }
}
```

## 🤖 AI安全

### AI模型安全

```csharp
// AI模型安全服务
public class AIModelSecurityService : IAIModelSecurityService
{
    private readonly IModelValidationService _validation;
    private readonly IInputSanitizationService _sanitization;
    
    public async Task<AISecurityResult> ValidateAIRequestAsync(AIRequest request)
    {
        var result = new AISecurityResult();
        
        // 1. 输入验证和清理
        var sanitizedInput = await _sanitization.SanitizeInputAsync(request.Input);
        if (sanitizedInput != request.Input)
        {
            result.InputModified = true;
            request.Input = sanitizedInput;
        }
        
        // 2. 提示注入检测
        if (DetectPromptInjection(request.Input))
        {
            result.ThreatDetected = true;
            result.ThreatType = AIThreatType.PromptInjection;
            result.Blocked = true;
            return result;
        }
        
        // 3. 敏感信息检测
        var piiDetection = await DetectPIIAsync(request.Input);
        if (piiDetection.HasPII)
        {
            result.PIIDetected = true;
            result.PIITypes = piiDetection.Types;
            
            // 根据策略决定是否阻止或匿名化
            if (_config.BlockPIIRequests)
            {
                result.Blocked = true;
                return result;
            }
            else
            {
                request.Input = await AnonymizePIIAsync(request.Input, piiDetection);
                result.InputModified = true;
            }
        }
        
        // 4. 内容过滤
        var contentFilter = await FilterContentAsync(request.Input);
        if (contentFilter.Blocked)
        {
            result.Blocked = true;
            result.ThreatType = AIThreatType.InappropriateContent;
            return result;
        }
        
        return result;
    }
    
    private bool DetectPromptInjection(string input)
    {
        var injectionPatterns = new[]
        {
            @"ignore\s+previous\s+instructions",
            @"system\s*:\s*you\s+are",
            @"<\|im_start\|>",
            @"<\|im_end\|>",
            @"###\s*instruction",
            @"forget\s+everything",
            @"new\s+task\s*:",
            @"override\s+your\s+instructions"
        };
        
        return injectionPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }
    
    public async Task<ModelIntegrityResult> ValidateModelIntegrityAsync(string modelPath)
    {
        var result = new ModelIntegrityResult();
        
        // 1. 文件哈希验证
        var currentHash = await ComputeFileHashAsync(modelPath);
        var expectedHash = await GetExpectedHashAsync(modelPath);
        
        if (currentHash != expectedHash)
        {
            result.IntegrityViolated = true;
            result.Issues.Add("Model file hash mismatch");
        }
        
        // 2. 数字签名验证
        if (!await VerifyDigitalSignatureAsync(modelPath))
        {
            result.IntegrityViolated = true;
            result.Issues.Add("Invalid digital signature");
        }
        
        // 3. 模型结构验证
        if (!await ValidateModelStructureAsync(modelPath))
        {
            result.IntegrityViolated = true;
            result.Issues.Add("Invalid model structure");
        }
        
        return result;
    }
}
```

### AI输出过滤

```csharp
// AI输出安全过滤
public class AIOutputSecurityFilter : IAIOutputFilter
{
    private readonly IContentModerationService _moderation;
    private readonly IPIIDetectionService _piiDetection;
    
    public async Task<FilteredAIResponse> FilterAIResponseAsync(AIResponse response)
    {
        var filtered = new FilteredAIResponse
        {
            OriginalResponse = response,
            FilteredContent = response.Content
        };
        
        // 1. 内容审核
        var moderationResult = await _moderation.ModerateTextAsync(response.Content);
        if (moderationResult.HasViolations)
        {
            filtered.Blocked = true;
            filtered.BlockReason = "Content policy violation";
            filtered.Violations = moderationResult.Violations;
            return filtered;
        }
        
        // 2. PII检测和移除
        var piiResult = await _piiDetection.DetectAndRedactAsync(response.Content);
        if (piiResult.PIIFound)
        {
            filtered.FilteredContent = piiResult.RedactedText;
            filtered.PIIRemoved = true;
            filtered.PIITypes = piiResult.PIITypes;
        }
        
        // 3. 有害内容检测
        if (await DetectHarmfulContentAsync(response.Content))
        {
            filtered.Blocked = true;
            filtered.BlockReason = "Potentially harmful content detected";
            return filtered;
        }
        
        // 4. 事实核查（可选）
        if (_config.EnableFactChecking)
        {
            var factCheckResult = await PerformFactCheckAsync(response.Content);
            if (factCheckResult.HasMisinformation)
            {
                filtered.FactCheckWarning = true;
                filtered.FactCheckDetails = factCheckResult.Details;
            }
        }
        
        return filtered;
    }
    
    private async Task<bool> DetectHarmfulContentAsync(string content)
    {
        var harmfulPatterns = new[]
        {
            @"how\s+to\s+(make|create|build)\s+(bomb|explosive|weapon)",
            @"suicide\s+(method|instruction|guide)",
            @"(hack|crack|break\s+into)\s+",
            @"illegal\s+(drug|substance)\s+(recipe|instruction)"
        };
        
        foreach (var pattern in harmfulPatterns)
        {
            if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            {
                await LogSecurityEventAsync(new SecurityEvent
                {
                    Type = SecurityEventType.HarmfulContentDetected,
                    Details = $"Pattern matched: {pattern}",
                    Content = content.Substring(0, Math.Min(100, content.Length))
                });
                
                return true;
            }
        }
        
        return false;
    }
}
```

## 🔍 安全监控

### 安全事件监控

```csharp
// 安全事件监控服务
public class SecurityEventMonitoringService : ISecurityEventMonitoringService
{
    private readonly IEventStore _eventStore;
    private readonly IAlertingService _alerting;
    private readonly SecurityConfiguration _config;
    
    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        // 丰富事件信息
        securityEvent.Id = Guid.NewGuid().ToString();
        securityEvent.Timestamp = DateTime.UtcNow;
        securityEvent.Severity = DetermineSeverity(securityEvent);
        
        // 存储事件
        await _eventStore.StoreEventAsync(securityEvent);
        
        // 实时分析
        await AnalyzeEventAsync(securityEvent);
        
        // 触发告警
        if (securityEvent.Severity >= SecuritySeverity.High)
        {
            await _alerting.SendAlertAsync(new SecurityAlert
            {
                EventId = securityEvent.Id,
                Type = securityEvent.Type,
                Severity = securityEvent.Severity,
                Message = GenerateAlertMessage(securityEvent),
                Timestamp = securityEvent.Timestamp
            });
        }
    }
    
    private async Task AnalyzeEventAsync(SecurityEvent securityEvent)
    {
        // 1. 模式检测
        await DetectAttackPatternsAsync(securityEvent);
        
        // 2. 异常检测
        await DetectAnomaliesAsync(securityEvent);
        
        // 3. 威胁情报匹配
        await MatchThreatIntelligenceAsync(securityEvent);
        
        // 4. 自动响应
        await TriggerAutomaticResponseAsync(securityEvent);
    }
    
    private async Task DetectAttackPatternsAsync(SecurityEvent securityEvent)
    {
        var recentEvents = await _eventStore.GetRecentEventsAsync(
            TimeSpan.FromMinutes(15), 
            securityEvent.Source
        );
        
        // 检测暴力破解攻击
        if (securityEvent.Type == SecurityEventType.LoginFailed)
        {
            var failedLogins = recentEvents.Count(e => e.Type == SecurityEventType.LoginFailed);
            if (failedLogins >= _config.BruteForceThreshold)
            {
                await HandleBruteForceAttackAsync(securityEvent.Source);
            }
        }
        
        // 检测DDoS攻击
        if (securityEvent.Type == SecurityEventType.RateLimitExceeded)
        {
            var rateLimitEvents = recentEvents.Count(e => e.Type == SecurityEventType.RateLimitExceeded);
            if (rateLimitEvents >= _config.DDoSThreshold)
            {
                await HandleDDoSAttackAsync(securityEvent.Source);
            }
        }
    }
    
    public async Task<SecurityDashboard> GetSecurityDashboardAsync()
    {
        var dashboard = new SecurityDashboard();
        
        // 获取最近24小时的安全事件统计
        var events = await _eventStore.GetEventsAsync(TimeSpan.FromHours(24));
        
        dashboard.TotalEvents = events.Count;
        dashboard.EventsByType = events.GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Count());
        dashboard.EventsBySeverity = events.GroupBy(e => e.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // 活跃威胁
        dashboard.ActiveThreats = await GetActiveThreatsAsync();
        
        // 被阻止的IP
        dashboard.BlockedIPs = await GetBlockedIPsAsync();
        
        // 安全趋势
        dashboard.SecurityTrends = await CalculateSecurityTrendsAsync();
        
        return dashboard;
    }
}
```

## 🚨 事件响应

### 安全事件响应

```csharp
// 安全事件响应服务
public class SecurityIncidentResponseService : ISecurityIncidentResponseService
{
    private readonly INotificationService _notification;
    private readonly IForensicsService _forensics;
    private readonly IRecoveryService _recovery;
    
    public async Task HandleSecurityIncidentAsync(SecurityIncident incident)
    {
        // 1. 事件分类和优先级
        incident.Priority = DetermineIncidentPriority(incident);
        incident.Category = ClassifyIncident(incident);
        
        // 2. 立即响应措施
        await ExecuteImmediateResponseAsync(incident);
        
        // 3. 通知相关人员
        await NotifyIncidentTeamAsync(incident);
        
        // 4. 开始调查
        await InitiateInvestigationAsync(incident);
        
        // 5. 记录事件
        await DocumentIncidentAsync(incident);
    }
    
    private async Task ExecuteImmediateResponseAsync(SecurityIncident incident)
    {
        switch (incident.Type)
        {
            case IncidentType.DataBreach:
                await HandleDataBreachAsync(incident);
                break;
            case IncidentType.MalwareDetection:
                await HandleMalwareAsync(incident);
                break;
            case IncidentType.UnauthorizedAccess:
                await HandleUnauthorizedAccessAsync(incident);
                break;
            case IncidentType.DDoSAttack:
                await HandleDDoSAttackAsync(incident);
                break;
        }
    }
    
    private async Task HandleDataBreachAsync(SecurityIncident incident)
    {
        // 1. 隔离受影响的系统
        await IsolateAffectedSystemsAsync(incident.AffectedSystems);
        
        // 2. 停止数据泄露
        await StopDataLeakageAsync(incident);
        
        // 3. 评估泄露范围
        var breachAssessment = await AssessBreachScopeAsync(incident);
        incident.ImpactAssessment = breachAssessment;
        
        // 4. 通知监管机构（如需要）
        if (breachAssessment.RequiresRegulatoryNotification)
        {
            await NotifyRegulatoryAuthoritiesAsync(incident);
        }
        
        // 5. 通知受影响用户
        if (breachAssessment.RequiresUserNotification)
        {
            await NotifyAffectedUsersAsync(incident);
        }
    }
    
    public async Task<IncidentReport> GenerateIncidentReportAsync(string incidentId)
    {
        var incident = await GetIncidentAsync(incidentId);
        var forensicsData = await _forensics.GetForensicsDataAsync(incidentId);
        
        var report = new IncidentReport
        {
            IncidentId = incidentId,
            Title = incident.Title,
            Description = incident.Description,
            Timeline = await BuildIncidentTimelineAsync(incident),
            ImpactAssessment = incident.ImpactAssessment,
            RootCause = await DetermineRootCauseAsync(incident),
            ResponseActions = incident.ResponseActions,
            LessonsLearned = await ExtractLessonsLearnedAsync(incident),
            Recommendations = await GenerateRecommendationsAsync(incident),
            ForensicsFindings = forensicsData
        };
        
        return report;
    }
}
```

---

*本安全指南应定期更新以应对新兴威胁和安全最佳实践的变化。*