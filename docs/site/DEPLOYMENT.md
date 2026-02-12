# TripMeta GitHub Pages Deployment Guide

## Overview

This guide covers deploying the TripMeta demo site to GitHub Pages.

## Files Structure

```
docs/site/
├── index.html          # Main demo page
├── vr.mp4             # Demo video (6.7MB)
├── .nojekyll          # Bypass Jekyll processing
├── test-page.html      # Automated test page
├── test-responsive.html  # Responsive design tester
├── create-poster.html  # Poster image generator
├── verify-site.sh      # Verification script
└── README.md          # Site documentation
```

## GitHub Pages Configuration

### Settings

Go to: https://github.com/trip-meta/TripMeta/settings/pages

**Configuration:**
- **Source**: Deploy from a branch
- **Branch**: `main`
- **Folder**: `/docs`
- **Custom domain**: (optional) Configure in DNS

### Why /docs folder?

GitHub Pages supports serving from `/docs` folder when configured.

## Deployment Process

### Automatic Deployment

1. Push changes to `main` branch
2. GitHub Pages builds automatically (1-2 minutes)
3. Site available at: https://trip-meta.github.io/TripMeta/site/

### Local Testing

```bash
# Run verification script
cd docs/site
bash verify-site.sh

# Start local server
python -m http.server 8000
# Open http://localhost:8000/
```

## Troubleshooting

### Video not playing
- Verify vr.mp4 is in same directory as index.html
- Check browser console for errors

### GitHub Pages 404
- Verify Pages source is set to `/docs` folder
- Wait 1-2 minutes for deployment

### Styles not loading
- Clear browser cache
- CSS is embedded in <style> tag

## Performance

| Metric | Value |
|---------|--------|
| HTML Size | ~5.5 KB |
| Video Size | ~6.7 MB |
| Video Preload | metadata only |

## Browser Compatibility

- Chrome 90+: Full support
- Firefox 88+: Full support
- Safari 14+: Full support
- Edge 90+: Full support
- Mobile browsers: Full support
