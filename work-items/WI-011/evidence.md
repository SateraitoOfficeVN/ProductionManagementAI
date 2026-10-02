# WI-011 — Evidence

## Intake / static assessment — 2026-10-02

User observed uneven button heights and buttons adjacent to upper/lower content
while reviewing the WI-010 Plant calendar videos. Prior captured capacity frames
show paging actions stretched higher than search/lookup actions; input/search
boundaries have no visible vertical gap. No runtime geometry claim yet.

Source inspection at c227acc:
- LineDialog.tsx shared lineButtonClass supplies min-h-12, not a maximum height.
- PlantCalendarPage.tsx Apply/search/paging sit in grid/flex parents with default
  stretch alignment next to taller labeled inputs.
- CapacityPanel.tsx two-column grid mixes multi-element search labels and paging
  groups; paging flex children stretch with row height; search buttons immediately
  follow input nodes with no explicit gap.
- WeeklyPatternEditor.tsx and DateExceptionEditor.tsx history/restart and recovery
  actions lack consistent containing vertical groups/margins in several states.

These are concrete candidate causes, not yet browser-measured proof. Approved
006_DD-SPD specifies 48 px actions and 320 px/200% reflow. No old design edited.

Initial main checkout clean. Created a dedicated bug worktree on c227acc for
routine planning records only. Existing evidence/video files preserved. Plan1
awaiting approval; no implementation, new test, fixture or external operation run.

## Baseline reproduction and design gate — 2026-10-02

Fresh isolated pmai-wi011-check-20261002 / wi011_layout, ports54811/5033/30511.
Owner migrations and explicit calendar activation succeeded; restricted API and
worktree Vite frontend ready. No existing/demo/live database writes.
Measured at widths1440/540/320: desktop Apply/search72px, capacity paging72/120px;
mobile540 search/paging72px. All search-input-to-button gaps0px. Baseline geometry
stored in baseline-layout.json and screenshots/before-1440.png / before-540.png.
New TC-407/408 regression run against unchanged baseline fails as intended:
Apply72px differs from48px by24px. One failed, zero retry; no gate weakened.

Proportional design-consistency review passes for restoring approved 48px actions
and accessible reflow. No state/contract/schema/navigation change or new design
is necessary. All prior approved designs remain read only. Steps1/2 complete;
calendar-local parent alignment/spacing correction now authorized under revision1.

## Correction, verification and recorded diagnostics — 2026-10-02

Four calendar components only. Grid/form alignment uses items-end or items-start
so commands do not stretch with taller labeled fields. Capacity search controls
use separate labeled fields and action wrappers with gap-2. Editors group actions,
history/restart and Unknown recovery with explicit gap-2/gap-3. No fixed maximum
height, shared style, UI catalog, handler, API, schema or dependency change.

| Executed check | Actual result |
| --- | --- |
| npm ci in frontend and E2E worktree packages | Locked versions restored; audits report zero vulnerabilities; package/lockfiles unchanged |
| npm run lint / npm run build in src/frontend | Pass; TypeScript and Vite production build succeed |
| npm test -- --run in src/frontend | 253 tests / 21 files passed, zero skips/failures |
| Initial new layout suite after fix | 6 passed / 3 failed: helper incorrectly required more than two actions in an error view that has exactly two; corrected this fixture-count assumption, retaining all geometry assertions |
| Corrected Vite affected Playwright run | 17 passed, zero skipped/failed/retries |
| Final Docker/Nginx production-build affected Playwright run | 17 passed in 26.6 s, workers1/retries0, zero skipped/failed |
| Final before/after browser measurements | All 17 ordinary month/capacity actions 48 px at widths1440/540/320; both capacity search gaps 8 px versus baseline0 |
| Native 200% zoom and modal keyboard/reflow | Pass; DPR1 to2, innerWidth1384 to692, layout/scrollWidth684; modal center342.25/226.25 against layout center342/226 |
| Approved artifact protection / whitespace | 209 SHA-256 values unchanged; git diff --check pass |

Final Playwright command in tests/e2e: npx playwright test
specs/plant-calendar-layout.spec.ts specs/plant-calendar.spec.ts
specs/plant-calendar.mobile.spec.ts --reporter=list. Credentials supplied only
from generated temporary fixture environment, never written to work-item records.
11 new browser cases TC-407–409 map to REQ-076–078; six existing calendar journeys
check saved result/Unknown/no-replay, reference capacity, retained deletion/history,
mobile keyboard/centered dialog and acknowledged success despite refresh failure.
Normal/history/Unknown/error layouts measured at 1440/540/320 widths; weekly Unknown
also checked at1440/320. Browser rectangles and content overflow, not CSS class
names or jsdom layout, establish the geometry assertions. Automated axe passes.

Actual screenshot/measurement artifacts: baseline-layout.json, after-layout.json,
native-zoom.json; screenshots/before-1440.png, before-540.png, after-1440.png,
after-540.png and native-200.png. Baseline screenshots were regenerated from a
read-only temporary copy of master frontend against the same disposable API after
correcting helper output paths/selectors; date inputs are excluded from search-gap
samples. The first baseline-server attempt lacked main frontend dependencies; a
temporary baseline copy reused worktree locked modules by a verified junction,
then both measurement sets were captured successfully. No baseline app edit.

Native zoom used OS Ctrl+0 / five Ctrl+Equal keystrokes on the uniquely titled
owned Chromium window. Observed ratio2 and reduced CSS viewport; visually reviewed
owned-window screenshot, enlarged Japanese text/actions readable. Modal Cancel
initial focus, Tab cycle, Escape and draft retention passed. Native browser closed.
Screen-reader speech and physical mobile keyboard/IME Not run; no certification.
Existing jsdom canvas getContext warnings and NO_COLOR/FORCE_COLOR diagnostics
occurred without failing/skipping final tests; no new package or bypass introduced.

## Review and cleanup — 2026-10-02

Proportional security/delivery review passes for local scope. Exact source diff
contains four calendar presentation components plus one new E2E spec and WI-011
records/artifacts. Shared controls, backend, API/auth/data permissions, package
locks and approved documents unchanged. Earlier backend results not rerun or
claimed as new proof. No GitHub CI, commit/push/PR/merge, live deployment/activation,
new videos or next feature operation. Main checkout remains clean.

Verified exact pmai-wi011-check-20261002 project/container/volume labels before
Compose down -v. Removed only its three containers/network/two volumes and three
project image tags. Stopped owned Vite/native helpers. Verified dependency junction
target and deleted only the junction before temporary-folder recursive removal;
worktree node_modules retained. Removed generated temporary credentials/helpers
and task-owned ignored failed traces. Fixture writes never reached demo/live data.
Existing evidence checkout and all prior final videos preserved.

## Separate evidence recording — 2026-10-02

After local revision 1 completion, the user directly requested new evidence videos.
This bounded artifact request authorizes recording and disposable fixture cleanup;
it does not revise the historical revision 1 scope or authorize commit/push/PR/merge.
No new work item or design file created; no application/test source changed.

Fresh Compose project pmai-wi011-video-20261002 / database wi011_video, ports
54813/18513/30513. Owner migrations and explicit activation succeeded only in this
disposable fixture. Production frontend built from the four local WI-011 fixes
on baseline c227acc76ee8463e0408858cfbbc90e327195518, uncommitted/not merged.
Headed Chromium: 500 ms action delay, approximately 6 seconds reading per step.
Web raw viewport1920x900; mobile Chromium touch emulation540x900, enlarged for
portrait output. Mobile is not a physical-device recording.

Both final captures passed all 16 checked steps: ordinary heights/search gaps,
line exception and 1,920 kg reference capacity, future weekly rule, retained
history/deletion, centered confirmation, Unknown/no-replay/current-state acceptance
and read-error/retry. Lost-response and 503 cases explicitly simulated in captions.
First capture stopped at the modal check: innerWidth included a 15 px scrollbar,
causing a false 7.5 px center discrepancy. Corrected measurement to clientWidth
without changing the application or the 2 px tolerance; both complete reruns pass.

Export initially stopped before encoding because video duration exceeded the
script duration. First/middle/final frames confirmed zero start offset and an
extra repeated final-frame tail; trim to last verified step end instead of shifting
subtitles. Publication initially stopped on Windows cp1252 manifest decoding;
explicit UTF-8 fixed the helper. No incomplete capture/export was published.

Exactly four MP4s in the main checkout's ignored
`demos/evidence/output/plant-calendar-wi011/`. H.264/30 fps, burned-in captions
in a separate bottom band plus one same-language mov_text subtitle stream.
ffprobe dimensions/language/duration checks and complete video decode passed for
each file. Reviewed all four caption frames and Japanese mobile recovery frame;
text readable, no caption coverage of the application. Provenance/source hashes
embedded in MP4 metadata. Output is based on local fixes, not a merged release.

| File | Dimensions | Language | Duration (s) | SHA-256 |
| --- | --- | --- | --- | --- |
| plant-calendar-web-en.mp4 | 1920x1080 | eng | 137.233 | 6676e7ef6d420b52062596e889ce89038df93b4508842f31eef1d6aa581d40a0 |
| plant-calendar-web-ja.mp4 | 1920x1080 | jpn | 137.233 | 0ee4757587242f857811e3bfa0960ed926a40f497103e9f93d68e47b8f16de6f |
| plant-calendar-mobile-en.mp4 | 1080x1920 | eng | 137.266 | f5b9e0cf8442af17352e4832364a7dd0a971dbd5d28d58f01e8cd1e7f4611b0c |
| plant-calendar-mobile-ja.mp4 | 1080x1920 | jpn | 137.266 | e5d16df92fde6ad8e1a23774f12efdcc8f65ca5c9e3570fd37acaafab818c270 |

Recorded source SHA-256 values:

- `PlantCalendarPage.tsx`: `0e9ad7c0764383166c5d7bfebd6c41c9fe7f42f37671a4f5871553d5f140daaf`
- `CapacityPanel.tsx`: `7e3588467c446168e6f436c627a961e0f352f41cdb66d65f9d35d88ddb49fdca`
- `WeeklyPatternEditor.tsx`: `f36f2f3fb0a2406c16dee5765de90bc4f07cd959bf8585aa343c09717be3afe9`
- `DateExceptionEditor.tsx`: `693bba4f07a152c0548fd7bd55ef2ed24e0178024b384711fcc065f27d07b536`

12 prior-video hashes and 209 protected artifact hashes unchanged. Four source
hashes unchanged during recording. Existing manual speech/physical keyboard/IME
limits remain Not run. Recording creates no fresh backend/remote-CI certification.

Final recording cleanup passed: exact three fixture containers, network, two volumes
and three project image tags removed; generated credentials, capture/export helpers,
raw WebM, subtitle/font/frame/manifest intermediates deleted from the verified
recording temp directory. Final folder contains only the four hash-verified MP4s.


## Revision 2 pre-commit review — 2026-10-02

Actual full component/test diff reviewed; only calendar layout/markup changes,
11 browser regression cases and records/artifacts/current-state reconciliation.
Four source hashes still match recorded verification and new video provenance;
four new MP4 hashes and 209 protected artifact hashes unchanged. Existing test
results remain valid for unchanged source; no extra runtime rerun needed.
Security/delivery checks pass for PR preparation: no changed API/auth/permissions,
secrets, dependency or approved design. Full remote CI pending; no merge claim.
