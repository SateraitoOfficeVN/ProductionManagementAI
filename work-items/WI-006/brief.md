# Product master — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-006 | Codex | requirements reconciled with DEC-007–DEC-010; approved plan revision 3 in design review | unscheduled |

## Overview

Product reference data consists of 30 seeded automobile parts. Users can select products on production orders, but they cannot maintain the catalog in the application. This proposed feature adds a Japanese Product master screen and write API for maintaining products while keeping existing orders readable.

## Objective

Start the Master data group requested by the user on 2026-09-29. The candidate scope comes from [Future development functions](../../FUTURE_DEVELOPMENT_FUNCTIONS.md) and its source PDF; that proposal is not, by itself, an approved business specification. Product master is the prerequisite for the later Production lines and Bill of materials work items.

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Maintain parts without database edits | Supported create, edit, and retire operations available in the application | All approved operations have successful and failure-path acceptance checks |
| Preserve production history | Existing orders retain valid product references and a readable product identity after migration and retirement | No broken reference in seeded or user-created orders |
| Control catalog changes | Unauthorized write attempts at the API | 0 accepted; `Admin` and `Operator` are the authorized write roles (DEC-001) |

## Assumptions and limits

- The application remains Japanese-only; UI identifiers and SKU values remain untranslated (WI-005 DEC-001 and DEC-005).
- Product IDs on the existing 30 seeded rows and production-order foreign keys must remain stable.
- The 30 seeded parts receive a suitable initial unit per part (user reply, 2026-09-29), with the proposed mapping in 004_DB; no blanket `個` backfill for them.
- Both `Admin` and `Operator` may maintain products; SKU is trimmed on creation, compared without regard to case for uniqueness, and immutable afterward. Retired products remain visible on old orders and unavailable for new selections; an old order may be saved with its retired product if the product ID is unchanged. Units are restricted to `個`, `本`, `枚`, `台`, `セット`, `kg`, `m`; drawing number is optional (user replies, 2026-09-29).
- No production-line, plant-calendar, or BOM behavior is included in WI-006.

## Actors and use cases

| Actor | Goal | Use case ID |
| --- | --- | --- |
| Catalog maintainer (`Admin` or `Operator`) | Find, create, edit, and retire a part without changing the database directly | UC-015 |
| Production-order editor | Choose an active part for a new order and read the part on an existing order | UC-016 |

## Requirements in scope

| ID | Draft requirement | Successful-path criterion | Failure-path criterion | Priority |
| --- | --- | --- | --- | --- |
| REQ-049 | View Product master | An authorized reader can find a product by SKU/name and see its active state and approved fields in Japanese. | No-match, loading, and API-failure states are understandable; unauthorized access is denied. | must |
| REQ-050 | Create a product | An `Admin` or `Operator` can add a SKU (trimmed at the ends), Japanese name, one of the approved units, and an optional drawing number; the new active product appears in the catalog and new-order picker. | Empty input, unknown unit, or a SKU equal to another after case-insensitive comparison is rejected without a partial row; an unauthorized write is denied. | must |
| REQ-051 | Edit a product | An authorized maintainer can save approved mutable fields while SKU stays fixed; subsequent catalog and order displays use the updated product data. | An attempted SKU change, invalid input, or conflicting concurrent update does not silently overwrite data; an unauthorized write is denied. | must |
| REQ-052 | Retire a product without deleting history | An authorized maintainer can mark a product inactive; existing orders remain readable, and a new order cannot select it. An old order can retain the retired product when its other fields are edited. | An inactive product submitted as a new or changed selection is rejected by the server; retirement does not delete historical orders. | must |
| REQ-053 | Preserve existing catalog and order references | After migration, the 30 seeded products keep their IDs and all order references remain valid. | Deployment or repeat startup does not reset user edits or create duplicate seed rows. | must |
| REQ-054 | Enforce catalog permissions | Both `Admin` and `Operator` can use Product master write actions and endpoints. | A signed-in user without either role receives a server-side denial even if they call the API directly. | must |
| REQ-055 | Keep the Japanese UI usable and accessible | Product workflows work by keyboard on desktop and phone layouts; labels, errors, focus, and status announcements are in Japanese. | Validation and API errors can be perceived without relying only on color; focus is not lost after a failed save. | must |
| REQ-056 | Keep existing order workflows correct | SCR-001–SCR-003 continue to display product identity and allow the approved product selection behavior. | An invalid or retired product ID cannot bypass server validation; existing orders do not become unreadable. | must |
| REQ-057 | Lock product unit after order reference | A product's unit cannot change once any production order references that product (DEC-010). | An attempted unit edit on a referenced product is rejected by client and server validation with an explanatory message; other mutable fields (name, drawing number) remain editable. | must |
| REQ-058 | Support measured decimal order quantities | Order quantities for products with `kg` or `m` units allow positive numbers with at most 3 decimal places (DEC-008, DEC-009). Quantities for `個`, `本`, `枚`, `台`, `セット` remain positive integers. Maximum quantity is 999,999,999. | Quantities with more than 3 decimal places, non-positive numbers, values exceeding maximum, or decimal values for discrete units are rejected by client and server validation. | must |
| REQ-059 | Display unit on order screens | SCR-001 and SCR-002 display the product's unit alongside order quantity inputs, detail displays, and table columns. | Missing or unassociated product unit does not cause render crashes or misaligned tabular data. | must |
| REQ-060 | Reconcile dashboard metrics for mixed units | Dashboard (SCR-003) aggregates quantities only within the same unit; cross-unit metrics display order counts rather than mixed-unit quantity sums (DEC-007). Top products and workload charts clearly identify units. | Incompatible unit quantities are never summed together; ambiguous unit displays are prevented. | must |

## Not doing

- Production lines, Plant calendar, Bill of materials, inventory, material requirements, or lot traceability.
- A general role-management UI or company sign-in.
- Physical deletion of products or renumbering existing product IDs.
- Application implementation in this revision; it covers requirements and design reconciliation only.

## Decisions applied

| Decision | Impact | Status |
| --- | --- | --- |
| [DEC-001](decisions.md) — catalog write roles | REQ-050–REQ-054: `Admin` and `Operator` authorized | decided |
| [DEC-002](decisions.md) — SKU comparison/format | REQ-050–REQ-051: trim, case-insensitive uniqueness, immutable after creation | decided |
| [DEC-003](decisions.md) — retirement on existing-order edit | REQ-052 and REQ-056: unchanged product may be retained on edit | decided |
| [DEC-004](decisions.md) — allowed unit values | REQ-049–REQ-051: `個`, `本`, `枚`, `台`, `セット`, `kg`, `m`; optional drawing number | decided |
| [DEC-005](decisions.md) — seed maintenance strategy | REQ-053: additive migration, unit backfill in 004_DB, preserve IDs/FKs | decided (technical) |
| [DEC-006](decisions.md) — order/retirement race prevention | REQ-052, REQ-056: product row share lock in order write transaction | decided (technical) |
| [DEC-007](decisions.md) — mixed-unit dashboard totals | REQ-060: sum quantities within same unit only; cross-unit uses order counts | decided |
| [DEC-008](decisions.md) — decimal order quantities | REQ-058: decimal allowed for `kg` and `m` | decided |
| [DEC-009](decisions.md) — quantity precision and range | REQ-058: at most 3 fractional digits for kg/m; integers for others; max 999,999,999 | decided |
| [DEC-010](decisions.md) — product unit lock | REQ-057: unit locked once any order references the product | decided |
