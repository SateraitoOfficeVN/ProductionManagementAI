# demos

Four walkthroughs of Screen A's lifecycle (production-order create/edit, WI-002). Each directory holds a scenario description (`README.md`) and, per work item, a subfolder with the recording and its written record.

All four steps were recorded on 2026-09-18 in one continuous sitting, in the order they were run: basic design (11:03) → database design (11:14) → detailed design (11:30) → implementation (12:22). The numbering of the directories follows the canonical step order, so `03-database-design` was recorded before `02-detailed-design`.

## What exists per step

| Step | Recording | Transcript | Edited cut | Presentation |
| --- | --- | --- | --- | --- |
| [01-basic-design](01-basic-design/README.md) | `ScreenA_BD.mp4` (9 min 59 s) | yes | `ScreenA_BD_edited.mp4` (3 min 49 s) | `ScreenA_BD_presentation.pdf` (16 slides) |
| [03-database-design](03-database-design/README.md) | `ScreenA_DB.mp4` (10 min 44 s) | yes | `ScreenA_DB_edited.mp4` (3 min 59 s) | `ScreenA_DB_presentation.pdf` (16 slides) |
| [02-detailed-design](02-detailed-design/README.md) | `ScreenA_DD.mp4` (15 min 37 s) | yes | `ScreenA_DD_edited.mp4` (4 min 00 s) | `ScreenA_DD_presentation.pdf` (16 slides) |
| [04-implementation](04-implementation/README.md) | `ScreenA_Implementaion.mp4` (50 min 44 s) | yes | `ScreenA_Implementaion_edited.mp4` (5 min 59 s) | `ScreenA_Implementaion_presentation.pdf` (15 slides) |

Videos (`*.mp4`) are gitignored and kept locally by the project owner. Transcripts and presentation PDFs are committed.

**[`ScreenA_all-four-steps.pdf`](ScreenA_all-four-steps.pdf)** is all four decks in one 65-page file, for presenting
the whole series without switching documents. The steps appear in the order the work ran — basic design, database
design, detailed design, implementation — which is the order each deck's "next in the series" slide points to.

It is not a plain merge: a contents slide gives each part's page range, the section counters that restart at "01" in a
stand-alone deck are dropped, every footer carries "Part N of 4" alongside a continuous page number, and each part's own
title slide serves as the divider.

The four decks share one design (960 × 540 pt, 16 slides each, the same palette and type) so they can be shown back to back, and each edited cut is paced for its source: about four minutes for each design step and six for the 50-minute implementation, with the densest frames held longer so they can actually be read. The implementation cut starts at the implementation planning: the session's opening minutes applied the *previous* step's RFC 0001, which that step's own deck covers.

## Rebuilding the videos and decks

The edit decision lists, deck sources and screenshots are in **[build/](build/README.md)** — one cut
script per step, the four deck HTMLs, and the replacement text for the passages the cuts paint over.
The finished PDFs and transcripts are committed; the `.mp4` files are not.

## Harness improvements the series triggered

Running the harness on a real work item is also how the harness gets fixed. Two of the four steps exposed an ambiguity in `ai/` and produced an improvement RFC:

| Step | What the run exposed | RFC |
| --- | --- | --- |
| 01-basic-design | — | — |
| 03-database-design | — | — |
| 02-detailed-design | The DD templates called three of the four documents optional, so only two were produced | [0001 — always produce all four DD documents](../ai/improvements/0001-dd-companions-always-produced.md) |
| 04-implementation | Nothing said to keep superseded plan revisions, so `plan.md` was overwritten when revision 2 was drafted. This session also *applied* RFC 0001, on its own branch | [0002 — keep every plan revision, oldest first](../ai/improvements/0002-plan-revision-history.md) |

In both cases the agent fixed the immediate output, recorded a preference so it would not repeat the mistake, and then *offered* the change to the shared templates and skills rather than making it — changes to shared harness files need the owner's review. `ai/improvements/README.md` lists every RFC, including the two (0003, 0004) that came from later harness-maintenance sessions rather than from these walkthroughs.

## How a walkthrough is produced

1. **Record** the Claude Code session in the IDE terminal, unedited.
2. **Transcribe** it into `transcript.md`: an at-a-glance table, a step-by-step timeline with an explicit output for every step, the files changed, the points worth presenting, and the gaps where the terminal scrolled faster than the recording could capture. Session housekeeping is left out; the timeline starts at the first real prompt.
3. **Cut** an edited video for the slides. Long stretches of agent work show their start, then jump to the result — jump cuts, never speed-ups — with a "Step N of M" caption per clip that matches the transcript's numbering.
4. **Build** the presentation PDF from the transcript, one slide per step, with the video timestamp on each slide so the deck and the cut stay in step.

`work-items/WI-002/` (plan revisions, decisions, status, evidence) is the authoritative written record of every step, whether or not a video exists.
