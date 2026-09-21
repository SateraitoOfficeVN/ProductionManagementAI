# Transcript: Screen A basic design (WI-002)

Source video: `demos/01-basic-design/WI-002/ScreenA_BD.mp4`. It runs 9 min 59 s, is a 1920×1034 screen recording with no audio, and was made on 2026-09-18 between about 10:53 and 11:03 local time.

Setup shown: JetBrains Rider with the `ProductionManagementAI` solution on branch `master`. Claude Code v2.1.275 (Opus 5) runs in the IDE terminal in a session named **Screen A**, with **auto mode on**. The editor above the terminal shows the generated Markdown files in preview as they are written.

How it was made: the transcript was built from frames sampled every 10 seconds, plus denser frames around the prompts and the question dialog. Wording in quotes is copied from the screen. Claude Code collapses the raw output of read-only commands (for example "Listed 3 directories, ran 3 shell commands"). For those steps, the output recorded here is what Claude concluded and reported from them. The user's prompts are quoted with their spelling and grammar lightly corrected; the recordings show them exactly as they were typed. The decision IDs were checked against `work-items/WI-002/decisions.md`.

---

## Edited video for the presentation

`demos/01-basic-design/WI-002/ScreenA_BD_edited.mp4` is a 3 min 49 s cut, rebuilt to match the pacing of the other three steps:

- **Jump cuts, not speed-ups.** 16 clips. Long stretches of agent work show their start, then jump to the result. Every jump inside a step is marked with a "» jumped ahead" badge.
- **Paced for reading.** The text-heavy frames — the two turn summaries, the decision log, BD-001 — are held 16 to 22 seconds each.
- **Step captions.** A "Step N of 12" card in the empty AI Assistant panel on the right matches the numbering below. The accent is amber where the human acts and teal where the agent works.
- **Hidden text.** Five passages are painted over and, where the sentence still carries meaning, redrawn without the omitted part: the session setup commands (`/clear`, `/model`), two sentences in step 2, the revision note in `brief.md`, the opening paragraph of `decisions.md`, and one line of the turn 2 summary. Each of them named WI-002's earlier design pass, which the presentation material leaves out.

| Step | Starts at (edited video) | Caption |
|---|---|---|
| 1 | 00:00 | Prompt 1: start Screen A |
| 2 | 00:14 | Claude reads the harness |
| 3 | 00:37 | Output: brief.md |
| 4 | 00:51 | Output: decisions.md |
| 5 | 01:07 | Output: BD-001 |
| 6 | 01:25 | Output: plan, status, evidence |
| 7 | 01:41 | Turn 1 summary + question DEC-009 |
| 8 | 02:03 | Prompt 2: are there still open decisions? |
| 9 | 02:15 | Decision form: four questions |
| 10 | 02:48 | Recording the answers |
| 11 | 03:16 | Turn 2 summary |
| 12 | 03:38 | Review: BD-001 |

`demos/01-basic-design/WI-002/ScreenA_BD_presentation.pdf` is the 16-slide deck built from this transcript; each step slide carries the edited-video timestamp above.

The times in the rest of this transcript refer to the original recording.

---

## At a glance

| # | Time | Who | Step | Output |
|---|---|---|---|---|
| 1 | 00:10 | User | Prompt 1: "OK, now let's work on Screen A as per what we planned" | — |
| 2 | 00:20–01:30 | Claude | Reads the harness, the template, the example BD workbook and the existing IDs | The plan of work (brief first, then BD) and the conventions and inputs for BD-001 |
| 3 | 01:40–02:20 | Claude | Writes the brief | `work-items/WI-002/brief.md` (70 lines, revision 1) |
| 4 | 02:20–02:40 | Claude | Writes the decision log | `work-items/WI-002/decisions.md` (261 lines, DEC-001–DEC-008 decided, DEC-009 proposed) |
| 5 | 02:40–04:30 | Claude | Writes the basic design | `docs/en/010_basic-design/BD-001-production-order-create-edit.md` (339 lines, v1) |
| 6 | ~04:30–04:59 | Claude | Writes the plan, status and evidence files and updates the indexes | `plan.md` (revision 1), `status.md`, `evidence.md`, two README index lines, `CLAUDE.md` "Current state" |
| 7 | 04:59 | Claude | Summarizes turn 1 | Summary plus one business question (DEC-009) and a request for plan review |
| 8 | 06:10 | User | Prompt 2: "There are still open decisions to be made, right?" | — |
| 9 | 06:40–07:20 | Claude ↔ User | Four-question decision form | Four answers: due-date check, concurrency, timezone, order-number format |
| 10 | 07:20–08:57 | Claude | Records the answers | DEC-009–DEC-012 decided; brief and BD-001 (v2) updated; plan, status and evidence updated; status now Green |
| 11 | 08:57 | Claude | Summarizes turn 2 | Summary, the technical choices left for DB-002 and DD-001, and a request for plan approval |
| 12 | 09:00–09:59 | User | Reviews `plan.md` and `status.md` in the editor | — (the recording ends while plan approval is pending) |

Total agent working time was 4 m 39 s for turn 1 and 1 m 37 s for turn 2. The user typed only two short prompts and answered four multiple-choice questions.

---

## Detailed timeline

### Step 1 · 00:10 — Prompt 1 (user)

> **OK, now let's work on Screen A as per what we planned**

This is the whole instruction. It names no files, templates or steps. Claude has to work those out from the repository harness (`AGENTS.md`, `ai/`).

### Step 2 · 00:20–01:30 — Claude orients itself (read-only)

Claude runs a series of read-only commands:

| # | Action shown on screen | Command (abridged) | Output |
|---|---|---|---|
| 2a | "Reading agent instructions and listing work items" | `cat AGENTS.md && ls work-items work-items/* demos demos/*/WI-002 … && cat ai/project.md` | Project state and entry points, and the confirmation that Screen A is WI-002 (production-order create/edit). |
| 2b | "Reading demo recordings, workflows, and policies" | `cat ai/workflows/feature-delivery.md ai/skills/README.md ai/skills/requirements/SKILL.md ai/skills/basic-design/SKILL.md; ls -R docs; ls ai/templates ai/templates/example …` | The workflow (feature delivery), the skills to use (requirements, then basic design) and the docs layout. Claude reported that it would start Screen A from the requirements brief and basic design, and read the BD template next so BD-001 follows it. |
| 2c | "Reading BD template, rules and checklist" | `cat ai/templates/basic-design.md ai/templates/README.md ai/rules/documentation.md ai/rules/frontend.md ai/checklists/design-consistency.md` | The section structure BD-001 must follow, the documentation and frontend rules, and the design-consistency checklist. |
| 2d | "Finding existing REQ-001/SCR-001 usages" | `grep` for `REQ-`, `FN-`, `SCR-` and `UC-` IDs across `work-items`, `docs` and `ai`; `grep` for DEC-015 in WI-001's decisions; `grep` for `Role`/`Authorize` in the backend | The ID numbering already in use (the new requirements start at REQ-010) and the existing roles (`Admin`/`Operator`, WI-001 DEC-015). |
| 2e | "Peek at example BD workbook item/validation/event sheets" | A Python/openpyxl script over the reference Excel BD in `ai/templates/example/BD` (run twice; the second run added `PYTHONIOENCODING=utf-8` to handle the Japanese sheet text) | The conventions for the item, validation and event tables. Claude reported: *"Still gathering inputs — checking the example BD workbook's validation/event conventions before writing the brief and BD."* |

**Output of this step:**

> "Inputs gathered. Writing the WI-002 work item (brief, decisions, plan, status, evidence) and BD-001 against the template now, …"

The business answers the user had already agreed for Screen A (who may use it, the fields, the status workflow, the validation rules and so on) went straight into the work item, so the user wasn't asked them again.

### Step 3 · 01:40–02:20 — Brief written

Action: `Write(work-items/WI-002/brief.md)`, followed by one `Update` that removed a redundant out-of-scope line (it duplicated the per-status editability rule that BD-001 covers).

**Output: `work-items/WI-002/brief.md`** (70 lines, draft revision 1):

- Header: `WI-002 | Claude (for ThanhTN) | draft — revision 1 | unscheduled (demo MVP)`
- **Overview:** Screen A lets a signed-in Admin or Operator create a new production order or edit an existing one, including moving it through its status workflow.
- **Objective:** show the full AI development lifecycle (requirements → BD → DB → DD → code → test → PR) on one small but realistic screen.
- **Success metrics:** full traceability (every REQ maps to a BD section, a DD section, a DB/API element and at least one test); correct business rules (acceptance criteria pass as unit, integration and E2E tests); demo usability (a reviewer can create and edit an order end to end in local Docker Compose).
- **Assumptions:** users arrive already authenticated through WI-001's cookie login. `Admin` and `Operator` are the only roles. Products are seeded reference data. There is no list screen yet. The site is single-site and single-timezone.
- **Requirements:** REQ-010 to REQ-018 (only partly visible in the video).
- **Not doing (out of scope):** Screen B list/search and Screen C dashboard; a product-catalog UI; deleting orders (Cancelled is the only way to retire one); audit history; finer-grained permissions.
- **Open questions:** "Does the 'due date ≥ today' rule apply to an existing order whose saved due date is already in the past…?" Owner: user. Status: open (DEC-009).

### Step 4 · 02:20–02:40 — Decision log written

Action: *"Now the decision log."* followed by `Write(work-items/WI-002/decisions.md)`.

**Output: `work-items/WI-002/decisions.md`** (261 lines). It records the business answers the user had already agreed (DEC-001 to DEC-008) and one new open question (DEC-009). Technical decisions such as order-number generation and CSRF are left to the later design steps.

| ID | Decision needed | Maker | Status | Outcome |
|---|---|---|---|---|
| DEC-001 | Who may create/edit a production order | user | decided | Both `Admin` and `Operator` |
| DEC-002 | Core form fields | user | decided | Order number (generated), product, quantity, due date, status, notes |
| DEC-003 | Status workflow | user | decided | Draft → InProgress → Completed; Cancelled reachable from Draft or InProgress |
| DEC-004 | Edit behavior once an order is not Draft | user | decided | Lock product and quantity once the order is not Draft |
| DEC-005 | How "product" is represented | user | decided | Minimal Product table, no catalog UI |
| DEC-006 | Validation rules | user | decided | Generated unique order number, quantity positive integer, due date ≥ today, notes ≤ 500 chars |
| DEC-007 | Locked-field change submitted with other valid changes | user | decided | Reject the whole request |
| DEC-008 | Status-transition UI control | user | decided | Restricted dropdown with only valid next states |
| DEC-009 | Due-date rule for an existing order already past its due date | *unresolved* | proposed | Proposal: check only on create or when the due date is changed |

### Step 5 · 02:40–04:30 — Basic design written

Action: *"Now BD-001 itself."* followed by `Write(docs/en/010_basic-design/BD-001-production-order-create-edit.md)`.

**Output: BD-001** (339 lines, version 1, based on `ai/templates/basic-design.md`). The sections seen in the editor preview:

- **Document control (改版履歴):** BD-001, category UI, system ProductionManagementAI, subsystem Production orders, WI-002, based on brief revision 1, created by Claude (for ThanhTN), 2026-09-18, version 1.
- **System overview:** a table mapping each requirement to the sections that cover it (REQ-010 create, REQ-011 edit, REQ-012 auth, REQ-013 quantity, REQ-014 due date, …).
- **Overall configuration and architecture:** React (Vite + TS + Tailwind v4) calling the .NET 10 JSON API, layered per ADR-0001, PostgreSQL 17, access gated by the WI-001 cookie session and roles (ADR-0002). No new architecture boundary.
- **Function list:** FN-001 Create, FN-002 Edit, FN-003 Load, FN-004 List products, FN-005 Validate input, FN-006 Enforce status transitions, FN-007 Enforce field lock by status, FN-008 Enforce authentication/authorization.
- **Status diagram:** `Draft → InProgress → Completed`, with a branch to `Cancelled` from both; Completed and Cancelled are terminal.
- **Screen design detail: SCR-001:** routes `/production-orders/new` and `/production-orders/{id}`; API `/api/production-orders` and `/api/products`; responsive PC (≥ 640px) and SP (< 640px); authentication required; roles Admin and Operator; page titles for create and edit mode.
- **ASCII mockups** for PC and SP with numbered items (1)–(16): header, back link, title, message banner, order number, status dropdown, product, quantity, due date, notes with a 0/500 counter, timestamps, and Save/Cancel.
- **Validation table** (V-01 onwards) and **item events** E-01 to E-06: screen load, field blur, notes input counter, save click, and the save success and failure responses.
- **Non-functional:** accessibility (WCAG 2.2 AA, `aria-required`, `aria-describedby`, live-region banner, focus moves to the first invalid field), observability, performance.
- **Open questions and linked DD:** DEC-009 due-date rule; concurrent edits; which timezone defines "today"; order-number format; CSRF for the new endpoints.

### Step 6 · ~04:30–04:59 — Work-item files and indexes

**Output:**

- `work-items/WI-002/plan.md`: revision 1, awaiting review, with six milestones (see step 12).
- `work-items/WI-002/status.md` and `work-items/WI-002/evidence.md`: created.
- `docs/en/000_requirements/README.md`: "No system artifacts have been produced yet" became "Screen A (WI-002) keeps its brief in `work-items/WI-002/brief.md`".
- `docs/en/010_basic-design/README.md`: now lists "Documents: BD-001 (Screen A, WI-002)".
- `CLAUDE.md`: the "Current state" paragraph now says WI-002 is in progress with a brief, decision log, plan revision 1 (design phase only, awaiting review) and BD-001. On screen the edit showed "Allowed by auto mode classifier".

### Step 7 · 04:59 — Turn 1 summary (Claude)

**Output:** the reply below. Its top part scrolled off before it was captured; the visible part says:

> - Products come from a small seeded table, with no product screen.
> - Standard validation rules.
> - If a request changes a locked field, the whole request is rejected.
> - The status dropdown only offers the allowed next states.
>
> Technical choices (order-number format, CSRF protection) will be decided in the database and detailed design steps.
>
> **One new business question (DEC-009):** the rule "due date must be today or later", applied on every save, would make an overdue In Progress order impossible to save, even just to mark it Completed. Options:
> 1. **Check only when creating an order or when the due date is changed** (recommended; BD-001 is currently written this way)
> 2. Check on every save
> 3. Check only when creating an order
>
> **Next:** plan revision 1 needs your review. Once you answer DEC-009, I'll finalize that rule and move on to the database design (DB-002), then the detailed design (DD-001) with a rendered mockup. Publishing the mockup will need your go-ahead when we get there.

*Sautéed for 4m 39s · done 10:58 AM*

Claude Code pre-filled a suggested reply in grey: *"Go with option 1, then move on to database design"*. The user did **not** accept it.

### Step 8 · 06:10 — Prompt 2 (user)

> **There are still open decisions to be made, right?**

The user noticed that BD-001's "Open questions" table listed more open items than the one question Claude had asked. They asked Claude to raise all of them now instead of deferring them.

### Step 9 · 06:40–07:20 — Decision form (AskUserQuestion)

Action: Claude opened a tabbed multiple-choice form with four tabs (Due date · Concurrency · Today · Order no.) and a Submit step. Each question offered a recommended option, a "Type something" free-text option, and "Chat about this".

**Tab 1: Due date (DEC-009).** *When should the "due date must be today or later" rule be checked?*

1. **On create or when changed (Recommended)**: overdue orders can still be completed, cancelled or annotated, but nobody can newly enter a past date. ← **chosen**
2. On every save: the simplest rule, but an overdue order can't be saved (even to mark it Completed) until its due date is moved forward.
3. On create only: the simplest for editing, but it lets someone move an existing due date into the past.

**Tab 2: Concurrency.** *If two users edit the same order at the same time, what should happen when the second one saves?*

1. **Reject stale save (Recommended)**: the second save fails with "This order was changed by someone else — reload to see the latest." Nothing is overwritten silently. ← **chosen**
2. Last write wins: the second save overwrites the first without warning. Simpler, but changes can be lost silently.

**Tab 3: Today.** *Which timezone defines "today" for the due-date rule?*

1. **Server's plant timezone (Recommended)**: one configured timezone for the site, applied by the server, so everyone gets the same answer. ← **chosen**
2. User's browser timezone: follows each user's local clock. The server would need the client's date, which can differ between users.

**Tab 4: Order no.** *Order number format?*

1. PO-000001 (Recommended): a "PO-" prefix plus a 6-digit zero-padded sequence number that never resets.
2. **PO-YYYY-00001**: a year prefix with a sequence that restarts each year. ← **chosen (the user overrode the recommendation)**
3. Plain number: a bare sequence number (1, 2, 3…).

**Output:** the answers as Claude Code recorded them:

> User answered Claude's questions:
> · DEC-009: When should the 'due date must be today or later' rule be checked? → On create or when changed (Recommended)
> · If two users edit the same order at the same time, what should happen when the second one saves? → Reject stale save (Recommended)
> · Which timezone defines 'today' for the due-date rule? → Server's plant timezone (Recommended)
> · Order number format? → PO-YYYY-00001

### Step 10 · 07:20–08:57 — Claude records the answers

Action: *"Recording the four answers now across the decision log, brief, BD-001 and status."* One shell command failed. Claude said: *"The shell heredoc breaks on apostrophes here, so I'll save the script to a file and run it from there."* It recovered without the user's help.

**Output:**

- `decisions.md`: four decisions moved to *decided* (wording as in the committed file):

  | ID | Decision | Outcome |
  |---|---|---|
  | DEC-009 | Due-date rule for an existing order already past its due date | Check only on create or when the due date value is changed |
  | DEC-010 | Concurrent edits of the same order | Reject a save based on stale data; user reloads |
  | DEC-011 | Timezone that defines "today" | One configured plant timezone, applied server-side |
  | DEC-012 | Order number format | `PO-YYYY-NNNNN`, sequence restarts each year |

- `brief.md`: requirements REQ-010, REQ-011 and REQ-014 updated.
- BD-001 **version 2**: validation rule V-08 added (the stale-save check); the coverage row for REQ-011 now includes "§5 V-08"; the mockup order numbers use the new `PO-YYYY-NNNNN` format.
- `status.md`: status set to **Green**, with these milestones logged:
  - "Brief revision 1 (REQ-010–REQ-018) and decision log (DEC-001–DEC-009)"
  - "DEC-009–DEC-012 answered by the user; BD-001 revision 2 and brief updated"
  - "BD-001 drafted against the BD template"
  - Risk: "Plan revision 1 not yet reviewed" (owner ThanhTN). Next action: "Review plan revision 1 so DB-002 can start."
- `plan.md` and `evidence.md`: updated to match.

### Step 11 · 08:57 — Turn 2 summary (Claude)

**Output:**

> I updated `decisions.md`, `brief.md` (requirements REQ-010, REQ-011 and REQ-014), and BD-001, which is now version 2 with V-08 added and the mockup numbers switched to the new format. `status.md`, `evidence.md` and `plan.md` are updated too, and the status is now Green.
>
> A few technical choices remain for the next design steps. They don't change how the screen behaves:
> - **Database design (DB-002):**
>   - How to generate the number sequence that restarts each year.
>   - What happens if a year goes past 99,999 orders.
>   - How the system detects that someone else changed the order.
> - **Detailed design (DD-001):** whether the new save endpoints need protection against cross-site request forgery (CSRF).
>
> Plan revision 1 still needs your review. Once you approve it, I'll start DB-002.

*Worked for 1m 37s · done 11:02 AM*

Suggested next prompt (grey placeholder, not sent in the video): *"Plan approved; let's work on the database design next"*.

### Step 12 · 09:00–09:59 — User reviews the output

The user scrolls through `plan.md` and `status.md` in the editor preview. `plan.md` shows **Inputs and assumptions** and **Deliverables and milestones**:

| # | Milestone | Depends on | Skill | Deliverable | Verification |
|---|---|---|---|---|---|
| 1 | Requirements brief + decision log | none | requirements | `brief.md`, `decisions.md` | Every REQ has success and failure criteria |
| 2 | Basic design | 1 | basic-design, screen-design | BD-001 | Every REQ mapped to a BD section; design-consistency checklist |
| 3 | Resolve DEC-009 | 2 | — | `decisions.md`, BD-001 V-04 | User answer recorded |
| 4 | Database design | 2 | database-design | DB-002 schema doc | Constraints and indexes justified; migration impact stated |
| 5 | Detailed design + rendered mockup | 3, 4 | detailed-design, screen-design | DD-001 + mockup | DD agrees with BD and DB; test viewpoints cover every REQ |
| 6 | Reconcile and close design phase | 5 | — | `status.md`, `evidence.md` | Design-consistency checklist passes; no open business decision |

The recording ends here, with plan approval still pending.

---

---

## Harness improvements triggered by this session

None. Nothing in this step exposed an ambiguity or gap in `ai/` that needed changing, so no improvement RFC came out of it.

Across the Screen A series, two steps did produce one: the **detailed design** (RFC 0001 — always produce all four DD documents) and the **implementation** (RFC 0002 — keep every plan revision, oldest first). Both are recorded in `ai/improvements/`.

## Files produced or changed in this session

| File | Action |
|---|---|
| `work-items/WI-002/brief.md` | Created (70 lines), then updated after the decisions |
| `work-items/WI-002/decisions.md` | Created (261 lines, DEC-001–DEC-009), then updated (DEC-009–DEC-012 decided) |
| `work-items/WI-002/plan.md` | Created (revision 1, awaiting review), then updated |
| `work-items/WI-002/status.md` | Created, then set to Green |
| `work-items/WI-002/evidence.md` | Created, then updated |
| `docs/en/010_basic-design/BD-001-production-order-create-edit.md` | Created (339 lines, v1), then updated to v2 (V-08 added, new order-number format) |
| `docs/en/000_requirements/README.md`, `docs/en/010_basic-design/README.md` | Index lines now point to the brief and BD-001 |
| `CLAUDE.md` | "Current state" paragraph updated to describe WI-002's progress |

---

## Key points for the presentation

1. **Short prompts, structured harness.** "OK, now let's work on Screen A as per what we planned" was enough. `AGENTS.md` → `ai/workflows` → `ai/skills` → `ai/templates` → `ai/checklists` told Claude what to read and produce.
2. **The agent reads before it writes.** It spent about 1.5 minutes only reading: instructions, the workflow, skills, the template, the example Excel BD, the existing IDs and the backend roles.
3. **Agreed answers aren't asked again.** Business answers the user had already given (DEC-001 to DEC-008) went straight into the decision log.
4. **The human decides business questions.** Claude recommended options, but only the user decided. In one case the user overrode the recommendation (order-number format `PO-YYYY-00001`), and Claude recorded that choice.
5. **Human in the loop catches a gap.** The user noticed that open questions were still sitting in BD-001 and asked for all of them to be raised. Claude batched four decisions into one form.
6. **Business and technical decisions are kept apart.** Items that don't change what the user sees (sequence generation, overflow, concurrency detection, CSRF) are deferred to DB and DD design, and the summary says so explicitly.
7. **Every decision is written down.** Each answer lands in `decisions.md`, the brief, BD-001, `status.md` and `evidence.md`, not just in the chat.
8. **Plan approval gate.** Claude stops and waits for the plan revision to be approved before it starts the database design (`ai/policies.md`).
9. **The agent recovers from its own errors.** A failed heredoc was fixed without the user's help.
10. **Time.** About 6 minutes of agent work plus about 1 minute of user input produced a brief, a decision log, a plan, a status file, an evidence file and a 339-line basic design document.

## Gaps in this transcript

- **Collapsed tool output.** Claude Code folds the raw output of read-only commands into a one-line summary ("Listed 3 directories, ran 3 shell commands"). Where that happens, the output recorded here is what Claude reported from them; the raw text was never on screen.
- **The top of the turn 1 summary was never displayed.** Claude Code paints a finished reply in a single frame with the viewport already at its end, so a reply longer than the terminal's visible height has its opening lines above the fold from the moment it appears. The user did not scroll up, so those lines are not in the recording — sampling the recording frame by frame confirms it. Only the visible part is quoted.
- **The requirement rows** REQ-013 to REQ-018 were never fully on screen here, but the later recordings show them in `evidence.md`'s traceability matrix, and the committed brief gives the same set:

  | ID | Requirement |
  |---|---|
  | REQ-010 | Create order |
  | REQ-011 | Edit order |
  | REQ-012 | Admin/Operator only |
  | REQ-013 | Quantity positive integer |
  | REQ-014 | Due date ≥ today |
  | REQ-015 | Product exists |
  | REQ-016 | Notes ≤ 500 |
  | REQ-017 | Status state machine |
  | REQ-018 | Product/quantity lock |

  (REQ-019, the Cancel confirmation, is added later by DEC-018 in the database-design step.)
- BD-001's full validation table was never on screen. The committed file in `docs/en/010_basic-design/` is the authoritative text.
