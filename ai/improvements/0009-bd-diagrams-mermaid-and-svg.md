<!-- Based on ai/templates/improvement.md. -->

# RFC: BD and DD diagrams as Mermaid and SVG wireframes, not ASCII

**Status:** under-review
**Affected:** `ai/templates/basic-design.md`, `ai/templates/detailed-design.md`, `ai/skills/basic-design/SKILL.md`, `ai/skills/detailed-design/SKILL.md`, `ai/skills/screen-design/SKILL.md`, `ai/checklists/design-consistency.md`, `ai/evaluations/baseline-cases.md`; 001_BD–003_BD and 001_DD–003_DD (redrawn), with new files under `docs/en/010_basic-design/###/wireframes/` and `docs/en/020_detailed-design/###/wireframes/`

## Summary

The basic and detailed design templates stop asking for ASCII art. In a BD, screen transitions and status workflows
become Mermaid diagrams and each screen's layout becomes a grey-box SVG wireframe per breakpoint. In a DD, the
implementation-fidelity layout becomes an SVG wireframe too, with dialogs and whole-screen states in their own SVGs,
and the state-transition table gets a Mermaid state diagram above it. All wireframes carry numbered callouts that match
the item tables. The three existing BDs and the three main DDs are redrawn to match.

## Motivation

Requested on 2026-09-23: "for the basic design template the screen translation and layout mock up compire of text
seem too hard to read and visualized so change it", then, after the BDs were converted, "let do the same for the detail
design files".

Both templates asked for ASCII layout sketches, and the BD left its transition diagram open, so every BD and DD drew
them in monospaced text. Those sketches are hard to read beyond a few boxes: 003_DD's dashboard sketch is 38 lines of
box characters with hand-drawn bar charts, and 002_DD's table row overflows its own frame. They also go stale without
anyone noticing: 001_BD's transition diagram still sent Cancel to the home page after version 5 moved every exit to the
list (SCR-002), while its screen list table said the opposite. The DD state transitions were tables only, which list
every edge but don't show the shape of the lifecycle.

## Guide-level explanation

When an agent writes or revises a BD:

- **Screen transition:** a Mermaid `flowchart LR` below the screen list table. One node per screen and mode, labelled
  with its `SCR-###`, name and route. Solid arrows for primary actions, dotted arrows for cancel/back and error exits.
  Every arrow is labelled. The diagram shows exactly the entries and exits in the table.
- **Status workflow** (when there is one): a Mermaid `stateDiagram-v2`.
- **Layout:** one SVG wireframe per breakpoint, ###_BD-SCR-###-pc.svg` and `-sp.svg`, embedded with
  `![…](wireframes/…)`. Grey boxes, realistic sample values, and an orange numbered badge next to every element whose
  number appears in the legend table and in §3. Anything the picture can't show (dialogs, empty, loading and error
  states) is named in a note at the bottom.

When an agent writes or revises a DD:

- **Layout:** an SVG wireframe ###_DD-SCR-###-pc.svg` in the same style, at DD fidelity: the
  representative populated state with real labels, hint texts, locked styling and error-text slots, badged with the
  BD's item numbers. Dialogs and whole-screen states get their own SVGs (`…-dialog.svg`, `…-states.svg`). An SP
  wireframe only where the DD changes the BD's SP layout. The rendered mockup (Artifact) is unchanged.
- **State transitions:** a Mermaid `stateDiagram-v2` above the table. The table stays authoritative, with side effects;
  "any"-state transitions and events that keep the state may stay table-only, with a note saying so.

The wireframes stay English, because the UI they show is English; the Japanese PDFs (RFC 0008) show the same SVGs.
GitHub, VS Code and JetBrains IDEs render both formats without extra tooling. Mermaid stays diffable text; the SVG is
small plain XML.

## Reference-level explanation

- **Current behavior:** both templates show an ASCII example and ask for an "ASCII layout sketch" (the DD calls it
  authoritative for the field/region mapping). The BD screen list section says "Link a wireframe/mockup or
  screen-transition diagram here if one exists". The DD state transitions are a table only. Nothing checks that a
  diagram agrees with its table.
- **Proposed behavior:**
  - `ai/templates/basic-design.md`: a required Mermaid transition diagram with an example and conventions, an optional
    `stateDiagram-v2` for state machines, and the SVG wireframe conventions (path, canvas widths, greyscale palette,
    callout badges, sample values, the "not drawn" note, and that the Japanese PDF shows the same SVGs).
  - `ai/templates/detailed-design.md`: the SVG wireframe replaces the ASCII sketch as the authoritative layout, following
    the BD conventions at DD fidelity, with separate dialog and state SVGs; a Mermaid state diagram with an example goes
    above the state-transition table.
  - `ai/skills/basic-design/SKILL.md`, `ai/skills/detailed-design/SKILL.md` and `ai/skills/screen-design/SKILL.md`:
    steps point to those conventions and say "never ASCII"; the basic-design tools section asks that each SVG be opened
    in a browser before handover.
  - `ai/checklists/design-consistency.md`: the BD transition diagram matches the screen list table, the DD state diagram
    matches its transition table, and every numbered wireframe item is in a legend or region table.
  - `ai/evaluations/baseline-cases.md`: new cases, "a BD is written or revised for a screen" and "a DD is written or
    revised for a screen".
  - 001_BD (version 8), 002_BD (version 5), 003_BD (version 4): every ASCII diagram replaced. That is 4 Mermaid
    diagrams and 8 SVG wireframes (SCR-001, SCR-002 and SCR-003 on PC and SP, plus 003_BD's shared header on PC and
    SP). 001_BD's transition now matches its screen list.
  - 001_DD (version 5), 002_DD (version 4), 003_DD (version 5): every ASCII sketch replaced by 5 SVG wireframes (the three
    PC layouts, 001_DD's discard dialog, 002_DD's empty and no-match states), and 3 Mermaid state diagrams added. The
    DD companions (API, FN, SPD) contain no text diagrams and are unchanged.
  - No field, rule, validation, event, API or processing changes in any document.
  - English and Japanese PDFs of the six edited documents under `docs/en/pdf/` and `docs/ja/pdf/`, since RFC 0008
    requires them on any edit.
- **Definition of done:** the templates, the skills, the checklist and the evaluations carry the rule; no BD or DD
  contains an ASCII diagram; every Mermaid block renders and every SVG opens in a browser; the twelve PDFs exist and
  show the diagrams.

## Drawbacks

- An SVG takes more effort to change than a text sketch, and its diff is harder to review. A wireframe can still go
  stale; the checklist item is the only guard.
- Mermaid's automatic layout isn't always tidy for dense graphs. Crowded labels need rewording or an extra node rather
  than manual positioning.
- A DD state diagram and its table say the same thing twice. The table stays authoritative and the checklist asks that
  they agree, but an edit to one can miss the other.

## Rationale and alternatives

- **SVG for the transition diagram too:** rejected by the reviewer. Mermaid keeps it in the document as text, easy to
  edit and to translate.
- **Mermaid `block-beta` for layouts:** rejected. It can only lay boxes out in rows and columns, which looks rough for
  forms and tables.
- **HTML wireframe plus a PNG screenshot:** rejected. It blurs the BD/DD line (the DD owns rendered mockups) and needs a
  render step on every change.
- **Replace the DD state-transition table with the diagram:** rejected. The table carries each transition's side
  effect, which a diagram label can't hold readably.
- **Rely on the DD's rendered mockup and drop its wireframe:** rejected. The mockup is a private Artifact outside the
  repository; the wireframe is the version-controlled record of where each field sits.
- **Commit a wireframe generator script:** not done. The SVGs were produced with a throwaway script, but a shared tool
  would add a dependency and a maintenance burden before the format has settled. The conventions are in the templates
  instead.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "a BD is written or revised for a screen" | ASCII transition and layout sketches | Mermaid transition, SVG wireframe per breakpoint with numbered callouts | pass (001_BD–003 redrawn in this change; all four Mermaid blocks rendered with Mermaid 11 in headless Chrome without errors; all eight SVGs rasterized and inspected) |
| New: "a DD is written or revised for a screen" | ASCII layout sketch, state-transition table only | SVG wireframes (layout, dialog, states), Mermaid state diagram above the table | pass (001_DD–003 redrawn in this change; five SVGs rasterized and inspected; the three state diagrams rendered in the DD PDFs) |
| "A one-line edit to an existing `docs/en/` document that has no PDFs yet" (RFC 0008) | — | The six edited documents get English and Japanese PDFs showing the diagrams and wireframes | pass (in this change) |
| "A change touches only Markdown, `docs/`, `work-items/`, `demos/` or `ai/`" | CI starts no run | Unchanged | pass |

## Risk and rollback

- **Risk:** low. Documentation only; no gate is relaxed. A Markdown viewer without Mermaid support shows the diagram
  source as a code block, which is still readable.
- **Rollback plan:** revert this RFC's commit. The ASCII diagrams are preserved in git history.

## Unresolved questions

- Whether a shared wireframe generator is worth committing once more screens exist.

## Adoption

- **Reviewer:** ThanhTN (requests, 2026-09-23; formats chosen the same day: Mermaid for transitions, SVG wireframes for
  layouts, convert 001_BD–003 now; then the same for the detailed designs)
- **Adopted revision:** not yet adopted
