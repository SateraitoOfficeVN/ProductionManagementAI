import { defineConfig, devices } from '@playwright/test'

// Runs against the local Docker Compose stack (deploy/README.md); it doesn't start the stack itself.
// E2E_ADMIN_PASSWORD must be the stack's SEED_ADMIN_PASSWORD (never committed).
export default defineConfig({
  testDir: './specs',
  fullyParallel: false, // journeys share one database; keep them ordered and deterministic
  workers: 1,
  retries: 0, // a flaky journey is quarantined with a recorded reason, not silently retried (ai/rules/testing.md)
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:3000',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] }, testIgnore: /mobile\.spec\.ts/ },
    { name: 'mobile', use: { ...devices['Pixel 7'] }, testMatch: /mobile\.spec\.ts/ },
  ],
})
