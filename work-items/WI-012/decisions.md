# WI-012 — Decisions

- Direct request: mockups follow design; implementation follows reviewed mockups.
- Standing instructions: new WI and approved plan for bugs; old completed/approved
  design documents immutable; create new additive documents when needed.
- Japanese UI, English persisted records/source designs, separate Japanese PDFs.
  Vietnamese conversation only.
- Initial scope assumption: SCR-006 reference capacity, the panel just reported;
  no authorization for a complete calendar redesign inferred.
- Bounded API paging is part of approved functionality. The visual selection flow
  should make all eligible items reachable and clearly associate secondary actions.
- Prioritize end-user simplicity per direct standing user preference. RFC0013
  source currently labels itself under-review; do not silently amend shared guidance.
- Revision1 approved on 2026-10-02; later design review and implementation-plan approval
  are distinct. No implementation or external GitHub action authorized yet.

## Revision1 approval — 2026-10-02

Explicit user "approved" after plan1 presentation. Additive design/mockup/render
and companion review authorized. Application changes and implementation revision
remain outside this phase. Existing local HTML/Chromium fallback used.

## Superseded proposal version1 — not approved

- Normal layout exposes line/product/date and explicit lookup. Three columns at
  640px and above; stacked below. Search is a native disclosure under its selector.
- Paging appears only inside the matching expanded choice group when total pages
  exceed one. Existing 50-item API bounds remain; no fetch-all or API/schema change.
- Searches/pages are independent; cached selected labels survive page/filter changes.
  Changing line clears product selection and related search/page/result context.
- Retained valid URL identifiers with unconfirmed labels may use explicit capacity
  verification; do not force reselection or invent eligible labels.
- Proposed new Japanese catalog labels are marked as additions, not existing keys.
- These are review proposals. No approved historical document was revised and no
  implementation or design approval is inferred from revision1 approval.

## User-requested restart — 2026-10-02

- Full SCR-006 scope replaces the initial capacity-only assumption.
- User explicitly authorized removing newly added proposal files and replanning.
  Removed seven proposal artifacts and five generated check/review files; retain
  brief/plan/status/decisions/evidence for required chronological WI traceability.
- Discarded proposal controls/layout are not the restart baseline. Approved WI-010
  designs and mockups define expected intent; audit all regions before new design.
- Revision1 authorization is superseded. Revision2 awaiting explicit approval;
  implementation requires later revision3 approval after the full design review.

## Revision2 approval — 2026-10-05

Explicit user "approved" after revision2 presentation authorizes full-screen audit
and additive design/mockup review. Application implementation remains a later phase.

## Full-screen layout decision — 2026-10-05

The user explicitly chose the 006_BD composition over the old SPD gallery's stacked
sample: desktop calendar left, selected-day/actions right; mobile stacked. This
resolves the material visual conflict. Proposed intermediate-width reflow and all
other gap coverage await review of 006_DD-SPD-ALIGNMENT version1.

Current runtime is unavailable. Per approved revision2, record source comparisons
and the missing current-runtime proof; do not create fixtures. Require real-browser
parity in revision3. Keep current Japanese catalog wording where business meaning
is unchanged and list missing labels as proposed additions. Shared shell stays the
existing component; its non-calendar icons/auth name arrangement are inherited.

## 2026-10-05 — Numbered design regions

User requested region numbers on DD-SPD images. Apply that feedback to the
unapproved WI-012 additive design as version2, with documentation-only badges
matching the existing1–16 table. No plan-scope change or implementation authority
is inferred. Completed approved design documents remain protected.

## Design-review continuation — 2026-10-05

User reviewed version2 and requested continuation on 2026-10-05. This approves the design package and authorizes drafting revision3; implementation-plan approval remains separate.

## Revision3 approval and local review — 2026-10-05

User explicitly approved revision3 and requested the corrected application run
locally for inspection. Keep the isolated local stack running at handoff; postpone
its teardown until user review is finished. No external GitHub action authorized.


## Revision3 local review handoff — 2026-10-05

Retain corrected pmai-wi012-review runtime at13012 per explicit user instruction;
remove only task-owned baseline observation container. Existing demo stack preserved.
User review remains pending. Manual speech/physical IME checks remain Not run, separately
from automated/native desktop keyboard proof. No commit/push/PR/merge authorized.


## User local review and continuation — 2026-10-05

User completed local inspection and requested the next WI-012 step. Revision3 local
review stop is satisfied; draft revision4 for delivery, without inferring external
operation authorization. Production-order layout defect reproduced on unchanged
baseline remains a separate bug scope; do not bundle its fix into WI-012.
User accepted current WI-012 design PNG figures; future work should use wireframe SVG.
No test screenshot capture or unnecessary generated JSON artifacts; consolidated
Markdown verification records suffice. Existing approved designs remain immutable.


## Revision4 approval — 2026-10-05

Explicit user "approved" after revision4 presentation authorizes scoped delivery: final
checks, feature-branch commit/push, PR creation and CI follow-up. Merge/live deployment
and unrelated production-order fix remain excluded. No test screenshot capture.
