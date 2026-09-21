# Transcript: Screen A detailed design (WI-002)

Source video: `demos/02-detailed-design/WI-002/ScreenA_DD.mp4`. It runs 15 min 37 s, is a 1920×1034 screen recording with no audio, and was made on 2026-09-18 between about 11:14 and 11:30 local time. It picks up directly where `demos/03-database-design/WI-002/ScreenA_DB.mp4` ends — the database design was recorded first, because that is the order the work ran in.

Setup shown: JetBrains Rider with the `ProductionManagementAI` solution on branch `master`. Claude Code v2.1.275 (Opus 5) runs in the IDE terminal in a session named **Screen A**, with **auto mode on**. The editor above the terminal shows the generated Markdown files in preview as they are written or opened. Partway through, the user switches to a browser to look at the published mockup.

How it was made: the transcript was built from frames sampled every second across the whole recording, keeping the frames where the terminal changed, plus denser frames around the prompts and the question form. Wording in quotes is copied from the screen. Claude Code collapses the raw output of read-only commands (for example "Read 3 files, listed 1 directory, ran 3 shell commands"). For those steps, the output recorded here is what Claude concluded and reported from them. The user's prompts are quoted with their spelling and grammar lightly corrected; the recordings show them exactly as they were typed. Document names, line counts and decision IDs were checked against `docs/en/020_detailed-design/` and `work-items/WI-002/decisions.md`.

---

## Edited video for the presentation

`demos/02-detailed-design/WI-002/ScreenA_DD_edited.mp4` is a 4 min 00 s cut to show alongside the slides:

- **Paced for reading.** The text-heavy frames — the authorization question, the two turn summaries, the memory being written — are held 14 to 18 seconds each.
- **Jump cuts, not speed-ups.** Long stretches of agent work show their start, then jump to the result. Every jump inside a step is marked with a "» jumped ahead" badge.
- **Step captions.** A "Step N of 12" card in the empty AI Assistant panel on the right matches the numbering below. The accent is amber where the human acts and teal where the agent works.

| Step | Starts at (edited video) | Caption |
|---|---|---|
| 1 | 00:00 | Prompt 1: DB design reviewed, start the DD |
| 2 | 00:12 | Claude reads first |
| 3 | 00:32 | Authorization: may the mockup be published? |
| 4 | 00:50 | Output: DD-001 |
| 5 | 01:03 | Output: DD-001-API |
| 6 | 01:16 | Loads artifact-design, reuses the app's palette |
| 7 | 01:38 | Mockup published |
| 8 | 01:51 | The published mockup, then the turn 1 summary |
| 9 | 02:23 | Review: DD-001 |
| 10 | 02:37 | Prompt 2: two DD templates produced no document |
| 11 | 02:49 | Saves the preference, then writes DD-001-FN and DD-001-SPD |
| 12 | 03:47 | Review: the four DD documents |

`demos/02-detailed-design/WI-002/ScreenA_DD_presentation.pdf` is the 16-slide deck built from this transcript; each step slide carries the edited-video timestamp above, and slide 14 covers the harness improvement on its own.

The times in the rest of this transcript refer to the original recording.

---

## At a glance

| # | Time | Who | Step | Output |
|---|---|---|---|---|
| 1 | 00:00–00:36 | User | Reviews `plan.md`, then prompt 1: "I reviewed the DB design and it looks okay, so let's move on to detailed design for this work item" | — |
| 2 | 00:36–01:47 | Claude | Reads the detailed-design and screen-design skills, the DD template, the backend rules, ADR-0001 and the actual frontend code | The conventions DD-001 must follow, and what the running app already looks like |
| 3 | 01:47–01:53 | Claude ↔ User | Asks for authorization before writing: may the mockup be published as a private artifact? | "Yes, publish privately" — the plan's `ask at execution` gate cleared up front |
| 4 | 01:55–04:11 | Claude | Writes the detailed design | `docs/en/020_detailed-design/DD-001-production-order-create-edit.md` (609 lines at this point) |
| 5 | ~04:57 | Claude | Writes the API companion | `DD-001-API-production-orders.md` (292 lines) |
| 6 | 05:43–06:44 | Claude | Loads the artifact-design skill, then states the mockup's visual approach | The mockup will reuse the app's existing light-only Tailwind palette, not invent one |
| 7 | ~07:30–07:37 | Claude | Builds and publishes the mockup | `mockups/DD-001-screen-a-mockup.html` (292 lines) + a private artifact with 11 static states |
| 8 | 07:40–08:33 | Claude | Cross-links and updates the tracking files, then summarizes turn 1 — while the user opens the published mockup in a browser | DEC-021–DEC-024 decided, BD-001 revision 4, status/evidence/docs index updated |
| 9 | 08:35–10:20 | User | Reviews DD-001 in the editor and opens the DD template folder | — |
| 10 | 10:20 | User | Prompt 2: "There are still 2 templates not output to an .md file: function-design and screen-processing-design" | — |
| 11 | 10:22–13:39 | Claude | Saves the preference, writes the two missing companions, rewires DD-001 | A memory, `DD-001-FN` (325 lines), `DD-001-SPD` (212 lines), DD-001 rewritten to point at them |
| 12 | 13:40–15:37 | User | Reviews the four detailed-design documents | — (the recording ends with the next prompt typed but not sent) |

Total agent working time was 7 m 27 s for turn 1 and 2 m 56 s for turn 2 — 10 m 23 s in all. The user typed two prompts and answered one question.

---

## Detailed timeline

### Step 1 · 00:00–00:36 — Prompt 1 (user)

The session opens with `work-items/WI-002/plan.md` in the editor. The user scrolls its **Authorization** table —

| Action | Authorization | State | Step |
|---|---|---|---|
| Publish rendered mockup as a private Artifact | ask at execution | not yet authorized | step 5 |
| git commit / push / PR / merge | no | not yet authorized | — |

— then its **Risks and mitigations** table ("Missing business decision → pause and ask per `ai/policies.md`; record in `decisions.md`") and its **Approval / sign-off** block, which records revision 1 as approved on 2026-09-18.

Claude Code has pre-filled a suggested reply, *"Move on to the detailed design"*. The user clears it and writes their own, which says what they did before saying what they want:

> **I reviewed the DB design and it looks okay, so let's move on to detailed design for this work item**

### Step 2 · 00:36–01:47 — Claude orients itself (read-only)

| # | Action shown on screen | Command (abridged) | Output |
|---|---|---|---|
| 2a | "Reading DD skill, screen-design skill, backend rules and DD template" | `cat` the detailed-design and screen-design skills, `ai/rules/backend.md`, `ai/templates/DD/*` | The four DD templates and what each is for, and the bar the documents must meet. |
| 2b | Same step | `cat src/frontend/src/features/auth/LoginPage.tsx …/AuthContext.tsx …/ProtectedRoute.tsx src/frontend/src/index.css; cat src/frontend/package.json` | The app's real visual language and routing — later used to decide how the mockup should look. |
| 2c | "Reading ADR-0001 and project references" | `ls tests/backend tests/integration tests/e2e tests/frontend; find tests -name "*.cs" …; find src/frontend -name "*.test.*" …` | The layered backend contract and the test projects that already exist. |

Claude Code collapses these to *"Read 3 files, listed 1 directory, ran 3 shell commands"*.

**Output of this step:** Claude does not start writing. It stops on a permission question instead:

> "Before writing: plan step 5 says publishing the rendered mockup as a private artifact needs your OK at that point, so I'm asking now so it doesn't hold things up later."

### Step 3 · 01:49–01:53 — Authorization question (AskUserQuestion)

Action: a one-question form, tab **Mockup**.

> **DD-001 includes a rendered mockup of Screen A covering create, edit, locked-fields, validation-error, success and discard-dialog states. Can I publish it as a private claude.ai artifact (only you can see it unless you share it)?**
>
> 1. **Yes, publish privately (Recommended)** — Publish the mockup as a private artifact and link it from DD-001. You decide later whether to share the link. ← **chosen**
> 2. Local HTML file only — Save the mockup as an HTML file in the repo (`docs/en/020_detailed-design/`) and don't publish it anywhere.
> 3. *Type something*

**Output:**

> User answered Claude's questions:
> · DD-001 includes a rendered mockup of Screen A covering create, edit, locked-fields, validation-error, success and discard-dialog states. Can I publish it as a private claude.ai artifact (only you can see it unless you share it)? → Yes, publish privately (Recommended)

The whole exchange takes about four seconds. The gate was written into `plan.md` when plan revision 1 was drafted; Claude read it, honoured it, and cleared it before doing any work that depended on it.

### Step 4 · 01:55–04:11 — Detailed design written

Claude states the plan for the document before writing it:

> "Writing DD-001 now, covering modules, states, processing flow, persistence, exception handling, observability, and test viewpoints, with the API contract split into a companion document since Screen B will reuse `/api/products`."

Action: `Write(docs/en/020_detailed-design/DD-001-production-order-create-edit.md)`.

**Output: DD-001**, 609 lines at this point (485 in the committed file, after step 11 moves content out into the companions). The head of the document:

- Title: "Production Order Create/Edit — Detailed Design Document (詳細設計書)".
- "DD-001 — implements BD-001 (SCR-001), requirements REQ-010–REQ-019."
- Document control (改版履歴), then modules, screen states, processing flow, persistence, exception handling, observability and test viewpoints.

Sections the user scrolls through later include a **Rule / validator** table mapping `ProductionOrder.ValidateQuantity` / `ValidateNotes` and `ProductionOrder.Update` to the BD's V-numbers; a **Condition / data references** table (`IProductionOrderRepository.ProductExistsAsync` for V-01, `IPlantClock.Today` for V-04, request-vs-entity `xmin` for V-08); and a **Check parameters** table typing each field (`productId` `Guid?`, `quantity` `int?`, `dueDate` `DateOnly?`, `notes` `string?`).

### Step 5 · ~04:11–04:57 — API companion written

Action: `Write(docs/en/020_detailed-design/DD-001-API-production-orders.md)`.

**Output: DD-001-API**, 292 lines — "Production Orders API — API Specification Design (API仕様設計)", "DD-001-API — supports DD-001, requirements REQ-010–REQ-018". It carries the endpoint list, the request and response shapes, and a **Processing flow** table per endpoint, for example:

| Step | Description | Calls |
|---|---|---|
| 1 | Authorize; JSON content type | policy; `[Consumes]` |
| 2 | Shape/required/range checks | Domain validators |
| 3 | Load tracked entity → 404 | repository |
| 4 | `version` ≠ current `xmin` → 409 `MSG-E009` | — |
| 5 | `order.Update(...)` → 422 with `code` MSG-E008 or MSG-E007 | Domain |
| 6 | Changed product exists? Changed due date ≥ plant today? → 400 | repository; `IPlantClock` |
| 7 | `SaveChanges`; `DbUpdateConcurrencyException` → 409 | EF Core |
| 8 | 200 with a fresh `ProductionOrderResponse` | — |

It was split out because Screen B will reuse `/api/products`.

### Step 6 · 05:43–06:44 — The mockup's visual approach

**Output:**

> "The DD documents are written. Before building the mockup I need to load the artifact design guidance."
>
> `Skill(artifact-design)` → "Successfully loaded skill"

and then, rather than inventing a look:

> "I'll build the mockup using the app's existing light-only Tailwind gray palette from `LoginPage.tsx` — gray-900 buttons, gray-300 borders, system font — while only the spec-sheet chrome around it adapts to the viewer's theme."

This is what step 2b's reading of the frontend was for.

### Step 7 · ~06:50–07:37 — Mockup built and published

Action: `Write(docs/en/020_detailed-design/mockups/DD-001-screen-a-mockup.html)` (292 lines), then the publish.

**Output:**

> Publishing the mockup privately now.
>
> `Artifact(D:\Work\AI\WMS\docs\en\020_detailed-design\mockups\DD-001-screen-a-mockup.html)`
>   Published `https://claude.ai/artifact/FEo1RG27UjZ6vxFCUjxoHq`
>
> Published. Next: linking it from DD-001, fixing the order of BD-001's revision table, and updating status and evidence.

The artifact is titled **SCR-001 Order Form** and holds 11 static states: create empty, product list open (30 items), validation errors, created → edit (Draft), in progress (locked), completed (terminal, overdue), stale save (409), discard dialog, not found, SP create, SP locked. The HTML source stays in the repository; the artifact is the rendered copy.

### Step 8 · 07:40–08:33 — Cross-links, tracking files, turn 1 summary

While Claude finishes, the user opens the artifact link in a browser (from about 07:49). The mockup renders as a working-looking screen: "Production order PO-2026-00042", order number, status, product, quantity 300, due date 2026/10/01, notes "Priority run for the Osaka line. Use batch 7 steel." with a 51/500 counter, created and updated timestamps, and Cancel / Save. The captured states below it include the **discard dialog** — "Discard your changes? Your changes to this production order haven't been saved." with **Keep editing** and **Discard** — and **not found**, the 404 route with no form.

**Output:**

- DD-001 gains the mockup link and a table of which artifact changes require which document change.
- **BD-001 revision 4** — the quantity upper bound (DEC-024) discovered while typing the API contract, fed back into the basic design.
- `decisions.md`: four technical decisions taken during detailed design —

  | ID | Decision | Outcome |
  |---|---|---|
  | DEC-021 | Client-side "today" for the due-date check | Browser-local date for early feedback; the server's plant date is authoritative |
  | DEC-022 | Product picker for 30 products | Native `<select>` ordered by SKU; no searchable combobox yet |
  | DEC-023 | API error contract | RFC 9457; 400 validation / 404 / 409 stale / 422 rule violation; `code` = message ID |
  | DEC-024 | Quantity upper bound | 999,999,999 (9 digits, BD-001 width), MSG-E010 |

- DB-002's API field names marked as confirmed; `status.md`, `evidence.md` (traceability now reaches the detailed design, and the design checklist passes) and the docs index updated.

**Turn 1 summary** (its top scrolled off before it was captured):

> I also marked DB-002's API field names as confirmed, and updated `status.md`, `evidence.md` (traceability now reaches the detailed design, and the design checklist passes) and the docs index.
>
> The next plan revision (implementation) will need to cover things the current code doesn't have yet:
> - The split database logins from DEC-016.
> - OpenTelemetry, which isn't set up in the backend at all.
> - A choice of end-to-end test tool.
> - New handling that turns error responses into the standard Problem Details format.
>
> Please review DD-001, the API document and the mockup. Once you're happy, I'll close the design phase (step 6) and draft plan revision 2 for implementation, for your review.

*Cogitated for 7m 27s · done 11:23 AM*

`status.md` now reads: "RAG: Green — on track. Plan revision 1 is approved. The brief, BD-001 (revision 4), DB-002 (reviewed by the user), and DD-001 + DD-001-API + the rendered mockup are drafted. Every recorded decision (DEC-001–DEC-024) is resolved. Step 6 (final reconciliation) and the user's DD review remain."

### Step 9 · 08:35–10:20 — User reviews the output

Back in the editor, the user first types a prompt accepting the work — *"The design looks okay, let's move on to the implementation"* — but does not send it. Instead they read DD-001: the **Rule / validator** table, the **Condition / data references** table and the **Check parameters** table. Then they expand `ai/templates/DD/` in the project tree, where `api-design.md`, `function-design.md` and `screen-processing-design.md` sit side by side — and delete the prompt they had typed.

### Step 10 · 10:20 — Prompt 2 (user)

> **There are still 2 templates not output to an .md file: function-design and screen-processing-design**

Having seen the template folder, the user notices that only two of the four DD templates produced a document.

### Step 11 · 10:22–13:39 — Claude fills the gap and changes its own future behaviour

**Output:**

> "Understood. I left those two out as optional, but you want the full template family produced. First I'll save that preference so later design steps include all four."
>
> Searching for 1 pattern, writing 1 memory…
>   Remembering in the WMS project, detailed design must output every DD template (main + api-design, function-design, screen-processing-design) as its own `.md` file

Then the two documents:

- **`DD-001-FN-production-order-service.md`** (325 lines) — the function design: per-function field tables, return types and notes, including the frontend components (`ProductionOrderPage`, the route component holding page state `loading | ready | notFound | forbidden | loadError`; `ProductionOrderForm`, the controlled form that keeps `initialValues` and computes `isDirty = !equal(values, initialValues)`).
- **`DD-001-SPD-production-order-create-edit.md`** (212 lines) — the screen-processing design, including the exception-handling table: authentication (cookie) → 401; authorization (policy) → 403; route match `{id:guid}` → 404; content type on POST/PUT → 415; model binding → 400 with message IDs; result mapping `Ok → 200`, `Invalid → 400`, `NotFound → 404`, `Conflict → 409`, `RuleViolation → 422`; unhandled exception → 500 `MSG-E013` with no exception detail. Its **Unresolved decisions** section reads "None."

Then the rewiring:

> "Now updating DD-001 so it points to the two companions instead of repeating their content, plus the API document's cross-references and the tracking files."

**Turn 2 summary:**

> - **API document:** its processing steps now point to the matching sections in the two new documents.
> - `docs/en/020_detailed-design/README.md`, `status.md` and the traceability matrix in `evidence.md` now include the new files.
>
> I saved this as a preference in memory: for this project, detailed design always produces all four template documents.
>
> The project's shared templates and the detailed-design skill still call the three companions optional. If you want that to be the rule for everyone, I can propose the change to those files. **Changes to shared templates need your review before they take effect.**
>
> Next, as before: once you've reviewed the four detailed-design documents and the mockup, I'll close the design phase and draft plan revision 2 for implementation.

*Worked for 2m 56s · done 11:28 AM*

Claude Code pre-fills a suggested next prompt in grey: *"Update the templates and skill so all DD companions are always produced"*. That proposal became harness RFC `ai/improvements/0001-dd-companions-always-produced.md`.

### Step 12 · 13:40–15:37 — User reviews the four documents

`docs/en/020_detailed-design/` now holds `DD-001-production-order-create-edit.md`, `DD-001-API-production-orders.md`, `DD-001-FN-production-order-service.md`, `DD-001-SPD-production-order-create-edit.md`, `mockups/` and `README.md`. The user reads each in the editor preview, ending on DD-001-SPD's exception-handling table and its "Unresolved decisions: None. Related decisions: DEC-007, DEC-008, DEC-009, DEC-018, DEC-020, DEC-021, DEC-023."

`status.md` now records the full set: "the full DD set (DD-001, DD-001-API, DD-001-FN, DD-001-SPD) plus the rendered mockup are drafted."

The suggested prompt about updating the templates is still sitting in the input line, unsent. The recording ends there.

---

---

## Harness improvement triggered by this session

This is the one step in the Screen A series where using the harness exposed a defect **in the harness itself**, and the session changed the harness rather than just working around it. The sequence is worth showing on its own, because it is three distinct scopes of fix in the right order:

| # | Scope | What happened | Where it landed |
|---|---|---|---|
| 1 | **This work item** | The two missing documents were written | `DD-001-FN-production-order-service.md`, `DD-001-SPD-production-order-create-edit.md` |
| 2 | **This agent, from now on** | Claude wrote the preference to memory *before* producing the documents, so later design steps would not repeat the omission | "in the WMS project, detailed design must output every DD template (main + api-design, function-design, screen-processing-design) as its own `.md` file" |
| 3 | **The whole team, after review** | Claude offered, but did not make, the change to the shared templates and skill — "Changes to shared templates need your review before they take effect" | harness RFC [`ai/improvements/0001-dd-companions-always-produced.md`](../../../ai/improvements/0001-dd-companions-always-produced.md) |

The RFC records the root cause in the harness rather than in the agent: the templates described the three companions as optional ("Use it when…", "None — this screen's design fits entirely in this file"), so folding their content into the main DD was a permitted reading. It was adopted and changed `ai/templates/detailed-design.md`, the three `ai/templates/DD/*` templates, `ai/templates/README.md`, `ai/skills/detailed-design/SKILL.md` and `ai/evaluations/baseline-cases.md` — the last so the case is checked in future evaluations.

**Why it matters for the presentation:** the harness is not fixed documentation. When a run reveals that a rule is ambiguous, the run produces a change proposal against that rule, with the evidence attached — and the boundary between "what I will do" and "what everyone will do" stays with the human.

## Files produced or changed in this session

| File | Action |
|---|---|
| `docs/en/020_detailed-design/DD-001-production-order-create-edit.md` | Created (609 lines), then rewritten to delegate to the companions (485 lines committed) |
| `docs/en/020_detailed-design/DD-001-API-production-orders.md` | Created (292 lines), then cross-referenced to the companions |
| `docs/en/020_detailed-design/DD-001-FN-production-order-service.md` | Created (325 lines) in turn 2 |
| `docs/en/020_detailed-design/DD-001-SPD-production-order-create-edit.md` | Created (212 lines) in turn 2 |
| `docs/en/020_detailed-design/mockups/DD-001-screen-a-mockup.html` | Created (292 lines), published as a private artifact |
| `docs/en/010_basic-design/BD-001-production-order-create-edit.md` | Revision 4 — quantity upper bound (DEC-024) |
| `work-items/WI-002/decisions.md` | DEC-021–DEC-024 added as decided |
| `work-items/WI-002/status.md`, `evidence.md` | Traceability now reaches the detailed design; design checklist recorded as passing |
| `docs/en/020_detailed-design/README.md`, `docs/en/database/0002-production-order-schema.md` | Index updated; DB-002's API field names marked confirmed |

---

## Key points for the presentation

1. **A written gate is honoured without being restated.** `plan.md` said publishing the mockup needs authorization "at execution". Claude read that, and asked *before* starting work rather than stopping halfway through.
2. **The agent reads the running app before designing for it.** It opened `LoginPage.tsx` and `index.css` in step 2 and used them in step 6 to decide the mockup's palette — so the mockup looks like the product, not like a generic wireframe.
3. **Design feeds back upstream.** Writing the API contract surfaced a missing quantity upper bound, which became DEC-024 and BD-001 revision 4 rather than a silent choice in the code.
4. **Technical decisions are recorded, not absorbed.** Four (DEC-021–DEC-024) were taken during this step, each with options and rationale, and each named in the summary.
5. **The human catches a gap the agent treated as optional.** Two of the four DD templates had produced no document. The user spotted it from the template folder and said so in one line.
6. **A correction changes future behaviour, not just this output.** Claude's first act was to write the preference to memory so later design steps produce all four — then it produced the missing two.
7. **Shared rules need human approval.** Claude explicitly separated "I changed how I will behave" from "changing the project's shared templates needs your review", and offered the latter as a proposal. That proposal became harness RFC 0001.
8. **Documents delegate instead of duplicating.** Once the companions existed, DD-001 was rewritten to point at them — the design set stays consistent rather than repeating itself in four places.
9. **The deliverable is reviewable by a non-developer.** The published mockup shows 11 real states, including the discard dialog and the 404, so the screen can be judged before any code exists.
10. **Time.** About 10.5 minutes of agent work and two prompts produced four design documents (1,314 lines), a rendered mockup, four decisions and a revision of the basic design.

## Gaps in this transcript

- **Collapsed tool output.** Claude Code folds the raw output of read-only commands into a one-line summary ("Read 3 files, listed 1 directory, ran 3 shell commands"). Where that happens, the output recorded here is what Claude reported from them; the raw text was never on screen.
- **The top of the turn 1 summary was never displayed.** Claude Code paints a finished reply in a single frame with the viewport already at its end, so a reply longer than the terminal's visible height has its opening lines above the fold from the moment it appears. The user did not scroll up, so those lines are not in the recording — sampling the recording frame by frame confirms it. Only the visible part is quoted.
- **File bodies as they stream.** The bodies of DD-001, DD-001-API, DD-001-FN and DD-001-SPD scrolled past as they were written. Their content here comes from the committed files and from the editor previews the user scrolls through in steps 9 and 12 — both of which show the same text.
- The committed files are the authoritative version: DD-001 and DD-001-FN were both revised again during implementation (the `AllowedNext()` extension, values returned via `RETURNING`, the instrumentation set), after this recording.
