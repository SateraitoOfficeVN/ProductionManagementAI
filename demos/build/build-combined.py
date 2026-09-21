"""Build one deck containing all four Screen A presentations, in narrative order.

Order follows the work, not the directory numbers: basic design -> database
design -> detailed design -> implementation, which is what each deck's
"next in the series" slide already points to.
"""
import os, re, shutil

HERE = os.path.dirname(os.path.abspath(__file__))
B = os.path.join(HERE, 'decks') + os.sep
OUT = B + 'combined' + os.sep
os.makedirs(OUT + 'img', exist_ok=True)

DECKS = [
    ('bd', 'basic-design',    'Basic design'),
    ('db', 'database-design', 'Database design'),
    ('dd', 'detailed-design', 'Detailed design'),
    ('im', 'implementation',  'Implementation'),
]

styles, sections, n_img = [], [], 0

for prefix, folder, label in DECKS:
    src = open(B + folder + '/deck.html', encoding='utf-8').read()

    styles.append(re.search(r'<style>(.*?)</style>', src, re.S).group(1))

    body = src[src.index('<body>') + 6: src.index('</body>')]
    # namespace the images so the four sets can share one folder
    used = sorted(set(re.findall(r'src="img/([\w.-]+)"', body)))
    for u in used:
        shutil.copy(B + folder + '/img/' + u, OUT + 'img/%s_%s' % (prefix, u))
    body = re.sub(r'src="img/([\w.-]+)"', lambda m: 'src="img/%s_%s"' % (prefix, m.group(1)), body)
    n_img += len(used)

    slides = re.findall(r'<section class="slide.*?</section>', body, re.S)
    sections.append((label, slides))
    print("%-17s %2d slides, %2d images" % (folder, len(slides), len(used)))

# ---- cover ----------------------------------------------------------------
cover = """<section class="slide title">
  <div class="eyebrow">ProductionManagementAI <span class="sep">·</span> WI-002 <span class="sep">·</span> Screen A, end to end</div>
  <div style="display:grid;grid-template-columns:1fr 300px;gap:46px;margin-top:64px">
    <div>
      <h1>One screen,<br>four steps</h1>
      <p class="lead" style="margin-top:22px">Production-order create/edit, from a one-line prompt to a merged pull
      request — basic design, database design, detailed design and implementation, each recorded and transcribed.</p>
      <p style="margin-top:24px;color:#8C98A4;font-size:9.5px">All four presentations in one file, <b>in the order the
      work ran</b> — so the demo numbers on the title slides read 01, 03, 02, 04. Each step keeps its own title
      slide, and its step numbers match its edited video.<br>
      Recorded 2026-09-18 · Claude Code (Opus 5) in the JetBrains Rider terminal, auto mode on.</p>
    </div>
    <div style="padding-top:10px">
      <span class="chip">Basic design — 3:49 video</span>
      <span class="chip">Database design — 3:59 video</span>
      <span class="chip">Detailed design — 4:00 video</span>
      <span class="chip">Implementation — 5:59 video</span>
      <span class="chip" style="background:none;color:#78BEB9;padding-left:0">17:48 of video · 64 slides</span>
    </div>
  </div>
</section>"""

all_slides = [cover] + [s for _, slides in sections for s in slides]
total = len(all_slides)

# ---- continuous footers ---------------------------------------------------
page = 0
fixed = []
for s in all_slides:
    page += 1
    if 'class="foot"' in s:
        s = re.sub(r'(<div class="foot"><span>.*?</span><span>)[^<]*(</span></div>)',
                   lambda m: m.group(1) + '%d / %d' % (page, total) + m.group(2), s, flags=re.S)
    fixed.append(s)

css = "\n".join(styles)
html = ("<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n"
        "<title>Screen A, end to end — WI-002</title>\n<style>\n"
        "/* Combined deck: the four step decks share one theme; their style blocks are\n"
        "   concatenated here so every slide renders exactly as it does on its own. */\n"
        + css + "\n</style>\n</head>\n<body>\n\n" + "\n\n".join(fixed) + "\n\n</body>\n</html>\n")

open(OUT + 'deck.html', 'w', encoding='utf-8').write(html)
print("\ncombined: %d slides, %d images, %.0f KB of html" % (total, n_img, len(html) / 1024))
