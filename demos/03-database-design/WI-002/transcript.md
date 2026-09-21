# Transcript: Screen A database design (WI-002)

Source video: `demos/03-database-design/WI-002/ScreenA_DB.mp4`. It runs 10 min 44 s, is a 1920×1034 screen recording with no audio, and was made on 2026-09-18 between about 11:03 and 11:14 local time. It picks up directly where `demos/01-basic-design/WI-002/ScreenA_BD.mp4` ends.

Setup shown: JetBrains Rider with the `ProductionManagementAI` solution on branch `master`. Claude Code v2.1.275 (Opus 5) runs in the IDE terminal in a session named **Screen A**, with **auto mode on**. The editor above the terminal shows the generated Markdown files in preview as they are written or opened.

How it was made: the transcript was built from frames sampled every second across the whole recording, keeping the frames where the terminal changed, plus denser frames around the prompts and the question form. Wording in quotes is copied from the screen. Claude Code collapses the raw output of read-only commands (for example "Read 1 file, ran 1 shell command"). For those steps, the output recorded here is what Claude concluded and reported from them. The user's prompts are quoted with their spelling and grammar lightly corrected; the recordings show them exactly as they were typed. The decision IDs and document contents were checked against `work-items/WI-002/decisions.md` and `docs/en/database/0002-production-order-schema.md`.

---

## Edited video for the presentation

`demos/03-database-design/WI-002/ScreenA_DB_edited.mp4` is a 3 min 59 s cut to show alongside the slides:

- **Paced for reading.** The text-heavy frames — the summaries, the five open items, the resolved decisions table — are held 13 to 18 seconds each.
- **Jump cuts, not speed-ups.** Long stretches of agent work show their start, then jump to the result. Every jump inside a step is marked with a "» jumped ahead" badge.
- **Step captions.** A "Step N of 12" card in the empty AI Assistant panel on the right matches the numbering below. The accent is orange where the human acts and teal where the agent works.
- **Hidden text.** In step 8 the two-line CSRF bullet is repainted so the cut does not quote an earlier design pass; the substance of the sentence is unchanged.

| Step | Starts at (edited video) | Caption |
|---|---|---|
| 1 | 00:00 | Prompt 1: approve the plan, start DB design |
| 2 | 00:12 | Claude reads first |
| 3 | 00:32 | Output: DB-002 |
| 4 | 00:46 | Claude corrects its own draft |
| 5 | 00:59 | Output: plan approved, status and evidence |
| 6 | 01:15 | Turn 1 summary |
| 7 | 01:33 | Prompt 2: are there any open questions left? |
| 8 | 01:44 | Five open items, three of them new |
| 9 | 02:02 | Prompt 3: Let's answer all of them |
| 10 | 02:11 | Decision form (four tabs) |
| 11 | 02:50 | Recording the answers, then the turn 3 summary |
| 12 | 03:32 | Review: decisions.md and DB-002 |

`demos/03-database-design/WI-002/ScreenA_DB_presentation.pdf` is the 16-slide deck built from this transcript; each step slide carries the edited-video timestamp above.

The times in the rest of this transcript refer to the original recording.

---

## At a glance

| # | Time | Who | Step | Output |
|---|---|---|---|---|
| 1 | 00:00–00:27 | User | Prompt 1: "Approved the plan; let's move on to DB design" | — |
| 2 | 00:27–01:10 | Claude | Reads the database-design skill, rules and template, DB-001, and the existing persistence code | The conventions DB-002 must follow (snake_case naming, `uuid` keys, EF migrations) |
| 3 | ~01:15–01:52 | Claude | Writes the database design | `docs/en/database/0002-production-order-schema.md` (216 lines) |
| 4 | ~02:02 | Claude | Corrects one line of its own draft | DB-002 operational note: the computed `order_number` is defined with EF Core, not raw SQL |
| 5 | 02:20–02:40 | Claude | Records the approval and updates the work item | `plan.md` (review status → approved), `status.md`, `evidence.md`, the database README, BD-001's open questions |
| 6 | 02:42 | Claude | Summarizes turn 1 | Three technical decisions made (DEC-013–DEC-015), one open business item (DEC-016) |
| 7 | 03:27 | User | Prompt 2: "Are there any open questions left?" | — |
| 8 | 03:27–03:40 | Claude | Greps the artifacts for open/proposed/unresolved items | Five items: two known, three gaps Claude had filled in silently and wanted confirmed |
| 9 | 04:20 | User | Prompt 3: "Let's answer all of them" | — |
| 10 | 04:21–06:16 | Claude ↔ User | Four-question decision form (Timezone · Cancel · Products · DB roles) | Four answers, one of them free text, one overriding the recommendation |
| 11 | 06:16–08:07 | Claude | Records five decisions, adds the runtime-login design to DB-002 | DEC-016–DEC-020 decided; REQ-019 added; BD-001 revision 3; DB-002, ADR-0002, DB-001 and the work item updated |
| 12 | 08:10–10:44 | User | Reviews `decisions.md` and DB-002 in the editor | — (the recording ends with the next prompt typed but not sent) |

Total agent working time was 2 m 15 s for turn 1, 13 s for turn 2 and 1 m 51 s for turn 3 — 4 m 19 s in all. The user typed three short prompts and answered four multiple-choice questions.

---

## Detailed timeline

### Step 1 · 00:00–00:27 — Prompt 1 (user)

The session opens on the closing summary of the basic-design step, and Claude Code has pre-filled a suggested reply in grey: *"Plan approved; let's work on the database design next"*. The user does **not** accept it. They clear the line and type their own:

> **Approved the plan; let's move on to DB design**

The prompt approves plan revision 1 and names the next step. It names no files, no template and no schema.

### Step 2 · 00:27–01:10 — Claude orients itself (read-only)

| # | Action shown on screen | Command (abridged) | Output |
|---|---|---|---|
| 2a | "Reading DB design skill, rules, template and DB-001" | `cat ai/skills/database-design/SKILL.md ai/rules/database.md ai/templates/database-design.md; cat docs/en/database/0001-identity-schema.md; cat docs/en/database/README.md` | The section structure DB-002 must follow, the database rules (least privilege, justify every index, state migration impact) and the conventions DB-001 already set. |
| 2b | Same step, second command | `ls src/backend/ProductionManagementAI.Infrastructure …/Domain; cat …/Persistence/AppDbContext.cs 2>/dev/null || find src/backend -name "*DbContext*.cs" | xargs cat …` | The persistence setup actually in the code, rather than what the docs claim. |

Claude Code collapses both to *"Read 1 file, ran 1 shell command"*.

**Output of this step:**

> "Plan approved, so I'm recording that and starting DB-002. I've checked the existing persistence setup (snake_case naming convention, uuid keys, EF migrations) so DB-002 can follow it."

### Step 3 · ~01:15–01:52 — Database design written

Action: `Write(docs/en/database/0002-production-order-schema.md)`.

**Output: DB-002** (216 lines at this point; 237 lines in the committed file after later steps). The head of the document, visible as it streams:

- Title: "ProductionManagementAI — Production Order Schema — Database Design Document (テーブル定義書)".
- Traceability line: "DB-002 — requirements REQ-010–REQ-018 (`work-items/WI-002/brief.md`), basic design BD-001 (`docs/en/010_basic-design/BD-001-production-order-create-edit.md`), implements DD-001 (not yet written; the API field names below are proposed for DD-001 to confirm)".
- Naming line: "Physical naming follows DB-001: `snake_case` tables and columns (`EFCore.NamingConventions`, `UseSnakeCaseNamingConvention()` in `DependencyInjection.cs`), `uuid` surrogate keys defaulting to `gen_random_uuid()`, `timestamptz` UTC timestamps suffixed `_utc`, and constraint names in the `pk_`/`fk_`/`ix_`/`ck_` style EF Core generates."
- **Table list** (seen later in the editor preview):

  | Table | Physical name | Purpose |
  |---|---|---|
  | Products | `products` | Minimal product reference data, seeded, no catalog UI (DEC-005) |
  | Production orders | `production_orders` | One row per production order created on SCR-001 |
  | Production order number counters | `production_order_number_counters` | Last issued order-number sequence per year, used to generate `PO-YYYY-NNNNN` (DEC-012, DEC-013) |

- **Rollback plan:** `dotnet ef database update InitialIdentitySchema --project src/backend/ProductionManagementAI.Infrastructure --startup-project src/backend/ProductionManagementAI.Api` drops the three tables. Nothing outside them references them.
- **Open decisions** table with a recommendation per row (resolved during this session — see step 12).

### Step 4 · ~02:02 — Claude corrects its own draft

Action: an `Update` to DB-002 line 193, shown as a red/green diff in the terminal.

**Output:** the operational note changes from

> "the generated `order_number` column **is written by raw SQL in the migration** (EF Core `HasComputedColumnSql(..., stored: true)`)"

to

> "the generated `order_number` column **is defined with** EF Core `HasComputedColumnSql(..., stored: true)`. The expression uses explicit `::text` casts because PostgreSQL requires generation expressions to be immutable."

No user prompt triggered this; Claude caught the inaccuracy itself while finishing the document.

### Step 5 · 02:20–02:40 — Approval recorded, work item updated

Three `Update` calls, each shown as a diff and each marked *"Allowed by auto mode classifier"*.

**Output:**

- `work-items/WI-002/decisions.md`: DEC-013 (per-year order-number generation and overflow), DEC-014 (optimistic-concurrency token) and DEC-015 (status storage) added as *decided*, each with its options table, maker ("Claude (technical decision within the approved plan, step 4)"), rationale and impact.
- `work-items/WI-002/plan.md` (+3 −3): under **Approval / sign-off**, "Review status: submitted for review" became "Review status: **approved**", and the lines "Approval source: steps 1–2 performed on the user's direct request of 2026-09-18; steps 3–6 await review of this revision" and "Approved revision: none yet" were replaced.
- `work-items/WI-002/evidence.md`: the traceability table gained a DB-002 reference on every affected requirement row — REQ-011 "DB-002 `xmin` concurrency", REQ-013 "DB-002 `ck_production_orders_quantity_positive`", REQ-015 "DB-002 `fk_production_orders_products_product_id`", REQ-016 "DB-002 `notes varchar(500)`", REQ-017 "DB-002 `ck_production_orders_status`". Under **Remaining limitations and next action**, "Only BD-level design exists. Next action: write DB-002 once plan revision 1 is reviewed." became "BD-001 and DB-002 exist; no DD yet. Next action: write DD-001 with a rendered mockup."
- `work-items/WI-002/status.md`: the next milestone became "Detailed design (DD-001) with a rendered mockup (step 5)", and the risk row "Plan revision 1 not yet reviewed | ThanhTN | 2026-09-18 | Steps 3–6 wait for review" was replaced by "DEC-016: DB role split | ThanhTN | 2026-09-18 | None for design; needed before any non-local deployment".
- The database README and BD-001's open-questions table were updated to match.

### Step 6 · 02:42 — Turn 1 summary (Claude)

**Output:** the reply below. Its top part scrolled off before it was captured; the visible part says:

> - A mapping from each column to a proposed API field, for DD-001 to confirm.
> - **Migration impact:** it only adds tables. The doc includes the rollback command and notes that rolling back after use deletes all order data.
>
> **New decisions:** I decided three technical points myself because they don't change how the screen behaves (details in `decisions.md`):
> - **DEC-013:** order numbers come from the per-year counter table. A year's 100,000th order is rejected rather than changing your chosen format.
> - **DEC-014:** use `xmin` to detect that someone else changed an order.
> - **DEC-015:** status is stored as text with a check on the four allowed values.
>
> **One open item, DEC-016:** today the app and the migrations both connect to the database as the owner account. The database rules call for a separate app login with only the permissions it needs. Changing that means editing WI-001's Compose setup, so I recommend doing it before any deployment beyond your machine. It doesn't block the design.
>
> `status.md`, `evidence.md` (traceability now includes DB-002), the database README, and BD-001's open-questions table are updated to match.
>
> Next is step 5: the detailed design (DD-001) plus a rendered mockup. Publishing the mockup as a private artifact will need your go-ahead when I get there.

*Brewed for 2m 15s · done 11:06 AM*

### Step 7 · 03:27 — Prompt 2 (user)

> **Are there any open questions left?**

The same pattern as in the basic-design step: rather than accepting that one open item was the whole list, the user asks Claude to go and check.

### Step 8 · 03:27–03:40 — Claude checks and finds three more

Action: *"Searching for `\| open|proposed|unresolved`"* across the work item and the design documents.

**Output:** five items, two of which Claude had already raised and three of which it had quietly decided for itself:

> 1. **DEC-016, splitting the database logins.** Today the app and the migrations both connect as the database owner. My recommendation is to keep that for the local demo and split them before any deployment beyond your machine. It needs your call eventually, but it doesn't block the detailed design.
> 2. **CSRF protection** (BD-001, open questions). This is a technical point that the detailed design (DD-001) is meant to settle. […] no extra token is needed (same-origin cookie with `SameSite=Lax` and no CORS). I'll confirm or revisit that while writing DD-001.
>
> Rechecking just now, I also found three gaps that aren't recorded as open questions. I filled them in myself, and you should confirm them:
>
> 3. **Which plant timezone.** You chose one configured plant timezone (DEC-011), but not which one. It decides what "today" means for due dates and which year goes into `PO-YYYY-…`. It needs a concrete value such as `Asia/Ho_Chi_Minh` or `Asia/Tokyo`.
> 4. **Cancel with unsaved changes.** BD-001 says Cancel just leaves the page without asking. A common alternative is to ask "Discard your changes?" when the form has been edited.
> 5. **Sample products.** DB-002 seeds five made-up products (`P-1001 Steel bracket` … `P-1005 Hydraulic valve`). They're placeholder demo data. Swap them if you want something more realistic for the videos.
>
> Items 3 and 4 change what users see, so I'd like your answers before DD-001 builds on them. Tell me the timezone and whether Cancel should ask to confirm, and I'll record them and go on to DD-001.

*Sautéed for 13s · done 11:07 AM*

### Step 9 · 03:51–04:20 — Prompt 3 (user)

The user first starts typing a direct answer to the two questions Claude asked for — *"Asia/Ho_Chi_Minh, and yes, confirm on cancel"* — then deletes it and asks for all five instead:

> **Let's answer all of them**

### Step 10 · 04:21–06:16 — Decision form (AskUserQuestion)

Action: Claude opened a tabbed multiple-choice form with four tabs (Timezone · Cancel · Products · DB roles) and a Submit step. Each question offered a "Type something" free-text option and "Chat about this".

**Tab 1: Timezone.** *Which timezone should the plant use for "today" (due dates) and the year in `PO-YYYY-NNNNN`?*

1. `Asia/Ho_Chi_Minh` — Vietnam time, UTC+7.
2. `Asia/Tokyo` — Japan time, UTC+9. ← **chosen**
3. `UTC` — No local offset. Near midnight, "today" may not match the plant's local date.

**Tab 2: Cancel.** *When the user presses Cancel after editing the form, what should happen?*

1. **Confirm if edited (Recommended)** — If any field changed, ask "Discard your changes?" (Discard / Keep editing). If nothing changed, leave immediately. ← **chosen**
2. Leave without asking — Always leave immediately and lose unsaved changes. Simplest.

**Tab 3: Products.** *What should the demo seed as products?*

1. **Keep the 5 samples (Recommended)** — `P-1001 Steel bracket`, `P-1002 Aluminium housing`, `P-1003 Control panel assembly`, `P-1004 Drive shaft`, `P-1005 Hydraulic valve`.
2. Larger sample (~15) — More products, so the dropdown looks more realistic in the demo videos.
3. *Type something* ← **chosen**, typed as: *"Let's do 30 samples so we can get a sense of the screen's workload"*

**Tab 4: DB roles.** *DEC-016: How should database logins be handled?*

1. **Split before non-local (Recommended)** — Keep the single owner login for the local demo. Split into a migration owner and a restricted app login before any shared or non-local deployment.
2. **Split now** — Add a restricted runtime login to the local Compose setup during Screen A's implementation. Changes WI-001's deploy config. ← **chosen (the user overrode the recommendation)**

**Submit step:** a "Review your answers" screen listing all four before sending.

**Output:** the answers as Claude Code recorded them:

> User answered Claude's questions:
> · Which timezone should the plant use for "today" (due dates) and the year in PO-YYYY-NNNNN? → Asia/Tokyo
> · When the user presses Cancel after editing the form, what should happen? → Confirm if edited (Recommended)
> · What should the demo seed as products? → Let's do 30 samples so we can get a sense of the screen's workload
> · DEC-016: How should database logins be handled? → Split now

### Step 11 · 06:16–08:07 — Claude records the answers

Action: before writing anything, Claude re-checks the code the answers affect — *"Checking migration startup and cookie/CSRF setup"*: `grep -rn "Migrate\|EnsureCreated\|IdentitySeeder" src/backend --include=*.cs | grep -v "/Migrations/" | head; sed -n 1,40p deploy/compose.yaml; ls deploy/docker`. It then reports:

> Recording all five: your four answers, plus the CSRF point, which I'm settling as a technical decision. The cookie setup (`HttpOnly`, `SameSite=Lax`, no CORS) matches what ADR-0002 assumed.

**Output:**

- `decisions.md`: five decisions recorded (wording as in the committed file):

  | ID | Decision | Maker | Outcome |
  |---|---|---|---|
  | DEC-016 | Separate migration-owner and runtime DB roles | user | Split now: owner login runs migrations, restricted login at runtime (local Compose included) |
  | DEC-017 | Which plant timezone | user | `Asia/Tokyo` |
  | DEC-018 | Cancel with unsaved changes | user | Ask "Discard your changes?" only if the form was edited |
  | DEC-019 | Demo product seed data | user | 30 sample products |
  | DEC-020 | CSRF protection for order endpoints | Claude (technical security) | No extra anti-forgery token: `SameSite=Lax` cookie + no CORS policy + JSON-only request bodies |

- **DB-002**, extended with the runtime-login design: a role table contrasting the **Owner** (`POSTGRES_USER`, existing — "`dotnet ef database update` only; owns the schema; runs DDL") with the **Runtime** login (new, proposed name `pmai_app` — `CONNECT` on the database, `USAGE` on schema `public`, the table grants, plus what ASP.NET Core Identity and the `IdentitySeeder` need on the DB-001 tables: `SELECT`/`INSERT`/`UPDATE` on `users`, `roles`, `user_roles`, `user_claims`, `role_claims`, `user_logins`, `user_tokens`, `DELETE` on `user_roles`/`user_claims`/`user_logins`/`user_tokens`, and `USAGE, SELECT` on the identity sequences — "No DDL, no TRUNCATE"). Claude then corrected its own paragraph again, replacing "The script also sets `ALTER DEFAULT PRIVILEGES` so the runtime login can read future tables only when a later migration grants it explicitly" with "No `ALTER DEFAULT PRIVILEGES` blanket grant is used: any later migration that adds a table must extend the script with that table's explicit grants."
- `brief.md`: **REQ-019** added (the Cancel confirmation).
- **BD-001 revision 3**, logged in its own revision history as "DEC-017–DEC-020: plant timezone `Asia/Tokyo`; Cancel confirmation when edited (REQ-019, items 17–19, E-07/E-09); 30 seeded products; CSRF approach decided". The confirm dialog starts focus on **Keep editing**, and Escape keeps the user's edits.
- ADR-0002 and DB-001: the "reaffirm or revisit later" note on CSRF replaced by a reference to DEC-020.
- `plan.md`, `status.md`, `evidence.md`: updated to match.

**Turn 3 summary:**

> All five items are answered and recorded. Nothing in Screen A's design is open now: every decision from DEC-001 to DEC-020 is decided.
>
> | ID | Question | Decision |
> |---|---|---|
> | DEC-016 | Split the database logins | **Split now** (your choice). The owner login only runs migrations, and the backend runs on a new restricted login (`pmai_app`), including in local Compose. DB-002 lists its exact permissions: for orders, read, insert and update, with no delete and no schema changes. It also covers what login and account management (Identity) and the startup seeder need. |
> | DEC-017 | Plant timezone | `Asia/Tokyo`. It sets "today" for due dates and the year in `PO-YYYY-NNNNN`, and it's kept as a config value. |
> | DEC-018 | Cancel with unsaved changes | Asks "Discard your changes?" only if something was edited. This is new requirement **REQ-019**. BD-001 gets a dialog (items 17–19) and events E-07/E-09: focus starts on **Keep editing**, and Escape keeps your edits. It covers the Cancel button only, not browser back or closing the tab. |
> | DEC-019 | Demo products | 30 sample products (`P-1001`–`P-1030`), listed in DB-002. The detailed design (DD-001) will check how usable a 30-item dropdown is. |
> | DEC-020 | CSRF | Decided as a technical point, with a "you can overrule it" note. |
>
> Updated files: `decisions.md`, `brief.md` (REQ-019 added), BD-001 (now revision 3), DB-002, ADR-0002, DB-001, `plan.md`, `status.md` and `evidence.md`.
>
> **Plan scope:** because the login split is happening now, the implementation plan (revision 2) will need to change WI-001's Docker Compose files:
> - a new required runtime password in `.env.example`, with no default;
> - a second connection string;
> - a script that grants the runtime login's permissions;
> - updating the migration command in `ai/project.md` to run as the owner login.
>
> That plan revision will come to you for review before any of it is built.
>
> Next is step 5: the detailed design (DD-001) plus a rendered mockup. I'll ask before publishing the mockup.

*Sautéed for 1m 51s · done 11:12 AM*

### Step 12 · 08:10–10:44 — User reviews the output

The user opens `work-items/WI-002/decisions.md` in the editor and reads the summary table (DEC-016 to DEC-020, each with its date, maker, status and outcome), then opens `docs/en/database/0002-production-order-schema.md` and scrolls from the table list down to its **Open decisions** table, which now shows every row resolved:

| Decision | Options | Recommendation | Status |
|---|---|---|---|
| How to generate the per-year order sequence | per-year counter table with upsert; `MAX(order_seq)+1` with retry; one PostgreSQL sequence per year | Counter table | decided — see `work-items/WI-002/decisions.md` DEC-013 |
| What happens past 99,999 orders in one year | reject the create; widen to 6 digits | Reject (CHECK constraint) | decided — DEC-013 |
| Optimistic-concurrency token | PostgreSQL `xmin`; explicit `version` column | `xmin` | decided — DEC-014 |
| Status storage | `varchar` + CHECK; PostgreSQL enum type; smallint code | `varchar` + CHECK | decided — DEC-015 |
| Split migration-owner and restricted runtime DB roles | split now; keep the single owner role for the local demo | Split before any shared/non-local deployment | decided — split now (DEC-016, user) |
| Demo product seed size | 5; ~15; 30 | 5 | decided — 30 (DEC-019, user) |

The next prompt, *"Move on to the detailed design"*, is typed in the input line but not sent. The recording ends there; it is sent at the start of `demos/02-detailed-design/WI-002/ScreenA_DD.mp4`.

---

---

## Harness improvements triggered by this session

None. Nothing in this step exposed an ambiguity or gap in `ai/` that needed changing, so no improvement RFC came out of it.

Across the Screen A series, two steps did produce one: the **detailed design** (RFC 0001 — always produce all four DD documents) and the **implementation** (RFC 0002 — keep every plan revision, oldest first). Both are recorded in `ai/improvements/`.

## Files produced or changed in this session

| File | Action |
|---|---|
| `docs/en/database/0002-production-order-schema.md` | Created (216 lines), then extended with the runtime-login design and grant list |
| `work-items/WI-002/decisions.md` | DEC-013–DEC-015 added as decided (technical), then DEC-016–DEC-020 added as decided |
| `work-items/WI-002/brief.md` | REQ-019 added (Cancel confirmation) |
| `docs/en/010_basic-design/BD-001-production-order-create-edit.md` | Revision 3: confirm dialog (items 17–19), events E-07/E-09, open questions closed |
| `work-items/WI-002/plan.md` | Review status → approved; implementation scope extended with the Compose changes |
| `work-items/WI-002/status.md` | Next milestone → DD-001; risk row replaced with DEC-016 |
| `work-items/WI-002/evidence.md` | Traceability rows gained their DB-002 references; next action → DD-001 |
| `docs/en/architecture/0002-auth-rbac-foundation.md`, `docs/en/database/0001-identity-schema.md` | CSRF "revisit later" note replaced by a reference to DEC-020 |
| `docs/en/database/README.md` | Index line now lists DB-002 |

---

## Key points for the presentation

1. **The approval gate works in both directions.** The previous step stopped and waited; this step opens by recording the approval in `plan.md` before any design work starts. The gate is a written state, not a chat memory.
2. **The agent reads the code, not just the docs.** Before writing the schema it inspected `AppDbContext`, the naming convention and the existing migrations, so DB-002 matches what is actually built.
3. **Technical and business decisions are kept apart, again.** Three schema choices (counter table, `xmin`, `varchar` + CHECK) were made by Claude and recorded as DEC-013–DEC-015 with options and rationale. The four that change what a user sees were escalated.
4. **The agent corrects itself.** Twice — the `order_number` operational note and the `ALTER DEFAULT PRIVILEGES` paragraph — Claude rewrote its own sentence without being asked.
5. **Human in the loop catches a gap, again.** "Are there any open questions left?" surfaced three items Claude had silently decided. Being asked to re-check produced a better list than the summary did.
6. **The user overrides the recommendation.** On DEC-016 Claude recommended deferring the database-login split; the user chose "Split now", and Claude recorded that choice and immediately reported what it costs (four changes to WI-001's Compose setup, needing a new plan revision).
7. **A free-text answer is a first-class answer.** "Let's do 30 samples so we can get a sense of the screen's workload" became DEC-019 and a concrete `P-1001`–`P-1030` seed in DB-002.
8. **One answer can create a requirement.** The Cancel decision (DEC-018) did not just update a document — it added REQ-019 to the brief and a dialog with focus and Escape behaviour to BD-001.
9. **Least privilege is enforced by the harness, not remembered.** `ai/rules/database.md` is what made the single-owner login a visible open question in the first place.
10. **Time.** About 4.5 minutes of agent work plus about 2 minutes of user input produced a 216-line database design, eight decisions, a new requirement, a BD revision and an updated work item.

## Gaps in this transcript

- **Collapsed tool output.** Claude Code folds the raw output of read-only commands into a one-line summary ("Read 1 file, ran 1 shell command"). Where that happens, the output recorded here is what Claude reported from them; the raw text was never on screen.
- **The top of the turn 1 summary was never displayed.** Re-sampling that second of the recording frame by frame confirms why: Claude Code paints a finished reply in a single frame with the viewport already scrolled to its end, so a reply longer than the terminal's ~13 visible lines has its opening lines above the fold from the moment it appears. The user did not scroll up, so those lines are not in the recording at all. Only the visible part is quoted.
- **The DEC-020 row** of the turn 3 summary table sits one row below the fold for the rest of the recording. Its outcome is recorded in step 11 from `work-items/WI-002/decisions.md`, not from the screen.
- **File bodies as they stream.** DB-002's body scrolled past between 01:15 and 01:52. Its content here comes from the committed file and from the editor preview the user scrolls through in step 12 — both of which show the same text.
- The committed files are the authoritative version: DB-002 and `decisions.md` were both extended again during implementation (DEC-029's index, the grants baked into the db image), after this recording.
