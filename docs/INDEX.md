# TripMeta Documentation Index

Complete navigation guide for all TripMeta documentation.

---

## Quick Navigation

| Document | Audience | Description |
|----------|-----------|-------------|
| [README](../README.md) | Everyone | Project overview and quick start |
| [QUICKSTART](QUICKSTART.md) | Developers | Detailed setup instructions |
| [ARCHITECTURE](ARCHITECTURE.md) | Developers | System design and patterns |
| [TECH_STACK](TECH_STACK.md) | Developers | Complete technology overview |
| [AI_INTEGRATION](AI_INTEGRATION.md) | Developers | AI services configuration |
| [DEVELOPMENT_STANDARDS](DEVELOPMENT_STANDARDS.md) | Developers | Coding conventions and Git workflow |
| [TESTING_GUIDE](TESTING_GUIDE.md) | Developers | Testing strategies and coverage |
| [DEPLOYMENT_GUIDE](DEPLOYMENT_GUIDE.md) | DevOps | Build and deployment procedures |
| [CONFIGURATION](CONFIGURATION.md) | Developers | Configuration system details |
| [TROUBLESHOOTING](TROUBLESHOOTING.md) | Everyone | Common issues and solutions |
| [CONTRIBUTING](CONTRIBUTING.md) | Contributors | Contribution guidelines |
| [FAQ](FAQ.md) | Everyone | Frequently asked questions |

---

## Documentation Categories

### 🚀 Getting Started

**New to TripMeta?** Start here:

1. **[README](../README.md)** - Project overview, features, and installation
2. **[QUICKSTART](QUICKSTART.md)** - Step-by-step setup guide
3. **[LIVE DEMO](https://trip-meta.github.io/TripMeta/site/)** - See the project in action

**For CI/CD Setup:**
- **[GITHUB_ACTIONS_SETUP](GITHUB_ACTIONS_SETUP.md)** - Configure GitHub Actions
- **[UNITY_APP_PASSWORD_GUIDE](UNITY_APP_PASSWORD_GUIDE.md)** - Create Unity credentials

### 🏗️ Architecture & Design

Understanding how TripMeta is built:

- **[ARCHITECTURE](ARCHITECTURE.md)** - Complete system architecture
- **[TECH_STACK](TECH_STACK.md)** - Technology choices and rationale
- **[CONFIGURATION](CONFIGURATION.md)** - Configuration system details
- **[AI_INTEGRATION](AI_INTEGRATION.md)** - AI services architecture

**Key Patterns:**
- Dependency Injection using custom container
- Event-driven communication between systems
- ScriptableObject-based configuration
- Service locator pattern for runtime resolution

### 👨 Development

Development workflow and standards:

- **[DEVELOPMENT_STANDARDS](DEVELOPMENT_STANDARDS.md)** - Coding conventions
- **[TESTING_GUIDE](TESTING_GUIDE.md)** - Testing strategies
- **[DEPLOYMENT_GUIDE](DEPLOYMENT_GUIDE.md)** - Build procedures

**Git Workflow:**
```
feature branch → commit → push → PR → review → merge
```

**Code Style:**
- C# naming conventions (PascalCase for public, _camelCase for private)
- File organization by feature/domain
- Immutable data patterns
- Comprehensive error handling

### 🧪 Testing

Testing strategies and requirements:

- **[TESTING_GUIDE](TESTING_GUIDE.md)** - Complete testing guide
- **80%+ coverage requirement** - Minimum test coverage target
- **Test Types**: Unit, Integration, E2E (Playwright)

**Test Organization:**
```
Assets/Scripts/Tests/
├── Unit/           # Isolated component tests
├── Integration/     # Cross-component tests
└── E2E/            # End-to-end scenario tests
```

### 📦 Deployment & Operations

Building and deploying TripMeta:

- **[DEPLOYMENT_GUIDE](DEPLOYMENT_GUIDE.md)** - Build procedures
- **[TROUBLESHOOTING](TROUBLESHOOTING.md)** - Common issues and fixes
- **[GitHub Actions](.github/workflows/unity-build.yml)** - CI/CD pipelines

**Build Targets:**
- StandaloneWindows64 - Windows desktop application
- Android - PICO VR headset
- WebGL - Web browser deployment

### 🔧 Configuration

Managing settings and preferences:

- **[CONFIGURATION](CONFIGURATION.md)** - Configuration system overview
- Unity Editor menus: `TripMeta > Create Configuration Assets`

**Config Locations:**
```
Assets/Resources/Config/
├── TripMetaConfig.asset          # Root configuration
├── AppSettings.asset            # Application settings
├── OpenAIConfig.asset           # AI service settings
└── AzureSpeechConfig.asset       # Speech service settings
```

### 🤝 Contributing

How to contribute to TripMeta:

- **[CONTRIBUTING](CONTRIBUTING.md)** - Contribution guidelines
- **[CODE_OF_CONDUCT](../CODE_OF_CONDUCT.md)** - Community guidelines
- **Pull Request Template** - [.github/PULL_REQUEST_TEMPLATE.md](../.github/PULL_REQUEST_TEMPLATE.md)
- **Issue Template** - [.github/ISSUE_TEMPLATE/](../.github/ISSUE_TEMPLATE/)

**Contribution Areas:**
- 🐛 Bug fixes
- ✨ New features
- 📖 Documentation
- 🎨 UI/UX
- ⚡ Performance
- 🌍 Internationalization

### 📚 Additional Resources

Supplementary documentation:

| Document | Purpose |
|----------|----------|
| [USER_MANUAL](USER_MANUAL.md) | End-user documentation |
| [VISUAL_RESULTS](VISUAL_RESULTS.md) | Visual reference outcomes |
| [ACTUAL_EFFECTS](ACTUAL_EFFECTS.md) | Visual effects documentation |
| [VIDEO_GENERATION_RESEARCH](VIDEO_GENERATION_RESEARCH.md) | Video generation research |
| [VIDEO_RECORDING_GUIDE](VIDEO_RECORDING_GUIDE.md) | Video capture guide |

### 🔮 Upgrade Planning

Unity upgrade documentation:

- **[Unity 2021.3 → 2023.3 Upgrade Plan](Unity_2021.3_→_2023.3_升级执行计划.md)** - Upgrade execution plan
- **[UPGRADE_PLAN_2023](UPGRADE_PLAN_2023.md)** - Detailed upgrade roadmap
- **[UPGRADE_CHANGESET_2023](UPGRADE_CHANGESET_2023.md)** - Breaking changes overview

---

## Document Structure

### Core Documentation Files

**Essential** (Maintain and Keep Updated):
- README.md
- QUICKSTART.md
- ARCHITECTURE.md
- DEVELOPMENT_STANDARDS.md
- CONTRIBUTING.md
- TROUBLESHOOTING.md

**Technical** (Reference Material):
- TECH_STACK.md
- AI_INTEGRATION.md
- CONFIGURATION.md
- TESTING_GUIDE.md
- DEPLOYMENT_GUIDE.md

### Archive Documentation

**Historical** (For Reference):
- PROJECT_COMPLETION_SUMMARY.md - Project completion status
- FINAL_SUMMARY.md - Final project summary
- SYSTEM_TEST_GUIDE.md - System testing procedures
- DEMO_GUIDE.md - Demo creation guide
- RUN_RESULTS.md - Test execution results
- DIRECTORY_STRUCTURE.md - Historical structure reference
- FUTURE_ROADMAP.md - Legacy roadmap
- SECURITY.md - Security guidelines
- API_REFERENCE.md - API documentation

---

## Reading Guidelines

### For New Developers

1. Start with [README.md](../README.md) for project overview
2. Follow [QUICKSTART](QUICKSTART.md) for initial setup
3. Reference [ARCHITECTURE](ARCHITECTURE.md) for system understanding
4. Follow [DEVELOPMENT_STANDARDS](DEVELOPMENT_STANDARDS.md) for code conventions
5. Check [TESTING_GUIDE](TESTING_GUIDE.md) before writing code
6. Review [CONTRIBUTING](CONTRIBUTING.md) before submitting PR

### For Contributors

1. Read [CONTRIBUTING](CONTRIBUTING.md) guidelines first
2. Check [DEVELOPMENT_STANDARDS](DEVELOPMENT_STANDARDS.md) for workflow
3. Reference [ARCHITECTURE](ARCHITECTURE.md) for patterns
4. Ensure [TESTING_GUIDE](TESTING_GUIDE.md) coverage requirements
5. Update relevant documentation when adding features

### For Users/Operators

1. Review [USER_MANUAL](USER_MANUAL.md) for usage
2. Check [TROUBLESHOOTING](TROUBLESHOOTING.md) for common issues
3. Reference [CONFIGURATION](CONFIGURATION.md) for settings
4. Review FAQ in [FAQ.md](FAQ.md) for quick answers

---

## Documentation Maintenance

### Writing Guidelines

- **Use clear headings** - H1 for title, H2 for main sections, H3 for subsections
- **Include code examples** - Show actual implementation patterns
- **Add diagrams** - Use ASCII art or mermaid for architecture
- **Link related docs** - Cross-reference between documents
- **Include dates** - Mark when documents were last updated

### Keeping Docs Updated

When making code changes:

1. **Update affected docs** - If changing architecture, update ARCHITECTURE.md
2. **Add new examples** - Include usage examples for new features
3. **Update README** - Keep badges and quick links current
4. **Review outdated sections** - Remove or mark as deprecated

### Documentation Review

**Review Checklist:**
- [ ] All links work (no broken URLs)
- [ ] Code examples are accurate
- [ ] Tables are properly formatted
- [ ] Spelling and grammar checked
- [ ] Cross-references are correct
- [ ] Dates are updated

---

## Quick Reference

### Common Commands

```bash
# Unity Setup
TripMeta > Create Configuration Assets
TripMeta > Setup Main Scene

# Git Workflow
git checkout -b feature/your-feature
git commit -m "feat: description"
git push origin feature/your-feature

# Testing
Unity Test Runner > Run All
```

### File Locations

```
TripMeta/
├── Assets/Scripts/          # Source code
├── docs/                      # Documentation (this directory)
├── .github/workflows/          # CI/CD pipelines
└── README.md                   # Project overview
```

### Key Contacts

- **Issues**: [github.com/trip-meta/TripMeta/issues](https://github.com/trip-meta/TripMeta/issues)
- **Discussions**: [github.com/trip-meta/TripMeta/discussions](https://github.com/trip-meta/TripMeta/discussions)
- **Pull Requests**: [github.com/trip-meta/TripMeta/pulls](https://github.com/trip-meta/TripMeta/pulls)

---

**Last Updated**: 2025-02-12

**For questions or contributions**, please visit [trip-meta.github.io/TripMeta](https://trip-meta.github.io/TripMeta/)
