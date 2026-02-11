# TripMeta 贡献指南

## 🤝 欢迎贡献

感谢您对TripMeta项目的关注！我们欢迎各种形式的贡献，包括但不限于：

- 🐛 Bug报告和修复
- ✨ 新功能建议和实现
- 📚 文档改进
- 🧪 测试用例添加
- 🎨 UI/UX改进
- 🔧 性能优化

## 📋 目录

- [开发环境设置](#开发环境设置)
- [贡献流程](#贡献流程)
- [代码规范](#代码规范)
- [提交规范](#提交规范)
- [Pull Request指南](#pull-request指南)
- [Issue报告](#issue报告)
- [社区准则](#社区准则)

## 🛠️ 开发环境设置

### 系统要求

- **Unity**: 2022.3 LTS或更高版本
- **.NET**: 6.0或更高版本
- **Git**: 最新版本
- **IDE**: Visual Studio 2022 或 JetBrains Rider

### 环境配置

1. **克隆仓库**
   ```bash
   git clone https://github.com/trip-meta/TripMeta.git
   cd tripmeta
   ```

2. **安装依赖**
   ```bash
   # 恢复NuGet包
   dotnet restore
   
   # 安装Unity包
   # 在Unity Package Manager中安装必要的包
   ```

3. **配置环境变量**
   ```bash
   # 复制环境变量模板
   cp .env.example .env
   
   # 编辑.env文件，填入必要的API密钥
   ```

4. **运行测试**
   ```bash
   # 运行单元测试
   dotnet test
   
   # 运行集成测试
   dotnet test --filter Category=Integration
   ```

## 🔄 贡献流程

### 1. Fork和Clone

```bash
# Fork项目到您的GitHub账户
# 然后克隆您的fork
git clone https://github.com/trip-meta/TripMeta.git
cd tripmeta

# 添加上游仓库
git remote add upstream https://github.com/trip-meta/TripMeta.git
```

### 2. 创建功能分支

```bash
# 从main分支创建新分支
git checkout -b feature/your-feature-name

# 或者修复bug
git checkout -b fix/bug-description
```

### 3. 开发和测试

```bash
# 进行开发工作
# 添加测试用例
# 运行测试确保通过
dotnet test

# 检查代码质量
dotnet format
```

### 4. 提交更改

```bash
# 添加更改
git add .

# 提交更改（遵循提交规范）
git commit -m "feat: add AI voice synthesis feature"

# 推送到您的fork
git push origin feature/your-feature-name
```

### 5. 创建Pull Request

- 在GitHub上创建Pull Request
- 填写详细的PR描述
- 等待代码审查
- 根据反馈进行修改

## 📝 代码规范

### C#代码规范

```csharp
// 使用PascalCase命名类和方法
public class AIServiceManager
{
    // 使用camelCase命名私有字段，添加下划线前缀
    private readonly IGPTService _gptService;
    
    // 使用PascalCase命名公共属性
    public string ServiceName { get; set; }
    
    // 使用PascalCase命名方法
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // 使用camelCase命名局部变量
        var response = await _gptService.GenerateResponseAsync(prompt);
        return response;
    }
}
```

### Unity代码规范

```csharp
// MonoBehaviour类使用PascalCase
public class VRInteractionManager : MonoBehaviour
{
    // SerializeField字段使用camelCase，添加下划线前缀
    [SerializeField] private float _interactionDistance = 5f;
    
    // Unity事件方法
    private void Start()
    {
        InitializeComponents();
    }
    
    private void Update()
    {
        HandleInput();
    }
}
```

### 注释规范

```csharp
/// <summary>
/// AI服务管理器，负责协调各种AI服务
/// </summary>
public class AIServiceManager
{
    /// <summary>
    /// 生成AI响应
    /// </summary>
    /// <param name="prompt">用户输入的提示</param>
    /// <param name="context">上下文信息</param>
    /// <returns>AI生成的响应</returns>
    public async Task<AIResponse> GenerateResponseAsync(string prompt, AIContext context)
    {
        // TODO: 添加输入验证
        // FIXME: 处理超时情况
        
        var response = await ProcessPromptAsync(prompt, context);
        return response;
    }
}
```

## 📋 提交规范

### 提交消息格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

### 提交类型

- **feat**: 新功能
- **fix**: Bug修复
- **docs**: 文档更新
- **style**: 代码格式化
- **refactor**: 代码重构
- **test**: 测试相关
- **chore**: 构建过程或辅助工具的变动

### 示例

```bash
# 新功能
git commit -m "feat(ai): add voice synthesis with Azure Speech Service"

# Bug修复
git commit -m "fix(vr): resolve hand tracking accuracy issue"

# 文档更新
git commit -m "docs: update API documentation for AI services"

# 重构
git commit -m "refactor(core): improve dependency injection container performance"
```

## 🔍 Pull Request指南

### PR标题格式

```
[Type] Brief description of changes
```

### PR描述模板

```markdown
## 📝 变更描述
简要描述此PR的目的和内容

## 🔧 变更类型
- [ ] Bug修复
- [ ] 新功能
- [ ] 文档更新
- [ ] 性能优化
- [ ] 代码重构

## 🧪 测试
- [ ] 添加了新的测试用例
- [ ] 所有现有测试通过
- [ ] 手动测试通过

## 📋 检查清单
- [ ] 代码遵循项目规范
- [ ] 添加了必要的文档
- [ ] 更新了CHANGELOG.md
- [ ] 没有引入破坏性变更

## 🔗 相关Issue
Closes #123
```

### 代码审查要点

1. **功能正确性**: 代码是否实现了预期功能
2. **代码质量**: 是否遵循编码规范和最佳实践
3. **性能影响**: 是否对性能产生负面影响
4. **测试覆盖**: 是否有足够的测试覆盖
5. **文档完整**: 是否更新了相关文档

## 🐛 Issue报告

### Bug报告模板

```markdown
## 🐛 Bug描述
清晰简洁地描述bug

## 🔄 复现步骤
1. 进入VR场景
2. 执行语音命令"带我去埃菲尔铁塔"
3. 观察AI响应

## 🎯 期望行为
描述您期望发生的情况

## 📱 实际行为
描述实际发生的情况

## 🖥️ 环境信息
- Unity版本: 2022.3.12f1
- PICO SDK版本: v2.1.1
- 操作系统: Windows 11
- 设备型号: PICO 4

## 📎 附加信息
添加截图、日志或其他相关信息
```

### 功能请求模板

```markdown
## 🚀 功能描述
清晰描述您希望添加的功能

## 💡 动机
解释为什么需要这个功能

## 📋 详细设计
描述功能的具体实现方案

## 🎯 验收标准
- [ ] 功能A正常工作
- [ ] 性能满足要求
- [ ] 通过所有测试

## 📚 参考资料
提供相关的参考链接或文档
```

## 🤝 社区准则

### 行为准则

1. **尊重他人**: 保持友善和专业的态度
2. **建设性反馈**: 提供有用的建议和批评
3. **包容性**: 欢迎不同背景的贡献者
4. **学习态度**: 保持开放的学习心态

### 沟通指南

- **Issue讨论**: 在相关Issue中进行技术讨论
- **PR评审**: 提供具体、建设性的代码审查意见
- **社区交流**: 在Discussions中分享想法和经验

### 冲突解决

如果遇到分歧或冲突：

1. 保持冷静和专业
2. 专注于技术问题本身
3. 寻求维护者的帮助
4. 遵循项目的最终决定

## 🏆 贡献者认可

### 贡献类型

- **代码贡献**: 提交代码、修复bug
- **文档贡献**: 改进文档、添加示例
- **测试贡献**: 添加测试用例、报告bug
- **设计贡献**: UI/UX设计、用户体验改进
- **社区贡献**: 帮助其他用户、参与讨论

### 认可方式

- 在README中列出贡献者
- 在发布说明中感谢贡献者
- 颁发贡献者徽章
- 邀请活跃贡献者成为维护者

## 📞 联系方式

- **GitHub Issues**: 技术问题和bug报告
- **GitHub Discussions**: 一般讨论和想法分享

## 📚 资源链接

- [项目文档](./README.md)
- [API参考](./docs/API_REFERENCE.md)
- [开发标准](./docs/DEVELOPMENT_STANDARDS.md)
- [架构设计](./docs/ARCHITECTURE.md)

---

感谢您的贡献！每一个贡献都让TripMeta变得更好。🚀