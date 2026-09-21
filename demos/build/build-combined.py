"""Build one deck containing all four Screen A presentations, in narrative order.

Order follows the work, not the directory numbers: basic design -> database
design -> detailed design -> implementation, which is what each deck's
"next in the series" slide already points to.

Read end to end, four stand-alone decks do not flow: each restarts its section
counter at "01", so a reader a third of the way through sees "01 - The session
at a glance" again and loses their place. This generator therefore also
    * inserts a contents slide with each part's page range,
    * strips the restarting section numbers from the recurring eyebrows,
    * stamps "Part N of 4" into every footer,
    * adds one series comparison up front, and
    * replaces the four per-part "what this demo shows" slides -- 24 takeaways,
      read back to back -- with a single series-level one near the end,
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
    # The per-part "what this demo shows" slide is dropped: four of them in a row
    # is 24 takeaways, and they overlap heavily. One series-level slide replaces
    # them, near the end, where a reader can actually weigh them.
    slides = [x for x in slides if 'What this demo shows' not in x]
    parts.append(dict(prefix=prefix, label=label, video=vid, slides=slides))
    print("%-17s %2d slides, %2d images" % (folder, len(slides), len(used)))

# ---- page numbering -------------------------------------------------------
# 1 cover, 2 contents, 3 series comparison, then the parts, then the takeaways.
p = 4
for part in parts:
    part['first'] = p
    p += len(part['slides'])
    part['last'] = p - 1
TOTAL = p  # + the closing takeaways slide

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

# ---- series comparison (page 3) and series takeaways (last page) ----------
COMPARE = """<section class="slide">
  <div class="eyebrow">The series at a glance</div>
  <h2>Eighty-seven minutes of recording, about an hour of agent work</h2>
  <p class="intro">Thirteen typed prompts and four question forms across the four steps. Everything else was the agent
  working through the harness in <code>ai/</code>.</p>
  <div class="body">
    <table style="margin-bottom:16px">
      <tr><th>Part</th><th>Recording</th><th>Prompts</th><th>Agent time</th><th>What it produced</th></tr>
      <tr><td class="id">1 Basic design</td><td>9:59</td><td>2</td><td>6 m 16 s</td>
          <td>Requirements brief, decision log, BD-001 (339 lines), plan, status, evidence</td></tr>
      <tr><td class="id">2 Database design</td><td>10:44</td><td>3</td><td>4 m 19 s</td>
          <td>DB-002 (216 lines), DEC-013\u2013DEC-020, REQ-019, BD-001 revision 3</td></tr>
      <tr><td class="id">3 Detailed design</td><td>15:37</td><td>2</td><td>10 m 23 s</td>
          <td>Four DD documents (1,314 lines) and a mockup published as a private artifact</td></tr>
      <tr><td class="id">4 Implementation</td><td>50:44</td><td>6</td><td>~40 m</td>
          <td>The screen: backend, database, frontend, 133 tests, two pull requests</td></tr>
    </table>
    <div class="grid g2">
      <div class="bar"><div class="lbl">What stayed constant</div>
        <p>Every step read the harness before writing, recorded each decision with its options and rationale, and
        stopped at a gate it had been given in writing \u2014 plan approval, publishing the mockup, merging.</p></div>
      <div class="bar h"><div class="lbl h">What the human did</div>
        <p>Chose between options, and twice asked a question that changed the session: <i>"are there any open questions
        left?"</i> and <i>"there are still 2 templates not output"</i>. Both surfaced things the agent had quietly
        settled on its own.</p></div>
    </div>
  </div>
  <div class="foot"><span>Screen A, end to end \u00b7 WI-002 \u00b7 ProductionManagementAI</span><span>3 / %d</span></div>
</section>"""

TAKEAWAYS = """<section class="slide">
  <div class="eyebrow">What the series shows</div>
  <h2>Nine things to notice</h2>
  <p class="intro">Drawn from all four steps; the part each comes from is named.</p>
  <div class="body">
    <div class="grid g3" style="row-gap:20px">
      <div class="pt"><span class="num">01</span><h4>Short prompts, structured harness</h4>
        <p>Thirteen prompts, most of them one line. <code>AGENTS.md</code> \u2192 workflows \u2192 skills \u2192 templates \u2192
        checklists told the agent what to read and produce. <i>Part 1</i></p></div>
      <div class="pt"><span class="num">02</span><h4>Read before write</h4>
        <p>Instructions, templates and the existing code \u2014 including the running app's own CSS, so the mockup looked
        like the product. <i>Parts 1 and 3</i></p></div>
      <div class="pt"><span class="num">03</span><h4>Humans decide business behaviour</h4>
        <p>The agent recommends; the user chooses. Recommendations were overridden twice, and both overrides were
        recorded as decisions. <i>Parts 1 and 2</i></p></div>
      <div class="pt"><span class="num">04</span><h4>Asking again finds more</h4>
        <p>Twice a re-check by the user surfaced things the agent had silently settled \u2014 three timezone/cancel/seed
        gaps, and two missing design documents. <i>Parts 2 and 3</i></p></div>
      <div class="pt"><span class="num">05</span><h4>Design feeds back upstream</h4>
        <p>A missing quantity bound found while writing the API contract became a decision and a revision of the basic
        design, not a silent choice in code. <i>Part 3</i></p></div>
      <div class="pt"><span class="num">06</span><h4>Fix the rule before the artifact</h4>
        <p>Corrected twice, the agent wrote the preference to memory <i>first</i>, then produced the missing output.
        <i>Parts 3 and 4</i></p></div>
      <div class="pt"><span class="num">07</span><h4>Gates are written, not remembered</h4>
        <p>Publishing the mockup, approving the plan, merging the PRs \u2014 each was a row in a file, honoured without
        being restated in the prompt. <i>Parts 3 and 4</i></p></div>
      <div class="pt"><span class="num">08</span><h4>Proved, not asserted</h4>
        <p>The restricted database login was closed by attempting the forbidden delete and recording the refusal; a
        real focus bug was caught by a test written from the requirement. <i>Part 4</i></p></div>
      <div class="pt"><span class="num">09</span><h4>The harness improves itself</h4>
        <p>Two runs exposed ambiguities in <code>ai/</code> and produced RFCs against it \u2014 with the change to shared
        files offered, not applied. <i>Parts 3 and 4</i></p></div>
    </div>
  </div>
  <div class="foot"><span>Screen A, end to end \u00b7 WI-002 \u00b7 ProductionManagementAI</span><span>%d / %d</span></div>
</section>"""

# ---- assemble, fixing orientation as we go --------------------------------
SECTION_EYEBROW = re.compile(r'(<div class="eyebrow">)\d\d\s*<span class="sep">·</span>\s*')

out, page = [cover, contents, COMPARE % TOTAL], 3
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

out.append(TAKEAWAYS % (TOTAL, TOTAL))

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
