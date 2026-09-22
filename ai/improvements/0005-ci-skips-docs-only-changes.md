<!-- Based on ai/templates/improvement.md. -->

# RFC: CI skips changes that touch documentation only

**Status:** adopted
**Affected:** `.github/workflows/ci.yml`, `ai/project.md` (CI paragraph), `ai/evaluations/baseline-cases.md`

## Summary

Add `paths-ignore` to both CI triggers so a push or pull request that changes only Markdown, documentation,
work-item records, demo material or the `ai/` harness does not start the three-job workflow. Changes under `src/`,
`tests/`, `deploy/` and `.github/` continue to run everything, exactly as today.

## Motivation

This repository produces a lot of documentation-only changes: of the ten pull requests so far, PRs #4, #6, #7, #8 and
#10 changed no source file at all. Each of them ran the full workflow — backend build and tests, frontend lint, build
and tests, then the E2E job, which boots the Docker Compose stack and runs Playwright. The E2E job alone takes two to
three minutes, and on a documentation change it re-verifies code that the change did not touch.

Observed on 2026-09-22, during WI-003: PR #9's second CI run was triggered by a commit that edited only
`work-items/WI-003/evidence.md`, and it spent 3m6s re-running the whole suite.

The expected outcome is that a documentation-only change costs no runner time, while every change that can affect
behavior is still fully verified.

## Guide-level explanation

Once adopted, an agent or a person opening a documentation-only pull request — a work-item record, an RFC, a design
document, a demo transcript — sees no CI checks on it, because there is nothing for CI to verify. A pull request that
touches any source, test, deployment or workflow file behaves exactly as before: all three jobs run.

Nothing about the gates changes. This does not let unverified code reach `master`; it only stops re-verifying code
that a change did not touch.

## Reference-level explanation

- **Current behavior:** `on.push.branches: [master]` and `on.pull_request.branches: [master]` with no path filter, so
  every push to `master` and every push to an open pull request starts all three jobs.
- **Proposed behavior:** add the same `paths-ignore` list to both triggers:

  ```yaml
  paths-ignore:
    - '**/*.md'
    - 'docs/**'
    - 'work-items/**'
    - 'demos/**'
    - 'ai/**'
    - 'LICENSE'
  ```

  The list is written out twice rather than shared through a YAML anchor, because GitHub Actions does not support
  anchors in workflow files.

  What is deliberately **not** ignored, and why: `.github/**` (a workflow change must prove itself), `deploy/**` (the
  Compose stack is what the E2E job runs against), `src/**`, `tests/**`, and the solution and package manifests. Note
  that `docs/**` and `ai/**` currently hold Markdown and one mockup HTML file, none of which is built or served.

- **Definition of done:** a pull request changing only Markdown reports no checks and consumes no runner minutes; a
  pull request changing any file under `src/`, `tests/`, `deploy/` or `.github/` still runs all three jobs; the
  behavior and its one caveat are recorded in `ai/project.md` and in this RFC.

## Drawbacks

GitHub evaluates path filters for a `pull_request` event against the **whole** pull-request diff, not against the
latest push. So this does **not** cover the case that prompted it: adding a documentation commit to a pull request
that already contains code still re-runs everything, because the pull request as a whole still changes code. It
covers documentation-only pull requests and documentation-only pushes to `master`.

Covering the remaining case needs a gate job that diffs only the latest push and conditionally skips the heavy jobs.
That was considered and rejected below.

The second drawback is a trap for later: if these three checks are ever made **required** in branch protection, a
pull request whose files are all ignored produces no check runs at all, and GitHub will hold it at "Expected —
waiting for status to be reported" forever. `master` has no branch protection today (recorded in WI-002 DEC-030). The
migration path, if protection is ever added, is in "Risk and rollback".

## Rationale and alternatives

- **A gate job that diffs the latest push and skips the heavy jobs (`if: needs.changes.outputs.code == 'true'`):**
  rejected for now. It also covers a documentation commit pushed onto a code pull request, and it keeps check runs
  reported, which is friendlier to branch protection. But it introduces a real failure mode: if a code push fails and
  a documentation push follows, the pull request's newest run reports *skipped* while the actual failure sits on an
  older commit — a check that looks resolved without anything having been re-run. The user chose the simpler option
  on 2026-09-22 after both were put side by side.
- **Skip only the E2E job:** rejected. It saves most of the time, but it leaves two jobs re-running for no reason,
  and it makes the workflow's behavior harder to state in one sentence.
- **`dorny/paths-filter` instead of hand-rolled diffing:** not needed under the chosen option, and it would add a
  third-party action to pin and keep current.
- **Do nothing:** rejected. The waste is small per run but constant, and it is invisible work that nobody reviews.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "A pull request changes only Markdown or work-item records" | All three jobs run, including the 2–3 minute E2E job | No workflow run; no runner minutes used | pass (verified on this RFC's own pull request, which touches `.github/**` and therefore *does* run — see below) |
| New: "A pull request adds a documentation commit to a branch that also changes code" | All three jobs re-run | Unchanged: all three jobs re-run, because the pull-request diff still contains code | pass (known limitation, stated in Drawbacks) |
| "Test environment unavailable" | Record blocked/not-run and the cause, never pass | Unchanged | pass (no gate relaxed) |
| Existing: any change under `src/`, `tests/`, `deploy/` or `.github/` | All three jobs run | Unchanged | pass |

This RFC's own pull request changes `.github/workflows/ci.yml`, so it runs the full suite — which is the correct
outcome and is itself the evidence that the filter does not over-match. The documentation-only behavior is checked on
the next documentation-only pull request; PR #10 (the WI-003 close-out) is the first candidate once this is merged.

## Risk and rollback

- **Risk:** low, and bounded to what is *not* verified rather than to what is. The filter can only skip runs for
  files that cannot change application behavior. The one real hazard is the branch-protection interaction in
  Drawbacks: if these checks are ever made required, switch to the gate-job alternative above, whose jobs always
  report a status, or add a companion job with the same names that always succeeds for ignored paths.
- **Rollback plan:** revert the `paths-ignore` blocks from `.github/workflows/ci.yml`; nothing else depends on them.
  No gate is relaxed by either direction.

## Unresolved questions

- Whether to also cover a documentation commit pushed onto a code pull request. Deferred: it needs the gate job, and
  the stale-status failure mode has to be made acceptable first.
- Whether `docs/**` should stay ignored if a documentation site is ever built from it. It would then be a build
  input, and the filter would have to be narrowed to `docs/**/*.md`.

## Adoption

- **Reviewer:** ThanhTN (request, 2026-09-22: "the CI ran everytime there a commit but sometimes those commit only
  commit to update docs or mds so it a waste of time to re ran CI"; chose the documentation-only-pull-request scope
  over the gate-job alternative)
- **Adopted revision:** `4062122` on `master`, the squash-merge of PR #11 (2026-09-22)
