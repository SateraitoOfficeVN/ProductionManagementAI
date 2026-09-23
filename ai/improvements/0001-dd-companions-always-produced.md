# RFC: Always produce all four DD documents

**Status:** adopted
**Affected:** `ai/templates/detailed-design.md`, `ai/templates/DD/api-design.md`, `ai/templates/DD/function-design.md`, `ai/templates/DD/screen-processing-design.md`, `ai/templates/README.md`, `ai/skills/detailed-design/SKILL.md`, `ai/evaluations/baseline-cases.md`

## Summary

The detailed-design step must always produce the whole DD template family as four separate Markdown files: the main DD plus the API, function-design and screen-processing companions. Previously the templates marked the companions as optional, to be used only when the main DD couldn't hold the content.

## Motivation

During WI-002 (Screen A, 2026-09-18), Claude produced 001_DD and only the API companion, because the template wording ("Use it when…", "None — this screen's design fits entirely in this file") allowed folding the function design and processing flows into the main document. The user pointed out that "there're still 2 templates not output to md file that are function-desing and screen-processing-design", had them produced (001_DD-FN, 001_DD-SPD), and then asked to "update the templates and skill so all DD companions are always produced". The template family mirrors the reference workbook set in `ai/templates/example/DD`, and the deliverables are expected to match that full set. Evidence: `work-items/WI-002/status.md` (2026-09-18 entries), `docs/en/020_detailed-design/001_DD-*.md`.

## Guide-level explanation

For every DD, an agent writes four files side by side in `docs/en/020_detailed-design/###/`: `###_DD_{slug}.md`, `###_DD-API_{slug}.md`, `###_DD-FN_{slug}.md` and `###_DD-SPD_{slug}.md`. Each piece of content has exactly one home:

- endpoint catalogs → API
- backend service/Application-layer methods, shared or not → FN
- step-by-step processing per component → SPD
- screen items, states, screen-owned modules, state transitions → main DD, with pointers to the rest

If a companion's subject doesn't exist (e.g. a backend-only DD has no screen processing), the file is still produced, with its sections marked "Not applicable — {reason}".

## Reference-level explanation

- **Current behavior:** the companions were optional. Function design was limited to modules shared across screens; the API and screen-processing content could be inlined in the main DD.
- **Proposed behavior:** the four documents are mandatory, with fixed file-name and document-ID conventions. The main template's "Companion design documents" table is pre-filled with three rows and loses its "None" option. "Module design", "Processing and state transitions" and "APIs used" point to the companions instead of offering to inline them. A new "Processing flows" pointer table is added. The skill gains an execution step, an output table, a checklist check and a termination condition for the full set.
- **Definition of done:** no remaining "optional"/"use it when" wording for the companions in the templates or skill; the new evaluation case passes against an existing DD set.

## Drawbacks

A small screen or a backend-only change produces more files, some of them mostly "Not applicable".

## Rationale and alternatives

- **Keep them optional and rely on memory/user reminders:** rejected, because it depends on each session remembering, and Codex doesn't share Claude's memory.
- **Mandatory only for screens:** rejected. The user asked for "always", and "Not applicable" sections cover the backend-only case cheaply.

## Prior art / evaluation

| Case (from ai/evaluations) | Before | After | Pass/fail |
| --- | --- | --- | --- |
| New: "DD step for a screen whose API and flows are small" | Main DD + API companion only; FN/SPD content folded into the main DD (observed in WI-002) | Rule requires all four files; checked against WI-002's current DD set: 001_DD, 001_DD-API, 001_DD-FN, 001_DD-SPD exist, and 001_DD's companion table lists all three | pass (manual check against existing files, 2026-09-18; the rule itself hasn't yet been exercised on a fresh DD) |
| "DD field conflicts with DB constraint" | Flag and reconcile | Unchanged (the rule adds files, not consistency relaxations) | pass (no gate relaxed) |

## Risk and rollback

- **Risk:** more documents to keep in sync. This is mitigated by the one-home-per-content rule and the skill's checklist step for duplication.
- **Rollback plan:** revert the files listed under **Affected** via version control to the revision before this change (`e7e0d36` for the templates and skill). No gate was relaxed.

## Unresolved questions

- None.

## Adoption

- **Reviewer:** ThanhTN (explicit request, 2026-09-18: "update the templates and skill so all DD companions are always produced")
- **Adopted revision:** commit `dc228ab` on `feature/harness-wi002-feedback` (effective for shared use once that branch's PR is merged)
