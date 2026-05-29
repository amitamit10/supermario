#!/usr/bin/env python3
"""
Combines all portfolio markdown files into a single PDF.
Usage:  python3 generate_pdf.py
Output: תיק_פרויקט_סופר_מריו.pdf  (in the same directory as this script)
"""

import os
import sys
import markdown
from weasyprint import HTML, CSS

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_PDF  = os.path.join(SCRIPT_DIR, "תיק_פרויקט_סופר_מריו.pdf")

FILES_ORDER = [
    "01_שער.md",
    "02_תקציר.md",
    "03_מבוא.md",
    "04_הגדרת_הבעיה.md",
    "05_ניתוח.md",
    "06_תיכון.md",
    "07_מימוש.md",
    "08_בדיקות.md",
    "09_הוראות_הפעלה.md",
    "10_סיכום.md",
    "11_ביבליוגרפיה.md",
]

CSS_STYLE = """
@import url('https://fonts.googleapis.com/css2?family=Heebo:wght@400;700&display=swap');

@page {
    size: A4;
    margin: 2.5cm 2.5cm 3cm 2.5cm;
    @bottom-center {
        content: counter(page);
        font-size: 10pt;
        color: #555;
    }
}

* {
    box-sizing: border-box;
}

body {
    font-family: 'Heebo', 'Arial Hebrew', 'Arial', sans-serif;
    font-size: 12pt;
    line-height: 1.7;
    color: #1a1a1a;
    direction: rtl;
    text-align: right;
}

/* ---- Page breaks between chapters ---- */
.chapter {
    page-break-before: always;
}
.chapter:first-child {
    page-break-before: avoid;
}

/* ---- Headings ---- */
h1 {
    font-size: 22pt;
    font-weight: 700;
    color: #1a3a6b;
    border-bottom: 3px solid #1a3a6b;
    padding-bottom: 6pt;
    margin-top: 0;
    margin-bottom: 14pt;
}

h2 {
    font-size: 15pt;
    font-weight: 700;
    color: #1a3a6b;
    margin-top: 20pt;
    margin-bottom: 8pt;
    border-right: 4px solid #f0a500;
    padding-right: 8pt;
}

h3 {
    font-size: 12pt;
    font-weight: 700;
    color: #2d5a9b;
    margin-top: 14pt;
    margin-bottom: 4pt;
}

/* ---- Cover page special ---- */
.cover h1 {
    font-size: 28pt;
    text-align: center;
    border: none;
    margin-top: 60pt;
    color: #1a3a6b;
}
.cover h2 {
    text-align: center;
    border: none;
    padding: 0;
    color: #444;
}
.cover p {
    text-align: center;
}
.cover table {
    margin: 30pt auto;
}

/* ---- Tables ---- */
table {
    width: 100%;
    border-collapse: collapse;
    margin: 12pt 0;
    font-size: 10.5pt;
}

th {
    background-color: #1a3a6b;
    color: white;
    padding: 6pt 10pt;
    text-align: right;
    font-weight: 700;
}

td {
    padding: 5pt 10pt;
    border-bottom: 1px solid #dde;
    vertical-align: top;
}

tr:nth-child(even) td {
    background-color: #f4f6fb;
}

/* ---- Code blocks ---- */
pre {
    background: #f0f2f8;
    border: 1px solid #ccd;
    border-right: 4px solid #1a3a6b;
    padding: 10pt 12pt;
    font-family: 'Courier New', monospace;
    font-size: 9pt;
    line-height: 1.5;
    overflow-wrap: break-word;
    white-space: pre-wrap;
    direction: ltr;
    text-align: left;
    margin: 10pt 0;
    border-radius: 4px;
}

code {
    font-family: 'Courier New', monospace;
    font-size: 9.5pt;
    background: #eef0f8;
    padding: 1pt 4pt;
    border-radius: 3px;
}

/* ---- Screenshot placeholders ---- */
.screenshot-placeholder {
    background: #fff8e1;
    border: 2px dashed #f0a500;
    border-radius: 6px;
    padding: 18pt 14pt;
    margin: 14pt 0;
    color: #7a5f00;
    font-size: 10.5pt;
    text-align: center;
    min-height: 60pt;
}

/* ---- Horizontal rules ---- */
hr {
    border: none;
    border-top: 1px solid #ccd;
    margin: 16pt 0;
}

/* ---- Block quotes ---- */
blockquote {
    border-right: 4px solid #f0a500;
    margin: 10pt 0 10pt 20pt;
    padding: 6pt 12pt;
    color: #444;
    background: #fafbff;
}

/* ---- Lists ---- */
ul, ol {
    padding-right: 24pt;
    padding-left: 0;
}

li {
    margin-bottom: 3pt;
}

/* ---- Strong emphasis ---- */
strong {
    color: #1a3a6b;
}

p {
    margin: 6pt 0;
}
"""

def read_md(filename):
    path = os.path.join(SCRIPT_DIR, filename)
    with open(path, encoding="utf-8") as f:
        return f.read()

def process_screenshot_placeholders(html):
    """Convert // צילום מסך — text comments into styled placeholder boxes."""
    import re
    # Match lines like: <p>// צילום מסך — some description</p>
    pattern = re.compile(
        r'<p>//\s*צילום\s+מסך\s*[—–-]\s*(.+?)</p>',
        re.DOTALL | re.IGNORECASE
    )
    def replace(m):
        desc = m.group(1).strip()
        return (
            f'<div class="screenshot-placeholder">'
            f'📸 <strong>צילום מסך:</strong> {desc}'
            f'</div>'
        )
    return pattern.sub(replace, html)

def md_to_html(md_text, is_cover=False):
    html = markdown.markdown(
        md_text,
        extensions=["tables", "fenced_code", "nl2br"],
        output_format="html",
    )
    css_class = "chapter cover" if is_cover else "chapter"
    return f'<section class="{css_class}">\n{html}\n</section>\n'

def build_full_html():
    parts = []
    for i, fname in enumerate(FILES_ORDER):
        md = read_md(fname)
        is_cover = (i == 0)
        parts.append(md_to_html(md, is_cover))

    body = "\n".join(parts)
    body = process_screenshot_placeholders(body)

    return f"""<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
<meta charset="utf-8"/>
<title>תיק פרויקט — סופר מריו</title>
</head>
<body>
{body}
</body>
</html>"""

def main():
    print("📖 מרכיב את תיק הפרויקט...")
    html = build_full_html()

    print("🖨️  מייצר PDF...")
    css = CSS(string=CSS_STYLE)
    HTML(string=html, base_url=SCRIPT_DIR).write_pdf(
        OUTPUT_PDF,
        stylesheets=[css],
    )

    size_kb = os.path.getsize(OUTPUT_PDF) // 1024
    print(f"✅ נוצר: {OUTPUT_PDF}  ({size_kb} KB)")

if __name__ == "__main__":
    main()
