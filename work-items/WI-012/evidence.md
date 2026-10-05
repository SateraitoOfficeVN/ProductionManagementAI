# WI-012 — Evidence

## Intake / static review — 2026-10-02

Main clean atde130ab; new isolated branch/worktree contains routine planning only.
User image shows reference capacity two-column placement with Previous/Next pairs
on separate uneven rows. CapacityPanel.tsx renders line paging after line search
and product paging after product search in one auto-flow grid. These buttons page
line/product choices, not capacity totals or calendar dates.

Approved006_BD F-14 defines line/product/date selector and explicit request.
006_DD-SPD4.4 specifies paged active line choices and applied product q/page;
006_DD-FN M-08/M-09 and api.ts getLines/getProducts use50-item pages. Existing
BD PC capacity drawing shows product/date/request/result; SPD gallery includes
capacity result and no-matches but does not show both paging pairs or a complete
selection flow. No browser reproduction or new design consistency pass claimed.

All application and approved design/mockup/PDF files unchanged at intake.
New revision1 plan presented for approval before design/application work.

## Revision1 design verification — 2026-10-02

- PASS: existing locked Playwright/Chromium rendered EN and JA galleries at widths
  1440, 640, 540 and 320. Each has 18 states, expected three/one columns, no
  document horizontal overflow and visible buttons at least 48px high. Keyboard
  Enter opens/closes native search disclosure. Multi-page actions belong to their
  field; single-page product search has no pager. Unconfirmed valid URL lookup is
  enabled; no-lines lookup disabled. See the verification summary in evidence.md and the verification summary in evidence.md.
- Visually inspected selected desktop/mobile, desktop product paging and mobile
  long-label browser screenshots. Long names wrap below native selection controls.
  Final desktop/mobile selected figures are included in the design companions.
- PASS: existing .venv Python markdown renderer and Chrome generated EN 9-page and
  JA 8-page PDFs with version1/date/page footers. Every page visually inspected in
  Chrome's native PDF viewer using its Page number control; Japanese glyphs, figures,
  table continuations and footers readable without clipped or overlapping content.
- Corrected tooling attempts: default Python lacks markdown; existing .venv used.
  Initial PDF command printed a Japanese output path through cp1252 and exited1
  after writing the file. Final commands use -X utf8 and both exit0. Initial PDF
  fragment navigation did not change viewer pages; actual Page number controls
  were used for final all-page review. No dependency installation or script edit.
- PASS: all 209 protected baseline documentation hashes match; new design relative
  links resolve; PDFs have valid signatures and expected page counts. See
  the historical immutable-document hash check and design-check-results.json for checks/hashes.
- NOT RUN: application/API/E2E behavior, native200% zoom, screen-reader speech and
  physical mobile keyboard/IME. This phase checks static design rendering only.
  Application parity and >50-item real interactions require later implementation.

User review pending. No application/test changes, runtime fixture/database writes,
new dependencies, old approved artifact changes, external actions or new videos.

## Restart and cleanup — 2026-10-02

User found wider screen differences and explicitly requested replanning from the
beginning and removal of newly added files. The previous static design results are
historical only; their generated evidence files and proposal artifacts are removed.
No whole-screen parity or current application browser verification claimed.

Before deletion, all 209 protected documentation hashes still matched. Each of the
12 removed files was verified to be an untracked regular file resolved inside the
WI-012 worktree; only exact listed paths were deleted. No recursive cleanup, tracked
file removal or shared directory removal. Removed: 006_DD-SPD-CAPACITY source,
two HTML galleries, two PNG figures, two PDFs, design-check-results.json,
the historical immutable-document hash check, the verification summary in evidence.md, the verification summary in evidence.md and review.md.
Five routine Markdown records retained; no new application/design produced in reset.

Read-only review reconfirmed approved SPD regions1–16 and twelve illustrated states.
Current PlantCalendarPage renders month, selected-day and capacity sections in
sequence; filters include line search/page controls. DateExceptionEditor currently
uses a select for working state while the approved gallery illustrates a radio
choice. CapacityPanel has independent search/paging controls in an auto-flow grid.
These are audit candidates, not a complete browser-proven difference register.

Revision2 prepared and awaiting review. Full-screen actual-browser comparison and
new design work have not started; application fixes/tests remain unrun.

## Revision2 full-screen audit — 2026-10-05

Approval: explicit user "approved" after revision2 presentation. All findings below
are static source/design comparisons. Actual runtime screenshots are unavailable:
`docker ps --format '{{.Names}} {{.Ports}}'` returned no running containers; read-only
HTTP probes to 127.0.0.1 ports 3000,5173,5033 all returned ECONNREFUSED. No fixture,
container, service, database or authenticated application session was created.
See the verification summary in evidence.md. Revision3 must obtain actual production-build parity.

Approved-reference screenshots: audit/approved-bd-desktop.png and
approved-spd-populated-desktop.png, approved-spd-populated-mobile.png,
approved-spd-weekly-desktop.png, approved-spd-validation-desktop.png,
approved-spd-history-desktop.png. These render existing approved artifacts, not the
application. For other states use the immutable SPD artboard references below;
current-side image reference for every finding is Not captured (runtime unavailable).

### Complete difference register

Source shorthand: BD = 006_BD; SPD = 006_DD-SPD, both under docs/en/*/006.
Current owners are src/frontend/src/features/plant-calendar/{PlantCalendarPage,
WeeklyPatternEditor,DateExceptionEditor,CapacityPanel}.tsx; header comes from
src/frontend/src/components/{AppHeader,AppNavbar}.tsx. All were read-only.

| Finding | Region / state | Approved source | Actual/source difference | Severity / type | Proposed correction | Later verification |
| --- | --- | --- | --- | --- | --- | --- |
| ALIGN-01 | 1–9 / month | BD PC regions5–7; SPD populated gallery | BD places calendar left and day/actions right; SPD sample and PlantCalendarPage stack them. | Moderate / conflicting reference and implementation drift | User selected BD on 2026-10-05; two columns at >=900px, readable intermediate reflow. | ready; plant; desktop geometry |
| ALIGN-02 | 3 / scope filter | BD F-01/02; SPD4.1 | Current month filter has a separate ungrouped search/pager row; approved visuals omit full bounded choice flow. | Moderate / visual gap | Associate search/count/conditional paging with its selector; preserve all pages. | scope-pages; >50 scope-line test |
| ALIGN-03 | 4 / date basis | BD regions4; SPD populated | Approved sample explicitly shows plant date/timezone; current month markup does not display that context. | Moderate / missing display | Display response plantToday/timeZone, never browser-derived today. | ready; plant; context test |
| ALIGN-04 | 5 / day cells | BD month wireframe; SPD4.1 | MonthView shows date/state/source but omits effective hours shown in BD. Phone day text uses day number without explicit month/weekday. | Moderate / missing display | Show date/state/hours/source; plant working hours line-dependent; full phone dates. | ready; plant; unconfigured; full-month test |
| ALIGN-05 | 6 / selected day | BD region6; SPD4.1 | Current selected-day fallback maps source kinds only; approved BD also explains the lower rule reason. | Moderate / incomplete explanation | Render available winning/fallback reasons with no partial rule merge. | ready; plant; source precedence regression |
| ALIGN-06 | 7 / restricted entry | BD F-03, editor entry and past restrictions | Current day edit entry remains enabled on past dates; editor later shows restriction. | Moderate / entry-state mismatch | Show reason beside disabled new/edit entry when forbidden; retain readable history and distinct retired removal. | exception-past; exception-retired; entry regression |
| ALIGN-07 | 10,11,14,15 / weekly | SPD weekly artboard; SPD4.2 | Current weekly screen puts range/history/list before primary draft. Original weekly artboard starts at effective date/weekdays; history detail was omitted. | Moderate / hierarchy and missing artboard detail | Keep primary edit first; illustrate secondary bounded current/history disclosure and expanded screen. | weekly-new; weekly-history; weekly-future |
| ALIGN-08 | 10,12,13 / date editor | SPD validation/locked/conflict; BD F-07–11 | Current working state is a select; approved visuals show radios. Current editor duplicates date in heading and read-only input. | Moderate / control mismatch | Restore labeled radio group and captured scope/date text; retain exact/inherited hours and errors. | exception-edit/new/closed/inherit/invalid |
| ALIGN-09 | 15 / exception history | SPD history artboard; SPD4.3 | Current mode=history opens DateExceptionEditor with history expanded and writable form still visible. Approved history artboard is read-only. | Moderate / mode presentation mismatch | Use dedicated read-only history presentation with existing data/APIs and explicit back. | history; mutation controls absent; snapshot paging |
| ALIGN-10 | 16 / confirmations | SPD4.5; confirm artboard | Existing dialog helper centers correctly; editor dialog content supplies generic consequence but not visible captured date/scope required by design. | Moderate / missing target context | Include captured logical target/date; retain native modal behavior and safe focus. | confirm-remove/withdraw/discard; runtime keyboard test |
| ALIGN-11 | 8 / capacity selection | SPD4.4; original user screenshot | Current auto-flow two-column grid distributes selectors/search/paging on uneven rows; both pager pairs visible for one-page data. Original visuals do not define these controls. | Moderate / missing visual grouping | Group secondary search/paging under matching selector; complete whole-screen artboards, no unexplained default pager. | capacity-line-pages/product-pages/empty |
| ALIGN-12 | 9 / capacity basis | BD region9; SPD4.4 step5 | CapacityResult shows quantity/hours/source but not response minutesPerUnit basis shown in BD. | Moderate / missing display | Show response quantity/unit/hours/minutes with exact formatting and independent result context. | ready; capacity-closed; unavailable |
| ALIGN-13 | 3,8 / retained choice labels | SPD4.1 and4.4 unconfirmed handling | CapacityPanel falls back to UUID text for retained identifiers; selections leaving pages can lose the matching option label. | Moderate / user-facing technical identifier | Keep cached business labels; unresolved URL displayed unconfirmed, explicit server verification allowed. | capacity-unconfirmed/long; retained page/filter tests |
| ALIGN-14 | 2,5,6,8,15 / read recovery | SPD4.1/4.4/4.7 | Read-error rendering varies by region; month/day/choice errors may expose only text plus distant global reload. | Moderate / recovery presentation gap | Place explicit read retry within affected region; no mutation replay. | month-error/day-error/choice-error/capacity-error |
| ALIGN-15 | 2,14 / settlement states | SPD success/conflict/unknown artboards | Current state text generally exists but approved semantic notice surfaces/grouping differ. Complete same-state visual parity has no runtime proof. | Low / visual fidelity and verification gap | Keep exact status semantics; show notice hierarchy and successful-observation gating. | success/refresh-error/exception-conflict/unknown/observed |
| ALIGN-16 | 1,2 / shell and headings | BD1–2; SPD mode headings; shared header contract | Current editors retain calendar h1 plus editor heading; approved mode artboards use a direct mode heading. Shared shell has evolved from schematic gallery labels. | Low / copy hierarchy reconciliation | Use clear mode heading, retain existing shared navigation/icon/auth shell; explicitly document inherited-shell exception. | weekly-new/exception-edit/history; mobile Menu checks |

### Resolved layout decision

User explicitly selected BD: calendar left, selected-day/actions right, mobile
stacked. No assumption from prior plan approval. Proposed 640–899px table-plus-
stacked-details reflow and the full package remain subject to design review.

### Static rendering results

- PASS: 50 artboards per EN/JA gallery, each a full screen/mode. Twelve combinations
  (two languages x 1440/900/640/639/390/320 CSS px), no document/control horizontal
  overflow, all visible button/disclosure targets >=48px, 31 table dates and31
  agenda dates; native disclosure Enter opens/closes. the verification summary in evidence.md.
- Visually inspected approved BD and SPD references; replacement desktop calendar,
  weekly entry/history, date edit/invalid/past/retired/observed, history, confirmation,
  product paging/closed capacity and unconfigured examples, plus mobile editor,
  confirmation and long-name examples. Figures retained in new design companions.
- Visual review corrected top-aligned filters, separate legend spacing, mobile
  inherited-hours checkbox wrapping and header/menu containment before final renders.
  Plant-scope working days use line-dependent hours; closed capacity sample uses
  October17; past editors use October1; validation input shows focus/error relation.
- Tool recovery: first direct SVG-document page screenshot timed out. Rendering the
  same SVG embedded in HTML produced the final approved-BD screenshot. No dependency
  installation or repository script change. No runtime availability inferred.
- NOT RUN: application tests/parity, native200% zoom, screen-reader speech, physical
  device/IME and real API interactions. Required future verification is in design8.

### Final design-package checks — 2026-10-05

PASS: final English/Japanese PDFs9 pages each, renderer commands exit0 using the
existing .venv Python with -X utf8. Every page viewed via Chrome PDF Page number
control, with version1/date/page footers, legible Japanese and continued table
headers. Figure layout was revised from13 pages to9 to group mobile images; full
HTML gallery and original-size PNGs remain the detailed visual reference. Final
PDF page counts/hashes in the verification summary in evidence.md and the verification summary in evidence.md.

PASS: 209 original documentation hashes unchanged; all new design links/images
resolve; all50 state anchors match the inventory; both galleries' Japanese screen
markup is identical. New proposed labels are explicitly listed, not changed in the
application catalog. No tracked source/test/config diff. Technical design-consistency
review is in review.md; user approval is still pending. Actual application checks
are deferred, not counted as passes. Temporary translation/render helpers and
intermediate screenshots are disposable; only review artifacts and audit proof remain.

### Region-number review feedback — 2026-10-05

User requested visible region numbers on DD-SPD images. Proposed additive design
version2 now labels regions1–16 in both 50-state galleries and eight PNG figures.
Three additional examples show weekly fields (11), history (15) and confirmation
(16). Badges are documentation annotations, excluded from application geometry
comparison as explained in design3.

PASS: 12 language/viewport combinations, each50 states, cover all16 numbers with
no horizontal overflow or clipped visible markers; the verification summary in evidence.md.
Mobile selected-day/capacity crops preserve the left badges. Final EN/JA PDFs each
have10 pages; both renderer commands exit0. All20 pages visually inspected with
version2 footers and complete figures/mapping continuation. Protected209 hashes
unchanged and all current design links resolve. Results: the verification summary in evidence.md.
User design review remains pending; runtime/application verification is not run.

## Design-review continuation — 2026-10-05

Design-review continuation received on 2026-10-05. Revision3 drafted with isolated runtime/fixtures, scoped frontend corrections and real-browser parity checks. No application or test edits, runtime writes or implementation checks performed in this planning turn.


## Revision3 implementation and local handoff — 2026-10-05

Approved revision3 executed in the dedicated worktree at baseline de130ab.
Isolated Compose project pmai-wi012-review uses frontend13012/backend18012/db15412;
existing migrations and owner calendar activation used only on disposable synthetic data.
Existing deploy services on3000/8081/5433 preserved. Temporary passwords/profile remain
outside the repository. Initial unavailable runtime gap is now resolved.

PASS: frontend lint/build and E2E lint/typecheck; 258 frontend unit tests in22 files.
Final calendar Playwright run:20 passed,0 skipped,0 flaky (the verification summary in evidence.md).
TC412 covers1440/900/640/639/390/320,31 dates, responsive geometry, editor/history and axe.
TC413/414 use real authenticated fixtures with explicitly intercepted bounded choice
responses to verify >50 paging, controlled pending reads, retained names and long names.
Existing calendar journeys exercise real API writes/history and conflict/Unknown recovery.

Native Chrome appearance zoom200%: innerWidth712, outerWidth1440, devicePixelRatio2;
no horizontal overflow, exact capacity, radio arrows, invalid precision error/focus,
modal initial Cancel focus, Tab containment, Escape and invoker return pass.
See the verification summary in evidence.md and historical zoom captures (removed by user instruction).

Same fixture baseline/corrected/reference captures preserved for desktop/mobile month,
weekly editor, date editor and history; corrected/reference confirmation also captured.
Mockup region annotations removed only in browser memory. Representative images visually
reviewed; records/dirty-save availability vary with actual fixture state. Native date
format/fonts and inherited shared shell are exceptions. No pixel equality or all50
runtime-state visual coverage claim. Source sample line/product labels/hours aligned
only in the isolated database; selected override remains4h and exact capacity1,920個.

ALIGN-01–06: approved two-column layout, grouped scope choices, server plant date,
complete dates/hours, winning/fallback reasons and restricted entry implemented;
TC412 plus existing calendar journeys verify layout and rule preservation.
ALIGN-07–10: edit-first weekly form, radios/captured target, read-only history and
captured confirmation context; TC410,TC412 and native keyboard/dialog evidence.
ALIGN-11–13: grouped capacity selectors, conditional pagers, exact basis and cached
business names; choices unit tests and TC413/414, real capacity journey.
ALIGN-14–16: adjacent read retries, semantic notices, direct mode headings and inherited
navigation; existing recovery/conflict/Unknown cases and screenshots. No mutation replay.

Intermediate failed checks were corrected before final passes: nested reason-label
accessible name, E2E selector ambiguity and fixture dependency, and pending choice label
loss. CLI invoked from repository root caused duplicate-runner failure; final run uses
locked test package CLI from tests/e2e. Windows reserved initial port required unique
ports above. Initial native zoom preference method did not change zoom; actual Chrome
settings control used and geometry verified. An auto-review usage-limit interruption
executed no rejected action; work resumed after user continuation.

PASS:209 historical protected hashes, approved version2 EN/JA PDF hashes, clean main
checkout, git diff --check and scan of changed/untracked text against temporary runtime
passwords. See the verification summary in evidence.md. Backend/API/schema/dependencies unchanged.
Not run: screen-reader speech and physical mobile keyboard/IME, unavailable environment.
Local handoff remains pending user review; no external delivery or work-item closure.

Local review browser opened visibly and authenticated at13012. Verified baseline container removed; task-owned closed capture profiles/helpers cleaned. Corrected Compose services, runtime env and active review profile retained.


## Production-order screen investigation — 2026-10-05

User reported compressed status labels and a shifted/widened list filter panel.
Read-only matched comparison uses unchanged baseline image (de130ab) on13013 and
WI-012 frontend on13012, same isolated backend,1920px viewport and fully loaded40
product options. Both reproduce identical geometry: main max width1024px but implicit
track1422px; status fieldset38px; product selector1326px. Thus this defect predates
WI-012 code; unchanged production-order markup has grid/native-control intrinsic-size
constraints. Synthetic long-name products introduced by calendar E2E are present
(longest option91 characters), exposing a pre-existing robustness gap on this fixture.
Browser-only minmax(0,1fr) main track plus min-width:0/width:100% product selector
restores976px track and535.5px status region. Shortening option text alone did not
restore layout in this Chrome probe; do not claim that probe proved a single cause.
An initial measurement before choices finished loading was discarded and repeated.
See the verification summary in evidence.md and historical comparison captures (removed by user instruction).
Application/database unchanged during investigation. Task-owned baseline container
removed after comparison; user review runtime/browser preserved. Fix needs its own
approved bug-fix scope because production-order/shared layout is outside revision3.


## Evidence retention correction — 2026-10-05

User instructed that test images must not be stored in work items and test screenshot
capture is unnecessary. Removed all59 test/reference-render images from this work item;
prior image paths above are historical records and no longer available. Approved design
figures/mockups in docs remain unchanged. Removed explicit screenshot writes and their
output-directory creation from the WI-012 alignment tests. Assertions remain intact.
Text/JSON results and recorded visual observations remain; no future screenshot is needed
for this work-item handoff. Existing other-work-item screenshot helpers are outside this
bounded change and were not executed. Use screenshot-off and trace-screenshot-off options
for subsequent runs.


## Artifact minimization — 2026-10-05

Removed twelve intermediate/generated audit JSON files per user instruction to avoid
unnecessary artifacts. Earlier JSON references now point to this consolidated record;
raw report detail is no longer retained. The209-file immutable-document check passed when run; its generated baseline is no longer retained. Subsequent preservation checks use Git against the recorded baseline commit.
Final actual results remain258 frontend unit tests,20 E2E with0 skipped/flaky,
lint/build/typecheck PASS, six responsive widths and native200% keyboard PASS.
Approved design has50 states,16 numbered regions,eight figures and two10-page PDFs.
Screen-reader speech and physical mobile keyboard/IME remain Not run.

Approved version2 en PDF SHA-256: `c4789bdd71ca05f6acb33eba3236125f4c596d7e00fba03d99742843cd21dab4`.

Approved version2 ja PDF SHA-256: `e231063d747116c88cb7b27ae22289445e46ead08c8dc8c28f9363cf46b15231`.

Local review fixture: line `L-001` (`01a10a06-26a7-7150-b8e0-c562643b2177`), product `P-1001` (`0197e4a0-0000-7000-8000-000000001001`), date `2026-10-12`, unit `個`. No credentials stored.


## Hash artifact removal — 2026-10-05

Removed the optional generated hash manifest for consistency with prior work items and
the user artifact-minimization instruction. It was an implementation choice, not a
workflow requirement. Historical209-file PASS remains an actual recorded result;
future preservation checks use Git baseline de130ab and reviewed design changes.


## Revision4 pre-PR verification — 2026-10-05

PASS: final affected calendar E2E20/20 in33.4s,0 skips/flakes; E2E typecheck.
Used temporary external config with screenshot/trace capture disabled, list reporter,
no new repository report files. CLI screenshot flag is unsupported in the pinned
version; that invocation ran no tests, replaced with config-based settings.
No application code changed after the earlier258-unit/lint/build verification.
PASS: git diff --check, historical docs unchanged against de130ab, approved EN/JA
version2 PDF SHA-256 matches recorded values, no work-item JSON/test images,
changed/untracked text scan contains no temporary runtime passwords.
User local inspection complete; explicit revision4 approval permits commit/push/PR/CI.
Final diff limited to calendar controls/styles/helpers, catalog, corresponding tests,
approved additive design/companions and required work-item records. No lock/config,
backend/API/schema/shared-header or production-order application edits.


## Revision4 PR created — 2026-10-05

Commit47d6dac created and feature branch pushed. PR #39 targets master:
https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/39
Required CI pending. No merge or deployment. No unrelated changes included.


## CI fixture-isolation correction — 2026-10-05

Run37284879613 on a8852bc: backend239 unit/195 integration and frontend258 tests
PASS; full E2E61 passed/1 failed. Failure: existing mobile order-list card click could
not complete after TC414 left its long-name product active. This is a WI-012 test
isolation defect exposing the previously reproduced baseline layout gap; not an
application regression. TC414 now uses finally to shorten only its own newly created
synthetic product through the existing versioned API, even when an assertion fails.
Long-label/paging assertions are unchanged; cleanup response/name explicitly checked.
No existing data or application code changed. Three alignment tests and typecheck
PASS after correction (11.2s), no screenshot capture. Full CI must pass on new head.
Existing global CI screenshot-on-failure behavior produced a failure attachment in
GitHub; no such file was downloaded or committed. WI-012 capture remains disabled.


## Revision4 successful CI handoff — 2026-10-05

PR #39 application head60b4f2a404bf4809733bf3b2e02a570d8ccf8838 passed all3 CI jobs
in run37285875312:239 backend unit,195 integration,258 frontend and62 E2E.
https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/37285875312
No failed/skipped/flaky final E2E; mobile order-list journey now passes after owned
fixture cleanup. PR remained OPEN/MERGEABLE, with all3 required check conclusions SUCCESS.
User authorization did not include merge or live deployment. Final record-only follow-up
changes no application/test/design content; latest remote checks are available on PR #39.
