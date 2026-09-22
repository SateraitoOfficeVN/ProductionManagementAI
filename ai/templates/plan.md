<!-- Implementation Plan template, based on common project/implementation-plan conventions (PMI-style plans, Smartsheet/TeamGantt implementation plan templates). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval.
Revision rule: plan.md keeps every revision in full, in chronological order (oldest first). A new revision is appended after the last one as its own "## Revision {N}" section; earlier revisions are never deleted, overwritten or moved below the new one. When a revision is superseded, fill in its Outcome column and its Closure line, and update the revision index table at the top so the current revision is marked. -->

# {Work Item Title} — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| {1} | {YYYY-MM-DD} | {e.g. design, implementation} | {draft \| submitted for review \| approved \| complete; superseded by revision N \| **current**} | {user message / reference, or "—"} |

## Revision {N} — {phase / purpose}

Revision {N}, {YYYY-MM-DD}. {If this supersedes a prior revision, state which and why; the prior revision stays above, unchanged except for its Outcome column and Closure line.}

### Objective

{One or two sentences stating the outcome this plan delivers, tracing back to brief.md.}

### Scope

#### In scope

- {bounded piece of work this plan covers}

#### Out of scope

- {adjacent work explicitly excluded from this plan}

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| {artifact} | {revision, or "not yet written"} | {assumption, flagged for confirmation} |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | {concrete action} | {step # or "none"} | {ai/skills/... name} | {file/path or "none"} | {command, review, or check that proves it is done} | {filled in as steps complete: done {date} — {result}, or blocked/skipped with reason} |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| {plan author \| implementer \| reviewer \| approver} | {who, or "this agent"} |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| {action} | {yes \| no \| ask at execution} | {request text, plan approval, or "not yet authorized"} | {branch, environment, or artifact the authorization is bounded to} |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| {requirement conflict, missing decision, scope change, missing access, action exceeding authorization} | {what would signal this} | {pause and ask per ai/policies.md; record the question in decisions.md} |

### Approval / sign-off

- **Review status:** {draft \| submitted for review \| approved \| rejected}
- **Approval source:** {the user message that approved this revision after it was shown to them; approval of an earlier artifact or phase does not count (ai/policies.md)}
- **Approved revision:** {revision number and date this authorization covers}
- **Closure:** {filled in when this revision is completed or superseded: date, outcome, and which revision supersedes it}

{Next revision: append "## Revision {N+1} — {phase}" below this line, after a `---` separator.}
