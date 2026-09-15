<!-- Implementation Plan template, based on common project/implementation-plan conventions (PMI-style plans, Smartsheet/TeamGantt implementation plan templates). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Work Item Title} — Implementation Plan

Revision {N}, {YYYY-MM-DD}. {If this supersedes a prior revision, state which and why.}

## Objective

{One or two sentences stating the outcome this plan delivers, tracing back to brief.md.}

## Scope

### In scope

- {bounded piece of work this plan covers}

### Out of scope

- {adjacent work explicitly excluded from this plan}

## Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| {artifact} | {revision, or "not yet written"} | {assumption, flagged for confirmation} |

## Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method |
| --- | --- | --- | --- | --- | --- |
| 1 | {concrete action} | {step # or "none"} | {ai/skills/... name} | {file/path or "none"} | {command, review, or check that proves it is done} |

## Roles and responsibilities

| Role | Owner |
| --- | --- |
| {plan author \| implementer \| reviewer \| approver} | {who, or "this agent"} |

## Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| {action} | {yes \| no \| ask at execution} | {request text, plan approval, or "not yet authorized"} | {branch, environment, or artifact the authorization is bounded to} |

## Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| {requirement conflict, missing decision, scope change, missing access, action exceeding authorization} | {what would signal this} | {pause and ask per ai/policies.md; record the question in decisions.md} |

## Approval / sign-off

- **Review status:** {draft \| submitted for review \| approved \| rejected}
- **Approval source:** {who/what approved it, e.g. user message, referenced request}
- **Approved revision:** {revision number and date this authorization covers}
