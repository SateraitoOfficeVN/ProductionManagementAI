# git-review rules

- Create a dedicated branch, in its own git worktree isolated from the main working directory, for implementation work before making any change; never implement directly on the main/default branch or in the main worktree.
- Keep that worktree and branch through implementation, review and merge; remove the worktree once the branch is merged or abandoned so stale worktrees don't accumulate.
- Inspect the diff and repository state before editing or preparing review.
- Keep the PR focused; explain behavior, verification and remaining limitations.
- Check consistency between requirements, design, code, migration and tests.
- Apply the agreed branch/merge policy when defined; review does not itself authorize merging.
- Keep PRs small and focused on one change; split unrelated work into separate PRs instead of bundling it.
- Never push directly to a protected/main branch; open a PR even for a small change so required checks and review still run.
- Look for design, functionality, complexity, tests, naming and comments, not only whether the diff compiles (see ai/templates/review.md).
