# Fix a defect

Read [policies](../policies.md) and [project context](../project.md). Select applicable [skills](../skills/README.md).

1. Capture the failing behavior, expected behavior and a reproducible case.
2. Assess impact and severity. For a live/production incident, prioritize restoring correct behavior over fully root-causing it first; a temporary mitigation is acceptable if the follow-up to fix the root cause is recorded. Create a proportional plan; honor existing explicit authorization.
3. Identify the cause and add a meaningful regression check where practical.
4. Fix the defect and update affected design/contracts.
5. Run targeted checks, inspect the diff and record evidence.
6. For a live/production incident, record a blameless postmortem in decisions.md or evidence.md: what happened, why, how it was found, how it was mitigated, and the prevention follow-up. Describe what failed, not who.

Use [templates](../templates/README.md) for durable state. Exit when the approved scope and its checks are complete; otherwise record the blocker and next action.
