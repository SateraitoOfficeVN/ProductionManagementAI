<!-- Product Requirements Document (PRD) template, based on common PRD conventions (Atlassian/Confluence, Aha, monday.com). Copy into the relevant work item; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# {Feature / Work Item Title} — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| {WI-###} | {who wrote this brief} | {draft \| in review \| approved} | {release/milestone, or "unscheduled"} |

## Overview

{What is this, and what problem does it solve, in two to three sentences.}

## Objective

{Why this should exist — how it supports a larger business or project goal. Link the source request instead of paraphrasing it at length.}

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| {what "success" means} | {how it is measured} | {threshold, or "not yet defined"} |

## Assumptions

- {assumption about users, technical constraints, or business rules this brief relies on}

## Actors and user stories

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| {actor} | {role} | {goal} | {benefit} | {UC-###} |

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| {REQ-###} | {testable statement of required behavior} | {observable condition that proves it is met} | {must \| should \| could} |

Keep `REQ-###` IDs stable once assigned; reference them from basic-design.md, detailed-design.md and test-plan.md instead of restating the requirement text.

## Not doing (out of scope)

- {related behavior explicitly deferred or excluded, and why}

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| {business-behavior question that blocks design or acceptance} | {what stays undecided} | {who can answer} | {open \| answered in decisions.md} |
