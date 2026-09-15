# testing rules

- Trace tests to acceptance criteria and important failure cases.
- Distinguish unit, integration, system, E2E and smoke coverage without duplicating tests for labels.
- Use isolated, resettable test data; record commands, environment and results.
- Do not choose thresholds or test frameworks silently. A test not run is not a pass.
- Keep most coverage at the unit level, less at integration, least at E2E; do not substitute slow E2E coverage for fast unit coverage of the same behavior.
- Quarantine a flaky test immediately and record why, instead of silently retrying or deleting it; fix or replace it before it blocks a gate again.
- Run tests on every commit/PR rather than on a delayed schedule, so failures are found while they are still cheap to fix.
