<!-- Basic Design Document (基本設計書) template, based on conventional Japanese SI basic-design composition and matching the format, style and content of the reference workbook at ai/templates/example/BD (00-04-01.画面設計_会員登録_入力画面.xlsx). Unlike detailed-design.md's example set, this workbook is a single file with no subfolders, so this stays one template file. Every content sheet is represented below: 改版履歴 → Document control; 0.基本情報 → 0-1/0-2/0-3 (basic info, page metadata split into its 6 head sub-sections, URL parameters); 1.画面レイアウト → Layout and mockup (per breakpoint); 2.CMS表示項目定義 → Content block definition; 3.動的表示項目定義 → Screen item definition; 4.項目加工定義 → Item value mapping; 5.バリデーションチェック定義 → Validation rules; 6.項目イベント定義 → Item events; 7.外部ID連携情報 → External identity linkage (column-per-provider). 資料テンプレ is a blank copy of another sheet's format, not distinct content, so it isn't represented separately. Copy into the relevant work item or docs/en/010_basic-design area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval.
The per-screen detail block deliberately mirrors detailed-design.md's field/validation/event structure, per the reference workbook's own convention (one document carries both business and field-level detail for a screen). Keep both documents in sync as a screen moves from basic-design to detailed-design rather than letting one drift. -->

# {System / Feature Name} — Basic Design Document (基本設計書)

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | {BD-###} |
| Category | {e.g., UI, Batch, API} |
| System name | {system name} |
| Subsystem name | {subsystem name, or "not applicable"} |
| Work item | {WI-###} |
| Based on brief.md revision | {N} |
| Created by | {name} |
| Created date | {YYYY-MM-DD} |
| Last updated by | {name} |
| Last updated date | {YYYY-MM-DD} |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | {YYYY-MM-DD} | {name} | Initial creation |

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

{Link a wireframe/mockup or screen-transition diagram here if one exists. Field/validation/event-level detail for each screen belongs in "Screen design detail" below, not here.}

## Screen design detail

{Repeat the block below once per screen listed above, headed by its {SCR-###} and name. Omit any numbered subsection with "Not applicable — {reason}" rather than leaving it blank; do not delete the subsection.}

### {SCR-###} {Screen name}

#### 0-1. Basic information (基本情報)

| No | Item | Content | Reference |
| --- | --- | --- | --- |
| 1 | Route / path | {frontend route, e.g. /production-orders/new} | |
| 2 | API base path | {backend endpoint prefix this screen calls} | |
| 3 | Character encoding | UTF-8 | |
| 4 | Error page / fallback | {route or component shown on unrecoverable error} | |
| 5 | Responsive | {yes/no; breakpoints if yes} | |
| 6 | Authentication required | {yes/no} | |
| 7 | Authorization / role restriction | {roles allowed, e.g. Admin, Operator} | |
| 8 | Applicable channel(s) | {or "single web app — not applicable"} | |

#### 0-2. Page metadata (head)

{Document only what this screen adds on top of the app's common head/document defaults (shared `<title>` suffix, shared meta tags, global stylesheets/scripts already loaded for every screen) — don't restate the common baseline here.}

##### 0-2-1. Title

| No | Title |
| --- | --- |
| 1 | {page `<title>` text} |

##### 0-2-2. Base

| No | href | target | Reference |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"Not applicable — no `<base>` override" if the app uses the default document base.}

##### 0-2-3. Link

| No | rel | type | media | href | Reference |
| --- | --- | --- | --- | --- | --- |
| {1} | {stylesheet \| icon \| preload \| …} | {} | {} | {} | {} |

##### 0-2-4. Meta

| No | name / http-equiv | content | Reference |
| --- | --- | --- | --- |
| {1} | {description \| viewport \| …} | {} | {} |

##### 0-2-5. Style

| No | Selector / scope | Content | Reference |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"Not applicable — no inline `<style>` block; all styling via Tailwind classes/tokens" if that's how this screen is built.}

##### 0-2-6. Script

| No | type | src | Reference |
| --- | --- | --- | --- |
| {} | {} | {} | {} |

{"Not applicable — no extra `<script>` tags; app JS is bundled" if that's how this screen is built.}

#### 0-3. URL parameters

| No | Parameter name | Content | Required / Optional | Reference |
| --- | --- | --- | --- | --- |
| {} | {} | {} | {required \| optional} | {} |

{"None — screen takes no URL parameters" if not applicable.}

#### 1. Layout and mockup

{Layout/navigation-level sketch or link to a rendered mockup, per ai/skills/screen-design/SKILL.md; pixel-perfect fidelity and per-state mockups belong in detailed-design.md's "Screen layout and mockup" section. Provide one layout below per breakpoint this screen supports (at minimum PC/desktop; add SP/mobile, tablet, etc. as applicable) — mirroring the reference workbook's separate PC and SP layouts. If a breakpoint spans multiple sections of the screen, split it into numbered parts (e.g. "SP (1/2)", "SP (2/2)") as the source does.}

##### PC / desktop

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

| Item No. | Region / element | Notes (behavior, condition) |
| --- | --- | --- |
| {49} | {} | {} |

##### SP / mobile

{Same structure as PC/desktop above: ASCII sketch (or rendered-mockup link) plus its own item-number legend. Item numbers should stay consistent with the PC layout where the same element appears in both. "Not applicable — this screen has no SP/mobile breakpoint" if responsive is "no" in 0-1.}

#### 2. Content block definition (CMS)

{Externally managed/static content blocks placed on this screen, if any.}

| No | Content ID | Sub ID | Site / channel | Display condition | Reference |
| --- | --- | --- | --- | --- | --- |
| {A} | {} | {} | {} | {} | {} |

{"None — no externally managed content blocks" if not applicable.}

#### 3. Screen item definition

| No | Item (label) | Variable name | Control type | I/O | Data type | Width / length | Initial value | Placeholder | Display condition | Data source | Reference |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| {1} | {} | {} | {textbox \| select \| radio \| checkbox \| label \| button} | {I \| O \| I/O} | {} | {} | {} | {} | {} | {} | {} |

#### 4. Item value mapping

{Conversion between an item's stored/source value and its displayed value, where they differ.}

| No | Item | Source value | Displayed value | Reference |
| --- | --- | --- | --- | --- |
| {} | {} | {} | {} | {} |

{"None — displayed values match source values 1:1" if not applicable.}

#### 5. Validation rules

| No | Item | Check content | Validation rule | Check condition | Error message (ID) | Reference |
| --- | --- | --- | --- | --- | --- | --- |
| {} | {} | {} | {} | {} | {} | {} |

#### 6. Item events

| No | Item | Event | Event content | Reference |
| --- | --- | --- | --- | --- |
| {} | {} | {click \| change \| blur \| …} | {what happens} | {} |

#### 7. External identity linkage

{Mapping between this screen's input fields and each external identity/SSO/payment provider's fields, if the screen supports sign-up, login or checkout via an external provider. Add one "Has mapping / Provider field / Example / Notes" column group per provider (copy the {Provider N} group below for each), matching the reference workbook's side-by-side Yahoo!-JAPAN-ID / Amazon-Pay layout — do not collapse providers into extra rows.}

| No | Input item | Input part | Required | {Provider 1} has mapping | {Provider 1} field | {Provider 1} example | {Provider 1} notes | {Provider 2} has mapping | {Provider 2} field | {Provider 2} example | {Provider 2} notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| {1} | {} | {} | {yes/no} | {yes \| no \| partial} | {} | {} | {} | {yes \| no \| partial} | {} | {} | {} |

{"None — no external identity linkage" if not applicable.}

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
