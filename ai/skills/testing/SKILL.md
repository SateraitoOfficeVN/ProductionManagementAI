---
name: testing
description: Plan and execute unit, integration, system, E2E or smoke verification.
---

# testing

## 1. Purpose and usage scenario

Plan and run the verification a change needs — unit, integration, system, E2E or smoke — and record real results against acceptance criteria. Use after implementation produces a checkable slice, and again whenever that slice changes.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** acceptance criteria, design (BD/DD), changed code.
**Optional:** runnable environment details (when not already documented).
**Source reference order:** the DD's "Test scenarios" section → brief.md acceptance criteria → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/testing.md) → the [template](../../templates/test-plan.md).

## 3. Execution steps and applicable rules

1. Map requirements and important risks to test cases and suitable test levels, keeping most coverage at unit, less at integration, least at E2E.
2. Use selected frameworks and isolated resettable data.
3. Run available checks; record failures, skipped checks and blockers distinctly; quarantine a flaky check with a recorded reason instead of retrying or deleting it silently.
4. Link results to acceptance criteria without duplicating tests for category labels.

## 4. Required tools/scripts and environmental conditions

The project's selected test framework(s) per [project context](../../project.md) (open until chosen), and an isolated, resettable test-data store or fixture set. A test not run because the environment is unavailable is recorded as not-run, never assumed passing.

## 5. Output artifacts, templates, ID conventions, and storage locations

Tests alongside the code they verify (`tests/`), reports under `docs/en/testing/`, and `work-items/<ID>/evidence.md`, starting from the [template](../../templates/test-plan.md). Test cases use stable `TC-###` IDs tracing to the `REQ-###` they verify. Each document under `docs/en/` is also rendered to an English PDF under `docs/en/pdf/` and a Japanese PDF under `docs/ja/pdf/`, in the same change, per the [documentation rules](../../rules/documentation.md).

## 6. Checklist and repeatable verification method

No dedicated checklist file; the repeatable method is the test plan itself — rerun the full suite from a clean environment periodically (not only once after writing it) to catch drift, and requeue any quarantined flaky check for a fix.

## 7. Termination criteria and failure handling

Done when coverage and evidence reflect actual execution, including limitations — a test not run is not a pass. If the environment can't run a required check, record it as blocked with the cause per [testing rules](../../rules/testing.md), and apply the pause conditions in [policies](../../policies.md) if that blocks acceptance.

## 8. Work item update procedure and handover for the next step

Update `evidence.md` (results per `TC-###`) and `status.md`, and `decisions.md` for any open question. Hand results to `pr-review`/`security-review` (as part of what they check) and back to `implementation` for any failure that needs a fix.
