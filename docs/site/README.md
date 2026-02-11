# GitHub Pages Configuration

## Enable GitHub Pages

1. Go to repository Settings: https://github.com/trip-meta/TripMeta/settings/pages

2. Source settings:
   - **Source**: Deploy from a branch
   - **Branch**: `main`
   - **Folder**: `/docs`

3. Click **Save**

4. After a few minutes, your site will be available at:
   https://trip-meta.github.io/TripMeta/

## Site Structure

```
docs/
├── site/
│   ├── index.html    # Demo page with video player
│   ├── vr.mp4        # Demo video
│   └── poster.jpg    # Video thumbnail (optional)
└── (other documentation)
```

## Custom Domain (Optional)

To use a custom domain:
1. Add a `CNAME` file in `docs/site/` with your domain
2. Configure DNS records
3. Update settings in GitHub Pages
