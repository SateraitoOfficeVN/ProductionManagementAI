---
name: ci-cd
description: Design or implement GitHub Actions and Docker delivery for an approved target.
---

# ci-cd

## 1. Purpose and usage scenario

Design or implement the GitHub Actions/Docker pipeline for an approved deployment target — CI verification, image publishing and deployment kept distinct. Use once build/test commands are verified and a deployment target is authorized; never to bootstrap deployment speculatively.

## 2. Mandatory inputs, optional inputs, and source reference order

**Mandatory:** verified build/test commands, selected runtime versions, target and permissions.
**Optional:** existing `.github/workflows` (when extending a pipeline rather than creating one).
**Source reference order:** existing `.github/workflows` and deploy configuration (if any) → work-items/<ID>/evidence.md (verified commands) → [project context](../../project.md) → [policies](../../policies.md) → [applicable rules](../../rules/ci-cd.md) → the [template](../../templates/evidence.md).

## 3. Execution steps and applicable rules

1. Separate CI verification from image publishing and deployment; build container images from a minimal, non-root base and scan them for known vulnerabilities before publishing.
2. Define triggers, secrets by name (third-party actions pinned to a commit SHA, least-privilege default workflow permissions, OIDC preferred over long-lived cloud credentials), environment, migration and recovery behavior.
3. Create runtime configuration only after required choices are resolved.
4. Validate available configuration/build paths; work through the [release-readiness checklist](../../checklists/release-readiness.md) before an authorized deploy; smoke-test only when authorized.

## 4. Required tools/scripts and environmental conditions

GitHub Actions runner semantics, Docker, and (once selected) the project's registry/target tooling. Deployment and smoke-testing require explicit authorization per the External operations section of [policies](../../policies.md) — never simulate a successful deploy without it.

## 5. Output artifacts, templates, ID conventions, and storage locations

`.github/workflows/` definitions, deploy configuration, and `work-items/<ID>/evidence.md`, starting from the [template](../../templates/evidence.md). No new ID is assigned; workflow files are named for the trigger/purpose they serve (e.g. `ci.yml`, `deploy.yml`).

## 6. Checklist and repeatable verification method

Work through the [release-readiness checklist](../../checklists/release-readiness.md) before every authorized deploy, not only the first one for a given pipeline.

## 7. Termination criteria and failure handling

Done when no runtime success is fabricated and every external action matches its granted scope. If a required choice (registry, target, secrets, rollback behavior) isn't resolved, apply the pause conditions in [policies](../../policies.md) instead of creating runtime configuration around a guess.

## 8. Work item update procedure and handover for the next step

Update `evidence.md` (image/version, smoke-test result) and `status.md`, and `decisions.md` for any open question. If the release causes an incident, hand off per bug-fix.md's incident steps (mitigate, then blameless postmortem).
