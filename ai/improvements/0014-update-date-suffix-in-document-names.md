<!-- Based on ai/templates/improvement.md. -->

# RFC: Update-date suffix in document file names

**Status:** adopted
**Affected:** `ai/rules/documentation.md`, `ai/skills/basic-design/SKILL.md`, `ai/skills/detailed-design/SKILL.md`, `ai/skills/database-design/SKILL.md`, `ai/templates/detailed-design.md`, `ai/templates/database-design.md`, `ai/harness-overview.md`; design documents created or revised after adoption, and their PDFs under `docs/en/pdf/` and `docs/ja/pdf/`

## Summary

A design document's file name ends with the date its content was last updated, as `_YYYYMMDD`:
`<ID>_<slug>_<YYYYMMDD>.md`. Only documents created or revised after this RFC is adopted take the suffix; approved
historical documents keep their current names. When a document is revised, its file is renamed in place to the new
date, and git history keeps the earlier versions.

## Motivation

Requested on 2026-10-25: "có gắn đuôi ngày update" (attach an update-date suffix to document names). Asked how it
should work, the user chose the `_YYYYMMDD` format, new or revised documents only, and renaming in place.

The file name then shows how current a document is at a glance, the way Japanese SI document control usually names
deliverables, without opening the file or checking git.

## Guide-level explanation

| Case | File name |
| --- | --- |
| Before (unchanged for approved historical documents) | `001_BD_製造指示登録・編集.md` |
| New document written on 2026-10-25 | `006_BD_<画面名>_20261025.md` |
| That document revised on 2026-11-03 | renamed to `006_BD_<画面名>_20261103.md` |

- The date is the day the document's content was last changed (plant calendar date, Asia/Tokyo), not the approval
  or render date.
- The PDFs carry the same name: `docs/en/pdf/010_basic-design/006/006_BD_<画面名>_20261103.pdf` and the same path under
  `docs/ja/pdf/`.
- A DD and its companions (`-API`, `-FN`, `-SPD`) each carry their own date. Changing one does not rename the others.
- Unchanged: document IDs, which are still the stable reference in text (`006_BD`, not the file name); folder names;
  wireframe and mockup names (`006_BD_SCR-006-pc.svg`), which carry no date; the `000_DB` and ADR naming apart from
  the suffix.
- An additive design for a closed work item (for example `002_DD-CSV`) is a new document and takes the suffix. The
  approved document it extends keeps its name.

## Reference-level explanation

1. `ai/rules/documentation.md` adds the suffix to the file-name rule: `<ID>_<slug>_<YYYYMMDD>.md` for documents
   created or revised after adoption; earlier approved documents keep `<ID>_<slug>.md` until they are next revised.
2. The BD, DD and DB skills, the DD and DB templates and `ai/harness-overview.md` show the dated name.
3. A revision renames the Markdown and both PDFs with `git mv`, updates every live link to them in the same change,
   and re-renders the PDFs with `scripts/docs-pdf.py` (RFC 0008).
4. Records of what happened (closed work items, earlier RFCs) keep the names they used at the time; only live links
   are rewritten.
5. Check: every relative link resolves and no live link points to an old dated name.

## Drawbacks

- Each revision changes the file name, so links in other documents must be updated in the same change, and an
  outside bookmark to the old name breaks. Text references use the document ID to limit this.
- For a while the repository mixes dated and undated names.
- `git log --follow` is needed to see a renamed file's full history.

## Alternatives

- `_YYYY-MM-DD` or `_vN_YYYYMMDD`. The reviewer chose `_YYYYMMDD`.
- Rename every existing document now. Rejected: it changes approved, immutable documents and their links.
- Keep every dated version side by side. Rejected in favor of renaming in place, with git as the version history.
- Show the update date only inside the document (revision table). Doesn't meet the request for a file-name suffix.

## Rollout

Once approved: one branch and PR, documentation only (CI skips it, RFC 0005), updating the rule, skills, templates
and `ai/improvements/README.md` (this RFC listed as adopted). No existing document is renamed.
