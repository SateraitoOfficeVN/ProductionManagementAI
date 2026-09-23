<!-- Based on ai/templates/brief.md. -->

# Japanese UI and automobile-parts domain — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-005 | Claude (for ThanhTN) | draft (revision 1) | unscheduled |

## Overview

The application's UI is English and its demo data describes generic industrial parts. This work item makes the UI
Japanese throughout and repositions the demo as production management for **automobile parts**, with Japanese demo
data. The design documents are updated to describe the Japanese screens. No screen gains or loses behavior.

## Objective

Requested by the user on 2026-09-23: "let change the UI language to japanense and also update the design documents",
then, on the demo data: "let make the project to be a production management for automobile parts and update the demo
data to this domain in japaneses". The target users read Japanese, and the design documents already follow Japanese SI
conventions; the screens should match.

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Japanese UI | Visible and assistive-technology text left in English on SCR-001–SCR-003 and the login page, other than data values, IDs and codes | 0 strings |
| No behavior change | Existing unit, integration, E2E and axe checks, updated only for the new text | all pass |
| Design documents match the screens | Quoted UI text in BD/DD equals the implemented text | every quoted label and message |

## Assumptions

- Japanese only; no language switcher and no English fallback (DEC-001).
- Message IDs (`MSG-E001`…) and the API contracts are unchanged: the server returns IDs, the client maps them to text.
  Only the client-side text changes.
- The system name `ProductionManagementAI`, order numbers (`PO-YYYY-NNNNN`) and product codes (`P-1001`…) are
  identifiers and stay as they are (DEC-005).
- The plant timezone (`Asia/Tokyo`) and every date rule are unchanged; only how dates and numbers are *displayed*
  changes.

## Actors and user stories

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| Admin, Operator | Japanese-speaking planner at an automobile-parts plant | use every screen in Japanese | I can work without translating | UC-013 |
| Presenter | person demoing the system | show realistic automobile-parts orders | the demo reads as a real plant | UC-014 |

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-043 | Every UI text is Japanese: headings, labels, placeholders, buttons, navbar, status labels, messages (MSG-E001–E021, MSG-I001–I008), empty/loading/error states, dialogs, page titles, the login page, and accessible names and descriptions | Success: a reviewer finds no English UI text on any screen or state, including screen-reader-only text. Failure: any English label, message, title or `aria-label` remains | must |
| REQ-044 | Dates, times and numbers are displayed the Japanese way | Success: dates `2026/09/22`, timestamps `2026/09/22 14:05`, counts with units (`24件`, `3,097個`, `13.7日`), all in the plant timezone as today. Failure: an English month name, `AM/PM`, or English unit word | must |
| REQ-045 | The page is marked and rendered as Japanese | Success: `<html lang="ja">`, a Japanese-capable font stack, no layout breaking or truncation on PC and SP (Pixel 7) at any state. Failure: `lang="en"`, tofu glyphs, or clipped Japanese labels | must |
| REQ-046 | The demo data describes automobile-parts production in Japanese | Success: the 30 seeded products are automobile parts with Japanese names; seeded order notes are Japanese; the project description names the automobile-parts domain. Failure: a seeded English product name or note remains | must |
| REQ-047 | The design documents describe the Japanese UI | Success: BD/DD quote the implemented Japanese text with an English gloss (DEC-003); wireframes and mockup sources show Japanese UI; DB design shows the new product seed; all English and Japanese PDFs regenerated. Failure: a quoted UI text that differs from the implementation | must |
| REQ-048 | No functional change | Success: every existing requirement (REQ-010–REQ-042) still holds, verified by the existing test suites with only their expected text updated. Failure: any change in validation, navigation, API or data rules | must |

## Not doing (out of scope)

- An English UI or a language switcher (DEC-001).
- Translating identifiers: system name, order numbers, product codes, message IDs, routes, API fields.
- Renaming the repository, solution or code identifiers.
- Republishing the three mockup Artifacts on claude.ai: their HTML sources in the repository are updated; publishing
  needs separate authorization.
- Re-recording or editing the demo videos in `demos/`, which show the English UI of the time.

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| The Japanese UI vocabulary (glossary) | Every label and message | ThanhTN | proposed in decisions.md DEC-004, for review with plan revision 1 |
| The 30 automobile-part names | Seed data, 001_DB, tests | ThanhTN | proposed in decisions.md DEC-002, for review with plan revision 1 |
