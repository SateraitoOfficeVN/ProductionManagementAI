<!-- Based on ai/templates/improvement.md. -->

# RFC: Closed work items are never reopened

**Status:** adopted
**Affected:** `ai/policies.md`, `ai/rules/common.md`, `ai/workflows/README.md` and the workflows that create or resume work items, `AGENTS.md`

## Summary

A work item that has ended (done, merged or closed out) is never restarted or reopened. Any follow-up, fix or change,
even to the same feature, starts a new work item with the next free number under `work-items/`, which may reference
the closed one.

## Motivation

Requested on 2026-10-25: "for ended WI do not restart it again create a new one".

Closed work items are the record of what was approved, built and verified. Reopening one mixes new plans and
evidence into a finished history and makes its status unclear. Recent practice already follows this (WI-011 to
WI-015 were each new work items for changes to earlier screens); this RFC makes it a rule.

## Guide-level explanation

- Before working on a request, check the related work item's `status.md`. If it is done, merged or closed out, do not
  edit its brief, plans, status, decisions or evidence.
- Create `work-items/WI-<next>/` instead, and link the closed work item from the new brief (for example "follows
  WI-016").
- Approved design documents of the closed work item stay unedited; changes go in additive documents, as WI-015 and
  WI-016 already do.
- Open (not yet ended) work items are resumed as before.

## Reference-level explanation

1. `ai/policies.md` and `ai/rules/common.md` state the rule above.
2. The workflows that start or resume work describe the check: a closed status means a new work item.
3. `AGENTS.md` mentions the rule where it points to work items.

## Drawbacks

- More work-item folders for small follow-ups.
- A follow-up's context is split across two work items; the link in the new brief covers this.

## Alternatives

- Reopen closed work items for small fixes. Rejected by the reviewer.

## Rollout

Once approved: one documentation-only branch and PR (CI skips it, RFC 0005), updating the files above and listing
this RFC as adopted in `ai/improvements/README.md`.
