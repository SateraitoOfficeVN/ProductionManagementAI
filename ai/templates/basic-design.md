<!-- Basic Design Document (基本設計書) template, based on conventional Japanese SI basic-design composition (system overview, architecture, function/screen list, business flow, data design overview, external interfaces, non-functional requirements). Copy into the relevant work item or docs/en/010_basic-design area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {System / Feature Name} — Basic Design Document (基本設計書)

{BD-###} — {WI-###}, based on brief.md revision {N}.

## System overview

{What this feature/system does, in business terms, and which requirements it covers.}

| Requirement ID | Description | Covered by section |
| --- | --- | --- |
| {REQ-###} | {short restatement, not a copy of the full requirement text} | {section below} |

## Overall configuration and architecture

{How this fits the confirmed stack (frontend/backend/database/hosting) and any new component boundaries. Link an architecture-decision.md ADR for any decision made here.}

## Function list

| Function ID | Function name | Description | Related requirement ID |
| --- | --- | --- | --- |
| {FN-###} | {name} | {what it does} | {REQ-###} |

## Actors and business flow

{Which actors from brief.md participate, and the end-to-end business flow in the order it happens:}

1. {actor} {does what} → {system response}
2. …

## Screen list and screen transition

| Screen ID | Screen name | Entry point | Exit / next screen |
| --- | --- | --- | --- |
| {SCR-###} | {name} | {how the user arrives here} | {where each primary action leads} |

{Link a wireframe/mockup or screen-transition diagram here if one exists; pixel-level layout belongs in detailed-design.md.}

## Actions and business rules

| Action | Trigger | Business rule | Related requirement ID |
| --- | --- | --- | --- |
| {user or system action} | {what initiates it} | {the rule governing whether/how it happens} | {REQ-###} |

## Success and exception flows

| Flow | Trigger condition | System behavior | Resulting state |
| --- | --- | --- | --- |
| {success \| exception name} | {condition that leads here} | {what the system does} | {screen/state the user ends up in} |

## Data design overview

{Entities involved and their key relationships, at a level a non-DB-specialist can follow. Full column-level detail belongs in database-design.md.}

## External interfaces

{Other systems/APIs this feature calls or is called by, or "none".}

## Non-functional requirements

{Performance, security, availability expectations relevant to this feature, or "not applicable — inherits project defaults".}

## Open questions and linked DD

| Question | Linked DD section | Status |
| --- | --- | --- |
| {business-behavior question left open at this level} | {detailed-design.md section that must resolve it} | {open \| answered in decisions.md} |
