<!-- Based on ai/templates/improvement.md. -->

# RFC: Japanese screen names as document slugs

**Status:** adopted
**Affected:** `ai/rules/documentation.md`, `ai/skills/basic-design/SKILL.md`, `ai/skills/detailed-design/SKILL.md`, `ai/skills/database-design/SKILL.md`, `ai/templates/detailed-design.md`, `ai/harness-overview.md`; the BD, DD (with companions) and DB documents numbered 001–003 under `docs/en/`, and their PDFs under `docs/en/pdf/` and `docs/ja/pdf/`; every path or link to those files in `ai/`, `docs/`, `work-items/`, `deploy/`, `src/`, `README.md` and `CLAUDE.md`

## Summary

The `{slug}` part of a screen document's file name becomes the screen's Japanese name, as shown in the UI, instead
of an English kebab-case phrase. Every document that shares a screen's number takes that screen's name: the BD, the
main DD, its three companions and the DB design. Document IDs (`001_BD`, `001_DD-API`, …) are unchanged; only the
part after the ID changes. Documents that serve no screen (`000_DB`, the ADRs) keep their English slugs.

## Motivation

Requested on 2026-09-24: "about the documents name the {slug} part could you make it the actual screen name in
japanese". Asked how far it should go, the user chose to apply the screen name to every document with that number,
to rename the Markdown sources along with both PDFs, and 「製造指示登録・編集」 as the name of SCR-001.

The documents are read by a Japanese audience, so the file name now reads as the screen it describes, the way a
Japanese SI design-document set names its files.

## Guide-level explanation

| No. | Screen | Old slug(s) | New slug |
| --- | --- | --- | --- |
| 001 | SCR-001 | `production-order-create-edit` (BD, DD, DD-SPD), `production-orders` (DD-API), `production-order-service` (DD-FN), `production-order-schema` (DB) | `製造指示登録・編集` |
| 002 | SCR-002 | `production-order-list` (BD, DD, DD-API, DD-SPD), `production-order-list-query` (DD-FN), `production-order-list-queries` (DB) | `製造指示一覧` |
| 003 | SCR-003 | `production-dashboard` (BD, DD, DD-API, DD-SPD), `dashboard-snapshot` (DD-FN), `completion-tracking-and-dashboard-queries` (DB) | `ダッシュボード` |

For example, `020_detailed-design/001/001_DD-API_production-orders.md` becomes
`020_detailed-design/001/001_DD-API_製造指示登録・編集.md`, and its PDFs become
`docs/en/pdf/020_detailed-design/001/001_DD-API_製造指示登録・編集.pdf` and the same path under `docs/ja/pdf/`.

The slug is the screen's name in the UI heading (`製造指示一覧`, `ダッシュボード`). Where the UI has no single heading, as
on SCR-001 (「新規製造指示」 in create mode, 「製造指示 {No.}」 in edit mode), the slug is the name the reviewer chose, recorded
here. A new screen's documents take its Japanese name from the start. Documents that serve no
screen keep an English kebab-case slug: `000_DB_identity-schema.md`, `0001_ADR_backend-layered-structure.md`.

Unchanged: document IDs, wireframe, mockup and folder names (they start with the ID and carry no slug, for example
`001_BD_SCR-001-pc.svg`), and the document contents, which stay English-only.

## Reference-level explanation

1. Rule and skills: `ai/rules/documentation.md` states the slug rule above. The BD, DD and DB skills, the DD
   template and `ai/harness-overview.md` use `{screen name in Japanese}` in place of `{slug}` for screen documents.
2. Rename with `git mv`: 18 Markdown files (6 per screen) and their 36 PDFs.
3. Rewrite every path and Markdown link to the old names across the repository. In Markdown links, the Japanese
   name is written as is (GitHub and the PDF renderer both resolve UTF-8 relative paths); the link text keeps the
   document ID.
4. Re-render the English and Japanese PDFs of every document whose text changed (renamed links), with
   `scripts/docs-pdf.py`, as RFC 0008 requires.
5. Records of what happened (RFC 0010, the demo transcripts and the built demo decks) keep the names they used at the
   time; only live links are rewritten.
6. Check: no reference to an old name is left outside those records, every relative link resolves, and the PDFs open.

## Drawbacks

- Non-ASCII file names: `git status` on Windows shows them octal-escaped unless `core.quotepath=false` is set, and
  they are harder to type in a shell. Tab completion and copying from the file list work around this.
- `・` (U+30FB) and katakana are valid on Windows, macOS and Linux, and in CI; no space or reserved character is used.
- DD-API, DD-FN and DB lose the English subject that told them apart (`production-order-service`); their ID suffix
  (`-API`, `-FN`, `_DB`) now carries that distinction alone.

## Alternatives

- Only the screen documents (BD, DD, DD-SPD) take the Japanese name, and the other documents keep their subject
  slugs. Rejected by the reviewer in favor of one name per number.
- Only the Japanese PDFs take the Japanese name. Rejected: the source and its PDFs would stop sharing a name.

## Rollout

One branch and PR, documentation only (CI skips it, RFC 0005). The PR adds this RFC to `ai/improvements/README.md`
as adopted.
