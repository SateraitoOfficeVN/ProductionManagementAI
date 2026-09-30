# Product master — Screen Processing Design (画面処理設計書)

`004_DD-SPD` specifies client processing for SCR-004 under the approved WI-006 plan revision 3. It elaborates [004_DD](004_DD_製品マスタ.md) version 2 and [004_BD](../../010_basic-design/004/004_BD_製品マスタ.md) version 2, using the approved [004_DD-API](004_DD-API_製品マスタ.md) and [004_DD-FN](004_DD-FN_製品マスタ.md) version 2 contracts. It covers REQ-049–REQ-052, REQ-054–REQ-055 and REQ-057. The detailed SCR-001–SCR-003 client changes belong to the later new WI-006 DD addendum. No client code has changed in this design phase.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | 004_DD-SPD |
| System / screen | ProductionManagementAI / SCR-004 「製品マスタ」 (Product master) |
| Work item | WI-006 |
| Created / updated | Codex, 2026-09-29 / 2026-09-30 |
| Version | 2 — reconciled list/form/dialog flows, unit lock, errors and accessible focus |

| Version | Change |
| --- | --- |
| 1 | Initial Product master screen-flow draft under plan revision 1 |
| 2 | Align with approved DD/API/FN, distinguish uncertain write outcomes, and defer affected-screen flows to their own addendum |

## Process and component catalog

| Process | Owning client component | Trigger | API / result |
| --- | --- | --- | --- |
| SPD-01 | `ProductMasterPage` | Open `/products`, apply filters, change page, retry | API-PM-01 → list, empty, error or forbidden state |
| SPD-02 | `ProductFormPage` | Open `/products/new` or `/products/:id/edit` | API-PM-02 for edit → empty or populated form, including `unitLocked` |
| SPD-03 | `ProductFormPage` | Validate and submit create/edit | API-PM-03 / API-PM-04 → success, field errors or conflict |
| SPD-04 | `RetireProductDialog` | Open, cancel or confirm retire | API-PM-05 → retained/retired row or conflict |
| SPD-05 | Shared protected route and dirty guard | Navigate, lose session or encounter permission denial | `/login`, forbidden state, or confirmed leave |

All UI text comes from the Japanese catalog planned for implementation; quoted text below is the proposed UI wording and must be implemented exactly once approved. The English gloss appears only in design. The PC [wireframe](wireframes/004_DD_SCR-004-pc.svg), [English-captioned mockup](mockups/004_DD_screen-product-master-mockup.html) and [Japanese-captioned mockup](mockups/004_DD_screen-product-master-mockup.ja.html) visualize the states. This SPD defines behavior, not new layout or backend transaction rules.

## Shared client state and request handling

| State | Owner and lifetime | Rule |
| --- | --- | --- |
| Applied `q`, `state`, `page` | URL while list route is active | Canonical source of list state; default `state=all`, `page=1`. Draft filter text is local until Apply. |
| Return URL | Route navigation state for create/edit | Internal `/products` URL only; validate it before use and fall back to `/products`. Do not accept an external redirect target. |
| List request generation / abort controller | `ProductMasterPage` effect | Abort previous GET on URL change/unmount and ignore any response whose generation is no longer current. |
| Form values, field errors, loaded `version`, `unitLocked` | `ProductFormPage` until save/leave | Keep raw text for SKU/name/drawing number and the selected unit. `unitLocked` comes from detail response, never from retirement state. |
| Dirty baseline | `ProductFormPage` until save/leave | Compare normalized current editable fields with loaded/empty baseline; changing a field then restoring it clears dirty state. |
| In-flight mutation | Form or dialog until settled | One submission at a time. Abort on unmount only to stop client work; an aborted request may already have committed, so never infer rollback. |

The shared API client maps only known Problem Details `code` and `errors` keys. Unknown 4xx/5xx responses use a generic Japanese failure notice without exposing server text or showing success. 401 follows the existing login redirect; 403 displays a forbidden state. Product fields and the session cookie are not put in browser telemetry or URLs, except the user-entered list search `q` required by the approved URL design; it is not used as a metric label.

## SPD-01 — ProductMasterPage list

| Step | Trigger / branch | Processing and call | Visible/focus result |
| --- | --- | --- | --- |
| 1 | Enter list or browser Back/Forward | Parse `q`, `state`, `page` from `/products`; trim `q`, bound it to 100 characters, accept only `all`/`active`/`retired` and page ≥1. Canonicalize invalid or repeated query keys with `replace` to safe defaults before a request. | URL, controls and result refer to the same applied values; no fetch for an invalid URL. |
| 2 | Canonical URL changes | Set loading state, start API-PM-01 with `AbortController` and generation ID; cancel/ignore superseded responses. | Keep heading and a non-noisy loading status; no stale rows presented as current. |
| 3 | 200 with items | Render stable SKU/ID order, exact total, page controls, active/retired text and actions. PC uses `<table>` with caption and column headers; SP uses labelled cards with the same fields. | Move focus to result heading after explicit Apply/Page; initial page load uses normal route-heading focus. Do not steal focus on background refresh. |
| 4 | 200 with no items | Distinguish an empty catalog from no match/page past end using `total`, filter state and page. If page is beyond the last valid page, replace URL with the last valid page and fetch once. | Show 「製品がありません。」 (No products.) or 「条件に一致する製品はありません。」 (No matching products.) with Clear/valid-page action. Add stays available. |
| 5 | Apply/Clear/page | Apply trims `q`, sets `state`, resets page to 1; Clear removes `q`/state and resets page; page buttons preserve filters. | URL is updated before fetch. Status text is polite; no auto-submit on focus or typing. |
| 6 | GET failure / 401 / 403 | Network/5xx shows Retry for same URL; 401 redirects; 403 shows forbidden. A late failed request is ignored if superseded. | Retry remains keyboard reachable; 403 performs no write. |

Only `Admin`/`Operator` see Product master actions; API authorization is the final guard. A retired row has Edit but no Retire. Repeated Edit/Retire controls need product-specific accessible names while visible button text can remain 「編集」 (Edit) and 「使用停止」 (Retire). The list contains all products, including retired ones; it does not call the order picker API.

## SPD-02 — ProductFormPage open, unit lock and leave

| Step | Trigger / branch | Processing and call | Visible/focus result |
| --- | --- | --- | --- |
| 1 | `/products/new` | Initialize empty SKU/name/unit/drawing number, no version and `unitLocked=false`; no GET. | Focus the page heading, then normal tab order through labelled form. |
| 2 | `/products/:id/edit` | Validate UUID route; call API-PM-02. On 200 retain `sku`, `name`, `unit`, `drawingNumber`, `isActive`, `version`, `unitLocked` as the baseline. | SKU is read-only text. If `unitLocked=true`, show unit as noneditable text plus 「製造指示で使用されているため、単位は変更できません。」 (The unit cannot change because production orders use this product.). Name/drawing remain editable. |
| 3 | Edit detail 404 / failure | 404 shows a not-found state with a list link. Network/5xx shows Retry without a partly populated form. 401/403 use SPD-05. | Focus heading/status after explicit retry; do not treat a missing product as a blank new form. |
| 4 | User edits / restores values | Update local state and dirty comparison. A retired product shows 「使用停止」 (Retired) and remains editable; retirement alone does not lock unit. A referenced active or retired product has its unit locked. | Read-only unit is described by visible Japanese help; its value stays in state for PUT. No disabled `<select>` silently drops the value. |
| 5 | Cancel or in-app navigation with dirty draft | Use the existing shared unsaved-change confirmation. Confirm discards and follows the intended internal destination; Cancel stays on form and restores focus. Clean form leaves immediately. | Back/Forward and navbar use the same route guard. Browser unload follows the existing app guard; no claim that a client abort cancels a committed write. |

`unitLocked` is a read-time hint. If an order starts using the product after the detail GET, FN-029 rejects a changed unit; SPD-03 handles the conflict and reloads only at the user's choice. The form never infers unit eligibility from `isActive` or an old order count. The page title uses the Japanese create/edit heading and ProductionManagementAI, with focus on the route heading after navigation.

## SPD-03 — Validate and save ProductFormPage

| Step | Trigger / branch | Processing and call | Visible/focus result |
| --- | --- | --- | --- |
| 1 | Blur or change | Validate a field after blur; while typing, clear a resolved error without announcing new errors every keystroke. Trim only at validation/submit so users can edit naturally. | Errors are absent on first render. A shown error sets `aria-invalid` and links text through `aria-describedby`. |
| 2 | Submit | Normalize SKU/name/drawing; validate required fields, 50/200/100 character limits and seven allowed units. SKU is sent unchanged on edit; locked unit comes from loaded state. | Invalid form retains all values, shows summary and field errors, focuses summary or first invalid field. Save remains available to expose errors. |
| 3 | Valid create/edit | Mark mutation pending; send one JSON request to API-PM-03 or API-PM-04. PUT includes loaded `version`; no state, `unitLocked`, ID or timestamps in body. | Disable duplicate Save only while pending; Cancel stays available but warns that a pending write may already commit. |
| 4 | 201/200 | Treat returned item/version as authoritative; clear dirty flag, navigate to validated return URL and refetch list. | Polite Japanese success notice. If current filters hide the row, show Clear rather than falsely claiming it is visible. |
| 5 | 400 `VALIDATION` | Map `errors.sku`, `.name`, `.unit`, `.drawingNumber` to fields; unknown keys go to summary. Keep draft. | Linked inline Japanese errors and focused summary/first invalid control. |
| 6 | 409 `PRODUCT_SKU_CONFLICT` | Create only: retain draft and mark SKU. | Focus SKU; user corrects it and may resubmit. |
| 7 | 409 `PRODUCT_STALE` or `PRODUCT_UNIT_LOCKED` | Retain draft. Explain stale version or newly locked unit; offer explicit Reload of API-PM-02. Do not auto-merge or retry PUT. | For unit lock, associate notice with unit as well as summary; focus notice. Reload replaces values only after confirming loss of unsaved changes. |
| 8 | 401 / 403 / uncertain failure | 401 uses existing login route; 403 shows forbidden and retains draft in memory until leave. On timeout/network/5xx, do not assume the write failed. Offer a read of the current product or catalog search before any user-initiated new submission. | No automatic retry or false success. Unknown response text is not rendered. |

The server remains authoritative for SKU uniqueness, version, unit lock and fields. A backend normalization or `xmin` change is reflected from the response after success. On an uncertain create outcome, checking the catalog for the submitted SKU distinguishes a possible committed create from a safe fresh attempt; the user still decides whether to submit again. On an uncertain edit outcome, reload detail/version before a new attempt. This follows the approved FN no-automatic-retry rule.

## SPD-04 — RetireProductDialog

| Step | Trigger / branch | Processing and call | Visible/focus result |
| --- | --- | --- | --- |
| 1 | Active row Retire | Capture triggering button and row ID/version; open native `<dialog>` with `showModal()`, product name/SKU in description, and Cancel plus confirm buttons. | Browser modal behavior makes outside content inert; focus Cancel first. Accessible name comes from the visible dialog heading. |
| 2 | Escape/platform close or Cancel | Close without API call. Native modal accepts close requests; no light-dismiss is required. | Restore focus to the invoking row button if still present. |
| 3 | Confirm | Submit `{ "version": loadedVersion }` once to API-PM-05; prevent duplicate confirmation while pending. | Keep modal status understandable; Cancel may close, but a request already sent may still commit. |
| 4 | 200 | Close and refetch current list URL or update from returned item; `isActive=false` is authoritative. | Announce 「製品を使用停止にしました。」 (Product retired.) politely and focus status/updated row. |
| 5 | 409 `PRODUCT_STALE` / 400 already retired | Close, refresh row/list and explain current state; do not replay the retire request. | If the old trigger disappeared, focus the list heading or status. Do not claim this user's request succeeded. |
| 6 | 401 / 403 / timeout/network/5xx | Use login/forbidden handling for auth. For uncertain failure, close dialog and read latest row state before allowing another confirmation. | Preserve visible error/status and no false success; no automatic retry. |

Native modal focus containment avoids a custom focus trap. Escape remains usable in browsers without `<dialog closedby>` support; Cancel is always visible, including on phones. Dialog focus restoration is explicit for unmount or row replacement. The dialog has visible action text and does not convey retirement by color alone.

## SPD-05 — Protected route, focus and failure mapping

| Event | Client processing | Result |
| --- | --- | --- |
| 401 from any read/write | Follow existing session-expired handling; stop local request state | `/login`; no request replay after sign-in. |
| 403 from any read/write | Stop pending action and show forbidden state | Do not infer rights from hidden controls; no alternate write path. |
| Route changes | Update document title, focus new page heading after user navigation and preserve only a validated internal return URL | Screen-reader users hear the new screen; no open redirect. |
| Save/list/retire success | Use one polite status announcer; avoid duplicate announcements with focus move | A clear Japanese text status and stable focus target. |
| Form validation or unit-lock conflict | Linked field text, summary and focus; avoid announcing each keystroke | Error is perceivable without color; correction does not silently erase other errors. |
| Loading / repeated query changes | Show visible status without noisy live announcements for every intermediate request | Only the latest completed request updates results. |

WCAG 2.2 AA verification must include keyboard-only PC/SP flows, 200% zoom, visible focus, table headers/cards, dialog Escape/Cancel/focus return, linked validation errors and Japanese announcements. Use automated axe plus manual keyboard and screen-reader checks. The modern-web-guidance `accessibility` and `platform-controls-dismiss-dialog` guides were retrieved on 2026-09-30. No click prototype or application has been run during this design turn.

## UI message and test handoff

| Outcome | Proposed Japanese UI text (English gloss) | Catalog status |
| --- | --- | --- |
| Empty catalog | 「製品がありません。」 (No products.) | Add in implementation |
| No match | 「条件に一致する製品はありません。」 (No matching products.) | Matches the local mockup; add in implementation |
| Fetch failure | 「製品を読み込めませんでした。」 (Could not load products.) | Add in implementation |
| Required field | 「入力してください。」 (Enter a value.) | Add in implementation |
| Duplicate SKU | 「この製品コードは既に使用されています。」 (This product code is already used.) | Matches the local mockup; add in implementation |
| Locked unit | 「製造指示で使用されているため、単位は変更できません。」 (Unit cannot change because orders use this product.) | Matches the local mockup; add in implementation |
| Stale product | 「製品が変更されました。再読み込みしてください。」 (Product changed. Reload it.) | Add in implementation |
| Save success | 「製品を保存しました。」 (Product saved.) | Matches the local mockup; add in implementation |
| Retire success | 「製品を使用停止にしました。」 (Product retired.) | Add in implementation |

These are proposed strings, since Product master has no implemented catalog entry yet. During implementation the Japanese catalog becomes the exact source, and tests compare rendered text with it. Do not add English glosses to the UI. [WI-006 test plan](../../../../work-items/WI-006/test-plan.md) currently covers list/search, create/edit/retire, permissions and accessibility in TC-303–TC-307, TC-311–TC-312 and TC-314; add focused cases for unit-lock race, uncertain write outcome and focus recovery when the plan is revised after the last design file. Actual result: no application tests run; design-consistency remains in progress until the final DD addendum is reviewed.

Unresolved business decisions: none. The later WI-006 DD addendum owns SCR-001–SCR-003 processing and will use the same approved API/FN contracts without editing the completed work-item designs.
