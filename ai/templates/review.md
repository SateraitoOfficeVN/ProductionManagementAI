<!-- Code review report template, based on Google's "What to look for in a code review" engineering practices (google.github.io/eng-practices/review/reviewer/looking-for.html). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Work Item Title} — Code Review

Reviewing {PR link or commit/revision}, {YYYY-MM-DD}.

## Change summary

{What this change actually does, in concrete behavioral terms — not a restatement of the PR title. Note what is explicitly out of scope for this review.}

## Review checklist

- [ ] **Design:** the change belongs where it is, and integrates sensibly with the rest of the system
- [ ] **Functionality:** the change does what it was intended to do, and that is good for its users
- [ ] **Complexity:** no more complex than necessary at the line, function and class level
- [ ] **Tests:** correct, sensible, useful coverage exists for the changed behavior
- [ ] **Naming:** names communicate what things are or do
- [ ] **Comments:** explain why, not what; code is clear enough to not need narration
- [ ] **Style:** conforms to the project's style/rules (ai/rules/)
- [ ] **Documentation:** relevant docs/templates updated where the change affects them

## Findings

| # | Severity (blocker / major / minor / nit) | Location (file:line) | Impact | Suggested resolution |
| --- | --- | --- | --- | --- |
| 1 | {severity} | {path:line} | {concrete failure scenario or consequence} | {specific fix} |

## Design and test consistency

| Design / test artifact | Consistent? | Note |
| --- | --- | --- |
| {basic-design.md / detailed-design.md / database-design.md / test-plan.md section} | {yes \| no} | {what diverges, if anything} |

## Verification performed

| Check | Result |
| --- | --- |
| {command or manual check actually run} | {actual result} |

## Unresolved issues and disposition

| Issue | Disposition (fix now / follow-up / won't fix) | Owner |
| --- | --- | --- |
| {issue not resolved in this review} | {disposition} | {who owns it} |
