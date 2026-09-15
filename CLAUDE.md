# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Claude entry point

Follow [AGENTS.md](AGENTS.md) as the shared project instruction source.
Use [ai/skills/README.md](ai/skills/README.md) to locate the relevant skill and read its SKILL.md directly.

Keep durable state in work-items, not only in chat. This repository does not yet configure Claude-native skill registration; do not duplicate the shared skills in this adapter.

## Current state

This repository is a documentation scaffold, not yet an application: there is no `src` implementation, no build/lint/test commands, no CI pipeline, and no Docker runtime. Do not invent or assume commands — `ai/project.md` lists what is confirmed (Vite + TypeScript frontend, .NET 10 backend, PostgreSQL, GitHub Actions, Docker) versus still open (UI framework, ORM, test frameworks, PostgreSQL version, auth scope, branch/deploy policy). When the application is implemented, verified commands belong in `ai/project.md`, not here.

## Working in this repo

- Start from [AGENTS.md](AGENTS.md): it points to `ai/project.md`, `ai/policies.md`, `ai/rules/common.md`, then a workflow in `ai/workflows/README.md`, the relevant skill(s) in `ai/skills/README.md`, and the applicable gate in `ai/checklists/README.md`.
- For an existing work item under `work-items/`, read its brief, approved plan, status, decisions and evidence before editing.
- Follow `ai/policies.md` for authorization and pause conditions — plans need review before implementation, but an approved plan doesn't need re-approval for each step within its scope.
- This scaffold does not authorize GitHub push, PR creation, merge, image publication or deployment; task-specific authorization is required for those.
