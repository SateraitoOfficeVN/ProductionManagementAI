# WI-014 — Status

2026-10-06: BUG-008 reproduced on `master` `46bf430` (order list 1763 px wide at 390 px, 1961 px at 1280 px;
product filter 1713 px). Brief and plan revision 1 written.

2026-10-06: Plan revision 1 approved ("approved, go ahead"). Fix implemented on `fix/WI-014-order-list-overflow`;
regression E2E fails before and passes after; lint/build/267 unit tests pass; full local E2E 56 passed, 11
known Plant-calendar environment failures. See [evidence](evidence.md). Next: commit, push, PR and CI.
Merge, branch deletion and deployment are not authorized.

2026-10-06: PR #43 opened; CI run 37427110603 passed (239 unit, 203 integration, 267 frontend, 67 E2E). User review
found the open product list still extends past the container; plan revision 2 (contain the list, truncate, full
name on hover) written and awaiting review. `test-plan.md` added (uncommitted).

2026-10-06: Plan revision 2 approved ("approved, go ahead"). Implemented: base-select containment, ellipsis, full-name
`title` (DEC-003, DEC-004). TC-121 fails before and passes after; 269 unit tests pass; full local E2E 57 passed, 11
known Plant-calendar environment failures. Next: one commit and push to PR #43, then CI. Merge, branch deletion and
deployment are not authorized.
