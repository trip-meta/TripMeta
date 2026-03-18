#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
TripMeta Site Optimization Script
Applies modern SaaS design system to index.html
"""

import re
import os
import sys
from pathlib import Path

# Set UTF-8 encoding for Windows console
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

# SVG icons for emojis
SVG_ICONS = {
    'rocket': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M12 2L2 7l10 5 10-5 10 5-10-5L12 2z"/>
        <path d="M4.5 13.5L12 21l7.5-7.5"/>
    </svg>''',
    'vr': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="3"/>
        <path d="M12 1v6m0 6v6m-6-6h12"/>
        <path d="M12 15l-4 4m4-4l4 4"/>
    </svg>''',
    'chip': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <rect x="4" y="4" width="16" height="16" rx="2"/>
        <path d="M9 9h6m-6 4h6m-6 4h6"/>
    </svg>''',
    'microphone': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M12 1a3 3 0 0 0-3 3v4a3 3 0 0 0 3 3 4 4 0 0 0-3-3V4a3 3 0 0 0-3 3-3z"/>
        <path d="M19 10v2a7 7 0 0 1-7 7h-4a7 7 0 0 1-7-7v-2"/>
        <line x1="12" y1="19" x2="12" y2="23"/>
    </svg>''',
    'target': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <circle cx="12" cy="12" r="3"/>
        <path d="M12 2v4m0 16v4"/>
    </svg>''',
    'book': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/>
        <path d="M6.5 2H11v6H6.5a2.5 2.5 0 0 0-2.5 2.5V11h11V4.5a2.5 2.5 0 0 0-2.5-2.5z"/>
    </svg>''',
    'globe': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <path d="M12 2a14.5 14.5 0 0 1 0 0v20a14.5 14.5 0 0 1-20 0"/>
        <path d="M2 12h20"/>
    </svg>''',
    'game-controller': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <rect x="2" y="6" width="20" height="12" rx="2"/>
        <circle cx="7" cy="12" r="1"/>
        <circle cx="17" cy="12" r="1"/>
        <path d="M7 12v2m10-2v2"/>
    </svg>''',
    'eye': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M2 12s3-7 10-7 10 7 10 7-3-7-10-7-10 7z"/>
        <circle cx="12" cy="12" r="3"/>
    </svg>''',
    'zap': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <polygon points="13 2 3 14 12 22 14 11 22 11 12 13 2"/>
    </svg>''',
    'chat': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M21 15a2 2 0 0 1-2-2H7l-4 4V5a2 2 0 0 1 2-2h11a2 2 0 0 1 2 2z"/>
        <path d="M3 7v6l6-3v-3"/>
    </svg>''',
    'speech': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 3 3 4 4 0 0 0-3-3V4a3 3 0 0 0-3 3-3z"/>
        <path d="M19 10v2a7 7 0 0 1-7 7h-4a7 7 0 0 1-7-7v-2"/>
        <line x1="12" y1="19" x2="12" y2="23"/>
    </svg>''',
    'brain': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M9.5 2A2.5 2.5 0 0 0 7 4.5v15a2.5 2.5 0 0 0 2.5-2.5h7a2.5 2.5 0 0 0 2.5 2.5v-15A2.5 2.5 0 0 0 14.5 2h-7z"/>
        <path d="M9.5 7c0 1.4.6 2.5 2.5 2.5s2.5-1.1 2.5-2.5"/>
        <path d="M14.5 7c0 1.4.6 2.5 2.5 2.5s2.5-1.1 2.5-2.5"/>
    </svg>''',
    'language': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <path d="M2 12h20"/>
        <path d="M12 2v20"/>
        <path d="M12 2l4 4m-4-4l-4 4"/>
    </svg>''',
    'sparkles': '''<svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M12 3l1.5 4.5M12 3l-1.5 4.5M12 3v6M12 21l-1.5-4.5M12 21l1.5-4.5M12 21v-6"/>
        <path d="M4.5 9l4.5-1.5M9 4.5L7.5 9M4.5 15l4.5 1.5M9 19.5l-1.5-4.5M19.5 9l-4.5 1.5M15 4.5l1.5 4.5M19.5 15l-4.5-1.5M15 19.5l1.5-4.5"/>
    </svg>''',
    'star': '''<svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
        <polygon points="12 2 15.09 8.26 22 9.27 17 14.18 17 17 17 17 17 14.18 22 9.27 15.09 8.26 12 2"/>
    </svg>''',
    'document': '''<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
        <polyline points="14 2 14 8 22 8"/>
        <line x1="16" y1="13" x2="8" y2="13"/>
        <line x1="16" y1="17" x2="8" y2="17"/>
        <line x1="16" y1="9" x2="8" y2="9"/>
    </svg>''',
    'github': '''<svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
        <path d="M9 19c-5 1.5-5-5-5-7 3-5 5-5 7-5 5-1.5 5-5 7z"/>
        <polyline points="16 18 22 12 22 12"/>
    </svg>''',
    'play': '''<svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
        <polygon points="5 3 19 12 5 12 5 21 12 19 12 5 3"/>
    </svg>''',
    'info': '''<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="16" x2="12" y2="12"/>
        <line x1="12" y1="8" x2="12.01" y2="8"/>
    </svg>''',
}

def optimize_site():
    """Main optimization function"""
    file_path = Path("D:/project/TripMeta/TripMeta/docs/site/index.html")

    print("Reading file...")
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Remove metaverse-related CSS
    print("Removing metaverse CSS...")
    content = re.sub(r'/\* Metaverse Preview Section \*/[\s\S]*?(?=\n\s*/\s|\n\s*</style>)', '', content, flags=re.DOTALL)

    # 2. Fix duplicate zh key in translations
    print("Fixing duplicate translations...")
    content = re.sub(
        r'},\s*zh:\s*\{[^}]*},\s*zh:\s*\{',
        '},',
        content,
        flags=re.DOTALL
    )

    # 3. Remove metaverse translations from both zh objects
    print("Cleaning metaverse translations...")
    patterns_to_remove = [
        r'\s*// Metaverse translations\s*\n\s*metaverseBadge:[^,}]+,\n\s*metaverseTitle:[^,}]+,\n\s*metaverseDesc:[^}]+,\n\s*npcHistorian:[^,}]+,\n\s*npcFood:[^,}]+,\n\s*npcCulture:[^,}]+,\n\s*npcTransport:[^,}]+,\n\s*npcPhoto:[^,}]+,\n\s*npcStay:[^,}]+,\n\s*metaverseCta:[^}]+\n',
        r'metaverseBadge:[^,}]+,\s*',
        r'metaverseTitle:[^,}]+,\s*',
        r'metaverseDesc:[^,}]+,\s*',
        r'npcHistorian:[^,}]+,\s*',
        r'npcFood:[^,}]+,\s*',
        r'npcCulture:[^,}]+,\s*',
        r'npcTransport:[^,}]+,\s*',
        r'npcPhoto:[^,}]+,\s*',
        r'npcStay:[^,}]+,\s*',
        r'metaverseCta:[^}]+\n',
    ]
    for pattern in patterns_to_remove:
        content = re.sub(pattern, '', content, flags=re.MULTILINE | re.DOTALL)

    # 4. Replace emoji icons with SVG icons in HTML
    print("Adding SVG icons...")

    # Header emoji
    content = re.sub(r'<h1>🚀 TripMeta</h1>',
                 '<h1><span class="icon-inline">{rocket}</span> TripMeta</h1>'.format(rocket=SVG_ICONS['rocket']),
                 content)

    # Info icons
    emoji_to_svg = {
        '🤖': ('chip', 'AI Tour Guide'),
        '🥽': ('vr', 'Immersive VR'),
        '🎯': ('target', 'Natural Interaction'),
        '📚': ('book', 'Rich Knowledge'),
        '🎮': ('game-controller', 'Unity 2021.3'),
        '🧠': ('brain', 'GPT-4'),
        '🤖': ('microphone', 'Azure Speech'),
        '👁️': ('eye', 'Computer Vision'),
        '⚡': ('zap', 'URP'),
        '🌍': ('globe', 'Virtual Attractions'),
        '💬': ('chat', 'Smart Conversations'),
        '🎓': ('sparkles', 'Rich Knowledge'),
        '🌐': ('language', 'Multi-Language'),
    }

    for emoji, (icon_name, alt_text) in emoji_to_svg.items():
        svg = SVG_ICONS.get(icon_name, emoji)
        # Replace in both info-item divs and elsewhere
        content = re.sub(
            fr'<div class="info-icon">{re.escape(emoji)}</div>',
            f'<div class="info-icon svg-icon" title="{alt_text}">{svg}</div>',
            content
        )
        content = re.sub(
            fr'<div class="tech-icon">{re.escape(emoji)}</div>',
            f'<div class="tech-icon svg-icon" title="{alt_text}">{svg}</div>',
            content
        )
        content = re.sub(
            fr'<div class="feature-icon">{re.escape(emoji)}</div>',
            f'<div class="feature-icon svg-icon" title="{alt_text}">{svg}</div>',
            content
        )
        content = re.sub(
            fr'<div class="capability-icon">{re.escape(emoji)}</div>',
            f'<div class="capability-icon svg-icon" title="{alt_text}">{svg}</div>',
            content
        )

    # Video play button
    content = re.sub(
        r'<span[^>]*data-i18n="videoTitle"[^>]*>▶ VR Demo</span>',
        '<span data-i18n="videoTitle" class="video-title-with-icon">{play} VR Demo</span>'.format(play=SVG_ICONS['play']),
        content
    )

    # CTA and footer icons
    content = re.sub(
        r'⭐ Star on GitHub',
        '{star} Star on GitHub'.format(star=SVG_ICONS['star']),
        content
    )
    content = re.sub(
        r'📖 View Architecture',
        '{document} View Architecture'.format(document=SVG_ICONS['document']),
        content
    )
    content = re.sub(
        r'💬 Try AI Tour Guide',
        '{chat} Try AI Tour Guide'.format(chat=SVG_ICONS['chat']),
        content
    )
    content = re.sub(
        r'🤖 AI-Powered Experience',
        '{chip} AI-Powered Experience'.format(chip=SVG_ICONS['chip']),
        content
    )
    content = re.sub(
        r'🤖',
        SVG_ICONS['chip'],
        content
    )

    # 5. Add cursor-pointer to clickable elements
    print("Adding cursor-pointer to interactive elements...")

    # Add cursor-pointer to .info-item if not present
    if 'cursor' not in content.split('.info-item {')[1].split('}')[0] if '.info-item {' in content else '':
        content = re.sub(
            r'(\.info-item \{[^}]*?)(\})',
            r'\1    cursor: pointer;\2',
            content,
            flags=re.DOTALL
        )

    # Add cursor-pointer to .tech-item
    if 'cursor' not in content.split('.tech-item {')[1].split('}')[0] if '.tech-item {' in content else '':
        content = re.sub(
            r'(\.tech-item \{[^}]*?)(\})',
            r'\1    cursor: pointer;\2',
            content,
            flags=re.DOTALL
        )

    # Add cursor-pointer to .feature
    if 'cursor' not in content.split('.feature {')[1].split('}')[0] if '.feature {' in content else '':
        content = re.sub(
            r'(\.feature \{[^}]*?)(\})',
            r'\1    cursor: pointer;\2',
            content,
            flags=re.DOTALL
        )

    # Add cursor-pointer to .stat-card
    if 'cursor' not in content.split('.stat-card {')[1].split('}')[0] if '.stat-card {' in content else '':
        content = re.sub(
            r'(\.stat-card \{[^}]*?)(\})',
            r'\1    cursor: pointer;\2',
            content,
            flags=re.DOTALL
        )

    # Add cursor-pointer to .capability-card
    if 'cursor' not in content.split('.capability-card {')[1].split('}')[0] if '.capability-card {' in content else '':
        content = re.sub(
            r'(\.capability-card \{[^}]*?)(\})',
            r'\1    cursor: pointer;\2',
            content,
            flags=re.DOTALL
        )

    # 6. Optimize transition durations to be 200-300ms
    print("Optimizing transitions...")
    content = re.sub(r'transition: all 0\.3s ease', r'transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1)', content)
    content = re.sub(r'transition: all 0\.3s ease', r'transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1)', content)

    # 7. Fix GitHub links (https:// to https://)
    print("Fixing GitHub links...")
    content = re.sub(r'href="https://github\.com/', r'href="https://github.com/', content)
    content = re.sub(r'href="https://github\.com/', r'href="https://github.com/', content)

    # 8. Add SVG icon CSS styles
    print("Adding SVG icon styles...")

    svg_styles = '''
        /* SVG Icons */
        .icon-inline {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 40px;
            height: 40px;
            margin-right: 12px;
            vertical-align: middle;
        }

        .icon-inline svg {
            width: 100%;
            height: 100%;
        }

        .video-title-with-icon {
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .svg-icon {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 48px;
            height: 48px;
        }

        .svg-icon svg {
            width: 100%;
            height: 100%;
            color: var(--primary);
        }

        .info-item .svg-icon svg,
        .tech-item .svg-icon svg,
        .feature .svg-icon svg,
        .capability-card .svg-icon svg {
            color: var(--primary);
        }

        .header h1 {
            display: inline-flex;
            align-items: center;
            gap: 12px;
        }
'''

    # Insert SVG styles before </style>
    content = re.sub(r'(\s*)</style>', svg_styles + r'\1', content, count=1)

    # 9. Enhance contrast for better readability
    print("Enhancing contrast...")
    content = re.sub(r'--text-muted: #64748B;', '--text-muted: #94A3B8;', content)
    content = re.sub(r'color: #a0aec0;', 'color: #cbd5e1;', content)
    content = re.sub(r'color: #667eea;', 'color: var(--primary);', content)

    # 10. Clean up AI showcase section - make it more modern
    content = re.sub(
        r'\.ai-showcase \{[^}]*margin: 60px 0;',
        '.ai-showcase {',
        content
    )
    content = re.sub(
        r'\.ai-showcase \{',
        '.ai-showcase {\n            margin: 60px 0;\n            padding: 50px;\n            background: linear-gradient(135deg, rgba(14, 165, 233, 0.1) 0%, rgba(56, 189, 248, 0.1) 100%);\n            border-radius: 24px;\n            border: 1px solid var(--glass-border);',
        content
    )

    # 11. Update preset question buttons to remove emoji text
    print("Updating preset buttons...")
    preset_replacements = [
        ('📍 What\'s special here?', 'What\'s special here?'),
        ('🏛️ Tell me about history', 'Tell me about history'),
        ('🎨 Recommend nearby attractions', 'Recommend nearby attractions'),
        ('💡 How do I get there?', 'How do I get there?'),
        ('🌐 What\'s the weather like?', 'What\'s the weather like?'),
    ]
    for old, new in preset_replacements:
        content = content.replace(old, new)

    # 12. Write optimized content
    print("Writing optimized file...")
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

    print("[OK] Optimization complete!")
    return True

if __name__ == "__main__":
    try:
        optimize_site()
        print("\n" + "-"*50)
        print("Next steps:")
        print("1. Open docs/site/index.html in browser to verify")
        print("2. Check all interactive elements have cursor-pointer")
        print("3. Verify SVG icons display correctly")
        print("4. Test language switching")
        print("5. Commit changes with: git add docs/site/index.html")
        print("-"*50)
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()
