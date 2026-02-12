# GitHub Actions Unity License Setup

This guide explains how to set up Unity license for GitHub Actions CI/CD.

## Prerequisites

- Unity 2022.3.10f1 installed locally
- GitHub repository access (Settings > Secrets and variables > Actions)

## Option 1: Unity Personal License (Free)

### Step 1: Activate Unity Locally

1. Open Unity Hub
2. Go to Preferences > Licenses
3. Sign in with your Unity account
4. Activate a Personal license (free)

### Step 2: Get License File

**Windows:**
```
C:\ProgramData\Unity\Unity_lic.ulf
```

**macOS:**
```
/Library/Application Support/Unity/Unity_lic.ulf
```

### Step 3: Encode License to Base64

**PowerShell (Windows):**
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\ProgramData\Unity\Unity_lic.ulf"))
```

**Bash (macOS/Linux):**
```bash
base64 -i /Library/Application\ Support/Unity/Unity_lic.ulf
```

### Step 4: Add to GitHub Secrets

1. Go to repository: https://github.com/trip-meta/TripMeta/settings/secrets/actions
2. Click "New repository secret"
3. Name: `UNITY_LICENSE`
4. Value: Paste the base64-encoded license content
5. Click "Add secret"

## Option 2: Unity Pro/Plus Serial License

Add these secrets to GitHub:

| Secret Name | Description |
|-------------|-------------|
| `UNITY_SERIAL` | Your Unity serial number (XXXX-XXXX-XXXX-XXXX-XXXX-XXXX) |
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity account password (use app password for security) |

### Creating an App Password (Recommended)

1. Go to https://id.unity.com
2. Sign in to your account
3. Go to Security > Two-Factor Authentication
4. Enable 2FA if not already enabled
5. Create an App Password
6. Use this app password (not your main password) for `UNITY_PASSWORD`

## Option 3: Unity Gaming Services (Cloud Build)

For teams using Unity Gaming Services, you can use:

| Secret Name | Description |
|-------------|-------------|
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password or app password |

The workflow will automatically activate Unity in the CI environment.

## Verification

After adding secrets, verify the workflow runs:

1. Go to Actions tab in GitHub
2. Click "Unity Build and Test" workflow
3. Click "Run workflow"
4. Select build target and run
5. Check logs for successful activation

## Troubleshooting

### Error: "Missing Unity License File"
- Ensure `UNITY_LICENSE` secret is set correctly
- Verify the base64 encoding is complete (no line breaks)

### Error: "Invalid Serial"
- Check `UNITY_SERIAL` format (XXXX-XXXX-XXXX-XXXX-XXXX-XXXX)
- Verify the serial is active and not expired

### Error: "Authentication Failed"
- Verify email and password are correct
- Use an app password if 2FA is enabled
- Check for special characters in password

## Security Best Practices

1. **Use App Passwords** - Never use your main Unity account password
2. **Enable 2FA** - Protect your Unity account
3. **Rotate Secrets** - Change app passwords periodically
4. **Limit Access** - Only enable Actions for trusted collaborators
5. **Monitor Logs** - Check for unauthorized usage

## License Type Selection

| License Type | UNITY_LICENSE | UNITY_SERIAL | UNITY_EMAIL/PASSWORD |
|--------------|----------------|----------------|---------------------|
| Personal (Free) | Required (base64 .ulf) | Not used | Not used |
| Pro/Plus | Optional | Required | Required |
| Gaming Services | Not used | Not used | Required |

For **TripMeta**, using Unity Personal license with `UNITY_LICENSE` is recommended.
