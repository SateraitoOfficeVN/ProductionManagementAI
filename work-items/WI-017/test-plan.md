# WI-017 — Product type — Test Plan

## Test plan identifier

TP-017, work item WI-017, revision 1, 2026-10-25.

## References

[brief](brief.md) revision 1; 004_BD-TYPE, 004_DB-TYPE, 004_DD-TYPE, 004_DD-API-TYPE and 004_DD-SPD-TYPE v1 (all `_20261025`, approved DEC-007–DEC-011).

## Introduction

Covers the required product type 「種別」: database column and migration, the Product master API, and the SCR-004 list,
filter and form. Cases come from the "Test viewpoints" sections of 004_DD-TYPE, 004_DD-API-TYPE and 004_DD-SPD-TYPE.

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-090 | Required type from a fixed list; existing rows backfilled |
| REQ-091 | Required 「種別」 select on register/edit |
| REQ-092 | 「種別」 column (PC) and card line (SP) |
| REQ-093 | Type filter on the list, kept in the URL |

## Features not to be tested

- Order screens, dashboard and order CSV: unchanged by design (DEC-003); covered by their existing suites only.
- Screen-reader speech and physical mobile keyboard/IME: Not run (same local limits as earlier WIs).

## Approach

| Level | Included? | Rationale |
| --- | --- | --- |
| Unit (backend) | yes | Code table and list query parsing |
| Integration (backend, Testcontainers) | yes | API contract, filtering, grants, CHECK, migration on populated data |
| Unit (frontend, Vitest) | yes | URL state, rendering, validation, focus, dirty guard, axe |
| E2E (Playwright, local Compose) | yes | Real journeys PC and SP, axe |

Backend commands ran inside the `mcr.microsoft.com/dotnet/sdk:10.0` container because no .NET SDK is installed on this
machine (evidence.md).

## Item pass/fail criteria

Every case below passes, and all pre-existing suites stay green.

## Cases

| TC | REQ | Level | Case | Test |
| --- | --- | --- | --- | --- |
| TC-470 | 090 | Unit | Each code round-trips; unknown/wrong-case/blank codes rejected; display order | `ProductTypeTests` |
| TC-471 | 093 | Unit | List `type`: absent/empty = all; known applied; unknown reported with other query errors | `ProductTypeTests` |
| TC-472 | 090, 091 | Integration | Create each code → 201 and read back | `ProductTypeEndpointTests.EachTypeIsCreatedAndReadBack` |
| TC-473 | 090, 091 | Integration | Missing / null / wrong-case / numeric `type` → 400 `errors.type` | `MissingNullOrUnknownTypeIsAFieldError` |
| TC-474 | 091 | Integration | Type change on a retired, unit-locked product → 200, new version | `TypeChangesOnRetiredUnitLockedProductAndBumpsVersion` |
| TC-475 | 091 | Integration | Field validation before stale-version check | `InvalidTypeIsReportedBeforeStaleVersion` |
| TC-476 | 093 | Integration | Filter with `q`, `state`, `pageSize`; total counts filtered rows | `ListFiltersByTypeWithSearchStateAndPageSize` |
| TC-477 | 093 | Integration | Unknown list `type` → 400 | `UnknownListTypeIsRejected` |
| TC-478 | 091 | Integration | Retire body with `type` → 400 | `RetireRejectsATypeProperty` |
| TC-479 | 090 | Integration | `pmai_app` has column UPDATE grant; CHECK rejects unknown codes | `AppRoleMayUpdateTypeButTheDatabaseRejectsUnknownCodes` |
| TC-480 | 090 | Integration | Migration on populated pre-WI-017 DB: 13/5/12/0 mapping, other rows FINISHED, no default left | `ProductTypeMigrationTests` |
| TC-481 | 092, 093 | Frontend unit | Column after 製品名, card line, filter options, axe | `ProductMasterPage.test.tsx` |
| TC-482 | 093 | Frontend unit | Filter applies only on 検索 | same |
| TC-483 | 093 | Frontend unit | `type` kept through 表示件数; クリア removes it | same |
| TC-484 | 093 | Frontend unit | Unknown or repeated `type` dropped without a request | same |
| TC-485 | 091 | Frontend unit | Create without type → MSG-E026, focus, error summary, no POST | same |
| TC-486 | 091, 093 | Frontend unit | Type editable when unit locked; save away from filter shows hidden hint | same |
| TC-487 | 091 | Frontend unit | Changing only the type triggers the discard question | same |
| TC-488 | 091, 092 | E2E | Create requires type, then list shows it; axe | `product-type.spec.ts` |
| TC-489 | 093 | E2E | Filter 部品: URL, count, paging, edit + return with hidden hint, クリア; axe | `product-type.spec.ts` |
| TC-490 | 093 | E2E | Unknown URL type dropped | `product-type.spec.ts` |
| TC-491 | 092 | E2E (SP 390 px) | Card 「種別：半製品」, filter fits, no horizontal scroll; axe | `product-type.mobile.spec.ts` |

Regression: existing integration and E2E fixtures that create products now send `type`; the WI-010 calendar migration
test stops at its own migration because WI-017 legitimately changes `products` rows.

## Results

Recorded in [evidence.md](evidence.md).
