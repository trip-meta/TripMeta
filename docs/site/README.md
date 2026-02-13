# TripMeta Demo Site

**Live Demo**: https://trip-meta.github.io/TripMeta/site/

---

## Overview

This directory contains the TripMeta demo website hosted on GitHub Pages.

## Files

| File | Purpose | Size |
|-------|-----------|-------|
| `index.html` | Main demo page with video player | ~24 KB |
| `vr.mp4` | VR demo video (6.7 MB) | 6.7 MB |
| `.nojekyll` | Bypass Jekyll processing for GitHub Pages | - |
| `DEPLOYMENT.md` | Deployment documentation | ~2 KB |
| `test-page.html` | Automated testing page | ~6.5 KB |
| `test-responsive.html` | Responsive design tester | ~5 KB |
| `verify-site.sh` | CLI verification script | ~2.5 KB |
| `check-deployment.sh` | Deployment status checker | ~1.7 KB |
| `create-poster.html` | Poster image generator | ~1.4 KB |

## Features

### Main Demo (index.html)

**Content Sections:**
- Header with badge and gradient title
- Video player with size controls (Small/Medium/Large)
- Feature highlights (AI Tour Guide, VR, Interaction, Knowledge)
- Technology stack showcase (Unity, PICO, GPT, Azure, Vision, URP)
- Project metrics (90 FPS, <20ms, 6+ AI Services, 4 Languages)
- Key features grid (6 items)
- CTA buttons (GitHub + Architecture)
- Footer with links and copyright

**Video Controls:**
- Small: 100% width (mobile optimized)
- Medium: 720px max (default, balanced)
- Large: 900px max (desktop)
- Selection saved to localStorage
- Maintains 16:9 aspect ratio

**Visual Features:**
- CSS variables for consistent theming
- Glassmorphism cards with backdrop-filter
- Gradient text effects
- Hover animations (translateY + shadow)
- Page load animations (fadeInUp/fadeInDown)
- Responsive breakpoints (480px, 768px, 1024px, 1440px)

**SEO:**
- Meta description tag
- Semantic HTML structure
- Proper heading hierarchy

### Testing Pages

#### test-page.html
Automated verification page that checks:
- Video file accessibility
- HTML structure validation
- CSS loading verification
- Link validity testing
- Responsive viewport detection

**Usage:**
```bash
# Local test
python -m http.server 8000
# Open http://localhost:8000/test-page.html
```

#### test-responsive.html
Interactive responsive design tester with:
- Device presets (Mobile 375px, Tablet 768px, Laptop 1024px, Desktop 1440px)
- Real-time viewport information
- Live preview in iframe

**Usage:**
```bash
# Local test
python -m http.server 8000
# Open http://localhost:8000/test-responsive.html
```

### Verification Scripts

#### verify-site.sh
Command-line verification tool that checks:
- File structure (index.html, vr.mp4, .nojekyll)
- HTML content (video tag, source, controls)
- External links (GitHub URL correctness)
- Responsive design (viewport meta, media queries)

**Usage:**
```bash
cd docs/site
bash verify-site.sh
```

#### check-deployment.sh
Deployment status checker that verifies:
- All site files return HTTP 200
- Video file is accessible
- Deployment is successful

**Usage:**
```bash
cd docs/site
bash check-deployment.sh
```

## Local Development

### Starting a Local Server

```bash
# Navigate to site directory
cd docs/site

# Start Python HTTP server (default port 8000)
python -m http.server

# Or specify a different port
python -m http.server 8001
```

**Access URLs:**
- Main demo: http://localhost:8000/index.html
- Test page: http://localhost:8000/test-page.html
- Responsive test: http://localhost:8000/test-responsive.html

### Making Changes

1. Edit HTML/CSS files
2. Test locally using above methods
3. Run verification script: `bash verify-site.sh`
4. Commit changes to Git
5. Push to GitHub
6. Wait 1-2 minutes for GitHub Pages deployment

## Deployment

### GitHub Pages Configuration

**Settings:**
- Source: Deploy from a branch
- Branch: `main`
- Folder: `/docs` (not `/docs/site`)
- Custom domain: (optional)

**Note:** The site is accessed at `https://trip-meta.github.io/TripMeta/site/` because GitHub Pages serves from the `/docs` folder.

### Automatic Deployment

When you push to the `main` branch:
1. GitHub Pages automatically rebuilds (1-2 minutes)
2. No CI/CD pipeline needed for static files
3. Site updates automatically

### Verifying Deployment

After pushing changes:

```bash
# Run deployment check
cd docs/site
bash check-deployment.sh
```

Expected output:
```
======================================
Checking GitHub Pages Deployment
======================================
Checking index.html... ✓ OK (200)
Checking vr.mp4... ✓ OK (6MB)
======================================
```

## Troubleshooting

### Video Not Playing

**Symptoms:**
- Video shows error or won't play
- Controls are missing

**Solutions:**
1. Check vr.mp4 is in same directory as index.html
2. Verify file permissions (readable)
3. Check browser console for error messages
4. Try different browser (Chrome, Firefox, Edge)

### Styles Not Loading

**Symptoms:**
- Page looks unstyled
- Colors appear wrong

**Solutions:**
1. Clear browser cache (Ctrl+Shift+Delete)
2. Check CSS is within `<style>` tags (no external CSS file)
3. Verify CSS variables are defined correctly

### GitHub Pages 404

**Symptoms:**
- Getting 404 errors
- "Page not found" message

**Solutions:**
1. Verify GitHub Pages source is set to `/docs` folder
2. Wait 1-2 minutes for deployment
3. Check repository settings > Pages
4. Verify branch is `main`, not `master`

### Jekyll Interference

**Symptoms:**
- Files not loading
- Underscores in filenames causing issues

**Solutions:**
1. Ensure `.nojekyll` file exists in `docs/site/`
2. Check Jekyll isn't processing files incorrectly

## Performance

### Optimization Techniques Used

1. **CSS Variables** - Single source of truth for colors and spacing
2. **aspect-ratio** - Modern CSS for video container sizing
3. **backdrop-filter** - GPU-accelerated glass effect
4. **clamp()** - Fluid typography without media query overload
5. **transform + will-change** - Optimize animation performance
6. **preload="metadata"** - Video loads on demand, not full upfront

### Metrics

| Metric | Value |
|---------|-------|
| HTML Size | ~24 KB (gzipped: ~6 KB) |
| Video Size | 6.7 MB |
| First Contentful Paint | <1s target |
| Time to Interactive | <2s target |
| Lighthouse Score | Target: 90+ |

## Browser Compatibility

| Browser | Version | Support |
|---------|----------|---------|
| Chrome | 90+ | ✅ Full |
| Firefox | 88+ | ✅ Full |
| Safari | 14+ | ✅ Full |
| Edge | 90+ | ✅ Full |
| Mobile Safari | 14+ | ✅ Full |
| Chrome Android | Latest | ✅ Full |

**Features Used:**
- CSS Grid
- CSS Flexbox
- CSS Variables
- aspect-ratio property
- backdrop-filter
- localStorage API

## Maintenance

### Regular Tasks

- [ ] Update demo video with latest footage
- [ ] Add new feature highlights to site
- [ ] Review and update technology stack
- [ ] Check for broken links quarterly
- [ ] Optimize video compression

### Content Updates

To update the demo video:
1. Replace `vr.mp4` with new file
2. Test playback locally
3. Commit and push changes

To update project info:
1. Edit `index.html`
2. Verify with test-page.html
3. Commit and push changes

---

**Last Updated**: 2025-02-12
