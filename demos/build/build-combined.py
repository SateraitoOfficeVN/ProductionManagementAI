"""Build one deck containing all four Screen A presentations, in narrative order.

Order follows the work, not the directory numbers: basic design -> database
design -> detailed design -> implementation, which is what each deck's
"next in the series" slide already points to.

Read end to end, four stand-alone decks do not flow: each restarts its section
counter at "01", so a reader a third of the way through sees "01 - The session
at a glance" again and loses their place. This generator therefore also
    * inserts a contents slide with each part's page range,
    * strips the restarting section numbers from the recurring eyebrows, and
    * stamps "Part N of 4" into every footer,
so the combined file reads as one document rather than four stapled together.
"""
import os, re, shutil

HERE = os.path.dirname(os.path.abspath(__file__))
B = os.path.join(HERE, 'decks') + os.sep
OUT = B + 'combined' + os.sep

DECKS = [
    ('bd', 'basic-design',    'Basic design',    '3:49'),
    ('db', 'database-design', 'Database design', '3:59'),
    ('dd', 'detailed-design', 'Detailed design', '4:00'),
    ('im', 'implementation',  'Implementation',  '5:59'),
]

BLURB = {
    'bd': 'Requirements brief, decision log and a 339-line basic design',
    'db': 'The schema, eight decisions and a new requirement',
    'dd': 'Four design documents and a published mockup',
    'im': 'The screen itself: 133 tests, two pull requests',
}

os.makedirs(OUT + 'img', exist_ok=True)

styles, parts, n_img = [], [], 0

for prefix, folder, label, vid in DECKS:
    src = open(B + folder + os.sep + 'deck.html', encoding='utf-8').read()
    styles.append(re.search(r'<style>(.*?)</style>', src, re.S).group(1))

    body = src[src.index('<body>') + 6: src.index('</body>')]
    used = sorted(set(re.findall(r'src="img/([\w.-]+)"', body)))
    for u in used:
        shutil.copy(B + folder + os.sep + 'img' + os.sep + u, OUT + 'img' + os.sep + '%s_%s' % (prefix, u))
    body = re.sub(r'src="img/([\w.-]+)"', lambda m: 'src="img/%s_%s"' % (prefix, m.group(1)), body)
    n_img += len(used)

    slides = re.findall(r'<section class="slide.*?</section>', body, re.S)
    parts.append(dict(prefix=prefix, label=label, video=vid, slides=slides))
    print("%-17s %2d slides, %2d images" % (folder, len(slides), len(used)))

# ---- page numbering -------------------------------------------------------
# 1 cover, 2 contents, then each part in turn.
page_of = {}
p = 3
for part in parts:
    part['first'] = p
    p += len(part['slides'])
    part['last'] = p - 1
TOTAL = p - 1

# ---- cover ----------------------------------------------------------------
cover = """<section class="slide title">
  <div class="eyebrow">ProductionManagementAI <span class="sep">·</span> WI-002 <span class="sep">·</span> Screen A, end to end</div>
  <div style="display:grid;grid-template-columns:1fr 300px;gap:46px;margin-top:64px">
    <div>
      <h1>One screen,<br>four steps</h1>
      <p class="lead" style="margin-top:22px">Production-order create/edit, from a one-line prompt to a merged pull
      request — basic design, database design, detailed design and implementation, each recorded and transcribed.</p>
      <p style="margin-top:24px;color:#8C98A4;font-size:9.5px">All four presentations in one file, <b>in the order the
      work ran</b>. Each part keeps its own title slide, and its step numbers match its edited video.<br>
      Recorded 2026-09-18 · Claude Code (Opus 5) in the JetBrains Rider terminal, auto mode on.</p>
    </div>
    <div style="padding-top:10px">
""" + "\n".join(
    '      <span class="chip">%s — %s video</span>' % (x['label'], x['video']) for x in parts
) + """
      <span class="chip" style="background:none;color:#78BEB9;padding-left:0">17:48 of video · %d slides</span>
    </div>
  </div>
</section>""" % TOTAL

# ---- contents -------------------------------------------------------------
rows = "\n".join(
    """        <tr><td class="id">Part %d</td><td><b>%s</b><br><span style="font-size:9px">%s</span></td>
            <td>%s</td><td class="id">%d–%d</td></tr>"""
    % (i + 1, x['label'], BLURB[x['prefix']], x['video'], x['first'], x['last'])
    for i, x in enumerate(parts))

contents = """<section class="slide">
  <div class="eyebrow">Contents</div>
  <h2>Four parts, in the order the work ran</h2>
  <p class="intro">Each part is a self-contained presentation with its own recorded video. The parts connect: every
  one ends by naming what the next picks up.</p>
  <div class="body">
    <table style="margin-bottom:18px">
      <tr><th>Part</th><th>Step</th><th>Video</th><th>Pages</th></tr>
%s
    </table>
    <div class="grid g2">
      <div class="bar"><div class="lbl">How to read a part</div>
        <p>A title slide, then <i>the session at a glance</i>, then <i>the walkthrough</i> — a grid of the video's
        steps — then one slide per step, and finally what the step shows and where it leaves the project.</p></div>
      <div class="bar h"><div class="lbl h">Reading the slides</div>
        <p><b style="color:var(--amber)">Amber</b> marks what the human did; <b style="color:var(--teal)">teal</b>
        marks what the agent did. Every step slide carries a <code>VIDEO mm:ss</code> badge pointing into that part's
        edited video.</p></div>
    </div>
  </div>
  <div class="foot"><span>Screen A, end to end · WI-002 · ProductionManagementAI</span><span>2 / %d</span></div>
</section>""" % (rows, TOTAL)

# ---- assemble, fixing orientation as we go --------------------------------
SECTION_EYEBROW = re.compile(r'(<div class="eyebrow">)\d\d\s*<span class="sep">·</span>\s*')

out, page = [cover, contents], 2
for i, part in enumerate(parts, start=1):
    for s in part['slides']:
        page += 1
        # the recurring "01 ... 04" section counters restart in every deck; drop them
        s = SECTION_EYEBROW.sub(r'\1', s)
        # continuous footer: part, step name, page of total
        if 'class="foot"' in s:
            s = re.sub(
                r'<div class="foot"><span>(.*?)</span><span>[^<]*</span></div>',
                lambda m: '<div class="foot"><span>Part %d of 4 <span class="sep">·</span> %s</span>'
                          '<span>%d / %d</span></div>' % (i, m.group(1), page, TOTAL),
                s, flags=re.S)
        out.append(s)

css = "\n".join(styles)
html = ('<!doctype html>\n<html lang="en">\n<head>\n<meta charset="utf-8">\n'
        '<title>Screen A, end to end — WI-002</title>\n<style>\n'
        '/* Combined deck: the four part decks share one theme; their style blocks are\n'
        '   concatenated here so every slide renders exactly as it does on its own. */\n'
        + css + '\n</style>\n</head>\n<body>\n\n' + "\n\n".join(out) + '\n\n</body>\n</html>\n')

open(OUT + 'deck.html', 'w', encoding='utf-8').write(html)
print("\ncombined: %d slides, %d images, %.0f KB of html" % (TOTAL, n_img, len(html) / 1024))
for i, x in enumerate(parts, start=1):
    print("   part %d  %-17s pages %2d-%2d" % (i, x['label'], x['first'], x['last']))
