# WI-012 — Verification mapping

Approved scope: revision3, REQ-079–081 while preserving REQ-070–075.

| Check | Purpose | Result |
| --- | --- | --- |
| TC410 / editor unit | History mode has no writable mutation form | PASS |
| ChoiceSearch unit | Single-page pager hidden; bounded navigation remains reachable | PASS |
| TC412 | Six widths, all31 dates, composition, radios, read-only history, axe | PASS |
| TC413 | >50 scope choices, retained names through controlled pending paging, clear | PASS |
| TC414 | >50 product choices, long cached names and independent line context | PASS |
| Existing calendar/API journeys and layout/mobile cases | Exact capacity, rules, history, conflict/Unknown recovery,48px actions and8px gaps | PASS |
| Native Chrome200% | Reflow, radio arrows, invalid-field focus, modal containment/return | PASS |
| Matched baseline/current/mockup image review | Representative month/editor/history/dialog structure | PASS with inherited shell/native controls and fixture-state differences |
| Screen-reader speech | Assistive speech review | Not run: environment unavailable |
| Physical mobile keyboard/IME | Real device input review | Not run: physical device unavailable |

Final frontend suite258 tests; final affected calendar E2E20 passed,0 skipped/flaky.
TC413/414 intercept choice metadata with real authenticated fixtures; existing journeys
use real calendar APIs. These are distinct proof types. No all50 runtime screenshot claim.
See evidence.md and the verification summary in evidence.md, the verification summary in evidence.md, the verification summary in evidence.md.
