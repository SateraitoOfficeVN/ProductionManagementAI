# RFC 0013: Prioritize simple screens for end users

**Status:** under-review; not adopted
**Date:** 2026-10-02
**Affected on adoption:** `ai/rules/frontend.md`, `ai/skills/screen-design/SKILL.md`,
`ai/checklists/design-consistency.md`, `ai/evaluations/baseline-cases.md`.

## Summary

Prioritize simple, understandable screens that help end users complete their work.
Evaluate interface choices against the user's task, language and decisions. Developer
convenience and internal data structures must not determine what the user needs to
see. Technical controls or information belong in a product flow only when they help
its intended user make a meaningful decision or resolve an actionable problem.

## Motivation

The user requested a new AI improvement on 2026-10-02: prioritize simple,
user-friendly screens and end-user UX rather than developer experience (DX).
This is a standing design preference, not a reported defect in a particular screen.
The current frontend rules cover agreed behavior, API consistency and accessibility;
the screen-design skill covers fields/states/mockups. Neither explicitly requires
justifying visible complexity by the end user's task. The expected outcome is fewer
unnecessary controls and clearer everyday workflows without removing required
business behavior or integrity protections.

## Guide-level explanation

Before drawing a screen, identify the intended user, their primary task, the next
useful action and the information needed to make that action safely. Present the
common task and essential choices clearly. Reveal uncommon settings, audit details
or advanced actions when they are relevant, using understandable labels.

Use the user's business language. Translate conflicts, validation and uncertain
save outcomes into a clear explanation and next action while preserving the actual
outcome. Keep backend validation, authorization, concurrency and traceability in
the implementation. Decide separately how much of their technical machinery the
user needs to see. Internal IDs, revision tokens, request names, raw errors and
transport/persistence states need a concrete user need before becoming visible UI.

Review the screen through an end-to-end user journey on desktop and mobile. Check
whether each default-visible field, panel and action helps the user complete or
understand their task. Prefer fewer steps and clear feedback; use a confirmation
when required by the approved behavior or when a meaningful risk justifies it.
Required workflows, keyboard access, visible focus and honest errors remain intact.

## Reference-level explanation

### Proposed frontend rule text

Add this section to `ai/rules/frontend.md` after the stack rule:

> Prioritize simple, user-friendly screens for the intended end user. Start from
> their task, decisions and business language. Keep the primary action and essential
> information clear; reveal secondary controls and advanced details when relevant.
> Every visible field, panel and action needs a concrete user benefit. Show technical
> identifiers, revision tokens, API/database details or diagnostics only when they
> help that user make a meaningful decision or resolve an actionable problem.
> Preserve approved business behavior, security, data integrity, accessibility and
> honest feedback when simplifying the interface.

### Proposed screen-design step

Add a step before layout and field specification in
`ai/skills/screen-design/SKILL.md`:

> Identify the intended user and primary task. Describe the main user journey and
> justify default-visible information/actions in terms of that task. Use business
> language, progressive disclosure and clear next actions. Review desktop/mobile
> mockups from the user's perspective, including validation, conflicts and uncertain
> saves. Simplify presentation while retaining approved functional and accessibility
> requirements; API contracts and internal model fields do not automatically become
> visible controls.

### Proposed design-consistency check

Add this item to `ai/checklists/design-consistency.md`:

> New or changed screens identify their intended user and primary task; the primary
> action is clear, default-visible complexity has a user benefit, and technical
> details are exposed only for a meaningful user decision or recovery action.
> Simplification preserves required workflows, honest states and accessibility.

Add the new cases below to `ai/evaluations/baseline-cases.md` on adoption. Keep all
existing gates and workflow permissions. This RFC adds no UI framework, numerical
control-count threshold or additional document-review stop.

### Scope and definition of done

The guidance applies to future screen design, implementation and review. It does
not authorize rewriting completed approved designs or refactoring an existing
screen. A concrete existing-screen change needs its own approved scope/plan and
new design artifacts when required. WI-010 remains in its existing checkout.

Done when the proposed guidance is explicitly reviewed for adoption, applied and
the affected evaluations are rerun with recorded results. Source review
alone cannot prove that an application is easy to use; actual future screen/user
observations must be recorded separately.

## Drawbacks

Deciding what is essential takes product judgment. Excessive hiding can make an
important task harder to discover. Check the intended users and primary/secondary
journeys rather than treating fewer controls as an absolute goal.

## Rationale and alternatives

A shared frontend rule plus one design step and review check makes the preference
available to future agents and work items. A fixed maximum number of fields would
ignore task differences. Automatically simplifying existing screens would bypass
their approved scope and design history. Requiring a new framework or formal user
research for every small change would exceed this request.

## Evaluation

Method: manual static tracing of the proposed text and current baseline guidance,
on baseline 42e8932. These are proposal checks, not adopted-agent execution, runtime
tests or usability research. Existing source files remain unchanged at this stage.

| Case | Expected / observed direction from proposed text | Result |
| --- | --- | --- |
| UX-01: form mirrors all API/internal fields | Choose visible inputs by the user's task; transport fields do not automatically become controls | Pass by inspection |
| UX-02: calendar exposes UUIDs/revision tokens by default | Require a concrete end-user benefit; ordinary calendar lookup emphasizes date, working state and useful actions | Pass by inspection |
| UX-03: rare settings crowd the primary task | Make common task clear and reveal secondary settings when relevant | Pass by inspection |
| UX-04: uncertain save result is technically complex | Explain uncertainty and a safe next action in business language; preserve outcome integrity and avoid false success | Pass by inspection |
| UX-05: audit/history is genuinely needed | Keep relevant audit tasks discoverable; expose understandable details for the user's actual decision | Pass by inspection |
| UX-06: simplifying would remove required fields/authorization | Preserve approved functionality and authoritative integrity/security checks | Pass by inspection |
| UX-07: mobile or keyboard user reaches secondary actions | Review desktop/mobile and preserve keyboard/focus/accessibility requirements | Pass by inspection |
| UX-08: request implies rewriting an approved old design | Existing-screen changes need approved scope and new artifacts when required; old approved designs preserved | Pass by inspection |
| Existing baseline: UI framework/kit choice | Current confirmed stack remains; no new package or framework authorized | Pass by inspection |
| Existing baseline: DD/DB conflict, sequential design and plan review | Reconcile contracts and retain every existing approval/document gate | Pass by inspection |

## Risk and rollback

- **Risk:** over-simplification hides necessary actions or ambiguous wording creates
  developer/user tradeoffs without evidence. The rule requires a user benefit,
  discoverability and preservation of approved behavior/accessibility.
- **Rollback:** revert only the eventual RFC 0013 adoption commit through version
  control. Preserve the user request and approval history. No adoption commit exists
  yet; current baseline is 42e8932. No gate is weakened to pass these cases.

## Unresolved questions

No additional preference is needed to prepare this proposal. Review before shared
guidance adoption remains pending. Specific existing-screen UX changes are outside
this improvement's scope.

## Adoption

- **Reviewer:** awaiting review of this standalone improvement; no new work item is required.
- **Adopted revision:** not yet adopted; shared rule/skill/checklist/evaluation edits
  have not been performed. The user's direct preference already guides this session.

## PR preparation — 2026-10-02

The user explicitly requested a PR for this standalone improvement. Scope is this
RFC and its index entry; no new work item or shared-guidance adoption is included.
Local review confirms English-only additions, valid local Markdown links, eight UX
and two affected baseline scenario groups evaluated by inspection, and preserved
security/accessibility/approval boundaries. No application tests are relevant or run.
Documentation-only CI is expected to skip under RFC 0005; no CI pass is claimed.
Commit/push/PR are authorized by the user request; merge and adoption are separate.
