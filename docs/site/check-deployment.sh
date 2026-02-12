#!/bin/bash
# Check GitHub Pages deployment status

SITE_URL="https://trip-meta.github.io/TripMeta/site/"
FILES=("index.html" "test-page.html" "test-responsive.html" ".nojekyll")

echo "======================================"
echo "Checking GitHub Pages Deployment"
echo "======================================"
echo ""
echo "Site URL: $SITE_URL"
echo ""

for file in "${FILES[@]}"; do
    url="${SITE_URL}${file}"
    echo -n "Checking ${file}... "

    status=$(curl -s -o /dev/null -w "%{http_code}" "$url")

    if [ "$status" = "200" ]; then
        echo "✓ OK (200)"
    elif [ "$status" = "404" ]; then
        echo "✗ Not Found (404)"
    else
        echo "? Status: $status"
    fi
done

echo ""
echo "======================================"
echo "Video file check..."
echo "======================================"

VIDEO_URL="${SITE_URL}vr.mp4"
echo -n "Checking vr.mp4 (this may take a moment)... "

# Just check HEAD, don't download entire file
video_status=$(curl -s -I "$VIDEO_URL" | head -1)

if echo "$video_status" | grep -q "200 OK"; then
    size=$(curl -s -I "$VIDEO_URL" | grep -i "content-length" | cut -d' ' -f2 | tr -d '\r')
    size_mb=$((size / 1024 / 1024))
    echo "✓ OK (${size_mb}MB)"
elif echo "$video_status" | grep -q "404"; then
    echo "✗ Not Found (404)"
else
    echo "? $video_status"
fi

echo ""
echo "======================================"
echo "Deployment Verification Complete"
echo "======================================"
echo ""
echo "Next steps:"
echo "1. Open browser to: $SITE_URL"
echo "2. Verify video plays correctly"
echo "3. Test responsive design on mobile"
echo "4. Check test-page.html for automated results"
