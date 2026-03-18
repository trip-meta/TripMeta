#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
TripMeta Site Optimization Script v2
Applies modern SaaS design system to index.html
"""

import re
import sys
import io
from pathlib import Path

# Set UTF-8 encoding for Windows console
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

# Optimized SVG icons (inline format)
SVG_ICONS = {
    'header': '<span class="header-icon"><svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2L2 7l10 5 10-5 10 5-10-5L12 2z"/><path d="M4.5 13.5L12 21l7.5-7.5"/></svg></span>',
    'play': '<svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><polygon points="5 3 19 12 5 12 5 21 12 19 12 5 3"/></svg>',
    'ai': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="4" y="4" width="16" height="16" rx="2"/><path d="M9 9h6m-6 4h6m-6 4h6"/></svg>',
    'vr': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M12 1v6m0 6v6m-6-6h12"/><path d="M12 15l-4 4m4-4l4 4"/></svg>',
    'target': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><circle cx="12" cy="12" r="3"/><path d="M12 2v4m0 16v4"/></svg>',
    'book': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H11v6H6.5a2.5 2.5 0 0 0-2.5 2.5V11h11V4.5a2.5 2.5 0 0 0-2.5-2.5z"/></svg>',
    'unity': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="6" width="20" height="12" rx="2"/><circle cx="7" cy="12" r="1"/><circle cx="17" cy="12" r="1"/><path d="M7 12v2m10-2v2"/></svg>',
    'gpt': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9.5 2A2.5 2.5 0 0 0 7 4.5v15a2.5 2.5 0 0 0 2.5-2.5h7a2.5 2.5 0 0 0 2.5 2.5v-15A2.5 2.5 0 0 0 14.5 2h-7z"/><path d="M9.5 7c0 1.4.6 2.5 2.5s2.5-1.1 2.5-2.5"/><path d="M14.5 7c0 1.4.6 2.5 2.5s2.5-1.1 2.5-2.5"/></svg>',
    'speech': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 3 3 4 4 0 0 0-3-3V4a3 3 0 0 0-3 3-3z"/><path d="M19 10v2a7 7 0 0 1-7 7h-4a7 7 0 0 1-7-7v-2"/><line x1="12" y1="19" x2="12" y2="23"/></svg>',
    'vision': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M2 12s3-7 10-7 10 7 10 7-3-7-10-7-10 7z"/><circle cx="12" cy="12" r="3"/></svg>',
    'zap': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="13 2 3 14 12 22 14 11 22 11 12 13 2"/></svg>',
    'globe': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M2 12h20"/></svg>',
    'chat': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15a2 2 0 0 1-2-2H7l-4 4V5a2 2 0 0 1 2-2h11a2 2 0 0 1 2 2z"/><path d="M3 7v6l6-3v-3"/></svg>',
    'sparkles': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 3l1.5 4.5M12 3l-1.5 4.5M12 3v6M12 21l-1.5-4.5M12 21l1.5-4.5M12 21v-6"/></svg>',
    'game': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="6" width="20" height="12" rx="2"/><circle cx="7" cy="12" r="1"/><circle cx="17" cy="12" r="1"/><path d="M7 12v2m10-2v2"/></svg>',
    'lang': '<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M2 12h20"/><path d="M12 2v20"/><path d="M12 2l4 4m-4-4l-4 4"/></svg>',
    'star': '<svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><polygon points="12 2 15.09 8.26 22 9.27 17 14.18 17 17 17 17 14.18 22 9.27 15.09 8.26 12 2"/></svg>',
    'doc': '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 22 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><line x1="16" y1="9" x2="8" y2="9"/></svg>',
}

CSS_ADDITIONS = '''
        /* SVG Icons Styles */
        .header-icon {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 48px;
            height: 48px;
            margin-right: 12px;
            vertical-align: middle;
        }

        .header-icon svg {
            width: 100%;
            height: 100%;
            color: var(--primary);
        }

        .video-title-with-icon {
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .video-title-with-icon svg {
            width: 18px;
            height: 18px;
            color: var(--text-white);
        }

        .icon-svg {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 48px;
            height: 48px;
            flex-shrink: 0;
        }

        .icon-svg svg {
            width: 100%;
            height: 100%;
            color: var(--primary);
            stroke-width: 1.5;
        }

        .info-item, .tech-item, .feature, .capability-card, .stat-card {
            cursor: pointer;
        }

        .ai-title-icon {
            display: inline-flex;
            align-items: center;
            gap: 10px;
        }

        .ai-title-icon svg {
            width: 32px;
            height: 32px;
        }

        .cta-btn-icon {
            width: 20px;
            height: 20px;
            margin-right: 8px;
        }

        .cta-btn-icon svg {
            width: 100%;
            height: 100%;
        }
'''

def optimize_site():
    """Main optimization function"""
    file_path = Path("D:/project/TripMeta/TripMeta/docs/site/index.html")

    print("Reading file...")
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Remove metaverse CSS and broken comment
    print("Removing metaverse CSS...")
    content = re.sub(r'/\* Metaverse Preview Section \*/\s*</style>', '</style>', content, flags=re.DOTALL)

    # 2. Fix duplicate zh key in translations (second zh after en)
    print("Fixing duplicate translations...")
    content = re.sub(r'},\s*zh:\s*\{[^}]*metaverseBadge:[^}]+\n\s*metaverseTitle:[^}]+\n', '},', content, flags=re.MULTILINE | re.DOTALL)

    # 3. Remove metaverse translations from first zh object
    print("Cleaning metaverse translations from zh...")
    content = re.sub(r',\s*metaverseBadge:[^}]+\n\s*metaverseTitle:[^}]+\n\s*metaverseDesc:[^}]+\n', '', content)

    # 4. Replace header emoji
    print("Adding SVG icons...")
    content = re.sub(r'<h1>🚀 TripMeta</h1>', '<h1>' + SVG_ICONS['header'] + ' TripMeta</h1>', content)

    # 5. Replace video title
    content = re.sub(
        r'>▶ VR Demo</',
        '>' + SVG_ICONS['play'] + ' VR Demo</',
        content
    )

    # 6. Replace AI title emoji
    content = re.sub(
        r'🤖 AI-Powered Experience',
        SVG_ICONS['ai'] + ' AI-Powered Experience',
        content
    )

    # 7. Replace AI tour guide emoji
    content = re.sub(r'🤖', SVG_ICONS['ai'], content)

    # 8. Replace info icons
    content = re.sub(r'<div class="info-icon">🥽</div>', '<div class="info-icon icon-svg">' + SVG_ICONS['vr'] + '</div>', content)
    content = re.sub(r'<div class="info-icon">🎯</div>', '<div class="info-icon icon-svg">' + SVG_ICONS['target'] + '</div>', content)
    content = re.sub(r'<div class="info-icon">📚</div>', '<div class="info-icon icon-svg">' + SVG_ICONS['book'] + '</div>', content)

    # 9. Replace tech icons
    content = re.sub(r'<div class="tech-icon">🎮</div>', '<div class="tech-icon icon-svg">' + SVG_ICONS['unity'] + '</div>', content)
    content = re.sub(r'<div class="tech-icon">🧠</div>', '<div class="tech-icon icon-svg">' + SVG_ICONS['gpt'] + '</div>', content)
    content = re.sub(r'<div class="tech-icon">🤤</div>', '<div class="tech-icon icon-svg">' + SVG_ICONS['speech'] + '</div>', content)
    content = re.sub(r'<div class="tech-icon">👁️</div>', '<div class="tech-icon icon-svg">' + SVG_ICONS['vision'] + '</div>', content)
    content = re.sub(r'<div class="tech-icon">⚡</div>', '<div class="tech-icon icon-svg">' + SVG_ICONS['zap'] + '</div>', content)

    # 10. Replace feature icons
    content = re.sub(r'<div class="feature-icon">🌍</div>', '<div class="feature-icon icon-svg">' + SVG_ICONS['globe'] + '</div>', content)
    content = re.sub(r'<div class="feature-icon">💬</div>', '<div class="feature-icon icon-svg">' + SVG_ICONS['chat'] + '</div>', content)
    content = re.sub(r'<div class="feature-icon">🎓</div>', '<div class="feature-icon icon-svg">' + SVG_ICONS['sparkles'] + '</div>', content)
    content = re.sub(r'<div class="feature-icon">🎮</div>', '<div class="feature-icon icon-svg">' + SVG_ICONS['game'] + '</div>', content)

    # 11. Replace capability icons
    content = re.sub(r'<div class="capability-icon">💬</div>', '<div class="capability-icon icon-svg">' + SVG_ICONS['chat'] + '</div>', content)
    content = re.sub(r'<div class="capability-icon">🧠</div>', '<div class="capability-icon icon-svg">' + SVG_ICONS['gpt'] + '</div>', content)
    content = re.sub(r'<div class="capability-icon">🎯</div>', '<div class="capability-icon icon-svg">' + SVG_ICONS['target'] + '</div>', content)
    content = re.sub(r'<div class="capability-icon">🌐</div>', '<div class="capability-icon icon-svg">' + SVG_ICONS['lang'] + '</div>', content)

    # 12. Replace CTA icons
    content = re.sub(r'⭐ Star on GitHub', SVG_ICONS['star'] + ' Star on GitHub', content)
    content = re.sub(r'📖 View Architecture', SVG_ICONS['doc'] + ' View Architecture', content)
    content = re.sub(r'💬 Try AI Tour Guide', SVG_ICONS['chat'] + ' Try AI Tour Guide', content)

    # 13. Fix GitHub links
    print("Fixing GitHub links...")
    content = re.sub(r'href="https://github\.com/', r'href="https://github.com/', content)

    # 14. Remove emojis from preset questions
    print("Cleaning preset buttons...")
    preset_patterns = [
        ('📍 What\'s special here?', 'What\'s special here?'),
        ('🏛️ Tell me about history', 'Tell me about history'),
        ('🎨 Recommend nearby attractions', 'Recommend nearby attractions'),
        ('💡 How do I get there?', 'How do I get there?'),
        ('🌐 What\'s weather like?', 'What\'s weather like?'),
    ]
    for old, new in preset_patterns:
        content = content.replace(old, new)

    # 15. Remove emojis from AI welcome and try asking
    content = re.sub(r'Hi! I\'m your AI tour guide', 'Hi! I\'m your AI tour guide', content)
    content = re.sub(r'Hi! I\'m your AI tour guide', 'Hi! I\'m your AI tour guide', content)

    # 16. Add CSS additions before </style>
    print("Adding CSS improvements...")
    content = re.sub(r'(\s*)</style>', CSS_ADDITIONS + r'\1', content, count=1)

    # 17. Write optimized content
    print("Writing optimized file...")
    with open(file_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(content)

    print("[OK] Site optimization complete!")
    return True

if __name__ == "__main__":
    try:
        optimize_site()
        print("\n" + "-"*50)
        print("Next steps:")
        print("1. Open docs/site/index.html in browser to verify")
        print("2. Check all interactive elements work correctly")
        print("3. Verify SVG icons display correctly")
        print("4. Test language switching")
        print("5. Test video player controls")
        print("6. Commit when ready: git add docs/site/index.html")
        print("-"*50)
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()
