<!-- Based on ai/templates/improvement.md. -->

# RFC: Number-first document IDs and file names

**Status:** adopted
**Affected:** `ai/rules/documentation.md`, `ai/skills/basic-design/SKILL.md`, `ai/skills/detailed-design/SKILL.md`, `ai/skills/database-design/SKILL.md`, `ai/skills/architecture/SKILL.md`, `ai/templates/` (ID placeholders and file-name patterns), `ai/harness-overview.md`; every document, wireframe, mockup and PDF under `docs/en/` and `docs/ja/pdf/`; every reference to those IDs in `ai/`, `docs/`, `work-items/`, `src/`, `tests/`, `deploy/`, `README.md` and `CLAUDE.md`

## Summary

Document IDs and file names put the number first. `BD-001` becomes `001_BD`; `DD-001` becomes `001_DD`, with its
companions `001_DD-API`, `001_DD-FN` and `001_DD-SPD`; `DB-002` becomes `001_DB` (see below); `ADR-0001` becomes `0001_ADR` (four
digits, as MADR numbers them). A document's file is `<ID>_<slug>.md`, and its wireframes, mockups and PDFs start with
the same ID. The IDs are rewritten everywhere they appear, not only in file names. Other ID families (`REQ-###`,
`SCR-###`, `FN-###`, `DEC-###`, `TC-###`, `MSG-…`, `WI-###`) are unchanged.

## Motivation

Requested on 2026-09-23: "let change up the documents naming format : do 001_BD instead of BD-001 do the same for
others docs". Asked how far it should go, the user chose to change the IDs used in text as well as the file names, the
`001_BD_slug` pattern, and to include the DB and ADR documents, wireframes, mockups and PDFs.

With the number first, a folder listing groups each screen's documents together and sorts them in order, which matches
the numbered folder convention the docs already use (`010_basic-design`, `020_detailed-design`) and the Japanese SI
convention of numbered design documents. Before this change, the DB and ADR files had no type in their names at all
(`0002-production-order-schema.md`), so their ID could only be found inside the file.

## Guide-level explanation

| Document | ID | File |
| --- | --- | --- |
| Basic design | `001_BD` | `010_basic-design/001/001_BD_production-order-create-edit.md` |
| Detailed design | `001_DD` | `020_detailed-design/001/001_DD_production-order-create-edit.md` |
| DD companions | `001_DD-API`, `001_DD-FN`, `001_DD-SPD` | `020_detailed-design/001/001_DD-API_production-orders.md` and so on |
| Database design | `001_DB` | `database/001/001_DB_production-order-schema.md` |
| Architecture decision | `0001_ADR` | `architecture/0001/0001_ADR_backend-layered-structure.md` |
| Wireframe | — | `010_basic-design/001/wireframes/001_BD_SCR-001-pc.svg` |
| Mockup source | — | `020_detailed-design/001/mockups/001_DD_screen-a-mockup.html` |
| PDF | — | `docs/en/pdf/010_basic-design/001/001_BD_production-order-create-edit.pdf` (and `docs/ja/pdf/…`) |

A new document takes the next number in its type and is written, cited and linked in this form from the start.

**DB numbers follow the screen.** Before this change the DB designs were numbered in creation order, so the identity
schema of WI-001 took `DB-001` and every screen's DB design sat one number above its BD and DD. At the reviewer's
request ("remap the database file so it matched the BD and DD"), a screen's BD, DD and DB now share one number, and a
DB design that serves no screen takes `000`:

| Old ID | New ID | Serves |
| --- | --- | --- |
| `DB-001` | `000_DB` | No screen: the WI-001 identity schema |
| `DB-002` | `001_DB` | Screen A, with `001_BD` and `001_DD` |
| `DB-003` | `002_DB` | Screen B, with `002_BD` and `002_DD` |
| `DB-004` | `003_DB` | Screen C, with `003_BD` and `003_DD` |

Unlike the rest of this RFC, this renumbers: `DB-002` is `001_DB`, not `002_DB`. A screen with no database change has
no DB design, and its number is simply unused.

**One folder per number.** Then, at the reviewer's request ("put the same prefix file like 001 of the same type docs
into subfolder with the name like 001"), every document moves into a folder named by its number inside its type folder,
together with everything that shares the number:

```
010_basic-design/001/        001_BD_….md, wireframes/
020_detailed-design/001/     001_DD_….md, 001_DD-API_….md, 001_DD-FN_….md, 001_DD-SPD_….md, wireframes/, mockups/
database/001/                001_DB_….md
architecture/0001/           0001_ADR_….md
```

The shared `wireframes/` and `mockups/` folders are gone; each screen's assets live with its documents. The PDFs
mirror the folders (`docs/en/pdf/010_basic-design/001/001_BD_….pdf`). ADRs get a folder each too, for consistency,
although each holds one file.

## Reference-level explanation

- **Current behavior:** IDs are type-first (`BD-001`, `DD-001-API`, `DB-002`, `ADR-0001`). BD and DD files start with
  the ID (`BD-001-production-order-create-edit.md`); DB and ADR files carry only the number (`0002-….md`).
- **Proposed behavior:**
  - `ai/rules/documentation.md`: one rule stating the ID forms, the `<ID>_<slug>.md` file name, and that derived files
    start with the ID.
  - The basic-design, detailed-design, database-design and architecture skills, and the templates' placeholders
    (`{###_BD}`, `{###_DD}-API_{slug}.md`, `{###_DD}_{SCR-###}-pc.svg`, …), use the new forms.
  - 37 files renamed (24 with `git mv`, keeping their history; 13 new, still-untracked wireframes). The 12 PDFs are
    regenerated under the new names.
  - Every reference rewritten: 178 files across `ai/`, `docs/`, `work-items/` (including closed work items' records),
    `src/` and `tests/` (code comments and test description labels only), `deploy/`, `README.md` and `CLAUDE.md`.
- **Not changed:**
  - `demos/`: its transcripts and decks record what appeared on screen in the recorded sessions, so they keep the IDs
    of the time.
  - The published mockup Artifacts on claude.ai, which are outside the repository.
  - Git history and merged pull request descriptions.
- **Definition of done:** no `BD-###`, `DD-###`, `DB-###` or `ADR-####` remains outside `demos/`; every Markdown link
  and every backticked document path resolves; the PDFs carry the new IDs; frontend lint and tests and the backend
  build still pass.

## Drawbacks

- The skills' rule "once assigned, an ID is never reused or renumbered" holds for BD, DD and ADR IDs, whose numbers
  don't change, only their written form. It is deliberately broken for the four DB designs, which are renumbered to
  match their screens: an old `DB-002` reference means today's `001_DB`, not `002_DB`. Anything outside the repository that cites the old IDs (the demo videos, published
  Artifacts, merged PR descriptions, people's notes) no longer matches a search in the repository.
- Closed work items now cite IDs in a form that didn't exist when they were written.
- A large diff: 178 files and 37 renames, which makes the change itself hard to review line by line.

## Rationale and alternatives

- **File names only, IDs unchanged:** offered and declined by the reviewer. It would have left two forms for the same
  document (`001_BD_….md` containing "BD-001").
- **`001_BD-slug` (hyphen after the type):** offered and declined; underscores between number, type and slug were chosen.
- **Keep the DB designs in creation order (`001_DB` = identity):** first applied, then replaced at the reviewer's request so the numbers line up by screen. The cost is that DB IDs are renumbered, not only reformatted.
- **Three digits for ADRs:** not done. ADRs keep MADR's four-digit sequence, so `0001_ADR`.
- **Also rewrite `demos/`:** not done, because it would make the transcripts disagree with the recordings they
  describe.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| "A skill writes a new document under `docs/en/`" | `BD-###-slug.md`, ID `BD-###` | `###_BD_slug.md`, ID `###_BD` | pass (by inspection of the rule and skills) |
| "DD step for a screen whose API and processing flows are small enough to fit in the main DD" | `DD-###-API-slug.md` and so on | `###_DD-API_slug.md` and so on | pass (by inspection) |
| Old-form ID scan outside `demos/` | hundreds of matches | 0 | pass |
| Link check over `docs/`, `ai/`, `work-items/`, `README.md`, `CLAUDE.md` | — | every relative link and backticked document path resolves | pass |

## Risk and rollback

- **Risk:** low for behavior (only comments and test labels changed in code); moderate for traceability outside the
  repository, as described under Drawbacks.
- **Rollback plan:** revert this RFC's commit; the renames revert with it.

## Unresolved questions

- Whether test-plan IDs (`TP-002`) and work-item IDs (`WI-002`) should follow the same form. Not included: they are
  not documents under `docs/`.

## Adoption

- **Reviewer:** ThanhTN (request, 2026-09-23; scope answered the same day: file names and IDs, `001_BD_slug`, DB and
  ADR documents, wireframes, mockups and PDFs included)
- **Adopted revision:** `dbc9527` on `master`, the squash-merge of PR #17 (2026-09-23)
