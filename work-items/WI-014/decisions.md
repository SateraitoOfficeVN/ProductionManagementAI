# WI-014 — Decisions

| ID | Date | Decision | Source |
| --- | --- | --- | --- |
| DEC-001 | 2026-10-06 | Separate bug-fix WI for the order-list overflow found by WI-013 CI; WI-013 changed Product master only. | User: "error in order list will be fix-bug in a new WI", "create new WI to fix order list overflow" |
| DEC-002 | 2026-10-06 | Plan revision 1 approved; fix is CSS sizing only (zero-minimum grid tracks, `w-full min-w-0` select). The closed select truncates long names; the open list shows them in full. | User: "approved, go ahead" |
| DEC-003 | 2026-10-06 | Plan revision 2 approved: keep the native `<select>` (002_DD) and use `appearance: base-select` so the open list is as wide as the control, long names are truncated with an ellipsis, and the select and each option carry the full `SKU — name` as `title`. Browsers without `base-select` keep the native popup. | User: "approved, go ahead" |
| DEC-004 | 2026-10-06 | The closed control truncates the chosen label only through an author `<button><selectedcontent>` inside the select; the browser default content cannot end in an ellipsis (prototype, Chromium 153). The button is `aria-hidden` with `tabIndex={-1}`: the combobox keeps its own name, options and keyboard handling (verified in the accessibility tree). React 19.3 has no `<selectedcontent>` type, so `src/selectedcontent.d.ts` declares it; jsdom logs an unknown-tag warning in unit tests only. | Plan revision 2 step 2; implementation finding |
| DEC-005 | 2026-10-06 | Squash-merge PR #43 (repository convention) and delete the feature branch; closeout records go in a separate documentation-only PR, as for WI-012 and WI-013. | User: "merge PR #43 and clean up branch", "yes, create closeout PR for WI-014" |
