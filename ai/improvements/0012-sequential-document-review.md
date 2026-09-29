<!-- Based on ai/templates/improvement.md. The adopted rule is limited to design Markdown documents. -->

# RFC: Review each design Markdown document before writing the next

**Status:** adopted
**Affected:** `AGENTS.md`, `ai/policies.md`, `ai/workflows/feature-delivery.md`, `ai/rules/documentation.md`, `ai/evaluations/baseline-cases.md`.

## Summary

For a work item that produces design Markdown documents, create or revise them in the planned order and present each design `.md` file with its required companion artifacts. Wait for the user's review and an explicit instruction to proceed before writing the next design `.md` file. The review stop applies within an already approved plan. It does not apply to briefs, test plans, plans, routine work-item records, README files or shared AI guidance.

## Motivation

On 2026-09-29, during WI-006 Product master, the agent drafted the brief, BD, DB, DD set, test plan and PDFs during one approved design phase. The user then instructed: “từ lần sau khi tạo tài liệu, tạo theo thứ tự, khi xong để tôi review trước khi viết tài liệu tiếp theo” and clarified: “coi yêu cầu của tôi ở trên là 1 ai improvement”. After reviewing the first `AGENTS.md` edit, the user narrowed the scope: “chỉ yêu cầu tôi review với các tài liệu thiết kế .md thôi”. The existing [plan approval rule](0006-plan-revision-approved-before-work.md) requires review of each plan revision, but it does not define a review stop between design Markdown files. The expected behavior is that the user can correct one design document before its assumptions propagate into later designs.

## Guide-level explanation

Before producing a series of design Markdown files, the agent lists their order in the work-item plan. For each design `.md` file, it completes that file and its required companion artifacts, performs the available checks, and sends a direct link and a concise review summary. It then waits for the user to review it and say to continue before writing the next design `.md` file. Feedback is handled on the same design file and presented again. Read-only investigation and non-design documentation may continue during the wait if they do not preempt the next design decision.

For a `docs/en/` Markdown design, the review package includes its required English and Japanese PDFs, plus any wireframe or mockup that belongs to it. The main DD and each of its three companion Markdown files remain separate design-file stops; required companion files are still produced in the approved sequence. An approved plan authorizes the documented scope, but it does not bypass these user-requested design review stops. A material scope change still requires a new plan revision and its own approval.

## Reference-level explanation

- **Current behavior:** plan revision approval can be followed by multiple design Markdown drafts without a review between them. WI-006 revision 1 showed why this can delay discovery of cross-design effects.
- **Proposed behavior:** add a concise sequential-document rule to `AGENTS.md` and `ai/policies.md`; require an ordered document/review sequence in `ai/workflows/feature-delivery.md`; define a review package and exceptions in `ai/rules/documentation.md`; add manual evaluation cases to `ai/evaluations/baseline-cases.md`.
- **Boundary:** the gate covers design `.md` files: ADR, BD, DB, the main DD and each DD Markdown companion. PDFs, diagrams and mockups belonging to a design file are included in its review package, not separate stops. Briefs, test plans, work-item records, plans, README files and shared AI guidance do not trigger this document-review gate. A new plan revision still has its own mandatory approval. A direct user request to produce several design documents in one batch may explicitly override the one-at-a-time preference for that request.
- **Definition of done:** the shared rule is adopted; the new manual cases and all affected existing cases have recorded results; no plan, design-consistency, PDF or delivery gate is weakened. Actual user-review behavior in a future design work item is reported separately from text inspection.

## Drawbacks

Each design Markdown file adds a user round trip and can leave a work item waiting between designs. This is intentional for design decisions that feed later designs. Non-design documents continue under their existing approval rules.

## Rationale and alternatives

- **Keep this only in WI-006:** the user's clarification calls it an AI improvement, so future work items and agents should follow it.
- **Review only at each plan revision:** this is the current gap; one approved design phase can contain many documents.
- **Treat all documents as stops:** the user rejected this broader interpretation and specified design `.md` files only.
- **Treat all companion artifacts as independent stops:** that would separate a Markdown source from the PDF, wireframe or mockup needed to review it. The review package keeps them together while each DD Markdown file remains its own stop.

## Prior art / evaluation

On 2026-09-29, the cases below were manually traced through the changed `AGENTS.md`, `ai/policies.md`, `ai/workflows/feature-delivery.md`, `ai/rules/documentation.md`, the existing DD skill and [baseline cases](../evaluations/baseline-cases.md). The input for each row is its scenario. “Observed” means what the written guidance directs an agent to do; no new agent run or user-review exchange was executed for these cases.

| Case | Expected | Observed from guidance | Result |
| --- | --- | --- | --- |
| Approved plan lists brief, BD, DB and DD | Brief has no new stop; BD is reviewed before DB | Policy excludes briefs; workflow presents each design `.md` and waits before the next | pass by inspection |
| User corrects BD during review | Re-present BD; do not start DB | Entry point, policy and workflow require feedback resolution on the current design file first | pass by inspection |
| BD is ready for review | Include current EN/JA PDFs and wireframe in one package | Documentation rule requires PDFs and relevant diagrams/mockups with the design source | pass by inspection |
| Main DD, API, FN and SPD are required | Keep four files and four design-file stops | Documentation rule names each DD file; DD skill still requires all four | pass by inspection |
| Plan approved; first design file not reviewed yet | Start first design file, then wait before the next | Plan approval rule permits the approved work; design review rule adds the later stop | pass by inspection |
| User explicitly requests a design-document batch | Honor that bounded exception without weakening PDF/design gates | Entry point, policy and documentation rule state the exception; PDF and design-consistency rules remain | pass by inspection |
| Brief, test plan, status or shared AI guidance is written | No design-file review stop | Policy explicitly excludes these; plan approval remains separate | pass by inspection |
| Existing “approved feature plan, clear next step” case | Continue within scope unless the next step is another design `.md` after an unreviewed design | Policy continues approved work and names only the design-file exception | pass by inspection |
| Existing new-plan-revision case | Show and obtain approval of each new revision | Entry point and policy retain the existing approval rule unchanged | pass by inspection |
| Existing `docs/en/` PDF case | Render EN/JA PDFs in the same change | Documentation rule retains the PDF requirement and includes both in the review package | pass by inspection |
| Existing four-file DD case | Do not omit a DD companion | DD skill's all-four requirement remains; the new rule sequences their reviews | pass by inspection |

**Limitations:** these are static guidance checks, not evidence that a future agent obeyed the rule. A design document whose links point to later DD companions may need to flag those links as pending during its review; the full design-consistency gate still runs after all dependent design files exist. No application tests were relevant or run for this guidance-only change.

## Risk and rollback

- **Risk:** the review rule may conflict with a plan's existing permission to continue, or accidentally create stops for non-design files. Resolve this by stating its precedence and design `.md` boundary in the shared guidance. Work already approved under an earlier plan remains within scope, but future design Markdown creation follows the user's review sequence.
- **Rollback plan:** revert the squash merge commit `e03246a` through version control. Keep the user's WI-006 instruction and any historical review record accurate.

## Unresolved questions

- None for the boundary. The user explicitly narrowed it to design `.md` files and approved WI-007 plan revision 2 on 2026-09-29. A future agent run is still needed to observe operational adherence beyond static guidance inspection.

## Adoption

- **Reviewer:** user message on 2026-09-29: “aprroved” for the initial RFC, followed by “chỉ yêu cầu tôi review với các tài liệu thiết kế .md thôi” and “approved hết” for the narrowed plan revision 2.
- **Adopted revision:** PR #29, squash-merged into `master` as `e03246ad9d4359f54fab8049e0442ac27fd11bfc` on 2026-09-29. Static evaluation results are recorded above; operational adherence remains unobserved.
