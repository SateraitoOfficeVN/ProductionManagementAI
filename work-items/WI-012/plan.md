# WI-012 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-02 | Additive capacity design and mockup review | Superseded by user; artifacts discarded | Explicit user "approved" after revision1 presentation, 2026-10-02 |
| 2 | 2026-10-02 | Full SCR-006 alignment audit and replacement mockups | Complete — design reviewed | Explicit user "approved" after revision2 presentation, 2026-10-05 |
| 3 | 2026-10-05 | Frontend correction and actual-browser verification | Complete — local inspection finished | Explicit user approval of revision3, 2026-10-05 |
| 4 | 2026-10-05 | Commit, PR and CI handoff | Complete — PR/CI handoff | Explicit user approval after revision4 presentation, 2026-10-05 |

## Revision 1 — Design/mockup alignment

### Objective and scope

Resolve BUG-005 visual-definition gap for SCR-006 reference capacity under
REQ-079–081. Create a complete, reviewable capacity mockup package consistent
with existing approved BD/SPD semantics and the user's simplicity preference.
This revision is design work only; application implementation follows a separately
presented/approved revision after this design is reviewed.

### Ordered steps and checks

| # | Step | Verification / stop | Outcome |
| --- | --- | --- | --- |
| 1 | Reconcile existing capacity BD/F-14/SPD4.4/API choices and current markup; record differences and intended user journey | Map every input/action/state; preserve paged eligible choices and exact current capacity. Capture hashes of old designs/mockups/PDFs. | Complete: reconciliation recorded; 209 baseline documentation hashes unchanged. |
| 2 | Create one new additive design Markdown: docs/en/020_detailed-design/006/006_DD-SPD-CAPACITY_稼働カレンダー.md, with new desktop/mobile HTML mockups, English/Japanese caption editions, supporting visuals and English/Japanese PDFs | Existing current line/product/date/request/result intent; fields/actions clearly grouped. Cover initial/selected/search/more-than50/loading/empty/error/invalid/unavailable states, keyboard/focus and retained URL selections. Preserve backend bounds without loose unexplained default-visible paging pairs. Use existing tokens, not a new framework. | Complete: additive version1 design, 18-state EN/JA galleries, desktop/mobile images and EN/JA PDFs. |
| 3 | Render and visually inspect companions, pass design-consistency, present the single new design file and mockups/PDFs for user review | Required artifacts rendered and checked, no invented API or inaccessible omitted options. Resolve feedback here; wait for explicit design approval before drafting the implementation revision. | Rendering and technical design-consistency checks complete; awaiting explicit user design review. |

### Resources and permissions on revision 1 approval

Read-only application/approved design inspection, additive English documentation,
new mockups/wireframes, English/Japanese PDFs via existing scripts, local headed
or headless browser rendering/screenshots, temporary translation/rendering assets
and verified cleanup. No old approved file edit; keep main checkout clean and work
in feature/WI-012-calendar-mockup-alignment dedicated worktree. Read planning and
screen-design skills directly; unavailable external design publisher has the
previously established local HTML/Chromium fallback.

Application/test edits, runtime fixture/database writes, dependency changes,
commit/push/PR/merge, deployment and replacement videos are excluded. Implementation
will be planned only after the new design is reviewed; approving revision1 does
not preapprove the next revision. No ADR/DB/API redesign is expected.

### Risks / stop conditions

- Simplification can truncate options: specify reachable >50 choices and label
  the associated selection flow; do not load only the first API page and stop.
- Removing independence of line/date query can change behavior: preserve approved
  capacity context, explicit lookup and stale-result invalidation.
- Mockup differs from design or lacks states: resolve in this additive package
  before implementation. Do not revise completed approved originals.
- Scope expands beyond this panel/API changes/new dependencies: present a revision
  before dependent work. No permission inferred from technical convenience.
- Visual consistency is verified in actual browser output after implementation;
  CSS class/unit tests alone will not establish that the screen matches mockups.

### Review and approval

Review status: approved.
Approval source: explicit user "approved" after revision1 presentation, 2026-10-02.
Design/mockup work authorized; application implementation and external actions excluded.

### Revision 1 closure — 2026-10-02

Superseded by the user's explicit request to restart WI-012 for the entire calendar
screen and remove the newly added files. The capacity-only proposal was not approved.
Its design, HTML galleries, figures, PDFs and generated check/review files were
removed by explicit user authorization. Historical steps above record what was
actually performed; their results do not approve or validate a whole-screen design.
No application changes were made. Retain these five routine work-item records for
chronological authorization and evidence; no old approved design was removed.

## Revision 2 — Restart: full Plant calendar screen alignment

### Objective and scope

Reassess the entire SCR-006 稼働カレンダー screen against approved WI-010
requirements, BD, DD/SPD/FN/API, wireframes and mockups. Produce a faithful complete
replacement review package for WI-012. This replaces the capacity-only scope.
Approved historical artifacts remain immutable. Existing designs define the target;
the current implementation is evidence of differences, not the visual source of truth.

Include shared header/current navigation, month/scope filters and related choice
search, state/source legend, desktop month/mobile agenda, selected-day details and
entry actions, weekly rules/current/future versions, date exceptions, history,
confirmations, validation/restrictions/recovery and reference capacity. Audit normal,
loading/unconfigured/empty/error/success/conflict/unknown/read-only and long-content
states, conditional controls and responsive behavior. Do not invent business changes.

### Ordered steps, artifacts and review stops

| # | Step | Verification / stop | Outcome |
| --- | --- | --- | --- |
| 1 | Inventory approved sources and all SCR-006 regions/actions/states; capture protected baseline hashes; inspect full current markup and render approved mockups and the real current frontend with comparable sample context | Create a difference register in evidence.md: stable finding ID, region/state, source reference, expected/actual, image reference, severity, proposed correction and verification. Mark missing evidence honestly. Cover every region, not only capacity. | Audit complete: all regions1–16 covered by ALIGN-01–16; approved-reference screenshots captured. Current runtime unavailable, recorded for revision3. |
| 2 | Resolve target for each difference from approved requirements and designs; distinguish implementation drift from incomplete or contradictory visuals | Fix direction follows approved intent. Any unresolved business change or material contradiction stops dependent design and is presented for decision; preserve >50-choice access, authorization, exact units, retained references, concurrency and Unknown/no-replay behavior. | Resolved: user selected BD composition on 2026-10-05; no new business/API decision. Proposed visual gap coverage documented for review. |
| 3 | Create one new additive full-screen design Markdown, 006_DD-SPD-ALIGNMENT_稼働カレンダー.md, with EN/JA full-screen HTML galleries, desktop/mobile illustrations and EN/JA PDFs | State/region/action inventory explicitly maps to original design and difference register. Reproduce approved layout, ordering, labels and conditional behavior; fill only documented visual gaps. No capacity-only redesign reused by default. | Complete: one full-screen design, 50-state EN/JA galleries, five preview figures and EN/JA PDFs. |
| 4 | Render both galleries, compare original approved targets to replacement artboards and inspect all PDF pages; complete design-consistency; present this single design file and its companions | Check desktop/mobile, breakpoint edges and 320px, long names/reasons, keyboard/focus, minimum48px actions and >=8px separated gaps. Static checks do not establish application parity. Stop for explicit user design review; resolve feedback before any next design file. | Static checks and technical design-consistency complete; awaiting explicit user design review. |
| 5 | After explicit review approval, draft and present revision3 for application correction and actual-browser parity verification | Revision3 names affected components, per-finding changes, meaningful regressions, same-state screenshots/comparison, native200% and frontend/E2E gates, fixture permissions and cleanup. Wait for explicit approval before implementation. | Complete: user reviewed version2 and instructed continuation; revision3 drafted for separate approval. |

### Resources and permissions on revision2 approval

Local read-only source/approved-document inspection, additive work-item records,
one new additive English design and its bilingual companions, existing rendering
scripts and locked browser tools, local browser observation of the current frontend,
read-only existing API access where available, temporary render assets and verified
cleanup. Use the existing dedicated WI-012 worktree; keep main and old approved
artifacts unchanged. Use planning/screen-design skills and established HTML/Chromium
fallback. Record actual browser base/build/fixture context and tool results.

No application/test edits or execution of application fixes in revision2. No runtime
fixture/database creation or mutation, dependency change, new video, commit/push/PR,
merge or deployment. If no usable current runtime exists, record the evidence gap;
fixture setup and application testing require explicit permissions in revision3 or
a separately presented revision. Do not modify production configuration to observe UI.

### Risks and stop conditions

- Approved prose and visuals may differ or omit necessary controls. Record the
  discrepancy and show proposed gap coverage; do not silently weaken business rules
  or treat undocumented current UI as approved design.
- Search/history pagination must keep all permitted data reachable. Do not remove
  required behavior merely to match an incomplete screenshot.
- Shared layout/control changes can affect other screens. Scope later implementation
  to SCR-006 unless a shared fix and its regression coverage are explicitly planned.
- Physical-device IME/screen-reader checks may remain unavailable. Report Not run;
  existing WI-010 acceptance does not prove a new implementation passed these checks.
- Protected file change, unavailable required access, added business/API/schema scope
  or new dependencies: stop dependent work and present the issue/revision.
- Screenshots/classes/unit tests alone are insufficient for parity. Revision3 must
  include visual review at matched state/viewport and actual interactions.

### Review and approval

Review status: approved.
Approval source: explicit user "approved" after revision2 presentation, 2026-10-05.
Cleanup and replanning were directly authorized by the user's restart request;
revision2 design execution is authorized; later implementation is not authorized.

### Revision2 design-review feedback — 2026-10-05

Within the approved design step, version2 adds requested region1–16 annotations
and three preview examples (eight figures total), with regenerated EN/JA10-page
PDFs. The single design document remains awaiting user review.

### Revision2 closure — 2026-10-05

User message "reviewed; continue" after version2 presentation approves the single
DD-SPD-ALIGNMENT design package. Static design-consistency evidence remains valid;
actual application parity remains unverified. Version2 and its companions are now
reviewed references; no design rewrite is planned.

## Revision 3 — Frontend alignment and browser verification

### Objective and scope

Implement BUG-005 corrections for SCR-006 against approved
006_DD-SPD-ALIGNMENT version2. Close ALIGN-01–16 with actual browser evidence and
REQ-079–081 acceptance, preserving REQ-070–075 and existing API/DB semantics.
Desktop calendar left/day details right; responsive table/agenda and stacked mobile;
capacity below; controls, labels, history, editors and confirmations follow mockups.

### Ordered steps and checks

| # | Step | Verification / stop | Outcome |
| --- | --- | --- | --- |
| 1 | Read implementation/testing skills and relevant frontend/testing rules; establish a dedicated disposable local Compose project, unique ports/volumes and synthetic calendar data using existing migrations/activation tooling | Confirm worktree/build/base URL and isolation before any write. Capture current actual screens and map differences to ALIGN-01–16. Protect historical and approved version2 artifact hashes. | Complete: isolated pmai-wi012-review runtime, existing migrations/activation and matched baseline screenshots. |
| 2 | Update PlantCalendarPage.tsx (including MonthView), CapacityPanel.tsx, WeeklyPatternEditor.tsx, DateExceptionEditor.tsx and the central Japanese messages catalog; use scoped helpers/styles where needed | Match numbered regions and approved control/state order. Preserve independent query contexts, all >50 choices, exact decimals/units, immutable targets, restrictions, retained labels, concurrency and Unknown/no-replay. Shared header/icons remain inherited. | Complete: scoped frontend corrections and Japanese catalog; approved designs and shared shell preserved. |
| 3 | Add meaningful frontend regressions and Playwright calendar/layout/mobile checks; cover real selection/search/paging, weekly/date/history/dialog and recovery interactions | Map checks to findings/requirements. Test happy and error/restriction states with isolated fixture/API interception as appropriate; distinguish intercepted scenarios from real API journeys. No weakened existing assertions. | Complete: meaningful choices/history regressions plus calendar E2E, including real API and explicitly intercepted paging scenarios. |
| 4 | Run frontend lint/build/unit suite, E2E typecheck and affected calendar E2E/axe; inspect actual screenshots side by side with unannotated mockups at matched state/data/viewport | Widths1440/900/640/639/390/320; native200% zoom; all31 dates, no clipping, >=48px targets and >=8px separate-control gaps; keyboard disclosure, radios, error focus and dialog containment/return. Review key modes and long content. Expand testing only for concrete shared-code impacts. | Complete: lint/build/typecheck, 258 unit tests, 20 E2E, six widths, browser axe and native200% keyboard checks. |
| 5 | Review diff, findings closure and design-consistency/security/delivery gates; record actual results and remaining manual limits; stop/remove only the task-owned disposable stack/helpers after preserving evidence | No application parity claim before visual and interaction proof. Historical/approved design hashes unchanged; main checkout preserved. Present local handoff and any unresolved failures. | Complete for local handoff: visual/security/delivery review recorded; corrected runtime retained by user instruction. Manual speech/physical IME remain Not run. |

### Resources and permitted actions upon revision3 approval

Frontend and test edits only in the existing feature/WI-012 worktree; routine
work-item/test-plan/evidence updates; existing installed/locked tools and dependencies.
Local Docker builds and startup of a uniquely named disposable Compose project,
unique volumes and unoccupied ports; owner migrations/calendar activation and
synthetic Admin/Operator, lines/products/calendar fixtures only in that isolated
project. Temporary credentials stay outside tracked artifacts/logs. Reset/remove
only this verified task-owned environment; never existing demo/live databases,
volumes, processes or other worktrees. Headed/headless browser observation,
screenshots and existing tests are permitted. This is local test execution, not a
live deployment. No videos are included.

Backend/API/schema/business rules, dependency upgrades, shared navigation redesign,
old approved documents or reviewed version2 artifacts are excluded. If a design
change becomes necessary, create an additive proposal and obtain review. No commit,
push, PR, merge, publication or live deployment is authorized by this revision.

### Risks and stop conditions

- Stop dependent work for business/API/schema changes, new dependencies, protected
  artifact changes or required access unavailable; present the affected revision.
- If isolated runtime cannot be established, record actual failure and retain
  parity as Not run; do not substitute static mockups for running application proof.
- Shared component changes require explicit scoped plan revision if outside the
  inherited-shell exception; local calendar helpers remain allowed.
- Screen-reader speech and physical mobile keyboard/IME may be unavailable. Record
  Not run separately; prior acceptance is not evidence for these new changes.
- A discrepancy requiring a new design is resolved before implementation depends
  on it. Existing design approval does not authorize silent alterations.

### Review and approval

Review status: approved.
Approval source: explicit user "approved" after revision3 presentation, 2026-10-05.
Additional instruction: leave the corrected local runtime running for user review.


## Revision 4 — Commit, PR and CI handoff

### Objective and scope

Deliver reviewed WI-012 calendar/mockup alignment as a focused PR to master.
Include only approved additive design/companions, scoped calendar frontend/test fixes
and required work-item records. Production-order defect investigation stays recorded;
its application fix is excluded. No redesign of approved documents.

### Ordered steps

1. Inspect final tracked/untracked diff, remove stale evidence references and verify
   approved historical design preservation against baseline de130ab. Review authorized
   new design companions and runtime-secret isolation. No generated test images/JSON.
2. Run final affected E2E after screenshot-removal edits using screenshot-off and trace
   screenshot-off settings, plus E2E typecheck. Existing258-unit/lint/build proof remains
   valid for unchanged application code. Broaden checks only for a concrete failure.
3. Pass design-consistency, security and delivery gates; update existing Markdown
   records with actual results and explicit manual limits. Apply pr-review skill.
4. Commit on feature/WI-012-calendar-mockup-alignment, push that feature branch and
   create a focused PR to master with scope/checks/manual limits in its description.
5. Observe required GitHub CI, resolve failures within approved scope and report PR,
   tested head and final CI state. Keep worktree/local runtime until later authorized
   merge/cleanup. Do not mark delivery complete while required CI fails or is pending.

### Permitted actions on approval

Local review and routine record/test adjustments within this scope; existing tools;
synthetic fixture operations only in the isolated pmai-wi012-review environment;
feature-branch commit/push, PR creation/update and CI observation. No test screenshot
capture, redundant artifact generation, dependency/backend/schema changes, unrelated
production-order fix, merge, live deployment, videos or runtime/worktree teardown.

### Risks and stop conditions

Stop dependent work for protected-design change, scope expansion, missing access or
failure requiring business/API changes. Screen-reader speech and physical mobile
keyboard/IME remain Not run. Existing production-order defect is separately documented
and must be disclosed in PR context without claiming WI-012 introduced it. CI can find
cross-screen regressions; correct only causes attributable to this approved scope.

### Review and approval

Approved by explicit user message "approved" after revision4 presentation, 2026-10-05. Commit/push/PR and CI handoff authorized; merge and live deployment excluded.


### Revision4 progress — 2026-10-05

Steps1–4 complete: focused diff/gates, final20 E2E/typecheck without image capture,
commit47d6dac, feature push and PR #39. Step5 CI observation in progress.


### Revision4 completion — 2026-10-05

Step5 complete: scoped fixture-isolation failure corrected; PR #39 tested application
head60b4f2a passed run37285875312 (239/195/258/62). Final record-only follow-up
preserves all tested code/design; no merge/live deployment. Await user merge instruction.


## Revision 5 — Merge and scoped cleanup

### Objective and ordered steps

1. Verify PR #39 remains reviewed, mergeable and all required checks pass on current
   head e3a1909 (or an explicitly reviewed equivalent record-only head).
2. Squash-merge PR #39 into master; confirm the actual merge commit and synchronize
   the main checkout without overwriting unrelated changes.
3. Update required closeout records and root README/ai/project/CLAUDE tracked state
   through a focused documentation PR if needed; report that PR separately.
4. Remove only the verified merged WI-012 feature branch/worktree and task-owned
   pmai-wi012-review runtime/volumes/temporary review resources after preserving the
   required Markdown records. Preserve existing demo services, prior videos and other
   worktrees. Close only the task-owned review browser if necessary for cleanup.
5. Report actual merge commit, remaining PR/check state and cleanup result.

### Permitted actions upon approval

GitHub squash merge of PR #39, main fetch/synchronization, scoped closeout records and
focused documentation commit/push/PR, removal of the verified merged WI-012 branch and
worktree, teardown of pmai-wi012-review only, and task-owned temporary review cleanup.
No existing demo database/volume/process changes, live deployment, videos, new feature
or production-order application fix. Do not merge any separate closeout PR without
explicit approval unless the user explicitly includes it in this revision approval.

### Risks and stop conditions

Stop for changed/unreviewed PR head, failed required checks, unrelated main/worktree
changes, unexpected ownership or cleanup target outside the verified task scope.
Preserve user changes; do not force cleanup through a dirty worktree. Manual accessibility
limits and the separate production-order defect remain recorded.

### Review and approval

Approved by explicit user "approved" after revision5 presentation, 2026-10-05. Merge PR #39, scoped cleanup and a separate closeout documentation PR authorized; merging that separate PR remains excluded.


### Revision5 execution — 2026-10-05

Steps1–2 complete: PR #39 reviewed head/checks confirmed; squash merge31b65f9 and
clean main synchronization. Step4 complete: implementation feature branch/worktree,
owned runtime/volumes/browser/temp removed; existing demo and prior evidence preserved.
Step3 closeout documentation PR preparation in progress; step5 reports its identity
and explicitly pending separate merge approval.
