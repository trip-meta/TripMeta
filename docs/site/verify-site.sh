#!/bin/bash
# TripMeta Site Verification Script

echo "======================================"
echo "TripMeta GitHub Pages Verification"
echo "======================================"
echo ""

SITE_DIR="D:/project/TripMeta/TripMeta/docs/site"
PASS=0
FAIL=0

# Test 1: Check files exist
echo "1. Checking file structure..."
if [ -f "$SITE_DIR/index.html" ]; then
    echo "   ✓ index.html exists"
    ((PASS++))
else
    echo "   ✗ index.html missing"
    ((FAIL++))
fi

if [ -f "$SITE_DIR/vr.mp4" ]; then
    SIZE=$(du -h "$SITE_DIR/vr.mp4" | cut -f1)
    echo "   ✓ vr.mp4 exists ($SIZE)"
    ((PASS++))
else
    echo "   ✗ vr.mp4 missing"
    ((FAIL++))
fi

if [ -f "$SITE_DIR/.nojekyll" ]; then
    echo "   ✓ .nojekyll exists (bypass Jekyll)"
    ((PASS++))
else
    echo "   ⚠ .nojekyll missing (GitHub Pages might use Jekyll)"
fi

# Test 2: Check HTML content
echo ""
echo "2. Checking HTML content..."

if grep -q '<video' "$SITE_DIR/index.html"; then
    echo "   ✓ Video tag found"
    ((PASS++))
else
    echo "   ✗ Video tag missing"
    ((FAIL++))
fi

if grep -q 'vr.mp4' "$SITE_DIR/index.html"; then
    echo "   ✓ Video source is vr.mp4"
    ((PASS++))
else
    echo "   ✗ Video source not set to vr.mp4"
    ((FAIL++))
fi

if grep -q 'controls' "$SITE_DIR/index.html"; then
    echo "   ✓ Video controls enabled"
    ((PASS++))
else
    echo "   ⚠ Video controls might be disabled"
fi

# Test 3: Check links
echo ""
echo "3. Checking external links..."

if grep -q 'github.com/trip-meta/TripMeta' "$SITE_DIR/index.html"; then
    echo "   ✓ GitHub link correct"
    ((PASS++))
else
    echo "   ✗ GitHub link incorrect"
    ((FAIL++))
fi

# Test 4: Check responsive design
echo ""
echo "4. Checking responsive design..."

if grep -q 'viewport' "$SITE_DIR/index.html"; then
    echo "   ✓ Viewport meta tag present"
    ((PASS++))
else
    echo "   ✗ Viewport meta tag missing"
    ((FAIL++))
fi

if grep -q '@media' "$SITE_DIR/index.html"; then
    echo "   ✓ Media queries present"
    ((PASS++))
else
    echo "   ⚠ No media queries found"
fi

# Summary
echo ""
echo "======================================"
echo "Results: $PASS passed, $FAIL failed"
echo "======================================"

if [ $FAIL -eq 0 ]; then
    echo "✓ All critical checks passed!"
    exit 0
else
    echo "✗ Some checks failed. Please review."
    exit 1
fi
