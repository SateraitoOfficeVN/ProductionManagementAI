<!-- Engineering RFC template, based on the widely-adopted Rust RFC format (github.com/rust-lang/rfcs) used by many open-source projects for proposing and documenting process/design changes. Copy into the relevant work item or ai/improvements area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# RFC: {short title of the proposed change}

**Status:** {draft \| under-review \| approved \| adopted \| rejected}
**Affected:** {path(s) under ai/skills/, ai/rules/, ai/workflows/, or a tool/command}

## Summary

{One paragraph explaining the proposed change.}

## Motivation

{The observed problem this addresses — what happened, where, and a link to the evidence (transcript, evaluation run, incident). Why is this worth doing? What is the expected outcome?}

## Guide-level explanation

{Explain the change as if it is already adopted: what does a future agent/session do differently day to day because of it?}

## Reference-level explanation

- **Current behavior:** {what the skill/rule/tool does today}
- **Proposed behavior:** {the smallest useful change, in enough detail to implement}
- **Definition of done:** {what must be true for this change to be considered complete}

## Drawbacks

{Why might we not want to do this?}

## Rationale and alternatives

{Why is this approach better than the alternatives? What other designs were considered, and why were they rejected?}

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| {case ID or description} | {behavior before the change} | {behavior after the change} | {pass \| fail} |

## Risk and rollback

- **Risk:** {what could regress or break if this change is adopted}
- **Rollback plan:** {revert via version control to revision {reference}; no silent relaxation of gates}

## Unresolved questions

- {question this RFC does not resolve, to be settled during or after review}

## Adoption

- **Reviewer:** {who reviewed this proposal}
- **Adopted revision:** {commit/revision where this took effect, or "not yet adopted"}
