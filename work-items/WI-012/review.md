# WI-012 — Revision2 design review

2026-10-05. Technical static-design checks complete. User design review pending.

| Gate item | Assessment |
| --- | --- |
| Stable requirements | REQ-079–081 and preserved REQ-070–075; BUG-005, ALIGN-01–16 |
| Full-screen coverage | Original SPD regions1–16, 50 whole-screen/mode states, all real month dates |
| BD/DD consistency | User resolved desktop composition in favor of BD; missing choice/history/recovery states explicitly proposed |
| Diagram/region agreement | Existing navigation/state diagrams retained; additive region1–16 table; no new transition diagram or numbered SVG |
| API/DB and constraints | Existing11 calendar API contracts, date/decimal limits, 50-item choices, snapshots and optimistic concurrency retained; no migration |
| Security | No auth-policy changes, secret/PII fixtures or new endpoints/jobs; synthetic display examples; existing telemetry unchanged |
| Accessibility | Native controls, 48px targets, responsive source order, text states, contrast/focus/error rules and modal contract; static layout/disclosure verified |
| Test traceability | Design8 and audit register map later screenshot/interactions/regression checks to findings |
| Companions | EN/JA50-state galleries, eight numbered PNG figures, two10-page PDFs with version/date/footer; all PDF pages inspected |
| Preservation | 209 historical docs hashes match; application/test/shared files untouched |
| Current runtime evidence | Not available; docker empty and three expected ports refused. Actual screenshots/parity deferred explicitly to revision3 |
| Approval boundary | One design Markdown submitted. No next design or implementation revision started; wait for explicit review |

This passes technical design-consistency for the static deliverable with an explicit
runtime evidence limitation. It does not close BUG-005, approve the design for the
user or establish an implemented application. Security merge, delivery and release
stages have not been reached.

## Design-review continuation — 2026-10-05

User design review completed on 2026-10-05 for version2. Technical static gate remains documented above. Implementation/parity verification and revision3 approval remain pending.


## Revision3 code, security and local-delivery review — 2026-10-05

Design consistency PASS for tested representative runtime states and six responsive
widths; findings ALIGN-01–16 mapped in evidence.md. Approved artifacts unchanged.
Regression assertions retain concurrency, exact values, Unknown/no-replay and restrictions.

Security checklist:
- Endpoints/authentication/authorization: unchanged; existing authenticated API and role gates retained.
- External input: existing normalized search bounds/date/precision validation retained;
  no query/command/file-path concatenation introduced in application code.
- Credentials and sensitive logs: PASS; runtime secrets outside git, changed/untracked
  text scan clear. Evidence contains synthetic business IDs, never cookies/passwords.
- Dependencies: no additions/upgrades; pinned lock files unchanged.
- Credentials/roles/permissions: existing isolated runtime roles only, no new privilege model.
- External content: business labels/reasons rendered as React text, no HTML injection or executable content.
- Errors: existing Japanese error mapping retained, no stack/path/token disclosure introduced.
- New trust boundary/threat model: not applicable, no new boundary/auth/payment/PII handling.

Local delivery PASS with explicit manual limits: lint/build/typecheck,258 unit and20 E2E
pass, no skipped/flaky final E2E. Native zoom/keyboard verified. Screen-reader speech and
physical mobile keyboard/IME Not run. Runtime retained for user review by explicit request.
English design companions already complete and approved; no design edits this phase.
Root tracked-state closeout and external PR/merge are not this local handoff stage.


## Revision4 pre-PR review — 2026-10-05

PASS: scoped diff matches reviewed region mapping and REQ-079–081, preserves existing
API validation/auth/concurrency/recovery and exact units. Changed selectors reflect
approved radios/read-only history; action-size/gap/overflow assertions retained.
Choice helpers have bounded paging, cached business labels, text-only rendering and
observer cleanup. No new dependencies, trust boundaries, secrets or privilege changes.
Design-consistency/security/local-delivery gates remain PASS with manual limits above.
Final calendar20 E2E and typecheck PASS after screenshot removal. Old approved docs
unchanged; approved new PDF hashes match. Current WI design PNG exception explicitly
accepted; no test images/JSON persisted. Production-order pre-existing defect disclosed.
User completed local inspection. No outstanding scoped review blocker identified.
PR/CI handoff still in progress; merge/release-readiness not applicable or authorized.
