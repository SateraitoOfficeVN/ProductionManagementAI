<!-- Based on ai/templates/improvement.md. -->

# RFC: English and Japanese PDFs of every document under docs/

**Status:** under-review
**Affected:** `ai/rules/documentation.md`, `ai/project.md`, the seven document-producing skills (`requirements`, `architecture`, `basic-design`, `database-design`, `detailed-design`, `screen-design`, `testing`), `ai/checklists/design-consistency.md`, `ai/checklists/delivery.md`, `ai/evaluations/baseline-cases.md`, `ai/harness-overview.md`, `ai/templates/basic-design.md`, `work-items/WI-001/decisions.md` (DEC-013), `scripts/docs-pdf.py` (new), `scripts/README.md`, `docs/README.md`, `docs/en/README.md`, `docs/ja/README.md`, root `README.md`, `CLAUDE.md`

## Summary

The Markdown under `docs/en/` stays the single, English-only source. Every document there is also published as two
PDFs, rendered in the same change as the document: an English PDF under `docs/en/pdf/` and a Japanese PDF under
`docs/ja/pdf/`, each at the document's path under `docs/en/`. The Japanese PDF is rendered from a translation made at
generation time, which is not committed. A new script, `scripts/docs-pdf.py`, renders both, including Mermaid diagrams
and embedded images. Documents that already exist get their PDFs the next time they change. This settles the
Japanese-translation-sync policy, an open project decision since WI-001 (DEC-013).

## Motivation

On 2026-09-23 the user asked that "the docs output will also output jp version instead of only english". The design
documents follow Japanese SI conventions (基本設計書, 詳細設計書, テーブル定義書), and Japanese readers need to read
them directly.

The first draft of this RFC answered that with a mirrored Japanese Markdown file per document (`docs/ja/<same
path>.md`), and three were written for 001_BD–003_BD. Later the same day the user changed direction: "change it up from
create 2 md file version to output pdf file from the en md from both en and jp instead of create new md file and keep it
under 'pdf' folder of the language folder in docs". Two Markdown copies of every document double what has to be edited
and reviewed, and can drift apart. What readers need is a document to read in each language, not a second source.

Before either draft, the harness said "English by default, optional Japanese translation". `docs/ja/` held only a
placeholder README, and no skill, checklist or evaluation asked for Japanese at all.

## Guide-level explanation

When a skill writes or changes a document under `docs/en/` (a brief's requirements, an ADR, a BD, the four DD files, a
DB design, a test report), the agent, in the same change:

1. Renders the English PDF:
   `python scripts/docs-pdf.py docs/en/<path>.md docs/en/pdf/<path>.pdf --source-note "<ID> version <N> (<date>)"`.
2. Translates the English Markdown into Japanese in a temporary file outside the repository, then renders it:
   `python scripts/docs-pdf.py <tmp>.md docs/ja/pdf/<path>.pdf --lang ja --base docs/en/<folder> --source-note "…"`.

The translation keeps the English structure (sections, numbering, tables, diagrams) and its relative links and image
paths, which `--base` resolves against the English document's folder. Prose, headings, table headers, descriptions and
Mermaid labels are translated. IDs, code, identifiers, routes, file paths, API and column names, enum values, and quoted
UI text stay as they are; the UI is English, so a quoted label or message stays English, with a Japanese gloss where it
helps. (Amended by WI-005, see "Amendment 1" below: the UI is now Japanese.) The footer of every page names the document, its revision and, for the Japanese PDF, the English source, so a
stale PDF is visible.

A small edit to a document that has no PDFs yet still produces both. `README.md` index files, work-item records, the
HTML mockups, and files that are already PDFs are not rendered. No Japanese Markdown is committed.

`scripts/docs-pdf.py` converts the Markdown with Python's `markdown` package, draws ```` ```mermaid ```` blocks with
Mermaid 11.17.2 loaded from jsDelivr, resolves images relative to `--base`, and prints with headless Chrome: A4
landscape (the BD item tables have 12 columns), a Japanese font stack for `--lang ja`, and page numbers in the footer.

## Reference-level explanation

- **Current behavior:** English only. `ai/rules/documentation.md` says "Write new system artifacts in English unless
  another language is requested" and "translations identify their source revision". `ai/project.md` lists "optional
  Japanese translation" as confirmed and the translation-sync policy as open (WI-001 DEC-013). No skill output section,
  checklist or evaluation mentions Japanese, and there is no tool to render a document.
- **Proposed behavior:**
  - `ai/rules/documentation.md`: the rule in full: English-only Markdown, the two PDF paths, how to render each, the
    footer revision, what the translation keeps and translates, render-on-first-edit, and what is not covered.
  - `scripts/docs-pdf.py` (new) and `scripts/README.md`; `ai/project.md` lists the command under the verified
    commands.
  - The seven document-producing skills: their output section adds the two PDFs.
  - `ai/checklists/design-consistency.md` and `ai/checklists/delivery.md`: every added or changed document has been
    re-rendered and its PDFs name the current revision.
  - `ai/evaluations/baseline-cases.md`: two new cases, one for a new document and one for a small edit to a document
    with no PDFs.
  - `ai/templates/basic-design.md`: the wireframes stay English and appear in the Japanese PDF unchanged (with RFC
    0009).
  - `ai/project.md`: the confirmed line becomes this rule; the open decision is removed. `work-items/WI-001/decisions.md`
    DEC-013 is marked decided.
  - `docs/README.md`, `docs/en/README.md`, `docs/ja/README.md`, the root `README.md`, `CLAUDE.md` and
    `ai/harness-overview.md`: describe the PDFs and stop calling translation sync open.
  - Every design document that exists today gets both PDFs in this change: the three BDs and three main DDs (changed by RFC 0009), and, at the reviewer's request, the nine DD companions (API, FN, SPD) and the four DB designs. That is 19 documents and 38 PDFs. ADRs and README index files have none yet.
- **Definition of done:** the rule is in the documentation rules, every document-producing skill, both checklists and
  the evaluations; the script renders a document with Mermaid, images and Japanese text; no file describes translation
  sync as open; no Japanese Markdown exists under `docs/ja/`; the next document written under `docs/en/` arrives with
  both PDFs without anyone asking.

## Drawbacks

- PDFs are binary. Git can't show what changed in them, the repository grows with every re-render, and two branches that
  both re-render a document conflict on the PDF. The Markdown diff remains the thing to review.
- The Japanese translation isn't kept, so it is redone from scratch on every edit and can word the same passage
  differently from one revision to the next. Reviewing it means reading the PDF.
- Rendering needs Python's `markdown` package, Chrome, and network access to jsDelivr for Mermaid. An agent without
  them can't meet the rule and has to record the step as blocked.
- Nothing checks automatically that a PDF is current; the footer's revision and the checklist are the only guards.

## Rationale and alternatives

- **A Japanese Markdown file per document (this RFC's first draft):** rejected by the reviewer. It doubles the source to
  maintain and review, and the copies can drift.
- **English PDF only, Japanese on request:** rejected; the request is for both languages on every document.
- **Commit the Japanese translation next to the PDF so it can be diffed and reused:** not done. It would be the second
  Markdown source the reviewer asked to avoid.
- **Generate PDFs in CI instead of committing them:** not done. It needs CI changes and a publishing location, and the
  reviewer asked for the files to live under `docs/`.
- **Bundle Mermaid locally instead of loading it from a CDN:** not done yet. It adds a dependency to vendor and update;
  the CDN version is pinned. Listed below as an open question.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "a skill writes a new document under `docs/en/`" | English Markdown only | English Markdown plus `docs/en/pdf/<path>.pdf` and `docs/ja/pdf/<path>.pdf`, footers naming the revision; no Japanese Markdown | pass (by inspection of the rule; to be observed on the next document produced) |
| New: "a one-line edit to an existing `docs/en/` document that has no PDFs yet" | English only | The edit plus both full PDFs, in the same change | pass (observed in this change: 001_BD–003_BD were edited by RFC 0009 and got all six PDFs) |
| "DD step for a screen whose API and processing flows are small enough to fit in the main DD" | All four DD files in English | All four DD files, each with two PDFs | pass (by inspection) |
| "A change touches only Markdown, `docs/`, `work-items/`, `demos/` or `ai/`" | CI starts no run | Unchanged for documentation changes; a change to `scripts/docs-pdf.py` itself is outside that list and runs CI | pass |

The script was run on all 19 design documents (3 BDs, 3 main DDs, 9 DD companions, 4 DB designs): 38 PDFs, from 5 to 27 pages each. All
Mermaid blocks rendered (no diagram source left as text) and the SVG wireframes and Japanese text appeared as expected
on the pages checked.

## Risk and rollback

- **Risk:** low. The change adds outputs and a script, and relaxes no gate. The main risk is a PDF left stale after an
  edit; the Markdown stays authoritative and the footer shows which revision a PDF came from.
- **Rollback plan:** revert this RFC's commit. PDFs rendered in the meantime can stay or be deleted; nothing reads them.

## Unresolved questions

- A shared Japanese glossary for recurring terms (production order, due date, work item and so on), so translations
  stay consistent across documents and revisions.
- Bundling Mermaid so rendering works offline.
- A check that lists `docs/en/` documents whose PDFs are missing or name an older revision.

## Adoption

- **Reviewer:** ThanhTN (request, 2026-09-23: "from now on the docs output will also output jp version instead of only
  english"; scope, existing documents and sync answered the same day: everything under `docs/`, new and changed
  documents only, same change; revised the same day from Japanese Markdown files to English and Japanese PDFs under
  each language's `pdf` folder)
- **Adopted revision:** not yet adopted

## Amendment 1 — quoted UI text after the Japanese UI (WI-005, 2026-09-23)

WI-005 made the UI Japanese only (WI-005 DEC-001), so the rule above that quoted UI text "stays English" no longer
describes the product. As decided in WI-005 DEC-003: the English documents quote the implemented Japanese text followed
by an English gloss in parentheses, for example 「製品を選択してください。」 (Select a product.), and catalog or
value-mapping tables carry the Japanese text and the gloss in separate columns. The Japanese translation quotes the
Japanese text alone. Wireframes and mockups show the Japanese text. `ai/rules/documentation.md` and
`ai/templates/basic-design.md` carry the amended rule; nothing else in this RFC changes.

## Amendment 2 — Japanese editions of the HTML mockups (WI-005, 2026-09-23)

At the user's request, each DD's HTML mockup has a committed Japanese edition beside it, `mockups/<name>.ja.html`, with the page captions in Japanese as well as the UI. It is published as its own Artifact, which the Japanese PDF links to; the English PDF keeps the English Artifact. Unlike the Japanese Markdown translations, these files are committed, so the Japanese pages can be updated from the repository. `ai/rules/documentation.md` carries the rule.
