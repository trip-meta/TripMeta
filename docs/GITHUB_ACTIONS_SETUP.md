# GitHub Actions Unity License Setup

This guide explains how to set up Unity license for GitHub Actions CI/CD.

## Quick Start (Recommended Method)

### Step 1: Create Unity Account (Free)

1. Go to https://id.unity.com
2. Click "Create account"
3. Fill in your information (Unity Personal is FREE)

### Step 2: Add GitHub Secrets

Go to: https://github.com/trip-meta/TripMeta/settings/secrets/actions

Add these two secrets:

| Secret Name | Value | Description |
|-------------|--------|-------------|
| `UNITY_EMAIL` | Your Unity account email | For auto-activation |
| `UNITY_PASSWORD` | Unity app password | NOT your main password! |

### Step 3: Create Unity App Password (Security)

**IMPORTANT: Use an app password, not your main Unity password!**

1. Go to https://id.unity.com
2. Sign in to your account
3. Go to **Security** section
4. Enable **Two-Factor Authentication** (if not enabled)
5. Create an **App Password**
6. Copy this app password

**Use the app password as `UNITY_PASSWORD` secret.**

### Step 4: Verify

1. Go to Actions tab in GitHub
2. Click "Unity Build and Test" workflow
3. Click "Run workflow"
4. Select build target and run
5. The workflow will automatically activate Unity using your credentials

---

## Alternative: Using Unity License File

If you have Unity installed locally and have already activated it:

### Getting License File

**Windows:**
```
C:\ProgramData\Unity\Unity_lic.ulf
```

**macOS:**
```
/Library/Application Support/Unity/Unity_lic.ulf
```

### Encoding to Base64

**PowerShell (Windows):**
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\ProgramData\Unity\Unity_lic.ulf"))
```

**Bash (macOS/Linux):**
```bash
base64 -i /Library/Application\ Support/Unity/Unity_lic.ulf
```

### Add to GitHub

| Secret Name | Value |
|-------------|--------|
| `UNITY_LICENSE` | Paste the base64-encoded license |

---

## Unity Personal License Details

| Feature | Details |
|----------|----------|
| **Cost** | Completely FREE |
| **Revenue Limit** | Under $100,000 USD annual revenue |
| **Features** | Full Unity engine capabilities |
| **Use Cases** | Personal projects, indie development, open source |
| **CI/CD** | Supported for GitHub Actions |

---

## Troubleshooting

### Error: "Authentication Failed"

- Verify email and password are correct
- Use an app password (not main password) if 2FA is enabled
- Check for extra spaces in secret values

### Error: "License Activation Failed"

- Ensure your Unity account is in good standing
- Verify you're using Unity Personal (not Pro/Plus trial)
- Check that the password hasn't expired

### Workflow Runs But Tests Fail

- Unity activation may have succeeded but tests have issues
- Check test logs for specific failure reasons
- Verify Unity version compatibility

---

## Security Best Practices

1. **Use App Passwords** - Never use your main Unity account password
2. **Enable 2FA** - Protect your Unity account
3. **Rotate Secrets** - Change app passwords periodically
4. **Limit Access** - Only enable Actions for trusted collaborators
5. **Monitor Logs** - Check for unauthorized usage

---

## How the Workflow Activation Works

The GitHub Actions workflow now uses `game-ci/unity-activate@v2` which:

1. Takes your `UNITY_EMAIL` and `UNITY_PASSWORD`
2. Activates Unity Personal license in CI environment
3. Returns activation file for subsequent build/test steps
4. License is discarded after workflow completes

This is more secure than storing a license file directly!
