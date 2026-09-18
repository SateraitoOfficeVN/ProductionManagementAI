# RFC: Align rules and skills with the project's decisions; run E2E in CI

**Status:** adopted
**Affected:** `ai/rules/frontend.md`, `ai/rules/backend.md`, `ai/skills/screen-design/SKILL.md`, `ai/skills/implementation/SKILL.md`, `ai/evaluations/baseline-cases.md`, `.github/workflows/ci.yml`

## Summary

Three gaps listed in `ai/harness-overview.md` ("Known gaps") are closed:

1. The frontend and backend rules still called the UI framework and ORM "not selected", although `ai/project.md` has recorded React + Tailwind and EF Core in a layered structure since WI-001.
2. The implementation skill's branch-name example (`work-items/<WI-###>`) contradicted the project's branch policy (`feature/<WI-###>-<slug>`, WI-001 DEC-009).
3. The Playwright E2E suite (WI-002, DEC-025) ran only locally. CI now has an E2E job.

## Motivation

- **Rules lagged the project.** An agent following `rules/frontend.md` literally would treat React as unapproved and pause to ask a question the project already answered. `screen-design` quoted the stale wording. The baseline evaluation case "do not assume React" encoded the same stale fact.
- **The implementation skill contradicted the project's branch policy.** WI-002 had to resolve the conflict by hand, recorded in its plan revision 2 inputs table.
- **E2E wasn't in CI.** E2E regressions would only be caught when someone runs the suite locally.
- **Source:** the user's request "fix all the gaps", 2026-09-18, after the overview refresh listed them.

## Guide-level explanation

- The frontend and backend rules state the confirmed stack. Adding a component kit, another UI framework, another ORM or another architectural pattern is a new project decision, not something an agent picks during implementation.
- Branches follow `project.md`'s branch policy; the implementation skill points there instead of inventing its own naming.
- Every PR to `master` runs the E2E job after the backend and frontend jobs. It:
  1. generates throwaway credentials and masks them;
  2. builds the Compose stack;
  3. applies migrations as the owner;
  4. runs the Playwright journeys against the running stack;
  5. uploads the report on failure;
  6. always tears the stack down.

## Reference-level explanation

- **Current behavior:** described under Motivation.
- **Proposed behavior:**
  - `frontend.md` and `backend.md` name the confirmed stack and make departures a new project decision.
  - `screen-design` step 3 refers to the rule rather than quoting stale text.
  - `implementation` section 5 names the branch per `project.md`.
  - The baseline evaluation case "do not assume React" is replaced by two cases:
    - a technology that is still open (deployment host), to be identified before depending on it;
    - adding a component kit or ORM, to be treated as a new decision.
  - `ci.yml` gains the `e2e` job. It uses the existing SHA-pinned actions plus `actions/upload-artifact` pinned to `043fb46d1a93c77aae656e7c1c64a875d1fc6a0a` (v7.0.1), with read-only default permissions, and adds no repository secrets.
- **Definition of done:**
  - No rule or skill contradicts `project.md`.
  - `actionlint` (with shellcheck) passes.
  - The E2E job's steps pass when rehearsed locally.
  - The job itself passes in GitHub Actions once CI can run.

## Drawbacks

- The E2E job adds several minutes to each PR: it builds three images and installs Chromium.
- The rules now name specific technologies, so a future stack change must update them as well as `project.md`.

## Rationale and alternatives

- **Keep the rules technology-neutral and point only to `project.md`:** rejected. The rules exist to state the current standard concretely, and "see project.md" alone lost the "a departure needs a decision" guidance.
- **Run E2E against the Vite dev server:** rejected. It would skip the Nginx proxy, the restricted DB login and the real image builds, the very things the Compose stack verifies.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| Old: "Request code without selecting a UI framework → do not assume React" | Encoded a stale fact | Replaced (below) | n/a (retired) |
| New: "Request depends on a technology `project.md` lists as open" | Not covered | Identify the missing choice before dependent setup | pass (manual check: `project.md` lists the deployment host as open; no rule or skill assumes one) |
| New: "Request to add a UI component kit or another ORM" | The rules would have treated the choice as still open | Pause and ask; record in decisions.md | pass (manual check against the updated `frontend.md` / `backend.md`) |
| E2E job | Not in CI | `actionlint` 1.7.12 + shellcheck: pass. Local rehearsal of the job's steps, on a clean worktree with generated credentials: the first run failed (8/8) because `dotnet ef` needs a restore on a fresh checkout — fixed by adding `dotnet restore`; the second run passed 8/8 | pass locally; **not run in GitHub Actions** (account billing lock) |
| "Approved feature plan, clear next step"; "DD field conflicts with DB constraint" | Unchanged | Unchanged | pass (no gate relaxed) |

## Risk and rollback

- **Risk:** the E2E job could behave differently on GitHub's Ubuntu runners than in the Windows rehearsal (ports, Docker Compose version, Playwright system dependencies via `--with-deps`). Its first real run needs watching once CI is unblocked.
- **Rollback plan:** revert this commit. The rule/skill wording and the E2E job are independent, so either part can be reverted alone. No gate was relaxed.

## Unresolved questions

- None. The remaining CI gap (jobs not starting) is an account issue outside the repository.

## Adoption

- **Reviewer:** ThanhTN (explicit request, 2026-09-18: "fix all the gaps and then push it then create PR")
- **Adopted revision:** the commit on `docs/harness-overview-refresh` that adds this RFC (effective for shared use once that branch's PR is merged)
