# Release readiness

- Target-environment configuration (env vars, secrets, connection strings) is confirmed present and correct for this release.
- Migration plan matches database-design.md's migration impact and recovery limits; a destructive step runs only after its compatible/expand phase is already live.
- A rollback path is defined and confirmed executable, not just described.
- Deployment is authorized per policies.md's External operations section; the authorization source is recorded.
- Smoke-test plan and pass/fail criteria are defined before the deploy, not improvised after.
- Monitoring, alerting or a manual check is in place to detect a failed release quickly.
- The image/version being deployed is recorded and traceable to the reviewed commit or PR.
- If this release causes an incident, bug-fix.md's incident steps (mitigate, then blameless postmortem) apply.
