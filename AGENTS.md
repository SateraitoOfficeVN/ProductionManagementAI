# Shared agent entry point

Read [ai/project.md](ai/project.md), [ai/policies.md](ai/policies.md) and [ai/rules/common.md](ai/rules/common.md).
Select the workflow from [ai/workflows/README.md](ai/workflows/README.md), then read only the relevant skills from [ai/skills/README.md](ai/skills/README.md). Pass the applicable gate in [ai/checklists/README.md](ai/checklists/README.md) — design-consistency, security-review, delivery, release-readiness — before calling that stage done.

For an existing work item, read its brief, approved plan, status, decisions and evidence before editing. Inspect the repository state and preserve unrelated user changes.

Create a reviewable plan before implementation. Existing explicit authorization remains valid; do not add redundant approvals for work already authorized. Continue within the approved scope and ask only at the defined stop conditions.

Record actual verification results. Never report an unimplemented application, unrun test or unconfigured deployment as working. Native skill registration is not configured; read the referenced SKILL.md files directly.
