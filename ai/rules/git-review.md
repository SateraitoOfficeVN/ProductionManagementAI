# git-review rules

- Inspect the diff and repository state before editing or preparing review.
- Keep the PR focused; explain behavior, verification and remaining limitations.
- Check consistency between requirements, design, code, migration and tests.
- Apply the agreed branch/merge policy when defined; review does not itself authorize merging.
- Keep PRs small and focused on one change; split unrelated work into separate PRs instead of bundling it.
- Never push directly to a protected/main branch; open a PR even for a small change so required checks and review still run.
- Look for design, functionality, complexity, tests, naming and comments, not only whether the diff compiles (see ai/templates/review.md).
