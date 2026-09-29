# WI-007 — Sequential document review — Plan

This work item adopts [RFC 0012](../../ai/improvements/0012-sequential-document-review.md) as shared AI guidance. It is separate from WI-006 Product master; WI-006 revision 2 remains awaiting approval.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-29 | Adopt and evaluate RFC 0012 for all substantive documents | closed — superseded after scope correction | User message, 2026-09-29: “approved” |
| 2 | 2026-09-29 | Apply RFC 0012 only to design Markdown documents | **current — complete locally** | User message, 2026-09-29: “approved hết” |

## Revision 1 — Adopt and evaluate RFC 0012

Revision 1, 2026-09-29. The user approved the RFC proposal on 2026-09-29; this execution plan still requires its own explicit approval before any step below starts.

### Objective

Make the user's one-document-at-a-time review rule part of the shared AI harness, so future work items stop for review after each substantive document. Verify that the rule coexists with plan approvals, required PDF companions, and the four-file DD set.

### Scope

#### In scope

- Update `AGENTS.md`, `ai/policies.md`, `ai/workflows/feature-delivery.md`, and `ai/rules/documentation.md` with the RFC's review boundary and sequence.
- Add manual cases in `ai/evaluations/baseline-cases.md`; run and record all new and affected existing cases by inspection against the final guidance.
- Record the results and limitations in RFC 0012, then index it in `ai/improvements/README.md` when adopted.
- Produce or revise one file at a time. Present each changed file for the user's review and wait for an explicit instruction to continue before editing the next file. Resolve feedback on the current file first.

#### Out of scope

- Changes to application code, product designs, PDFs, CI or deployments.
- Retroactive reformatting of already completed work-item documents.
- GitHub push, PR creation, merge, publication or deployment.
- Changes to document-producing skills unless evaluation identifies a concrete conflict; that would require a revised plan.

### Inputs and assumptions

| Input | State | Assumption or limit |
| --- | --- | --- |
| [RFC 0012](../../ai/improvements/0012-sequential-document-review.md) | Approved proposal, not adopted | Its substantive-document boundary is the working definition; approval was for the proposal, not this plan. |
| [Current policies](../../ai/policies.md) and [entry point](../../AGENTS.md) | Active | Existing plan-revision approval remains mandatory. The new document review stop operates inside approved plans. |
| [PDF rule](../../ai/rules/documentation.md) and DD companion rule | Active | A document's companion PDFs/diagrams/mockups are completed before its review; DD Markdown companions are separate document stops. |
| WI-006 plan revision 2 | Awaiting approval | This improvement does not approve or start WI-006 design work. |

### Deliverables and milestones

The order is strict. Each numbered file is one review stop. Read-only checks may continue, but no later file is edited until the user reviews the current one and explicitly says to proceed.

| # | Step / document | Depends on | Skill | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Add the concise entry-point rule | Approval of revision 1 | harness-improvement | `AGENTS.md` | Rule names the review stop, applies inside approved plans, and does not weaken plan approval; present file for review | edited 2026-09-29; user narrowed the rule to design `.md` only; wording corrected in the same file; further work moved to revision 2 |
| 2 | State precedence and exceptions in policy | User review of 1 | harness-improvement | `ai/policies.md` | Compare against current plan authorization and pause conditions; present file for review | pending |
| 3 | Add ordered-document review to feature workflow | User review of 2 | harness-improvement | `ai/workflows/feature-delivery.md` | Workflow has an explicit stop after each substantive document; present file for review | pending |
| 4 | Define the review package and tracking-file boundary | User review of 3 | harness-improvement | `ai/rules/documentation.md` | PDF, diagram, mockup, DD companion and bookkeeping behavior match RFC 0012; present file for review | pending |
| 5 | Add new manual evaluation scenarios | User review of 4 | harness-improvement | `ai/evaluations/baseline-cases.md` | Cases cover sequential brief/BD/DB/DD, feedback, PDF package, plan approval and explicit batch override; present file for review | pending |
| 6 | Evaluate all affected cases and record results | User review of 5 | harness-improvement | Updated RFC 0012 | Record inputs, observed guidance behavior, expected behavior, pass/fail and limits for new and regression cases; present RFC for review | pending |
| 7 | Index the adopted improvement | User review of 6 and passing evaluation | harness-improvement | `ai/improvements/README.md` | Link resolves; RFC status reflects actual adoption; present index for review | pending |
| 8 | Record work-item status and delivery evidence | User review of 7 | planning, harness-improvement | `work-items/WI-007/status.md` and `evidence.md`, one at a time | Run `git diff --check`, link checks and `ai/checklists/delivery.md`; record real results and limitations; present each file separately | pending |

### Roles and permitted actions

| Role or action | Owner / permission |
| --- | --- |
| Local edits to the listed shared guidance and records after this plan is approved | Codex, one document per review stop |
| Document review and instruction to proceed | User |
| Plan approval | User; RFC approval alone does not approve plan revision 1 |
| Push, PR, merge, publication and deployment | Not authorized by this plan |

### Risks and stop conditions

| Risk / stop condition | Trigger | Response |
| --- | --- | --- |
| Review stop conflicts with an approved plan's instruction to continue | Guidance can be read both ways | Make the document review stop explicit in policy and test both cases; do not weaken plan approval. |
| Bookkeeping files cause unnecessary review loops | Status/decision/evidence updates are treated as substantive deliverables | Apply the RFC boundary; keep required tracking current without inserting extra gates. |
| Required PDFs or DD companions are delayed until after review | A design source is presented without its review package | Revise the current guidance before proceeding; evaluate the corresponding regression cases. |
| Evaluation reveals another skill or rule must change | Listed files cannot produce consistent behavior | Stop and show a revised plan before editing additional files. |
| User feedback on a current file remains unresolved | Review requests correction or does not instruct continuation | Revise and re-present that file; do not start the next one. |

### Approval / sign-off

- **Review status:** approved.
- **Approval source:** User message, 2026-09-29: “approved”, responding to revision 1.
- **Approved revision:** revision 1, 2026-09-29.
- **Closure:** Superseded on 2026-09-29 after the user clarified that only design `.md` files require a review stop. The first `AGENTS.md` edit was corrected as feedback on that file. Steps 2–8 were not started.

## Revision 2 — Design Markdown review only

Revision 2, 2026-09-29. **Awaiting explicit user approval; no revision 2 step has started.** This revision narrows the process rule in response to the user's correction: “chỉ yêu cầu tôi review với các tài liệu thiết kế .md thôi”.

### Objective

Adopt a shared rule that presents each design Markdown document for user review before writing the next design Markdown document. Keep plan-revision approvals and existing PDF/design gates intact.

### Scope

#### In scope

- Apply the sequential review stop only to design `.md` files: ADR, BD, DB, the main DD and each DD Markdown companion. A changed design file is reviewed with its required PDFs, diagrams and mockups as one package, without a separate stop for each companion asset.
- Update the RFC proposal and the shared entry point, policy, feature workflow and documentation rule to use this narrower boundary.
- Add and evaluate manual cases for design-document sequencing and affected existing plan/PDF/DD cases.
- Record real evaluation results, limitations, adoption state and rollback path.

#### Out of scope

- Review stops for briefs, test plans, status/decision/evidence records, plans, README files or shared AI guidance files. Mandatory plan-revision approval remains in force.
- Product design or application implementation, CI, deployment and retroactive changes to completed documents.
- GitHub push, PR creation, merge or publication.

### Inputs and assumptions

| Input | State | Limit |
| --- | --- | --- |
| User correction, 2026-09-29 | Explicit scope correction | Only design Markdown documents trigger the new review stop. |
| RFC 0012 | Previously approved for a broader proposal | Update its scope and evaluation before marking it adopted. |
| `AGENTS.md` | First edit made under revision 1, then corrected from user feedback | Reconcile wording with the final shared policy; do not treat its earlier review request as a requirement to review non-design guidance. |
| Existing plan approval and PDF rules | Active | Preserve them; the new stop applies between design `.md` files within an approved plan. |

### Deliverables and milestones

| # | Step | Depends on | Skill | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Narrow the RFC to design `.md` files and record the user's correction | Approval of revision 2 | harness-improvement | `ai/improvements/0012-sequential-document-review.md` | All sections and cases exclude non-design review stops; proposal and approval history remain accurate | done 2026-09-29 — correction and approval history recorded |
| 2 | Reconcile the entry point and shared policy | 1 | harness-improvement | `AGENTS.md`, `ai/policies.md` | Both state the same design-only boundary and preserve plan-revision approval | done 2026-09-29 — guidance limits the stop to design `.md` and retains plan approval |
| 3 | Update feature workflow and documentation rule | 2 | harness-improvement | `ai/workflows/feature-delivery.md`, `ai/rules/documentation.md` | Design `.md` order, feedback, PDFs, diagrams and DD companions are clear; non-design docs do not create stops | done 2026-09-29 — workflow and review package documented |
| 4 | Add manual cases and evaluate affected cases | 3 | harness-improvement | `ai/evaluations/baseline-cases.md`, RFC evaluation results | Record inputs, observed guidance behavior, expected behavior, pass/fail and limits; test new cases and affected plan/PDF/DD regressions | done 2026-09-29 — 11 cases passed by static inspection; actual agent behavior not run |
| 5 | Record adoption and index the RFC | 4 and passing evaluation | harness-improvement | RFC 0012, `ai/improvements/README.md` | Status reflects actual adoption; link resolves; rollback is stated | done 2026-09-29 — indexed as applied locally, uncommitted/unmerged; link check passed |
| 6 | Record work-item evidence and check delivery | 5 | planning, harness-improvement | `work-items/WI-007/status.md`, `evidence.md` | Run `git diff --check`, link checks and `ai/checklists/delivery.md`; record pass/fail/not run with reasons | done 2026-09-29 — status, decisions and evidence recorded; diff/link checks passed; delivery limits stated |

### Roles and permitted actions

| Role or action | Owner / permission |
| --- | --- |
| Local edits to listed AI guidance and records after revision 2 approval | Codex |
| Plan revision approval and shared-rule adoption review | User; the correction alone does not approve revision 2 |
| Design `.md` review stops in future work items | User |
| Push, PR, merge, publication and deployment | Not authorized |

### Risks and stop conditions

| Risk | Response |
| --- | --- |
| “Design `.md`” is interpreted inconsistently | Name ADR, BD, DB and every DD Markdown file in the rule and evaluation. |
| PDF/diagram production is deferred until after the design review | Require each design file's current companion artifacts in its review package. |
| New rule accidentally overrides plan approval or creates stops for non-design files | Check explicit exclusions and affected regression cases before adoption. |
| Evaluation finds another shared file must change | Draft and show an affected plan revision before editing it. |

### Approval / sign-off

- **Review status:** approved.
- **Approval source:** User message, 2026-09-29: “approved hết”, responding to revision 2.
- **Approved revision:** revision 2, 2026-09-29.
- **Closure:** Local scope completed on 2026-09-29. RFC 0012 is applied in the working tree and evaluated by static inspection; no commit, push, PR or merge occurred. A future design work item will provide behavioral evidence.
