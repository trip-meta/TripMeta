# GitHub Repository Setup Guide

Complete guide to optimize the TripMeta GitHub repository settings.

## 1. Repository Settings

### About Section

Go to: https://github.com/trip-meta/TripMeta/settings

**Description (120 chars max):**
```
AI-powered VR tourism platform with intelligent tour guides. Built with Unity for PICO VR headsets. Explore virtual attractions worldwide.
```

**Website URL:**
```
https://trip-meta.github.io/TripMeta/
```

**Topics/Tags:**
Add these to https://github.com/trip-meta/TripMeta/settings/topics:
- `unity` - Unity game engine
- `vr` - Virtual reality
- `virtual-reality` - Virtual reality
- `ai` - Artificial intelligence
- `openai` - OpenAI integration
- `gpt` - GPT language models
- `tourism` - Tourism application
- `pico` - PICO VR platform
- `pico-vr` - PICO VR specific
- `unity3d` - Unity 3D
- `csharp` - C# language
- `metaverse` - Metaverse related
- `smart-tour-guide` - AI tour guide
- `chatgpt` - ChatGPT integration
- `vrc` - VR chat
- `ai-tourism` - AI in tourism

### Features

Enable these features in repository settings:

| Feature | Setting | Description |
|---------|----------|-------------|
| Issues | Enabled | Track bugs and feature requests |
| Projects | Disabled (or use GitHub Projects) | Project management |
| Wiki | Enabled | Extended documentation |
| Discussions | Enabled | Community Q&A |
| Actions | Enabled | CI/CD workflows |
| Pages | Enabled | Demo site hosting |
| Security & Analysis | Enabled | Dependency security |

## 2. Branch Protection Rules

Go to: https://github.com/trip-meta/TripMeta/settings/branches

### Main Branch Protection

**Rule name:** `main`

| Setting | Value |
|---------|--------|
| Require pull request | ✅ Yes |
| Require approvals | 1 approval |
| Dismiss stale reviews | ✅ Yes (7 days) |
| Require status checks | ✅ Yes |
| Required checks | code-quality |
| Require branches to be up to date | ✅ Yes |
| Block force pushes | ✅ Yes |
| Require signed commits | ❌ No |

### Develop Branch Protection (Optional)

| Setting | Value |
|---------|--------|
| Require pull request | ✅ Yes |
| Require approvals | 1 approval |
| Require status checks | ✅ Yes |

## 3. GitHub Secrets

Go to: https://github.com/trip-meta/TripMeta/settings/secrets/actions

### Unity Secrets

| Secret Name | Required | Description |
|-------------|------------|-------------|
| `UNITY_LICENSE` | Yes* | Base64-encoded Unity license file (.ulf) |
| `UNITY_EMAIL` | Conditional | Unity account email |
| `UNITY_PASSWORD` | Conditional | Unity app password (not main password) |
| `UNITY_SERIAL` | Conditional | Unity Pro/Plus serial number |

*Use UNITY_LICENSE for Personal licenses, or email/password/serial for Pro/Plus.

See [docs/GITHUB_ACTIONS_SETUP.md](../docs/GITHUB_ACTIONS_SETUP.md) for detailed instructions.

### Other Secrets (Optional)

| Secret Name | Purpose |
|-------------|-----------|
| `OPENAI_API_KEY` | OpenAI GPT integration |
| `AZURE_SPEECH_KEY` | Azure Speech Services |
| `DISCORD_WEBHOOK` | Build notifications |

## 4. Labels

Go to: https://github.com/trip-meta/TripMeta/settings/labels

Create these labels for better issue tracking:

### Priority Labels

| Name | Color | Description |
|------|--------|-------------|
| `critical` | `#d73a4a` | Critical bugs, blocking release |
| `high` | `#ff7b72` | High priority issues |
| `medium` | `#f0883e` | Medium priority issues |
| `low` | `#7ee787` | Low priority issues |

### Type Labels

| Name | Color | Description |
|------|--------|-------------|
| `bug` | `#d73a4a` | Bug report |
| `enhancement` | `#a2eeef` | Feature request |
| `documentation` | `#0075ca` | Documentation improvement |
| `question` | `#7ee787` | User question |
| `duplicate` | `#768393` | Duplicate issue |
| `wontfix` | `#768393` | Won't be fixed |
| `help wanted` | `#008672` | Community contributions welcome |
| `good first issue` | `#7057ff` | Good for beginners |

### Component Labels

| Name | Color | Description |
|------|--------|-------------|
| `AI` | `#5319e7` | AI services and features |
| `VR` | `#6f42c1` | VR-specific functionality |
| `UI/UX` | `#e99695` | User interface |
| `Performance` | `#fbca04` | Performance issues |
| `CI/CD` | `#111111` | Build and deployment |
| `Tests` | `#2ea44f` | Testing related |

## 5. Issue Templates

Create these templates in `.github/ISSUE_TEMPLATE/`:

### bug_report.md
```markdown
---
name: Bug report
about: Create a report to help us improve
title: '[BUG] '
labels: bug
assignees: ''
---

## Description
A clear and concise description of what the bug is.

## Reproduction Steps
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

## Expected Behavior
What you expected to happen.

## Actual Behavior
What actually happened.

## Environment
- Unity Version: [e.g. 2022.3.10f1]
- Platform: [e.g. PICO 4, Windows, WebGL]
- Build Type: [e.g. Release, Debug]
```

### feature_request.md
```markdown
---
name: Feature request
about: Suggest an idea for TripMeta
title: '[FEATURE] '
labels: enhancement
assignees: ''
---

## Problem Description
What problem does this feature solve?

## Proposed Solution
What would you like to see implemented?

## Alternatives
Describe any alternative solutions or features you've considered.

## Additional Context
Any other context, screenshots, or examples.
```

## 6. Pull Request Template

Create `.github/PULL_REQUEST_TEMPLATE.md`:

```markdown
## Description
Please include a summary of the changes and the related issue.

Fixes # (issue)

## Type of Change
- [ ] Bug fix (non-breaking change)
- [ ] New feature (non-breaking change)
- [ ] Breaking change (fix/feature causing existing behavior to break)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review of code completed
- [ ] Documentation updated
- [ ] No new warnings generated
```

## 7. Social Image

For GitHub social preview, add `social-preview.png` (1280x640px recommended):

Content suggestions:
- VR scene screenshot with AI guide
- Tourist viewing virtual attraction
- PICO VR headset with TripMeta interface
- Split screen showing VR view + AI dialogue

## 8. Community Settings

### Discussions

Enable categories:
- `Announcements` - For release notes and updates
- `General` - General discussion
- `Q&A` - Help and questions
- `Show and Tell` - Share your projects
- `Ideas` - Feature suggestions

### Security Advisories

Enable security advisories to receive security vulnerability alerts.

## 9. CI/CD Configuration

Workflow file: `.github/workflows/unity-build.yml`

Current workflow stages:
1. Code Quality Check
2. Unit Tests
3. Build (multi-platform)
4. Performance Tests
5. Deploy (staging/production)
6. Notifications

## 10. Webhook Configuration (Optional)

For external notifications, configure webhooks:

| Service | Use Case |
|----------|-----------|
| Discord | Build notifications to Discord channel |
| Slack | Team notifications |
| Email | Commit notifications |

## Checklist

After completing setup:

- [ ] About description and website set
- [ ] Topics/tags added
- [ ] Branch protection enabled for main
- [ ] GitHub secrets configured (UNITY_LICENSE)
- [ ] Labels created
- [ ] Issue/PR templates added
- [ ] Social preview image added
- [ ] Discussions enabled with categories
- [ ] Security advisories enabled
- [ ] GitHub Actions workflow tested

## Verification

To verify everything is working:

1. Create a test issue (should use template)
2. Run workflow manually (Actions > Unity Build and Test > Run workflow)
3. Check that all status checks pass
4. Verify build artifacts are uploaded

## Related Documentation

- [GitHub Actions Setup](../docs/GITHUB_ACTIONS_SETUP.md)
- [Contributing Guide](../docs/CONTRIBUTING.md)
- [README](../README.md)
