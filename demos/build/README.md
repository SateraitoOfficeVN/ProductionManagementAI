# demos/build

Sources for the demo walkthroughs' **edited videos** and **presentation decks**. The finished
artifacts live next to each recording under `demos/<step>/WI-002/`; this directory holds what
produced them, so they can be rebuilt or amended rather than redone by hand.

| File | What it is |
| --- | --- |
| `cut_bd.sh`, `cut_db.sh`, `cut_dd.sh`, `cut_im.sh` | One per step. Each holds the full segment list of its edited cut and burns in the step captions. |
| `extract-shots.sh` | Re-extracts the screenshots each deck embeds, from that step's edited cut. |
| `overlays/` | Replacement text drawn over passages that are hidden in the cuts (see **Hidden material**). |
| `decks/<step>/deck.html` | The 15- or 16-slide deck. Rendered to PDF with headless Chrome. |
| `decks/<step>/img/` | The screenshots that deck embeds, extracted by `extract-shots.sh`. |

## Prerequisites

- **ffmpeg / ffprobe** on `PATH`.
- **Google Chrome** (headless) for HTML → PDF.
- **Fonts.** The cut scripts draw captions with Arial and Consolas. ffmpeg's filter parser cannot
  handle a Windows drive-letter colon in a path, so the fonts must be reachable by a *relative*
  path. Copy them once into a local, gitignored folder:

  ```sh
  cd demos/build
  mkdir -p fonts
  cp /c/Windows/Fonts/arial.ttf /c/Windows/Fonts/arialbd.ttf /c/Windows/Fonts/consola.ttf fonts/
  ```

  Set `FONTS=/some/other/dir` to point elsewhere.
- **The source recordings.** `demos/**/*.mp4` is gitignored, so the raw and edited videos are only
  on the project owner's machine. The decks can still be re-rendered without them, because
  `decks/*/img/` is committed.

## Rebuilding an edited cut

Run from this directory, passing the raw recording and the output path:

```sh
cd demos/build
bash cut_db.sh \
  ../03-database-design/WI-002/ScreenA_DB.mp4 \
  ../03-database-design/WI-002/ScreenA_DB_edited.mp4
```

Each script's `SEGS` array is the edit decision list — one line per clip:

```
"start | duration | step | kind(h/a) | jumped(0/1) | title | subtitle | paint"
```

`kind` picks the caption's accent (`h` amber for the human, `a` teal for the agent), `jumped` adds
the "» jumped ahead" badge for the first 2.6 s, and `paint` selects an overlay (see below). Clips
are encoded separately and concatenated, so changing one line only re-encodes that clip.

**Pacing.** The cuts are paced for reading, not for coverage: text-heavy frames — turn summaries,
decision forms, plan documents — are held 16 to 30 seconds. Roughly four minutes per design step
and six for the 50-minute implementation.

**A caution when moving a clip.** Claude Code collapses a command's raw output into a one-line
summary when it finishes, which re-flows the terminal. A window that straddles that moment will
scroll mid-clip, and any paint overlay will land on the wrong lines. Always check the first and
last frame of a clip you have moved.

## Rebuilding a deck

```sh
cd demos/build
bash extract-shots.sh database-design          # only if the cut changed
cd decks/database-design
chrome --headless=new --disable-gpu --no-pdf-header-footer \
  --print-to-pdf=ScreenA_DB_presentation.pdf deck.html
```

Then copy the PDF next to its recording. The decks share one theme — 960 × 540 pt, Segoe UI and
Consolas, light `#F3F5F7` content slides with a 6 pt teal top bar and a dark `#111B24` title slide —
so they can be shown back to back. The page is authored at 960 × 540 px with `zoom:1.3333333`
inside a `1280px × 720px` `@page`; without that, Chrome emits 720 × 405 pt pages.

Every step slide carries a `VIDEO mm:ss` badge pointing into that step's edited cut, so **a change
to a cut's durations invalidates its deck's timings**. `ai/` has no automated check for this; after
re-cutting, recompute each step's start time from the script's `SEGS` and update the deck's step
cards, badges and figure captions.

## The combined deck

`decks/combined/deck.html` is all four decks in one file, rendered to
`demos/ScreenA_all-four-steps.pdf` (64 pages). It is **generated**, not edited by hand:

```sh
cd demos/build
python build-combined.py                 # run from demos/build/
cd decks/combined
chrome --headless=new --disable-gpu --no-pdf-header-footer \n  --print-to-pdf=../../../ScreenA_all-four-steps.pdf deck.html
```

The generator concatenates the four decks' `<style>` blocks and slide sections in narrative order
(basic → database → detailed → implementation), copies each deck's images into one folder under a
`bd_`/`db_`/`dd_`/`im_` prefix so the filenames cannot collide, prepends a series cover, and rewrites
every footer to a continuous `N / 64`. The five dark title slides carry no footer and act as section
dividers.

**Rebuild it whenever a step deck changes**, otherwise the combined file silently keeps the old
slides.

## Hidden material

WI-002's design phase was restarted after the templates were rewritten, and the presentation
material treats the work as one fresh run. The basic-design recording refers to the earlier pass in
five places, and the database-design recording in one. Those passages are painted over in the cuts
and, where the sentence still carries meaning, redrawn without the omitted part — the replacement
text is in `overlays/`.

| Overlay | Used by | Replaces |
| --- | --- | --- |
| `bd_scrapped.txt`, `bd_inputs.txt` | `cut_bd.sh` paint `B` | Two sentences naming the scrapped pass |
| `bd_rev.txt` | `cut_bd.sh` paint `C` | `brief.md`'s revision note |
| `bd_log1.txt`, `bd_log2.txt` | `cut_bd.sh` paint `D` | `decisions.md`'s opening paragraph |
| `bd_csrf.txt` | `cut_bd.sh` paint `E` | The turn 2 summary's CSRF line |
| `csrf1.txt`, `csrf2.txt` | `cut_db.sh` paint `PAINT` | The CSRF bullet in the five open items |
| `jumped.txt` | all four | The "» jumped ahead" badge |

`cut_bd.sh` paint `A` needs no replacement text — it simply blanks the session setup commands
(`/clear`, `/model`).

**If you re-cut the basic-design video, re-check every clip for these references**, not only the
painted ones. They also appear in `status.md`'s accomplishments table and in BD-001's open-questions
table, which is why step 12 uses a window showing BD-001's item-events table instead.
