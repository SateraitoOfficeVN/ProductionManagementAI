# WI-011 — Bug-fix execution plan

| Revision | Date | Purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-02 | Calendar action height and spacing correction | Complete — local scope | Explicit user "approved" after revision 1 presentation, 2026-10-02 |
| 2 | 2026-10-02 | Commit, PR and CI handoff | Current — approved | Explicit user "approved" after revision 2 presentation, 2026-10-02 |

## Revision 1 — Local diagnosis, correction and verification

### Objective and scope

Resolve BUG-003/004 under REQ-076–078. Correct calendar action alignment and spacing
in the four calendar components without altering business behavior or approved
screen design. Do not force fixed heights on multi-line actions or date cells.

### Steps and verification

| # | Step | Verification / stop condition | Outcome |
| --- | --- | --- | --- |
| 1 | Reproduce on the baseline and measure affected button rectangles/gaps at desktop/mobile; inspect normal/history/error/Unknown layouts | Record actual failing geometry and screenshots; distinguish intentional date-cell size from command-button stretching. Use existing locked Playwright and isolated fixtures only. | Done: baseline 72/120 px actions and 0 px search gap; regression fails before correction |
| 2 | Confirm correction conforms to approved 006 designs and accessibility targets | Design-consistency review for presentation-only correction. If a design/contract change is needed, present a new revision/additive design and stop before implementation. | Done: proportional design-consistency pass; approved designs immutable, no amendment needed |
| 3 | Correct calendar-local grid/flex alignment and vertical grouping | Ordinary single-line actions 48 px; at least 8 px separation; long labels grow/wrap; no old design edits or unrelated-screen/shared style change. | Done: four calendar components corrected; shared styling/handlers untouched |
| 4 | Add practical Playwright layout regressions and run affected checks | Browser geometry assertions for heights/gaps, desktop/mobile/320 px, history/recovery states; existing calendar journeys, frontend lint/build/unit tests, keyboard/centered-dialog checks and native 200% zoom. Record actual results; no CSS-class/jsdom claim of visual proof. | Done: lint/build, 253 frontend tests, 17 production Playwright cases, native 200% zoom/keyboard pass |
| 5 | Review diff and perform local delivery/security assessment; reconcile records and clean owned fixtures | No application/API/data/permission/dependency drift, immutable designs preserved, existing videos untouched. Record remaining limits and hand off local results. | Done: scoped review/delivery pass, 209 protected hashes unchanged, fixtures/helpers/traces cleaned |

### Resources and permissions if revision 1 is approved

Permitted: local calendar frontend/test changes in this dedicated worktree,
locked dependency restore, local build/test/browser checks and disposable fixture
creation/migration/explicit owner activation/cleanup strictly for reproduction.
Fresh named Compose project/database/unused ports, temporary generated credentials;
verify resource labels/resolved paths before cleanup. No existing demo/live writes.
Screenshots/logs are verification artifacts, not replacement final videos.

Not authorized: commit/push/PR/merge, deployment/live activation, publishing,
new videos, dependencies/actions/jobs, business/API/schema changes or next feature.
Retain main checkout and prior evidence checkout/videos; no direct master edits.

### Artifacts and gates

Work-item brief/plan/status/decisions/evidence and eventual review/test results.
No new design document is planned: restore the approved action/reflow intent.
Use implementation/testing/pr-review skills after approval; applicable gates are
proportional design-consistency, security-review and delivery. Release-readiness
is not applicable without an authorized deployment.

### Risks and stop conditions

- Fixed height clips Japanese text/zoom: use content-safe sizing, test wrapping/reflow.
- Shared button class change regresses Production lines: prefer local parent layout;
  if unavoidable, propose scope revision before changing other screens.
- Recovery/history state coverage differs from normal layout: exercise controlled
  fixtures and verify actual geometry, preserving Unknown/no-replay behavior.
- Unrelated behavior defect or design change needed: document it and stop affected
  work for a reviewed revision. Never silently weaken a test/gate.
- Unavailable manual tools/device: record Not run; prior WI-010 limitations remain.

### Review and approval

Review status: approved. Approval source: explicit user "approved" after this revision
was presented, 2026-10-02. Approved revision: 1. Local execution authorized;
commit/push/PR/merge remain excluded.

### Closure — 2026-10-02

All five local revision 1 steps completed after explicit approval. Remains
uncommitted in the dedicated feature worktree. Commit/push/PR/merge not authorized.
Physical mobile keyboard/IME and screen-reader speech not run; existing manual
verification limits remain. Final local evidence/review supports review and a
subsequent separately authorized external-delivery step. No new videos produced.
## Revision 2 — Commit, PR and CI handoff

### Objective and scope

Deliver the completed BUG-003/004 corrections and TC-407–409 regressions through
one reviewable PR to master. User requested continuation of WI-011 after the four
new videos were delivered. This request authorizes drafting this next revision;
revision 1 explicitly excluded commit/push/PR and does not approve revision 2.

Existing package: four calendar components, one browser regression spec and WI-011
records/screenshots/measurements. Local lint/build, 253 frontend tests, 17 production
Playwright cases and native 200% keyboard checks already pass. Four separately
requested videos are ignored local artifacts; preserve them without force-adding
MP4s or intermediary assets. Completed approved design files remain immutable.

### Steps, checks and stop conditions

| # | Step | Verification / stop condition | State |
| --- | --- | --- | --- |
| 1 | Reconcile local review, evidence and current-state records for PR delivery; inspect complete candidate diff and selected files | Preserve unrelated changes and historical approvals. README.md, ai/project.md and CLAUDE.md describe only tracked/current PR state, not a merged release. Recheck source hashes against recorded evidence, approved artifact protection, whitespace and proportional delivery/security gates. Reuse unchanged-source test results; rerun affected checks only if code changes. | Complete: diff reviewed, four source/video hashes and 209 protected hashes unchanged; gates pass |
| 2 | Commit only WI-011 source/test/records and necessary current-state updates on feature/WI-011-calendar-button-layout; push that branch and create a PR targeting master | Inspect staged diff/file list; no secrets, old design edits, ignored MP4s, dependency or unrelated changes. PR English summary identifies BUG-003/004, actual checks, local evidence paths and manual limits. Record exact commit and PR URL. | Complete: reviewed commit 7c55605 pushed; PR #38 opened |
| 3 | Observe GitHub CI and reconcile results/review on the exact PR head | Report real job outcomes and counts, never substitute local results for CI. Diagnose failures; fix only defects within existing calendar layout/test scope, rerun affected checks and update PR. Material scope/design or external-access problems stop affected work. Record handoff when required CI and review pass. | In progress: final PR head CI under observation |

### Permissions requested by approval of revision 2

Local routine records/current-state updates, scoped checks when needed, git commit,
branch push, PR creation/update and observation of resulting GitHub Actions.
Disposable local fixtures and their verified cleanup only if an affected rerun
needs them. No new dependency, CI job/action, application feature or business rule.

Merge, branch/worktree deletion, production deployment/calendar activation and
external image/video publishing remain outside this revision. Keep the feature
worktree for PR review. Do not treat successful CI as merge authorization.

### Risks and gates

- Unrelated edits or stale evidence: inspect exact candidate files and source hashes;
  preserve changes outside WI-011 and report evidence invalidated by code changes.
- CI failure: record actual failure, investigate within approved scope; no weakened
  assertion, hidden retry or unsupported claim of passing jobs.
- Protected designs/video loss: compare hashes and exclude ignored video files;
  no old approved design edits or replacement evidence recording.
- Manual speech/physical mobile keyboard/IME remain Not run as previously accepted.
- Apply git-review/pr-review skills and proportional security/delivery checklists.
  Release-readiness is not applicable because deployment is not authorized.

### Review and approval

Review status: approved.
Approval source: explicit user "approved" after revision 2 presentation, 2026-10-02.
Current and approved revision: 2. Commit/push/PR and CI handoff authorized.
Merge, branch/worktree deletion and deployment remain excluded.
