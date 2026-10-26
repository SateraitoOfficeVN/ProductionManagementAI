# WI-018 — Fix intermittent E2E failures — Decision Log

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-10-25 | Open a follow-up for the 2 failing master E2E specs | User ("approved, create WI-018 to fix the flaky E2E specs") | decided | New WI per RFC 0015; plan revision 1 drafted for approval |
| DEC-002 | 2026-10-25 | Plan revision 1 approval | User ("approved, open the closeout PR now too") | decided | Diagnose, fix, verify, commit, push, PR, CI watch |
| DEC-003 | 2026-10-25 | Plan revision 2 approval | User ("approved, continue with revision 2") | decided | App fixes in AddProductDialog (and ProductMasterPage if proven), spec hardening |
| DEC-004 | 2026-10-26 | Skip full local suite; push, PR, merge | User ("push, create PRS then merge") | decided | Full local Playwright run stopped by the user; CI is the full-suite check; merge WI-018 PR and WI-017 closeout PR #53 after CI |
| DEC-005 | 2026-10-26 | Closeout PR and merge | User ("yes, create the WI-018 closeout PR and merge") | decided | Docs-only closeout, merge after checks |
