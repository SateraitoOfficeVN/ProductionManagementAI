# Transcript: Screen A implementation (WI-002)

Source video: `demos/04-implementation/WI-002/ScreenA_Implementaion.mp4`. It runs 50 min 44 s, is a 1920×1034 screen recording with no audio, and was made on 2026-09-18 between about 11:31 and 12:22 local time. It picks up directly where `demos/02-detailed-design/WI-002/ScreenA_DD.mp4` ends, on the same unsent suggested prompt.

Setup shown: JetBrains Rider with the `ProductionManagementAI` solution. Claude Code v2.1.275 (Opus 5) runs in the IDE terminal in a session named **Screen A**, with **auto mode on**. Partway through, the IDE attaches to a git worktree at `D:\Work\AI\WMS-worktrees\WI-002` and the solution view gains all six backend projects. Near the end the user switches to a browser to use the running application.

How it was made: the transcript was built from frames sampled every second across the whole recording, keeping the frames where the terminal changed, plus denser frames around the prompts and the question form, and frame-by-frame sampling inside the long implementation turn to recover each milestone as it was announced. Wording in quotes is copied from the screen. Claude Code collapses the raw output of read-only commands (for example "Ran 1 shell command"). For those steps, the output recorded here is what Claude concluded and reported from them. The user's prompts are quoted with their spelling and grammar lightly corrected; the recording shows them exactly as they were typed. Commit hashes, decision IDs and step outcomes were checked against `work-items/WI-002/plan.md`, `decisions.md` and `evidence.md`.

This is the longest of the four recordings, and the shape is different: **three quarters of the agent's working time is a single 34-minute turn.** The interesting material is at both ends — the harness work and plan negotiation before it, and the merge reconciliation after it.

---

## Edited video for the presentation

`demos/04-implementation/WI-002/ScreenA_Implementaion_edited.mp4` is a 5 min 59 s cut of the 50 min 44 s recording:

- **The cut starts at step 2.** The session opens by applying RFC 0001 — the harness improvement proposed in the *detailed-design* step. That belongs to that step's story, so the cut begins at the implementation planning. Step 1 of this transcript is not in the video.
- **Plan revision 2 is shown, not just described.** Three clips cover it: the terminal summary with its caveats, then the document itself — the *Deliverables and milestones* table, and the *Roles and responsibilities* and authorization tables beneath it. The same plan reappears at 05:03 with its **Outcome** column filled in, beside the closing summary of the 34-minute turn.
- **Jump cuts, not speed-ups.** 25 clips, paced for reading — the text-heavy frames get 17 to 28 seconds each. The 34-minute turn alone is crossed by five "» jumped ahead" badges.
- **Step captions.** The video renumbers the remaining steps 1 to 11, so its captions do not match this transcript's numbering. The mapping is below. The accent is amber where the human acts and teal where the agent works.

| Transcript step | Video step | Starts at | Caption |
|---|---|---|---|
| 1 | — | *not in the cut* | RFC 0001 applied (see the detailed-design deck) |
| 2 | 1 | 00:00 | Decision form: E2E tool, accessibility, harness PR |
| 3 | 2 | 00:32 | Plan revision 2 — the caveats, then the document itself |
| 4 | 3 | 01:16 | Keep the design plan — restored below revision 2 |
| 5 | 4 | 01:42 | Chronological order — the rule first, then the file |
| 6 | 5 | 02:08 | Approved, and RFC 0002 written |
| 7 | 6 | 02:32 | Branches and worktrees |
| 8 | 7 | 02:44 | 35/35 tests, the CI verdict, decisions in code, CRLF prevented |
| 9 | 8 | 03:29 | REQ-019 in the component, then the focus bug |
| 10 | 9 | 03:58 | E2E, the least-privilege proof, and "Cooked for 34m 0s" |
| 11 | 10 | 05:17 | The screen exists |
| 12 | 11 | 05:35 | Reconciling `master` |

`demos/04-implementation/WI-002/ScreenA_Implementaion_presentation.pdf` is the 15-slide deck built from this transcript; slide 5 is the plan document, and slide 13 covers RFC 0002 — and records that RFC 0001 was applied here, even though the cut does not show it.

The times in the rest of this transcript refer to the original recording.

---

## At a glance

| # | Time | Who | Step | Output |
|---|---|---|---|---|
| 1 | 00:06–01:50 | User → Claude | Prompt 1: "Update the templates and skill so all DD companions are always produced" | RFC 0001 applied: 4 templates, the templates README, the detailed-design skill, a new evaluation case, an RFC record |
| 2 | ~02:00–03:17 | Claude ↔ User | Drafts plan revision 2, then a four-question form (E2E tool · A11y check · Git scope · Harness PR) | DEC-025–DEC-028 |
| 3 | 03:17–06:25 | Claude | Finishes plan revision 2 and lists what the user should know before approving | Plan revision 2, 14 steps; CI, dependencies and branch-name caveats stated |
| 4 | 08:18–09:35 | User → Claude | Prompt 2: "Keep the design plan in there so we can back-track it" | Revision 1 rebuilt and restored — but placed *below* revision 2 |
| 5 | 10:05–11:04 | User → Claude | Prompt 3: "No — put it in chronological order" | Rule updated **first**, then the file reordered oldest-first |
| 6 | 12:00–13:15 | User → Claude | Prompt 4: "OK, the revision 2 plan seems solid, so approved. Also update the harness" | Plan approved; RFC 0002 written |
| 7 | 13:15–15:28 | Claude | Two worktrees, two branches, harness commits; PR #2 opened | `feature/harness-wi002-feedback`, `feature/WI-002-production-order-screen` |
| 8 | 15:28–24:00 | Claude | Domain → Application → Infrastructure, migration, DB login split | 35/35 then 49 unit tests; CI blocked by a billing lock; DEC-029; a CRLF failure prevented before it happened |
| 9 | 24:00–35:20 | Claude | Api, integration tests, frontend | 38 integration + 38 frontend tests; **a real focus bug found and fixed** |
| 10 | 35:20–46:08 | Claude | E2E, the least-privilege proof, test plan, PR #3 | 8 journeys pass on the first run; `permission denied` for `pmai_app` confirmed; **"Cooked for 34m 0s"** |
| 11 | 46:10–48:30 | User | Prompt 5: "Merge both PRs" — then uses the running app | Screen A live at `http://localhost:3000` |
| 12 | 48:30–50:44 | Claude → User | Reconciles `master` after the squash merges | Two items needing the user's call; prompt 6 typed |

Timers the recording shows: **2 m 45 s** (plan revision 2), **23 s** (the reorder), **34 m 0 s** (the implementation), **1 m 21 s** (the reconciliation). The user typed six prompts and answered one four-question form.

---

## Detailed timeline

### Step 1 · 00:06–01:50 — Prompt 1, and the harness changes for real

The recording opens on the detailed-design step's closing summary, with Claude Code's suggestion still grey in the input line. The user sends it unchanged:

> **Update the templates and skill so all DD companions are always produced**

**Output:**

> "I'll treat this as a harness improvement per the standard workflow, documenting an RFC and evaluation case alongside the edits. First, I'm reading the exact passages that need to change."
>
> "Now applying the changes: the main DD template and its 3 companion templates, the templates README, the detailed-design skill, a new evaluation case, and an RFC record."

This is where **RFC 0001** stops being a proposal and becomes files. The detailed-design session offered the change; this one makes it, with the RFC and an evaluation case written alongside the edits rather than after them.

### Step 2 · ~02:00–03:17 — Plan revision 2, and four questions

Claude drafts the implementation plan, then stops on the choices it cannot make alone. A tabbed form opens: **E2E tool · A11y check · Git scope · Harness PR**.

**Tab 1: E2E tool.** *Which end-to-end test tool should Screen A use? No E2E tool has been chosen yet; WI-001 deferred it.*

1. **Playwright (Recommended)** — `@playwright/test` driving Chromium against the Docker Compose stack. Covers the create → edit and discard-dialog journeys; can also run automated accessibility (axe) checks. ← **chosen** (DEC-025)
2. Cypress — Also works well, but only runs JavaScript in the browser and has weaker multi-tab support.
3. No E2E for now — Rely on unit + integration tests; record E2E as not run. The DD's E2E test scenarios stay uncovered.

**Tab 2: A11y check.** *How should automated accessibility checks run?*

1. **axe in Vitest + E2E (Recommended)** — Add `vitest-axe` for component tests and `@axe-core/playwright` in E2E. Two new dev dependencies. ← **chosen** (DEC-026)
2. Manual only — No new dependencies; a manual keyboard + screen-reader pass is recorded in evidence.

**Tab 3: Git scope.** Which git operations are authorized for the implementation. ← **chosen:** branch, worktree, local commits, push, open PR; **merge not authorized** (DEC-027)

**Tab 4: Harness PR.** *The DD-template harness change isn't part of Screen A. Where should it go?*

1. **Separate branch/PR (Recommended)** — Commit the harness change on its own branch, so Screen A's PR contains only Screen A. ← **chosen** (DEC-028)
2. Same branch as Screen A — One branch, with the harness change as a separate commit. Simpler, but the PR mixes two topics.

### Step 3 · 03:17–06:25 — Plan revision 2, and the caveats

**Output:** a 14-step plan (worktrees and branches, domain, application, infrastructure, the DB login split, API, integration tests, frontend, E2E, test plan, verification run, push and PRs), plus an explicit list of what the user should weigh before approving:

> **Things to know before approving:**
> - **CI:** opening the PRs starts GitHub Actions. It will be the project's first real CI run.
> - **End-to-end tests stay out of CI:** they need the whole Compose stack running. They'll run locally, with results recorded in `evidence.md`.
> - **New dependencies:** OpenTelemetry, a fake-clock testing package for .NET, Playwright, `@axe-core/playwright` and `vitest-axe`. All pinned to exact versions and checked for known vulnerabilities.
> - **Branch name:** the implementation skill suggests `work-items/WI-002`, but I used `feature/WI-002-…` because that's the project's own decision from WI-001 (DEC-009).
>
> Approve revision 2 and I'll start with step 1.

*Baked for 2m 45s · done 11:37 AM*

The branch-name line is the agent reporting that it **deliberately disobeyed a skill** because a project decision outranked it. That contradiction was later fixed in the harness itself, as part of RFC 0003.

### Step 4 · 08:18–09:35 — Prompt 2, and a fix that isn't quite right

The user has been reading the plan. Revision 1 — the design plan they approved that morning — is gone, replaced by revision 2.

> **Keep the design plan in there so we can back-track it; don't just remove it when moving to the implementation phase**

**Output:**

> Revision 1 is back in `work-items/WI-002/plan.md`, kept in full **below** revision 2.
>
> - **Layout:** revision 2, the current one, stays at the top. At the end of the file there's now a "Revision history" section: a short table of both revisions (date, phase, state, who approved it), then all of revision 1 — objective, scope, inputs, steps, authorizations, risks and approval.
> - **Where it came from:** revision 1 was never committed, so git couldn't restore it. I rebuilt it from this session's own edits, exactly as it stood when you approved it, and added a note saying so.
> - **What I added:** an "Outcome" column showing when each step was done and what it produced — for example, step 5 lists all four DD files and the mockup link; a closing line recording your design approval and that revision 2 supersedes it; and, in the authorization table, a note that you approved publishing the mockup when I asked.

The content is restored. The order is wrong.

### Step 5 · 10:05–11:04 — Prompt 3, and the rule before the file

> **No — put it in chronological order; don't just move it below revision 2**

**Output:**

> "Got it: oldest first, so the file reads in the order things happened. **I'll update the saved rule first, then reorder the file.**"

and then:

> - **Revision 1: design phase**, complete and superseded. It keeps all of its sections, including the Outcome column and the closing note. It still carries the note that I rebuilt it from this session because it was never committed.
> - **Revision 2: implementation**, current. Its content is unchanged. Its opening now says revision 1 is "kept in full above".
>
> Each revision is a top-level section in the file, with its usual sections one level below. Future revisions will be added at the bottom.
>
> I updated the saved rule to match: every revision is kept in full, oldest first, and a new revision goes after the last one rather than above it.

*Brewed for 23s · done 11:41 AM*

Same pattern as the detailed-design step: **the rule is corrected before the artifact is.**

### Step 6 · 12:00–13:15 — Prompt 4: approval, and the second RFC

> **OK, the revision 2 plan seems solid, so approved. Also update the harness**

**Output: RFC 0002**, written the same way as RFC 0001 — status, affected files, summary, motivation with this session as evidence:

> **Status:** adopted
> **Affected:** `ai/templates/plan.md`, `ai/skills/planning/SKILL.md`, `ai/evaluations/baseline-cases.md`
>
> `plan.md` keeps every revision of a work item's plan, in full and in chronological order. A new revision is appended after the last one. The previous revision is closed in place (Outcome column, Closure line), and a revision index table at the top marks the current one.

### Step 7 · 13:15–15:28 — Branches, worktrees, and the first PR

**Output:**

- Worktree `../WMS-worktrees/harness-wi002-feedback` on `feature/harness-wi002-feedback`, carrying RFC 0001 and RFC 0002 as one commit each (`dc228ab`, `4912857`).
- Worktree `../WMS-worktrees/WI-002` on `feature/WI-002-production-order-screen`, with the WI-002 design files committed as the "design phase" commit (`5c40eaa`).
- The main worktree cleaned so `master` shows only `demos/` untracked — each copied file verified with `git diff --no-index` before being reverted.
- **PR #2 (harness) opened.**

> "PR #2 is open and CI has started. I'll check its result later. Now step 4, the domain layer. First the project settings, so the new code matches."

### Step 8 · 15:28–24:00 — Domain, application, infrastructure

The single long turn begins. Along the way Claude reports a deviation from the design rather than hiding it:

> "Writing the domain code per DD-001, but adapting the allowed-next-status table into an extension method (`status.AllowedNext()`) since C# enums can't hold methods — I'll update the docs to match later."

**17:02 — the first test run, and the CI verdict:**

> "Step 4 is done: **35/35 unit tests pass**, including the 4 existing ones. A quick check on the harness PR's CI run:"
>
> "Both CI jobs failed after 1 second. That points to **setup, not code**: the harness PR only changes Markdown. Checking the logs."
>
> "CI can't run since GitHub rejected both jobs due to a **billing lock on the account** — that needs to be cleared on your end. I'll note this as blocked in `evidence.md` and move on to step 5: adding the two packages needed at the application layer and checking them for known vulnerabilities."

**20:17 — decisions carried into the code.** `PlantOptions` is written with the decision IDs in its doc comment — *"IANA timezone ID that defines 'today' and the order-number year (DEC-011, DEC-017)"* — then: "Now wiring these into `AppDbContext` and the dependency-injection setup."

**24:01 — a clean build, a recorded deviation, and a failure caught before it happened:**

> "The solution builds with **0 warnings**. Recording the index deviation in DB-002 and `decisions.md` now, while it's fresh." *(DEC-029)*
>
> "Moving to step 7: splitting the database logins. First I'll check the current Compose, env file, and git line-ending setup, since CRLF endings in shell scripts can break inside the Postgres container."
>
> "There's no `.gitattributes` file, and with `core.autocrlf` set to true, the new init script would get CRLF line endings and **fail on Linux**. I'll add a `.gitattributes` rule to keep `*.sh` files as LF alongside the script."
>
> "Now Compose: the database container gets the init script and the new required password setting, and the backend connects as `pmai_app` instead of the owner."

**Output:** `ProductionOrder`, `ProductionOrderStatus`, `Product`, `DomainRuleViolation`; `ProductionOrderService`, DTOs, `Result`, `IPlantClock`, `IOrderNumberIssuer`, `IProductionOrderRepository` with `FakeTimeProvider` unit tests (`ProductionOrderServiceTests.cs`, 265 lines); EF configurations (`xmin`, the generated `order_number` column, the CHECK constraints), the `AddProductionOrders` migration seeding 30 products and creating the restricted `pmai_app` login with the DB-002 grants — **49 unit tests**, migration SQL reviewed, **DEC-029** recorded.

### Step 9 · 24:00–35:20 — API, integration tests, frontend

**Output:** controllers, the `ProductionOrderEditor` policy, `AddProblemDetails` with message-ID mapping, `[Consumes]`, `Plant:TimeZone` options with startup validation, OpenTelemetry; integration tests covering every DD-001 I-level viewpoint including concurrent creates, stale save, 401/403/415, the Problem Details shape and the runtime login's privileges (**38 passing**); then the frontend — `apiClient`, `AppHeader`, the feature folder, routes.

**31:10 — requirements quoted in the component.** `DiscardChangesDialog.tsx` (62 lines) opens with:

> `// BD-001 items 17-19 (REQ-019, DEC-018). The native <dialog> + showModal() gives the focus trap, Escape and`
> `// backdrop for free; Escape fires "cancel", which is treated as Keep editing.`

**35:20 — the tests catch a real defect:**

> "**Found a real bug:** React only calls `.focus()` on mount, not when the always-mounted dialog opens, so `autofocus` never fires as expected. I'll add an explicit focus call on **Keep editing** right after `showModal()`."
>
> "All **38 frontend tests pass**, including the existing login and route tests. Lint and `tsc` are clean. Running the production build, then committing step 10."

REQ-019 says focus must start on *Keep editing*. The component looked correct and was wrong; the test written from the requirement is what found it.

### Step 10 · 35:20–46:08 — E2E, the least-privilege proof, and the Screen A PR

**37:02 — careful with the user's environment:**

> "Step 11 begins now: running end-to-end tests against the real Docker Compose stack. First I'll check what's currently running and note that the worktree shares the `deploy` project name with the main folder, meaning they share the same database volume."
>
> "Nothing's running, so I'll wipe the local database volume **as authorized in step 7**, leaving the Data Protection keys volume intact. I'll create the worktree its own `deploy/.env`, copied from yours with a fresh `PMAI_APP_DB_PASSWORD`, **without printing any values**, while your main `.env` stays unchanged."

The password is generated with `python -c 'import secrets;print(secrets.token_urlsafe(24))'`, appended to the worktree's own `.env`, and the same command verifies that file is gitignored before the volume is recreated.

**39:06 — a harness rule quoted in the config.** The Playwright config carries its reasoning inline: `retries: 0, // a flaky journey is quarantined with a recorded reason, not silently retried (ai/rules/testing.md)` and `fullyParallel: false, // journeys share one database; keep them ordered and deterministic`.

**40:18 — the E2E run, and DEC-016 proved rather than asserted:**

> "**All 8 end-to-end journeys**, including the axe WCAG 2.2 AA scans and the two-user stale-save case, **pass on the first run** against the real stack. Next I'll verify on the Compose database that the runtime login can't delete orders."
>
> "**Confirmed the `permission denied` error for `pmai_app` on `production_orders`**, with the expected orders `PO-2026-00001` and `-00002` present on the fresh database. Next I'll update the test READMEs, `ai/project.md`'s verified commands, and the small DD alignments from implementation."

The least-privilege decision taken in the database-design step is closed here by attempting the forbidden operation and recording the refusal.

**Output:** the `tests/e2e` Playwright project with its own `package.json` (**8 journeys, desktop + Pixel 7**); `test-plan.md` TP-002 (TC-001–TC-027) with the DD-001 test-viewpoint IDs filled in; the security-review and delivery checklists; `evidence.md` and `status.md`; and **PR #3 (Screen A) opened**.

**The turn's closing summary:**

> - **An extra index (DEC-029):** EF Core always adds an index on `product_id`, which DB-002 had left out. Removing it would make the migrations drift, so I kept it and updated DB-002.
> - **Database init script:** Docker Desktop won't mount files from the worktree folder, so the script that creates the restricted login is built into a small Postgres image instead. It's also more portable.
> - **A real bug the tests caught:** the discard dialog wasn't putting focus on "Keep editing" when it opened. It's fixed and covered by tests.
> - **A harmless log line:** the backend container logs a Kerberos-library warning from the database driver. The app works normally; it's recorded in `evidence.md`.
>
> **What you need to do locally:** your old database volume was wiped, as the plan authorized, and recreated. Add `PMAI_APP_DB_PASSWORD` to your `deploy/.env` — my copy is only in the worktree's `.env`, and I didn't change yours. Migrations now run separately, as the owner login; `deploy/README.md` has the new order. The stack is still running at `http://localhost:3000` if you want to try the screen.
>
> **Limitations:** end-to-end tests aren't in CI yet and run on Chromium only. No manual screen-reader check was done.
>
> Next is your review of #3 and #2. Merging, and removing the two worktrees afterwards, needs your go-ahead.

*Cooked for 34m 0s · done 12:17 PM*

### Step 11 · 46:10–48:30 — Prompt 5, and the screen exists

> **Merge both PRs**

The user squash-merges both pull requests from their own GitHub account (DEC-030), then opens `http://localhost:3000/production-orders/new`.

**Output:** the real screen, signed in as **Seed Admin** — breadcrumb "Home / Production orders / New", "Order number: *Assigned on save*", status **Draft**, the product `<select>` showing `P-1001 — Steel bracket`, quantity, a date field, a notes box with its 0/500 counter, and Cancel / Save. The same layout as the mockup published in the detailed-design step, now backed by the API, the database and the restricted login.

### Step 12 · 48:30–50:44 — Reconciling after the merge

**Output:**

> Your local `master` is now at `1eccf9c`, with your `demos/` files untouched. The only difference from PR #3's head is the 12 harness files that came in with #2, so nothing was lost in the squash.
>
> **Two things still need your call:**
> 1. **The merge record is missing from `master`, and it's wrong.** Commit `c36a852`, which closes plan revision 2 and marks the work item done, only exists on the old branch, and it says I merged with merge commits. My suggestion: I correct it to say you squash-merged both PRs from your account, and open a small follow-up PR for you to merge. The other option is to leave the work-item files as they are on `master`: `status.md` then still says "in-review" and plan revision 2 has no closure line.
> 2. **Cleanup:** the two worktrees can now be removed and both remote branches deleted. The Docker stack is still running from the WI-002 worktree, whose `deploy/.env` holds the generated `PMAI_APP_DB_PASSWORD` — removing that worktree deletes that `.env` too, so add the password to your main `deploy/.env` first if you want to keep running the stack from `master`.
>
> CI still hasn't run on any of this; it's waiting on the GitHub billing lock.

*Crunched for 1m 21s · done 12:19 PM*

The user types the next prompt — **"Fix the record and open the follow-up PR, then clean up"** — and the recording ends.

---

## Harness improvements triggered by this session

This step produced the series' second improvement RFC, and it also *applied* the one the detailed-design step had proposed.

| # | Scope | What happened | Where it landed |
|---|---|---|---|
| 1 | **This work item** | Revision 1 rebuilt from the session's own edits (git could not help — it had never been committed) and restored in full | `work-items/WI-002/plan.md` |
| 2 | **This agent, from now on** | "I'll update the saved rule first, then reorder the file" — the memory was corrected before the artifact was | every revision kept in full, oldest first, new revisions appended |
| 3 | **The whole team, after review** | The user asked for the harness itself to be updated in the same prompt as the approval | RFC [`0002-plan-revision-history.md`](../../../ai/improvements/0002-plan-revision-history.md) |

The root cause was again a gap in the harness wording, not a lapse by the agent: `ai/templates/plan.md` said only "If this supersedes a prior revision, state which and why", and nothing said to keep the old one.

Two other harness observations surfaced here and were fixed later, in RFC 0003: the implementation skill's branch-name example contradicted the project's own branch policy (Claude flagged the conflict in step 3 and followed the project decision), and the Playwright suite ran only locally rather than in CI.

Also in this session, **RFC 0001 from the detailed-design step was applied** — four templates, the templates README, the detailed-design skill and a new evaluation case — and committed on its own branch as PR #2, because the user chose to keep harness changes out of Screen A's pull request (DEC-028).

---

## Files produced or changed in this session

| Area | Output |
|---|---|
| Harness | RFC 0001 applied (4 templates + README + skill + evaluation case); RFC 0002 written and applied (`ai/templates/plan.md`, `ai/skills/planning/SKILL.md`, `ai/evaluations/baseline-cases.md`) |
| Work item | `plan.md` revision 2 (14 steps) with revision 1 restored above it; `decisions.md` DEC-025–DEC-029; `test-plan.md` TP-002; `status.md`, `evidence.md` |
| Backend | `Domain/ProductionOrders/`, `Application/ProductionOrders/`, `Infrastructure/ProductionOrders/` + `Migrations/AddProductionOrders`, `Api/` controllers, policy, Problem Details, OpenTelemetry |
| Database | Restricted `pmai_app` login baked into `deploy/docker/db.Dockerfile`; `.env.example`, `deploy/README.md`, `ai/project.md` updated |
| Frontend | `apiClient`, `AppHeader`, the production-orders feature folder, routes |
| Tests | 49 unit, 38 integration, 38 frontend (with `vitest-axe`), 8 Playwright journeys (with `@axe-core/playwright`) |
| Git | Two worktrees, two branches, PR #2 (harness) and PR #3 (Screen A) |

---

## Key points for the presentation

1. **A proposal becomes files.** The previous step *offered* a harness change; this one writes it — with the RFC and an evaluation case produced alongside the edits, not afterwards.
2. **The plan names its own costs.** Before asking for approval, Claude listed the first real CI run, the five new dependencies (pinned and vulnerability-checked), what stays out of CI and why.
3. **The agent reports when it overrides a rule.** The implementation skill suggested one branch name; the project's own DEC-009 said another. Claude followed the project and *said so* in the approval request.
4. **The human protects the audit trail.** "Keep the design plan in there so we can back-track it" — and then, when the fix put it in the wrong place, "put it in chronological order". Two corrections, one minute of typing.
5. **Correct the rule before the artifact.** "I'll update the saved rule first, then reorder the file." The same ordering as the detailed-design step.
6. **Harness changes travel separately.** The user chose a separate branch and PR so Screen A's review contains only Screen A (DEC-028).
7. **34 minutes, one turn.** Domain through E2E ran unattended — and ended with a summary that separates what was built, what the tests caught, what the user must do locally, and what is still missing.
8. **Tests caught a real bug, and the cause was understood.** "React only calls `.focus()` on mount, not when the always-mounted dialog opens, so `autofocus` never fires." REQ-019's dialog looked correct and was wrong; the test written from the requirement found it.
9. **A failure was prevented, not debugged.** Before writing the database init script, Claude checked `core.autocrlf`, saw the shell script would get CRLF endings and fail inside the Linux container, and added a `.gitattributes` rule first.
10. **A decision was proved, not asserted.** DEC-016's restricted login was closed by attempting a `DELETE` as `pmai_app` and recording the `permission denied` error.
11. **Secrets were handled carefully without being asked.** A fresh `PMAI_APP_DB_PASSWORD` was generated into the worktree's own `.env`, never printed, checked as gitignored, and the user's own `.env` was left untouched.
12. **Deviations are reported, not buried.** The extra EF index (DEC-029), the Docker file-sharing workaround, the `AllowedNext()` departure from DD-001 and the harmless driver warning — all surfaced in the summary and recorded in `evidence.md`, DB-002 or the DD.
13. **Merging stayed with the human.** DEC-027 authorized push and PR but not merge. The user squash-merged both PRs themselves, and Claude's next job was to reconcile `master` and flag a merge record that was now wrong.

## Gaps in this transcript

- **Collapsed tool output.** Claude Code folds the raw output of read-only commands into a one-line summary ("Ran 2 shell commands"). Where that happens, the output recorded here is what Claude reported from them. This cannot be recovered from the recording — the raw text was never on screen.
- **Inside the 34-minute turn.** Every milestone Claude announced during that turn was recovered by sampling the recording at its full frame rate, and the quotes in steps 8 to 10 are from the screen. What still scrolled past too fast to read is the *body* of the files as they streamed — the source of the committed code, which is in the repository at `5c40eaa`, `a2c1ea7`, `e4a5256` and `f5609d7`.
- **Commit hashes** are from `work-items/WI-002/plan.md`'s Outcome column rather than the terminal, where they appeared only inside `git` output that Claude Code collapsed.
- CI had not run on any of this by the end of the recording — the GitHub billing lock blocked it, as step 8 records. CI first executed later, on PR #5; see `ai/project.md`.
