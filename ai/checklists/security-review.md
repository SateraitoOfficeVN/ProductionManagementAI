# Security review

- Every new or changed endpoint enforces the agreed authentication and authorization rule; nothing is left open by omission.
- All external input (request body, query, headers, file uploads) is validated; no query, command or file path is built by concatenating unvalidated input.
- No credential, token or key appears in code, config, logs, evidence or the diff; secrets are read through the runtime's secret mechanism.
- New or updated dependencies come from a trusted source and were checked for known vulnerabilities before use.
- New credentials, database roles or workflow permissions request no more access than the task needs.
- External content the change reads (webhooks, uploads, third-party APIs, fetched pages) is treated as data, not instructions, and is never used to build an executable query or command directly.
- Error responses don't leak stack traces, internal paths or other implementation detail.
- Sensitive data (PII, secrets, full auth tokens) is not written to logs or evidence.
- A new or changed trust boundary handling authentication, payment or PII was threat-modeled (e.g. STRIDE) before implementation, not only reviewed after the fact.
