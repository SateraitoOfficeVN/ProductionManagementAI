# frontend

Frontend unit/component tests (Vitest + React Testing Library, per DEC-004) live at `src/frontend/tests/unit/`, not here.

`src/frontend` is a standalone npm package with its own `node_modules`; Vite's module resolution can't reach package imports from files outside that package without hoisting to an npm-workspaces monorepo, which is out of scope for this work item. See `work-items/WI-001/decisions.md` (step 19 implementation note) for the full rationale. This directory is kept only so `tests/` documents where frontend test evidence is discussed, even though the test files themselves live under `src/frontend`.

Run: `npm test` from `src/frontend`.
