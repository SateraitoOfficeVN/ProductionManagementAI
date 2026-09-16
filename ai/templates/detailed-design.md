<!-- Detailed Design Document (詳細設計書) template, based on conventional Japanese SI detailed-design composition (module design, screen item definition, interface definition, DB schema detail, exception handling, test viewpoints). Copy into the relevant work item or docs/en/020_detailed-design area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Screen / Module Name} — Detailed Design Document (詳細設計書)

{DD-###} — implements {BD-###}, requirements {REQ-###, …}.

## Module design

| Module | Input | Preconditions | Processing summary | Return value |
| --- | --- | --- | --- | --- |
| {module/function name} | {inputs} | {what must be true before it runs} | {what it does} | {return value or side effect} |

## Screen layout and mockup

{Visual layout for this screen, at implementation fidelity: an ASCII sketch below, and/or a link to an external artifact (image file under the work item's `assets/` folder, or a design-tool URL such as Figma). If this refines a BD-level wireframe, note what changed and why. "None" is not acceptable once fields are defined — a reader must be able to see where each field/control sits.}

```
{ASCII layout sketch, e.g.:
+----------------------------------------------------+
| {Header: title / breadcrumbs}                       |
+----------------------------------------------------+
| {Region A: fields/controls}    | {Region B: ...}    |
|                                 |                    |
+----------------------------------------------------+
| {Primary actions: e.g. Save / Cancel}                |
+----------------------------------------------------+
}
```

{Mockup artifact: link, or "none — ASCII layout above is authoritative".}

| Region | Contains (field/control) | Notes |
| --- | --- | --- |
| {region name from sketch above} | {field/control, cross-referenced to Screen item definition} | {responsive behavior, conditional visibility, or "none"} |

## Screen item definition

| Field | Type | Required | Validation rule | Source (BD ref) |
| --- | --- | --- | --- | --- |
| {field name} | {data type} | {yes \| no} | {format, range, cross-field rule} | {basic-design.md section} |

## Loading / empty / error / success states

| State | Trigger | UI behavior | Data shown |
| --- | --- | --- | --- |
| {loading \| empty \| error \| success} | {what puts the screen in this state} | {what the user sees/can do} | {placeholder, real data, or error message} |

## Processing and state transitions

{Numbered processing steps for each action from basic-design.md's "Actions and business rules," in execution order:}

1. {step}

| From state | Event | To state | Side effect |
| --- | --- | --- | --- |
| {state} | {event} | {state} | {e.g. record created, notification sent} |

## Interface definition (API)

| Endpoint | Method | Request fields | Response fields | Error codes |
| --- | --- | --- | --- | --- |
| {/path} | {GET/POST/PUT/DELETE} | {fields} | {fields} | {code → meaning} |

## Database and transaction mapping

| Operation | Table(s) | Transaction boundary | Concurrency handling |
| --- | --- | --- | --- |
| {create/update/delete/read} | {table(s) from database-design.md} | {what is atomic} | {locking/optimistic-concurrency approach, or "not applicable"} |

## Exception handling

| Failure | Retry policy | Timeout | User-facing error | Logging |
| --- | --- | --- | --- | --- |
| {what can fail} | {retry rule or "none"} | {timeout value or "not applicable"} | {message shown} | {what gets logged} |

## Test viewpoints and unresolved decisions

| Scenario | Precondition | Expected result | Test-plan ID |
| --- | --- | --- | --- |
| {scenario name} | {starting state} | {expected outcome} | {TC-### once test-plan.md is written} |

Unresolved decisions: {list, or "none" — link each to decisions.md}.
