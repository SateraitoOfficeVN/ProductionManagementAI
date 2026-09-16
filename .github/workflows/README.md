# workflows

`ci.yml` — build + lint + test only, per `work-items/WI-001/plan.md` step 22 scope (no push/PR/merge/deploy execution automated here). Two jobs: backend (`dotnet build`/`dotnet test`, including the Testcontainers-backed integration tests) and frontend (`npm run lint`/`npm run build`/`npm test`). Reviewed manually, not run — this scaffold doesn't authorize triggering GitHub Actions execution (`ai/policies.md`).

CD is a future addition, once a target and permissions are agreed.
