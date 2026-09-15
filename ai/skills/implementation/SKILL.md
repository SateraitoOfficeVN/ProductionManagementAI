---
name: implementation
description: Implement approved feature behavior from reconciled design artifacts.
---

# implementation

## 1. Purpose and usage scenario

Implement the approved, reconciled design as a bounded vertical slice of working code, migrations/configuration and tests. Use once BD/DD/DB are reconciled (or, for a small bug-fix, once the plan defines the fix directly) and the plan authorizes local implementation.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** approved plan, BD/DD, schema/API contracts, current repository state.
**Optional:** existing tests or partial implementation (when continuing prior work).
**Source reference order:** work-items/<ID>/plan.md → BD/DD/DB documents it implements → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/common.md) plus [frontend](../../rules/frontend.md)/[backend](../../rules/backend.md)/[database](../../rules/database.md) rules for the parts of the change they cover → the [template](../../templates/evidence.md).

## 3. Execution steps and applicable rules

1. Read frontend/backend/database rules applicable to the change.
2. Implement a bounded vertical slice — including the tracing/metrics the DD specifies — preserving unrelated changes, per [common rules](../../rules/common.md).
3. Add meaningful unit/integration checks for the implemented behavior; quarantine, don't silently retry or delete, any check that proves flaky.
4. Run available commands and record actual results; report missing runtime setup.
5. For a security-relevant change (auth, secrets, external input, dependencies), apply the [security-review skill](../security-review/SKILL.md) before treating the change as done.

## 4. Required tools/scripts and environmental conditions

The confirmed runtime toolchains once selected per [project context](../../project.md): .NET SDK for the backend, Node/Vite toolchain for the frontend, and the project's package manager. A local, resettable environment/test data store for running checks. Do not fabricate a tool result if the required toolchain isn't installed in this environment — report it as missing runtime setup instead.

## 5. Output artifacts, templates, ID conventions, and storage locations

Source under `src/`, relevant migration/configuration files, and tests under `tests/`, plus `work-items/<ID>/evidence.md` starting from the [template](../../templates/evidence.md). No new ID is assigned here; code and tests reference the `REQ-###`/`DD-###`/`TC-###` IDs they implement.

## 6. Checklist and repeatable verification method

Work through the [delivery checklist](../../checklists/delivery.md) before reporting completion; re-run it after any further change to the same slice, not only once at the end.

## 7. Termination criteria and failure handling

Done when the build and appropriate checks pass or have explicit, recorded blockers, the design remains consistent with what was implemented, and the delivery checklist passes. If a check fails, diagnose the root cause — never silently weaken a rule or test to make it pass, per [policies](../../policies.md).

## 8. Work item update procedure and handover for the next step

Update `status.md` (completed milestones, current step) and `evidence.md` (checks and results) and `decisions.md` for any open question. Hand the implemented slice to `testing` (for verification beyond the checks already run) and to `pr-review`/`security-review` (for the review gate).
