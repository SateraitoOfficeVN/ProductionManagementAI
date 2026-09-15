<!-- Requirements Traceability Matrix + Test Execution Log template, based on common RTM and test-evidence conventions (Katalon, Atlassian, IEEE 829 test logs). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Work Item Title} — Requirements Traceability & Evidence

As of source revision/commit {reference}, {YYYY-MM-DD}.

## Traceability matrix

| Requirement ID | Requirement | Design artifact | Code / PR | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| {REQ-###} | {short restatement} | {basic-design.md / detailed-design.md section} | {file path or PR link} | {TC-###} | {implemented \| verified \| not started} |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| {YYYY-MM-DD} | {unit / integration / lint / build / …} | {exact command run} | {local \| CI \| other, with version if relevant} | {actual result — never assumed} | {link to output} |

## Defects, failures and blockers

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| {failing or skipped check} | {why it failed or wasn't run} | {what is blocking it, if any} | {planned next step} |

## External references

- PR: {link, or "not opened — not authorized"}
- CI run: {link, or "not applicable"}
- Deployment: {link/image tag, or "not applicable"}

## Remaining limitations and next action

{What is still unverified or out of scope for this evidence, and the single next action to close the gap.}
