import AxeBuilder from '@axe-core/playwright'
import { expect, type Locator, type Page } from '@playwright/test'

export function adminPassword(): string {
  const password = process.env.E2E_ADMIN_PASSWORD
  if (!password) {
    throw new Error('Set E2E_ADMIN_PASSWORD to the stack SEED_ADMIN_PASSWORD before running E2E tests.')
  }
  return password
}

export async function signIn(page: Page) {
  await page.goto('/login')
  await page.getByLabel('Username').fill('admin')
  await page.getByLabel('Password').fill(adminPassword())
  await page.getByRole('button', { name: 'Sign in' }).click()
  // Login lands on the dashboard (WI-004 DEC-005). Not a navbar link: on SP those sit behind the Menu button.
  await expect(page.getByRole('heading', { level: 1, name: 'Dashboard' })).toBeVisible()
}

/**
 * The Product field by its stable id: a <select> while Draft, a read-only input once locked. Not getByLabel('Product'):
 * label matching is case-insensitive and partial, so while the page is loading it also matches the skeleton's
 * "Loading production order" label (a flaky failure seen in CI).
 */
export function productField(page: Page): Locator {
  return page.locator('#productId')
}

/** Opens the create screen and waits until the form has finished loading (the Save button only exists then). */
export async function openCreateForm(page: Page) {
  await page.goto('/production-orders/new')
  await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
}

/** A due date safely in the future regardless of the browser's or the plant's timezone. */
export function futureDate(daysAhead = 30): string {
  const d = new Date(Date.now() + daysAhead * 86_400_000)
  return d.toISOString().slice(0, 10)
}

export async function createOrder(page: Page, opts: { product?: string; quantity?: string; notes?: string } = {}) {
  await openCreateForm(page)
  await productField(page).selectOption({ label: opts.product ?? 'P-1004 — Drive shaft' })
  await page.getByLabel('Quantity').fill(opts.quantity ?? '250')
  await page.getByLabel('Due date').fill(futureDate())
  if (opts.notes) {
    await page.getByLabel('Notes').fill(opts.notes)
  }
  await page.getByRole('button', { name: 'Save' }).click()
  const status = page.getByRole('status')
  await expect(status).toHaveText(/^Production order PO-\d{4}-\d{5} created\.$/)
  const orderNumber = (await status.textContent())!.match(/PO-\d{4}-\d{5}/)![0]
  return { orderNumber, url: page.url() }
}

/** WCAG 2.2 AA rule set (ai/rules/frontend.md, DEC-026). */
export async function expectNoAxeViolations(page: Page) {
  const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa']).analyze()
  expect(results.violations, JSON.stringify(results.violations.map((v) => v.id))).toEqual([])
}
