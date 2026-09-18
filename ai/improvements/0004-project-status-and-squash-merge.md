# RFC: Refresh project status after WI-002; record the squash-merge rule

**Status:** adopted
**Affected:** `ai/project.md`, `ai/rules/git-review.md`, `ai/harness-overview.md`, `ai/evaluations/baseline-cases.md`

## Summary

This RFC updates `ai/project.md` to match the repository after WI-002. WI-002 is now recorded as merged and Screen B as the next planned work, and CI execution is no longer listed as an open decision. It also adds the squash-merge convention (WI-002 DEC-030) to `ai/rules/git-review.md`, so the convention is a shared rule rather than something only one agent's local memory holds.

## Motivation

- **Stale open decision.** `project.md` listed "Merge/deploy permissions and actual CI execution — not authorized for this scaffold yet" as open. CI has run and passed since PR #5 (recorded in the same file's **CI** section), so the file contradicted itself. Only standing merge/deploy permissions are still open.
- **Current status visible to Claude only.** `CLAUDE.md` said WI-002 was done and Screen B was next, but `project.md`, which both agents read via `AGENTS.md`, described only WI-001 as the baseline. A Codex session would not see the current state.
- **Merge convention not in the harness.** PRs #2 to #6 were all squash-merged (the only merge commit on `master` is PR #1's), starting with WI-002 DEC-030. No rule under `ai/` said so. The convention lived only in Claude's local auto-memory, which Codex never sees and which could be lost.
- **Source:** a review of whether `AGENTS.md` needed updating, 2026-09-18. `AGENTS.md` itself was still accurate, and the gaps were in the files it points to.

## Guide-level explanation

- An agent starting from `AGENTS.md` learns from `project.md` that WI-001 and WI-002 are merged, that Screen B is next, and that CI runs on every PR. It won't treat CI execution as an open question.
- When a merge is authorized, the agent uses "Squash and merge". Authorization for the merge itself is still required, per `policies.md`.

## Reference-level explanation

- **Current behavior:** described under Motivation.
- **Proposed behavior:**
  - `project.md`, **Open decisions:** the combined entry now covers only standing merge/deploy permissions, and it states that CI execution is settled.
  - `project.md`, **Current implementation:** lists WI-001 and WI-002 as merged (with PR numbers), notes the `pmai_app` / owner split introduced in WI-002, and names Screen B as next.
  - `git-review.md`: new rule that PRs into `master` are merged with "Squash and merge".
  - `harness-overview.md`: the `project.md` and `git-review.md` summaries match the changes above, and the "Where it stands" section mentions this RFC.
  - `baseline-cases.md`: new case for an authorized merge.
- **Definition of done:** no file under `ai/` lists CI execution as open or omits WI-002's status, and the squash-merge rule appears in `git-review.md` and has an evaluation case.

## Drawbacks

- `project.md`'s status now has to be updated whenever a work item merges. That is already expected of it, as the file that tracks current implementation.

## Rationale and alternatives

- **Put the status in `AGENTS.md`:** rejected. `AGENTS.md` is deliberately routing-only, and `project.md` is the shared home for project state.
- **Leave the merge convention in decisions.md (DEC-030) only:** rejected. DEC-030 records a single decision about two PRs. An agent preparing a future merge reads the rules, not old work-item decisions.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "Authorized to merge a reviewed PR into `master`" | Not covered: no rule named a merge method | Squash and merge per `git-review.md`; authorization still required | pass (manual check against the updated `git-review.md` and `policies.md`) |
| "Switch from Claude to Codex midway" | `project.md` omitted WI-002's status, which only `CLAUDE.md` had | `project.md`, reached from `AGENTS.md`, states WI-002 is merged and Screen B is next | pass (manual check) |
| "Request depends on a technology choice `ai/project.md` still lists as open" | CI execution was wrongly listed as open | Only genuinely open items remain | pass (manual check) |
| "Asked to prepare code, deployment not authorized" | Unchanged | Unchanged: merge and deploy still need task-specific authorization | pass (no gate relaxed) |

## Risk and rollback

- **Risk:** low. This change adds and corrects documentation only and relaxes no gate. The merge rule constrains how a merge happens, not whether one is allowed.
- **Rollback plan:** revert this commit.

## Unresolved questions

- Whether to enforce squash-only merges in GitHub's repository settings. That is a repository-administration action and outside this change.

## Adoption

- **Reviewer:** ThanhTN (explicit request, 2026-09-18: "make the three edits", then "commit, push and open the PR")
- **Adopted revision:** the commit on `feature/harness-project-status-refresh` that adds this RFC (effective for shared use once that branch's PR is merged)
