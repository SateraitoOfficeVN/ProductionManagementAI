# WI-007 — Decision Log

| ID | Decision | Source | Effect |
| --- | --- | --- | --- |
| DEC-001 | Treat sequential document review as a shared AI improvement, not only a WI-006 convention. | User message, 2026-09-29: “coi yêu cầu của tôi ở trên là 1 ai improvement” | RFC 0012 and WI-007 were created. |
| DEC-002 | Require a review stop only for design `.md` files: ADR, BD, DB, the main DD and each DD Markdown companion. | User message, 2026-09-29: “chỉ yêu cầu tôi review với các tài liệu thiết kế .md thôi” | RFC and plan revision 2 narrowed the boundary. Other document types do not gain this review stop. |
| DEC-003 | Approve WI-007 plan revision 2 for local shared-guidance edits and evaluation. | User message, 2026-09-29: “approved hết”, responding to revision 2 | Local adoption may proceed within the plan; no push, PR, merge or deployment was authorized. |
| DEC-004 | Commit WI-007 and create a pull request. | User message, 2026-09-29: “commit rồi tạo pr”; user message after interruption: “tiếp tục” | A dedicated feature branch may be committed and pushed to open a PR. Merge and deployment remain outside this authorization. |
| DEC-005 | Merge PR #29. | User message, 2026-09-29: “merge pr” | PR #29 was squash-merged into `master` as `e03246ad9d4359f54fab8049e0442ac27fd11bfc`; merge was verified through GitHub. |
| DEC-006 | Approve post-merge close-out plan revision 3. | User message, 2026-09-29: “approved”, responding to revision 3 | Update records and project entry points, then create a focused close-out PR. This does not authorize merging that later PR. |
