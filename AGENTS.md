# Shared agent entry point

Read [ai/project.md](ai/project.md), [ai/policies.md](ai/policies.md) and [ai/rules/common.md](ai/rules/common.md).
Select the workflow from [ai/workflows/README.md](ai/workflows/README.md), then read only the relevant skills from [ai/skills/README.md](ai/skills/README.md). Pass the applicable gate in [ai/checklists/README.md](ai/checklists/README.md) — design-consistency, security-review, delivery, release-readiness — before calling that stage done.

For an existing work item, read its brief, approved plan, status, decisions and evidence before editing. Inspect the repository state and preserve unrelated user changes.

Create a reviewable plan before implementation. Show every new plan revision to the user and wait for explicit approval of that revision before starting its steps; approval of a prior design or phase is not approval of the next revision. Existing explicit authorization remains valid; do not add redundant approvals for work already authorized. Continue within the approved scope and ask only at the defined stop conditions.

For a work item with multiple design Markdown documents (such as ADR, BD, DB and each DD file), create or revise them in the order stated in its plan. Complete each design `.md` file with its required companion artifacts, present that file for user review, and wait for an explicit instruction to continue before writing the next design `.md` file. Resolve feedback on the current design file first. Other documents and routine work-item records do not create review stops under this rule; plan-revision approval still applies. An explicit user request to produce design documents as a batch can override this sequence for that request.

Record actual verification results. Never report an unimplemented application, unrun test or unconfigured deployment as working. Native skill registration is not configured; read the referenced SKILL.md files directly.
