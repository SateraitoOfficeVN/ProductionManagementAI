# WI-018 — Fix intermittent E2E failures after WI-017 — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-018 | Claude Code (user request 2026-10-25) | done — merged via PR #54 as `4a97798` | unscheduled |

## Overview

The first `master` CI run after WI-017 merged (`ba7b930`, run
[40125879405](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/40125879405)) failed E2E with
78 passed / 2 failed, while the same code passed on PR #52 (run 40124507111). Follow-up to closed
[WI-017](../WI-017/brief.md) (RFC 0015: closed WIs are not reopened).

## Defects

| ID | Spec | Symptom |
| --- | --- | --- |
| BUG-010 | `tests/e2e/specs/product-type.spec.ts` "filter by type keeps it through paging and return after save" (WI-017 TC-489) | After changing the last PART product on page 2 to 半製品, the return list is past its end and moves back a page; 「製品を保存しました。現在の検索条件では表示されません。」 was not visible within 5 s |
| BUG-011 | `tests/e2e/specs/production-lines-redesign.spec.ts` TC-425/426 | After P-1001 is added to the draft line, searching P-1001 in the add dialog did not show 「選択できる製品はありません。」. Runs right after the WI-017 spec; possibly affected by the products it creates (unconfirmed) |

Severity: CI reliability only; no product defect is known yet. If diagnosis finds an application defect (for example
the hidden-after-save notice being lost when the list moves back a page), it is reported and handled by a plan revision.

## Requirements

| ID | Requirement | Acceptance |
| --- | --- | --- |
| REQ-094 | Root cause of each failure is identified from the CI traces and a local reproduction | Recorded in evidence.md |
| REQ-095 | Both specs pass reliably | Each spec passes 20 consecutive local runs (`--repeat-each=20`), and the full suite passes in CI on the PR and on `master` after merge |
| REQ-096 | Tests keep testing the same behaviour | No assertion is removed or weakened; retries stay 0 (ai/rules/testing.md) |

## Not doing

Changes to approved designs; unrelated specs; enabling Playwright retries.
