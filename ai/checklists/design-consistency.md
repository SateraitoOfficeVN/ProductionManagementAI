# Design consistency

- Requirements have stable IDs and acceptance criteria.
- BD covers navigation, primary actions and exceptions.
- BD's Mermaid screen transition shows exactly the entries and exits in its screen list table, and every numbered item in a BD's SVG wireframe appears in its legend table (RFC 0009).
- DD field/validation/state behavior agrees with BD.
- DD's Mermaid state diagram agrees with its state-transition table, and every numbered item in its SVG wireframes appears in its region table or the BD legend (RFC 0009).
- API and DB mappings agree, including constraints and errors.
- Missing decisions are resolved before dependent implementation.
- Relevant test scenarios map to the design.
- Security-relevant fields (auth, PII, secrets) are identified before implementation depends on them.
- Accessibility requirements (WCAG 2.2 AA) are captured for new or changed screens.
- Migration impact (additive vs. destructive, recovery limits) is described before dependent code assumes the new schema.
- What gets traced/logged (OpenTelemetry spans, metrics) for a new endpoint or job is specified, not left to be decided during implementation.
- Every document added or changed under `docs/en/` has been re-rendered to `docs/en/pdf/` and `docs/ja/pdf/`, and both PDFs' footers name its current revision (RFC 0008).
