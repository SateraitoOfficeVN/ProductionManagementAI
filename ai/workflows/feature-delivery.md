# Deliver a feature

Read [policies](../policies.md) and [project context](../project.md). Select applicable [skills](../skills/README.md).

1. Read the brief and acceptance criteria; identify missing business decisions.
2. Draft the plan revision, show it to the user and wait for explicit approval before starting its steps. Repeat for each later revision (e.g. implementation after design): approving a design does not approve the next revision.
3. Check SA; produce BD/layout, DB/API and DD using linked skills.
4. Reconcile design artifacts before implementing their dependent code.
5. Implement and run unit/integration checks, then system/E2E verification.
6. Review the diff, prepare the PR summary and execute only authorized external operations.
7. Record evidence and remaining limitations; deploy/smoke only if included and authorized.
8. After the PR merges, close out in one follow-up change: the work item's status, evidence and plan closure; `ai/project.md`; `CLAUDE.md`'s current-state section; and the root `README.md` (status list, "Next", and any CI, workflow or layout change the work item introduced).

Use [templates](../templates/README.md) for durable state. Exit when the approved scope and its checks are complete; otherwise record the blocker and next action.
