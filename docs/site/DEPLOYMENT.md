# TripMeta GitHub Pages Deployment Guide

## Overview

This guide covers deploying the TripMeta demo site to GitHub Pages.

## Files Structure

```
docs/site/
├── index.html          # Main demo page
├── vr.mp4             # Demo video (6.7MB)
├── ai-npc-metaverse.html  # AI NPC scenario demo
├── robots.txt
├── sitemap.xml
├── .nojekyll          # Bypass Jekyll processing
└── README.md          # Site documentation
```

## GitHub Pages Configuration

### Settings

Go to: https://github.com/trip-meta/TripMeta/settings/pages

**Configuration:**
- **Source**: GitHub Actions
- **Workflow**: `.github/workflows/deploy-pages.yml`
- **Artifact path**: `docs/site`
- **Custom domain**: (optional) Configure in DNS

### Why docs/site?

The Pages workflow uploads `docs/site` as the static artifact, so its files are served at the repository Pages root.

## Deployment Process

### Automatic Deployment

1. Push changes to `main` branch
2. GitHub Pages builds automatically (1-2 minutes)
3. Site available at: https://trip-meta.github.io/TripMeta/

### Local Testing

```bash
cd docs/site
python -m http.server 8000
# Open http://localhost:8000/
# Verify locally
curl -I http://localhost:8000/
curl -I http://localhost:8000/vr.mp4
```

## Troubleshooting

### Video not playing
- Verify vr.mp4 is in same directory as index.html
- Check browser console for errors

### GitHub Pages 404
- Verify the GitHub Actions Pages workflow completed
- Verify the workflow artifact path is `docs/site`
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
