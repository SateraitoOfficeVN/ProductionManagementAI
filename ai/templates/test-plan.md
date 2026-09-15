<!-- Test Plan template, based on the IEEE 829-1998 Standard for Software Test Documentation. Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Work Item Title} — Test Plan

## Test plan identifier

{TP-###}, work item {WI-###}, revision {N}, {YYYY-MM-DD}.

## References

{brief.md, basic-design.md, detailed-design.md, database-design.md revisions this plan tests against.}

## Introduction

{What this test plan covers and why, in two to three sentences.}

## Test items

| Requirement ID | Description |
| --- | --- |
| {REQ-###} | {short restatement} |

## Features to be tested

- {behavior covered by this test plan, referencing REQ-### / TC-### IDs}

## Features not to be tested

- {behavior explicitly not covered here, and why}

## Approach

| Level (unit / integration / system / E2E / smoke) | Included? | Rationale |
| --- | --- | --- |
| {level} | {yes \| no} | {why this level is or isn't used for this work item} |

## Item pass/fail criteria

{The condition under which a test item is considered passed — e.g. matches the acceptance criteria in brief.md with no unhandled error.}

## Suspension criteria and resumption requirements

{What would pause test execution (e.g. environment unavailable, blocking defect) and what must be true to resume.}

## Test deliverables

- {this test-plan.md, test code/files, execution log, evidence.md entries}

## Cases

| Test ID | Requirement ID | Precondition / setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| {TC-###} | {REQ-###} | {starting state/data} | {action(s) taken} | {observable expected outcome} | {high \| medium \| low} |

## Environmental needs

{Target environment(s) — local, CI, staging — required tooling/services, and how test data is seeded and reset between runs so cases are isolated and repeatable.}

## Commands and prerequisites

```
{exact command(s) to run this test plan}
```

Prerequisites: {tooling, services, environment variables, or "none"}.

## Responsibilities and schedule

{Who authors/executes/reviews these tests, and when relative to the plan.md steps, or "not applicable — single-agent execution".}

## Risks and contingencies

| Risk | Contingency |
| --- | --- |
| {what could prevent testing as planned} | {fallback approach} |

## Results and linked evidence

| Test ID | Result (pass / fail / not run) | Evidence link | Date |
| --- | --- | --- | --- |
| {TC-###} | {actual result} | {link into evidence.md or CI output} | {YYYY-MM-DD} |

## Known gaps

| Gap | Reason | Risk | Follow-up |
| --- | --- | --- | --- |
| {requirement or case not covered} | {why it wasn't covered} | {what could go undetected} | {planned action, or "none planned"} |

## Approvals

{Who must sign off before this test plan/results are considered final, or "not applicable".}
