# e2e

Playwright user journeys (DEC-025, WI-002) with `@axe-core/playwright` accessibility scans (DEC-026). A standalone npm package that drives the browser only; it doesn't import app code.

Runs against a running Docker Compose stack; it doesn't start one. See `deploy/README.md` for bringing the stack up (fresh `db` volume, owner migration, then `up`).

```
cd tests/e2e
npm ci
npx playwright install chromium
E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD from deploy/.env> E2E_BASE_URL=http://localhost:3000 npx playwright test
```

Projects: `desktop` (Desktop Chrome) and `mobile` (Pixel 7, SP layout). Journeys create their own data; they need only the seeded admin user and products. CI runs the same suite in the `e2e` job of `.github/workflows/ci.yml` (RFC 0003): throwaway credentials, the Compose stack built on the runner, owner migrations, then these journeys.
