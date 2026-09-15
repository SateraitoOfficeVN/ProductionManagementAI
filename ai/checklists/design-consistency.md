# Design consistency

- Requirements have stable IDs and acceptance criteria.
- BD covers navigation, primary actions and exceptions.
- DD field/validation/state behavior agrees with BD.
- API and DB mappings agree, including constraints and errors.
- Missing decisions are resolved before dependent implementation.
- Relevant test scenarios map to the design.
- Security-relevant fields (auth, PII, secrets) are identified before implementation depends on them.
- Accessibility requirements (WCAG 2.2 AA) are captured for new or changed screens.
- Migration impact (additive vs. destructive, recovery limits) is described before dependent code assumes the new schema.
- What gets traced/logged (OpenTelemetry spans, metrics) for a new endpoint or job is specified, not left to be decided during implementation.
