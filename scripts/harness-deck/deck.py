"""Builds the 16:9 presentation PDF of ai/harness-overview.md with ReportLab, in English or Japanese.

    python scripts/harness-deck/deck.py docs/en/presentations/ai-harness-overview.pdf en
    python scripts/harness-deck/deck.py docs/ja/presentations/ai-harness-overview.pdf ja

Slide content follows ai/harness-overview.md (last refreshed 2026-09-23); update both when the harness changes.
Japanese strings live in ja_strings.py next to this file. Needs reportlab and the Windows fonts Segoe UI, Consolas and
BIZ UDGothic (C:/Windows/Fonts).
"""
import re
import sys
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor, white
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import Paragraph
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.enums import TA_LEFT

OUT = sys.argv[1]
LANG = sys.argv[2] if len(sys.argv) > 2 else "en"
JA = LANG == "ja"
if JA:
    sys.path.insert(0, str(__import__("pathlib").Path(__file__).resolve().parent))
    from ja_strings import J
else:
    J = {}


def t(s):
    if JA and s not in J:
        raise KeyError(f"no Japanese for: {s!r}")
    return J.get(s, s)


W, H = 960, 540
F = "C:/Windows/Fonts/"
pdfmetrics.registerFont(TTFont("JP", F + "BIZ-UDGothicR.ttc", subfontIndex=0))
pdfmetrics.registerFont(TTFont("JP-B", F + "BIZ-UDGothicB.ttc", subfontIndex=0))
if JA:
    # The Japanese edition sets all text in BIZ UDGothic; the UI font names map onto it.
    for name, file in [("UI", "BIZ-UDGothicR.ttc"), ("UI-SL", "BIZ-UDGothicR.ttc"), ("UI-SB", "BIZ-UDGothicB.ttc"),
                       ("UI-B", "BIZ-UDGothicB.ttc"), ("UI-I", "BIZ-UDGothicR.ttc")]:
        pdfmetrics.registerFont(TTFont(name, F + file, subfontIndex=0))
else:
    pdfmetrics.registerFont(TTFont("UI", F + "segoeui.ttf"))
    pdfmetrics.registerFont(TTFont("UI-SL", F + "segoeuisl.ttf"))
    pdfmetrics.registerFont(TTFont("UI-SB", F + "seguisb.ttf"))
    pdfmetrics.registerFont(TTFont("UI-B", F + "segoeuib.ttf"))
    pdfmetrics.registerFont(TTFont("UI-I", F + "segoeuii.ttf"))
pdfmetrics.registerFont(TTFont("MONO", F + "consola.ttf"))
pdfmetrics.registerFont(TTFont("MONO-B", F + "consolab.ttf"))
from reportlab.pdfbase.pdfmetrics import registerFontFamily
registerFontFamily("UI", normal="UI", bold="UI-SB", italic="UI-I", boldItalic="UI-SB")
registerFontFamily("MONO", normal="MONO", bold="MONO-B", italic="MONO", boldItalic="MONO-B")

# Palette: cool paper ground, graphite ink, one deep-teal accent, amber reserved for "gates".
GROUND = HexColor("#F3F5F7")
SURFACE = white
INK = HexColor("#16212C")
MUTED = HexColor("#5A6674")
LINE = HexColor("#D3DAE1")
ACCENT = HexColor("#0C6B68")
ACCENT_SOFT = HexColor("#DCEEEC")
GATE = HexColor("#B7770A")
GATE_SOFT = HexColor("#F7ECD6")
DARK = HexColor("#111B24")

CJK = re.compile(r"([\u3000-\u30ff\u4e00-\u9fff\uff01-\uff5e]+)")


def rich(text):
    """Mini markup: `code`, **bold**, CJK runs switched to the JP font. Literal <, >, & are escaped."""
    text = text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
    text = re.sub(r"`([^`]+)`", r'<font name="MONO" color="#0C6B68">\1</font>', text)
    text = re.sub(r"\*\*([^*]+)\*\*", r"<b>\1</b>", text)
    if JA:
        return text
    return CJK.sub(r'<font name="JP">\1</font>', text)


def style(size=13, color=INK, font="UI", leading=None):
    if JA:
        leading = (leading or size * 1.38) * 1.08
    return ParagraphStyle("s", fontName=font, fontSize=size, leading=leading or size * 1.38,
                          textColor=color, alignment=TA_LEFT, wordWrap="CJK" if JA else None)


def para(c, text, x, y_top, w, st=None):
    """Draw a wrapped paragraph with its top at y_top; return its height."""
    p = Paragraph(rich(text), st or style())
    _, h = p.wrap(w, 1000)
    p.drawOn(c, x, y_top - h)
    return h


def rect(c, x, y, w, h, fill=SURFACE, stroke=LINE, r=6, lw=0.8):
    c.setFillColor(fill)
    if stroke is not None:
        c.setStrokeColor(stroke)
    c.setLineWidth(lw)
    c.roundRect(x, y, w, h, r, stroke=1 if stroke is not None else 0, fill=1)


def arrow(c, x1, y1, x2, y2, color=MUTED, lw=1.2):
    import math
    c.setStrokeColor(color)
    c.setFillColor(color)
    c.setLineWidth(lw)
    c.line(x1, y1, x2, y2)
    a = math.atan2(y2 - y1, x2 - x1)
    s = 5.5
    p = c.beginPath()
    p.moveTo(x2, y2)
    p.lineTo(x2 - s * math.cos(a - 0.45), y2 - s * math.sin(a - 0.45))
    p.lineTo(x2 - s * math.cos(a + 0.45), y2 - s * math.sin(a + 0.45))
    p.close()
    c.drawPath(p, fill=1, stroke=0)


page_no = [0]
TOTAL = 14
FOOT = t("The ai/ harness · ProductionManagementAI")


def slide(c, eyebrow, title, subtitle=None):
    page_no[0] += 1
    c.setFillColor(GROUND)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    c.setFillColor(ACCENT)
    c.rect(0, H - 6, W, 6, stroke=0, fill=1)
    para(c, t(eyebrow).upper(), 48, H - 25, 860, style(9.5, MUTED, "MONO" if not JA else "UI", 12))
    para(c, t(title), 48, H - 44, 860, style(26, INK, "UI-SB", 32))
    if subtitle:
        para(c, t(subtitle), 48, H - 84, 860, style(13.5, MUTED, "UI", 19))
    c.setStrokeColor(LINE)
    c.setLineWidth(0.6)
    c.line(48, 30, W - 48, 30)
    c.setFont("UI", 8.5)
    c.setFillColor(MUTED)
    c.drawString(48, 16, FOOT)
    c.drawRightString(W - 48, 16, f"{page_no[0]} / {TOTAL}")


c = canvas.Canvas(OUT, pagesize=(W, H))
c.setTitle(t("The ai/ harness"))
c.setAuthor("ProductionManagementAI")
c.setSubject(t("How Claude and Codex share one process: structure, routing, skills, gates and principles"))

# 1 — Title ----------------------------------------------------------------------------------------------
page_no[0] += 1
c.setFillColor(DARK)
c.rect(0, 0, W, H, stroke=0, fill=1)
c.setFillColor(ACCENT)
c.rect(0, 0, 10, H, stroke=0, fill=1)
c.setFont("MONO", 11)
c.setFillColor(HexColor("#7FB9B5"))
c.drawString(72, 400, "PRODUCTIONMANAGEMENTAI · ai/harness-overview.md")
para(c, t("The ai/ harness"), 72, 385, 640, style(56, white, "UI-SB", 64))
para(c, t("How Claude and Codex share one file-based process — what each part does, how a request flows through it, and why it is built this way."),
     72, 305, 620, style(17, HexColor("#C9D3DC"), "UI-SL", 25))
labels = ["project.md", "policies.md", "rules/", "workflows/", "skills/", "templates/", "checklists/", "evaluations/", "improvements/"]
for i, lab in enumerate(labels):
    yy = 392 - i * 26
    c.setFillColor(HexColor("#1B2A36"))
    c.roundRect(740, yy - 6, 148, 20, 4, stroke=0, fill=1)
    c.setFont("MONO", 10)
    c.setFillColor(HexColor("#7FB9B5") if lab.endswith("/") else HexColor("#C9D3DC"))
    c.drawString(752, yy, "ai/" + lab)
para(c, t("Source: ai/harness-overview.md, as of 2026-09-23 · Reference, not a source of authorization — ai/policies.md remains the authority."),
     72, 60, 820, style(10, HexColor("#8795A3"), "UI", 13))
c.showPage()

# 2 — Why -------------------------------------------------------------------------------------------------
slide(c, "01 · Why it exists", "Two agents, many sessions, no shared memory")
para(c, t("Claude and Codex both work on this repository, often in different sessions. Without a shared, file-based source of instructions, each session would re-derive — or silently disagree about — the things that matter."),
     48, 420, 430, style(14, INK, "UI", 21))
items = [("Scope", "What is in and out of this piece of work"),
         ("Technology choices", "What the project has actually decided vs. what is still open"),
         ("Quality bar", "What “correct” and “done” mean for each kind of change"),
         ("Authorization", "What an agent may do alone, and when it must stop and ask")]
y = 300
for h_, b_ in items:
    c.setFillColor(ACCENT)
    c.circle(56, y - 7, 3, stroke=0, fill=1)
    hh = para(c, f"**{t(h_)}** — {t(b_)}", 68, y, 410, style(12.5, INK, "UI", 17))
    y -= hh + 14
rect(c, 520, 150, 392, 270, fill=ACCENT, stroke=None, r=8)
para(c, t("THE HARNESS’S JOB"), 546, 396, 340, style(10, HexColor("#BFE3E0"), "MONO" if not JA else "UI", 14))
para(c, t("A request plus the repository’s own files should be enough for any agent, in any session, to pick up correct, consistent work — without reading prior chat history."),
     546, 372, 340, style(19, white, "UI-SB", 26))
para(c, t("Context is made **durable** (in files) and **agent-neutral** (one source both agents read)."),
     546, 222, 340, style(12.5, HexColor("#DCEEEC"), "UI", 18))
c.showPage()

# 3 — Anatomy ---------------------------------------------------------------------------------------------
slide(c, "02 · Anatomy", "What lives in ai/ — and why each part exists")
parts = [
    ("project.md", "Confirmed vs. open technology decisions", "No agent invents or assumes a choice the project hasn’t made"),
    ("policies.md", "Authorization, plan approval, pause conditions, untrusted-content guardrails", "The safety boundary: act alone, or stop and ask"),
    ("rules/ · 8 files", "common, documentation, frontend, backend, database, testing, git-review, ci-cd", "One current standard per area, applied by every skill"),
    ("workflows/ · 4", "bootstrap, feature-delivery, bug-fix, harness-improvement", "A starting sequence for each way work begins"),
    ("skills/ · 13", "The “how to do X” procedures", "Bounded, checkable units with one 8-part shape"),
    ("templates/", "Starting document for every skill output", "Modeled on real formats (MADR, IEEE 829, 基本設計書 …)"),
    ("checklists/ · 4", "Point-in-time pass/fail gates", "“Did I follow the rules” becomes an explicit check"),
    ("evaluations/", "Manual scenarios: process rules and adversarial cases", "Guidance changes are tested, not trusted on faith"),
    ("improvements/", "One record per harness change", "Keeps the history of why the harness changed"),
]
cw, ch, gx, gy = 280, 118, 12, 12
for i, (n, what, why) in enumerate(parts):
    col, row = i % 3, i // 3
    x = 48 + col * (cw + gx)
    yb = 392 - row * (ch + gy) - ch + 30
    rect(c, x, yb, cw, ch)
    para(c, "ai/" + t(n), x + 12, yb + ch - 10, cw - 24, style(11, ACCENT, "MONO-B" if not JA else "UI-B", 14))
    para(c, t(what), x + 12, yb + ch - 30, cw - 24, style(11, INK, "UI", 15))
    para(c, t(why), x + 12, yb + 42, cw - 24, style(10, MUTED, "UI-I", 13.5))
c.showPage()

# 4 — Three questions --------------------------------------------------------------------------------------
slide(c, "03 · Separation of concerns", "Three different questions, three different places",
      "Collapsing them would bury authorization inside technical detail — or turn every rule into an unenforced suggestion.")
cols = [("policies.md", "Is this agent allowed to do this, or must it stop and ask?", "Plan approval, pause conditions, external operations (push, PR, merge, deploy), untrusted content is data — never instructions.", ACCENT),
        ("rules/", "What does correct look like for this kind of change?", "RFC 9457 errors, strict TypeScript, parameterized SQL, WCAG 2.2 AA, SHA-pinned actions, test-pyramid shape …", INK),
        ("checklists/", "Right now, before the next step — did I actually meet that bar?", "design-consistency, security-review, delivery, release-readiness: explicit gates at the moment they matter.", GATE)]
for i, (name, q, detail, col) in enumerate(cols):
    x = 48 + i * 292
    top = 380
    rect(c, x, 150, 280, top - 150)
    c.setFillColor(col)
    c.rect(x, top - 5, 280, 5, stroke=0, fill=1)
    c.setFont("MONO-B", 13)
    c.drawString(x + 18, top - 34, "ai/" + name)
    qh = para(c, f"“{t(q)}”" if not JA else f"「{t(q)}」", x + 18, top - 48, 244, style(16, INK, "UI-SB", 22))
    c.setStrokeColor(LINE)
    c.setLineWidth(0.6)
    c.line(x + 18, top - 62 - qh, x + 262, top - 62 - qh)
    para(c, t(detail), x + 18, top - 74 - qh, 244, style(11, MUTED, "UI", 15.5))
c.showPage()

# 5 — Routing chain ----------------------------------------------------------------------------------------
slide(c, "04 · Routing", "How a request actually flows — for either agent",
      "Every task enters through the same chain. The sequence matters: each step narrows what the next one reads.")
steps = [("Entry point", "`AGENTS.md` (shared). `CLAUDE.md` is a thin adapter that points at it — never duplicates it."),
         ("Always read first", "`project.md`, `policies.md`, `rules/common.md`."),
         ("Pick a workflow", "bootstrap · feature-delivery · bug-fix · harness-improvement."),
         ("Pick skill(s)", "`skills/README.md` routes to a `SKILL.md`, read directly (no auto-discovery)."),
         ("Start from a template", "Every skill output begins from `ai/templates/`."),
         ("Pass the checklist", "The skill names the gate it must pass at that point."),
         ("Record durable state", "`status.md`, `decisions.md`, `evidence.md` in `work-items/<WI-###>/`.")]
bw, bh = 114, 188
for i, (h_, b_) in enumerate(steps):
    x = 48 + i * (bw + 10)
    fill = GATE_SOFT if i == 5 else (ACCENT_SOFT if i == 6 else SURFACE)
    rect(c, x, 150, bw, bh, fill=fill)
    c.setFont("UI-B", 22)
    c.setFillColor(GATE if i == 5 else ACCENT)
    c.drawString(x + 10, 150 + bh - 32, str(i + 1))
    hh = para(c, t(h_), x + 10, 150 + bh - 42, bw - 18, style(12, INK, "UI-SB", 15))
    para(c, t(b_), x + 10, 150 + bh - 48 - hh, bw - 16, style(9.6, MUTED, "UI", 13))
    if i < 6:
        arrow(c, x + bw + 1, 150 + bh / 2, x + bw + 9, 150 + bh / 2)
rect(c, 48, 72, 864, 54, fill=SURFACE)
para(c, t("**Outside the per-task chain:** `ai/evaluations/` is how the harness checks itself — run when a change to shared guidance is proposed. Step 7 is what makes work resumable: the next step, possibly another agent in another session, continues from the repository, not from the chat."),
     62, 118, 836, style(11, INK, "UI", 15.5))
c.showPage()

# 6 — Workflows --------------------------------------------------------------------------------------------
slide(c, "05 · Workflows", "Four ways work begins, four starting sequences",
      "Each workflow orders the skills a task needs and ends with an explicit exit condition — without hard-coding every path.")
wfs = [("project-bootstrap", ["Confirm open tech decisions and demo scope", "Plan: skeleton, local env, CI checks", "Build in bounded, approved steps", "Verify real commands; update project.md", "Record gaps; hand off to feature-delivery"]),
       ("feature-delivery", ["Brief + acceptance criteria", "Plan, shown and approved", "Architecture check → BD / DB / API / DD", "Reconcile designs, then implement", "Unit/integration, then system/E2E", "Review diff, PR within authorization", "Record evidence"]),
       ("bug-fix", ["Reproducible failing vs. expected case", "Severity: live incident mitigates first", "Find cause; add a regression check", "Fix; update affected design/contracts", "Targeted checks; record evidence", "Blameless postmortem if live"]),
       ("harness-improvement", ["Record observed failure / change", "Propose the smallest useful change", "Evaluate with ai/evaluations cases", "Record limits; review before adoption", "Rollback via version control — never relax a gate"])]
for i, (name, st) in enumerate(wfs):
    x = 48 + i * 219
    rect(c, x, 60, 209, 340)
    c.setFont("MONO-B", 11)
    c.setFillColor(ACCENT)
    c.drawString(x + 12, 60 + 340 - 24, name)
    y = 60 + 340 - 40
    for j, s in enumerate(st):
        c.setFont("UI-B", 10)
        c.setFillColor(MUTED)
        c.drawString(x + 12, y - 11, f"{j + 1}")
        hh = para(c, t(s), x + 28, y, 170, style(10.6, INK, "UI", 14))
        y -= hh + 10
c.showPage()

# 7 — Skills pipeline --------------------------------------------------------------------------------------
slide(c, "06 · Skills", "13 skills, composed as a resumable hand-off chain",
      "A workflow picks the subset a task needs. Each skill’s step 8 hands its artifact to the next — the chain survives agent and session switches.")
row1 = [("planning", "plan.md · approved"), ("requirements", "brief.md · REQ-###"), ("architecture", "ADR · STRIDE"), ("basic-design", "BD · Mermaid · SVG"), ("database-design", "###_DB")]
row2 = [("detailed-design", "DD + API/FN/SPD · mockup"), ("screen-design", "BD/DD sections"), ("implementation", "branch · src/ · tests/"), ("testing", "TC-### · evidence")]
row3 = [("pr-review", "review.md"), ("security-review", "review / evidence"), ("ci-cd", ".github/workflows")]


def skill_box(x, y, name, out, w=158, h=58, fill=SURFACE, head=INK):
    rect(c, x, y, w, h, fill=fill)
    c.setFont("MONO-B", 11)
    c.setFillColor(head)
    c.drawString(x + 10, y + h - 22, name)
    para(c, t(out), x + 10, y + 24, w - 16, style(9.5, MUTED, "UI", 12))


r1y, r2y, r3y = 330, 232, 134
for i, (n, o) in enumerate(row1):
    x = 48 + i * 178
    skill_box(x, r1y, n, o)
    if i < len(row1) - 1:
        arrow(c, x + 159, r1y + 29, x + 176, r1y + 29)
arrow(c, 48 + 4 * 178 + 79, r1y, 48 + 4 * 178 + 79, r2y + 59)
for i, (n, o) in enumerate(reversed(row2)):
    x = 48 + (i + 1) * 178
    skill_box(x, r2y, n, o)
for i in range(3):
    x = 48 + (4 - i) * 178
    arrow(c, x, r2y + 29, x - 18, r2y + 29)
arrow(c, 48 + 178 + 79, r2y, 48 + 178 + 79, r3y + 60)
for i, (n, o) in enumerate(row3):
    x = 48 + (i + 1) * 178
    skill_box(x, r3y, n, o)
    if i < 2:
        arrow(c, x + 159, r3y + 29, x + 176, r3y + 29)
skill_box(48, r3y, "harness-improvement", "improvement.md → ai/", fill=ACCENT_SOFT, head=ACCENT)
para(c, t("Runs on its own trigger: when the harness itself is wrong or stale."), 48, r3y - 8, 158, style(9, MUTED, "UI-I", 12))
rect(c, 48 + 4 * 178, r3y, 158, 58, fill=GROUND, stroke=None)
para(c, t("Order shown is the normal composition; not every task runs all 13."), 48 + 4 * 178, r3y + 50, 158, style(9.5, MUTED, "UI-I", 13))
c.showPage()

# 8 — Skill shape ------------------------------------------------------------------------------------------
slide(c, "07 · The skill shape", "Every SKILL.md has the same eight parts — on purpose")
shape = [("1–2", "Purpose · inputs & source order", "Bound what the skill needs before it starts."),
         ("3", "Execution steps & applicable rules", "Every step is tied to the rule that governs it — no implicit “current standard”."),
         ("4", "Tools & environment", "Honest about what can’t be done without a missing toolchain."),
         ("5", "Outputs, templates, IDs, locations", "`REQ` `BD` `DD` `DB` `SCR` `TC` `DEC` `WI` IDs and paths are explicit, not guesswork."),
         ("6–7", "Checklist · termination & failure", "“Done” becomes a checkable claim, not an assertion."),
         ("8", "Work-item update & handover", "Makes the pipeline resumable across agents and sessions.")]
for i, (num, h_, b_) in enumerate(shape):
    col, row = i % 2, i // 2
    x = 48 + col * 438
    yb = 322 - row * 122
    rect(c, x, yb, 426, 108)
    c.setFont("UI-B", 26)
    c.setFillColor(ACCENT)
    c.drawString(x + 16, yb + 62, num)
    para(c, t(h_), x + 92, yb + 92, 318, style(13.5, INK, "UI-SB", 18))
    para(c, t(b_), x + 92, yb + 62, 318, style(11, MUTED, "UI", 15.5))
c.showPage()

# 9 — Rules -----------------------------------------------------------------------------------------------
slide(c, "08 · Rules", "Eight rule files: one current standard per area",
      "Skills cite these instead of restating them, so the same change always meets the same bar.")
rules = [("common", "Preserve approved scope · stable IDs, link don’t duplicate · record unresolved decisions · focused changes"),
         ("documentation", "English source · EN + JA PDF of every doc · Japanese UI text quoted with an English gloss · number-first IDs, one folder per number"),
         ("frontend", "Vite + TypeScript, strict (no `any`) · WCAG 2.2 AA · config via import.meta.env, no secrets in the browser"),
         ("backend", ".NET 10 · nullable types · RFC 9457 errors, no leaked detail · structured logs · OpenTelemetry on new endpoints"),
         ("database", "PostgreSQL · parameterized SQL only · expand/contract on live tables · least-privilege app role · recovery limits first"),
         ("testing", "Trace to acceptance criteria · pyramid shape · resettable data · quarantine flaky tests · not run ≠ pass"),
         ("git-review", "Branch + worktree per work item · small focused PRs · no direct push · squash-merge · review ≠ merge"),
         ("ci-cd", "SHA-pinned actions · read-only default permissions · OIDC over long-lived secrets · non-root scanned images")]
for i, (n, b_) in enumerate(rules):
    col, row = i % 2, i // 2
    x = 48 + col * 438
    yb = 348 - row * 86
    rect(c, x, yb, 426, 76)
    c.setFont("MONO-B", 12)
    c.setFillColor(ACCENT)
    c.drawString(x + 14, yb + 52, n + ".md")
    para(c, t(b_), x + 14, yb + 44, 398, style(10.4, INK, "UI", 14))
c.showPage()

# 10 — Checklists as gates ---------------------------------------------------------------------------------
slide(c, "09 · Checklists", "Four gates, each at the moment it matters",
      "Point-in-time pass/fail checks — “did this follow the rules” stops being something held in memory.")
gates = [("design-consistency", "before dependent implementation", ["Stable IDs + acceptance criteria", "DD agrees with BD; API/DB agree", "Decisions resolved; tests map to design", "Security/PII, WCAG, migration, tracing specified"]),
         ("security-review", "before merging a security-relevant change", ["Auth/authz on every changed endpoint", "Input validated; no concatenated queries", "No secrets; vetted dependencies", "No leaky errors; least privilege; threat model"]),
         ("delivery", "before reporting completion", ["Scope and plan revision identifiable", "Checks recorded; not-run has a reason", "Stayed within authorization", "Another agent can continue from the files"]),
         ("release-readiness", "before an authorized deployment", ["Target config and migration plan confirmed", "Rollback executable, not just described", "Smoke criteria and monitoring defined", "Image/version traceable to its PR"])]
c.setStrokeColor(GATE)
c.setLineWidth(2)
c.line(70, 360, 890, 360)
for i, (n, when, checks) in enumerate(gates):
    x = 48 + i * 219
    c.setFillColor(GATE)
    c.circle(x + 104, 360, 7, stroke=0, fill=1)
    c.setFillColor(white)
    c.circle(x + 104, 360, 3, stroke=0, fill=1)
    rect(c, x, 64, 209, 276, fill=SURFACE)
    c.setFont("MONO-B", 11)
    c.setFillColor(GATE)
    c.drawString(x + 12, 64 + 276 - 26, n)
    para(c, t(when), x + 12, 64 + 276 - 34, 186, style(11.5, INK, "UI-SB", 15))
    y = 64 + 276 - 80
    for ck in checks:
        c.setFillColor(GATE)
        c.rect(x + 13, y - 11, 6, 6, stroke=0, fill=1)
        hh = para(c, t(ck), x + 26, y, 172, style(10.4, INK, "UI", 14))
        y -= hh + 9
c.showPage()

# 11 — Templates -------------------------------------------------------------------------------------------
slide(c, "10 · Templates", "Every output starts from a real, named document format",
      "Chosen to match the artifact’s purpose — so the output is useful, not a formality.")
tpl = [("brief.md", "PRD conventions", "REQ-### with success + failure criteria, “not doing”"),
       ("plan.md", "PMI-style implementation plan", "Steps with verification, authorized actions, revisions kept"),
       ("status.md", "RAG status report", "State, accomplishments, risks, single next action"),
       ("decisions.md", "Decision log", "Context, options, answer, rationale, impact"),
       ("evidence.md", "Traceability matrix + test log", "REQ → design → code → test; real results"),
       ("architecture-decision.md", "MADR", "Drivers, options, outcome, consequences"),
       ("basic-design.md", "基本設計書", "Flows, screens, Mermaid diagrams, SVG wireframes"),
       ("detailed-design.md + DD/*", "詳細設計書 family", "Main DD + API · function · screen-processing"),
       ("database-design.md", "テーブル定義書 + ER", "Tables, indexes, constraints, migration limits"),
       ("test-plan.md", "IEEE 829-1998", "Items, approach, cases, results, gaps"),
       ("review.md", "Google code-review categories", "Design, functionality, complexity, tests …"),
       ("improvement.md", "Rust-style RFC", "Motivation, alternatives, evaluation, adoption")]
rect(c, 48, 50, 864, 336)
c.setFont("UI-SB", 10.5)
c.setFillColor(MUTED)
c.drawString(62, 368, t("TEMPLATE"))
c.drawString(292, 368, t("MODELED ON"))
c.drawString(542, 368, t("CAPTURES"))
for i, (tp, m, cap) in enumerate(tpl):
    y = 342 - i * 25.5
    if i % 2 == 0:
        c.setFillColor(GROUND)
        c.rect(49, y - 8, 862, 25.5, stroke=0, fill=1)
    c.setFont("MONO", 10.5)
    c.setFillColor(ACCENT)
    c.drawString(62, y, tp)
    para(c, t(m), 292, y + 11, 240, style(11, INK, "UI-SB", 14))
    para(c, t(cap), 542, y + 11, 360, style(10.5, MUTED, "UI", 14))
c.showPage()

# 12 — Work items ------------------------------------------------------------------------------------------
slide(c, "11 · Durable state", "The work item is the memory — not the chat",
      "Switching from Claude to Codex mid-task means reading the work item and continuing, not restarting.")
files = [("brief.md", "What and why: REQ-### and acceptance criteria"),
         ("plan.md", "Every revision shown and approved before its work; all kept, oldest first"),
         ("status.md", "Current state, risks, the single next action"),
         ("decisions.md", "Every question that stopped an agent, and its answer"),
         ("evidence.md", "What makes “done” checkable after the fact")]
rect(c, 48, 60, 420, 330)
c.setFont("MONO-B", 12)
c.setFillColor(ACCENT)
c.drawString(64, 362, "work-items/<WI-###>/")
for i, (f_, d_) in enumerate(files):
    y = 330 - i * 52
    c.setStrokeColor(LINE)
    c.line(64, y + 16, 64, y - 22)
    c.line(64, y - 4, 76, y - 4)
    c.setFont("MONO-B", 11.5)
    c.setFillColor(INK)
    c.drawString(82, y, f_)
    para(c, t(d_), 82, y - 6, 370, style(10.5, MUTED, "UI", 14))
para(c, t("+ design docs under `docs/en/…` (BD, DB, DD, ADR, tests), each with EN and JA PDFs"), 64, 84, 390, style(10.5, MUTED, "UI-I", 14))
c.setFont("UI-SB", 12.5)
c.setFillColor(INK)
c.drawString(500, 362, t("status.md states"))
states = ["draft", "awaiting-plan-review", "ready", "in-progress", "in-review", "done"]
y = 334
for i, s in enumerate(states):
    fill = ACCENT if s == "done" else SURFACE
    rect(c, 500, y - 24, 190, 30, fill=fill, stroke=ACCENT if s != "done" else None, r=15)
    c.setFont("MONO", 11)
    c.setFillColor(white if s == "done" else ACCENT)
    c.drawCentredString(595, y - 13, s)
    if i < len(states) - 1:
        arrow(c, 595, y - 25, 595, y - 34, color=ACCENT)
    y -= 44
rect(c, 720, 164, 192, 34, fill=GATE_SOFT, stroke=GATE, r=17)
c.setFont("MONO", 11)
c.setFillColor(GATE)
c.drawCentredString(816, 176, "blocked")
arrow(c, 718, 182, 692, 202, color=GATE)
para(c, t("Any active state can become **blocked** — recorded with its owner and impact."), 720, 150, 192, style(10, MUTED, "UI", 13.5))
c.showPage()

# 13 — Principles ------------------------------------------------------------------------------------------
slide(c, "12 · Design principles", "Five principles behind every file")
pr = [("One shared, agent-neutral source", "Process lives only in ai/. Adapters (AGENTS.md, CLAUDE.md, .claude/, a .codex/ if ever needed) route to it and never duplicate it — agent-specific tooling stays out of shared skills."),
      ("Markdown is the contract, not the executor", "The harness defines what correct and done mean. Scripts, test runners and GitHub Actions do the work — and their real output is recorded, not assumed."),
      ("An approved plan is the scope of authority", "Steps don’t need re-approval, but the plan’s scope and the pause conditions are hard limits. Content an agent merely read is never authorization."),
      ("A conclusion requires evidence", "“Done”, “passing”, “deployed” are claimed only after a recorded, actually-run check. Not run is recorded as not run."),
      ("The harness improves itself, on the record", "Gaps are fixed through the harness-improvement skill and evaluation cases — never as a silent edit to shared guidance.")]
for i, (h_, b_) in enumerate(pr):
    y = 430 - i * 76
    c.setFont("UI-B", 30)
    c.setFillColor(HexColor("#8FC3BF"))
    c.drawString(52, y - 38, f"{i + 1}")
    c.setFillColor(ACCENT)
    c.rect(96, y - 58, 3, 58, stroke=0, fill=1)
    hh = para(c, t(h_), 112, y - 2, 780, style(14.5, INK, "UI-SB", 19))
    para(c, t(b_), 112, y - 4 - hh, 780, style(11, MUTED, "UI", 15))
c.showPage()

# 14 — Where this stands -----------------------------------------------------------------------------------
slide(c, "13 · Where this stands", "Four runs of the chain — Screens A, B and C are done")
rect(c, 48, 60, 470, 360, fill=SURFACE, stroke=ACCENT, lw=1.2)
c.setFillColor(ACCENT)
c.rect(48, 400, 470, 20, stroke=0, fill=1)
para(c, t("WI-001 – WI-004 · WHAT EACH RUN EXERCISED"), 62, 416, 440, style(9.5, white, "MONO" if not JA else "UI", 12))
runs = [("WI-001", "project-bootstrap: skeleton, auth foundation, local Compose environment, CI"),
        ("WI-002 · Screen A", "The first end-to-end feature: requirements → plan → BD → DB → four DD documents + mockup → code → unit, integration and E2E tests with axe → checklists → PR"),
        ("WI-003 · Screen B", "Building on an existing screen: evolving a live schema, reusing shared modules and endpoints, amending Screen A’s design"),
        ("WI-004 · Screen C", "A read-only dashboard over one consistent snapshot, a design revised after mockup review, and a navbar on every screen")]
y = 386
for head, body in runs:
    c.setFillColor(ACCENT)
    c.circle(68, y - 8, 2.8, stroke=0, fill=1)
    hh = para(c, f"**{t(head)}** — {t(body)}", 80, y, 422, style(11, INK, "UI", 15.5))
    y -= hh + 14
para(c, t("Each run is recorded in `work-items/WI-00N/`."), 62, 84, 440, style(10.5, MUTED, "UI-I", 14))
para(c, t("Where it stands"), 548, 420, 364, style(14.5, INK, "UI-SB", 19))
notes = [("CI runs green", "Backend, frontend and E2E (Compose + Playwright) on every pull request to master; documentation-only changes skip it."),
         ("Gaps fixed on the record", "What the chain exposed was fixed through harness-improvement, not by silent edits to shared guidance."),
         ("Open project decisions", "Deployment host, merge/deploy permissions, demo videos, the full role matrix — the owner’s to make.")]
y = 392
for head, body in notes:
    hh = para(c, f"**{t(head)}** — {t(body)}", 548, y, 364, style(11, INK, "UI", 15.5))
    y -= hh + 14
rect(c, 548, 60, 364, 108, fill=ACCENT_SOFT, stroke=None, r=6)
para(c, t("NEXT"), 564, 156, 332, style(9.5, ACCENT, "MONO" if not JA else "UI", 12))
para(c, t("The locked roadmap is complete and no next work item is planned. The untested part of the harness is deployment: release-readiness and the ci-cd skill’s deployment half wait on a deployment target."),
     564, 138, 332, style(11.5, INK, "UI-SB", 16))
c.showPage()

c.save()
print("pages", page_no[0])
