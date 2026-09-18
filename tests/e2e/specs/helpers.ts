import AxeBuilder from '@axe-core/playwright'
import { expect, type Page } from '@playwright/test'

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
  await expect(page.getByRole('link', { name: 'New production order' })).toBeVisible()
}

/** A due date safely in the future regardless of the browser's or the plant's timezone. */
export function futureDate(daysAhead = 30): string {
  const d = new Date(Date.now() + daysAhead * 86_400_000)
  return d.toISOString().slice(0, 10)
}

export async function createOrder(page: Page, opts: { product?: string; quantity?: string; notes?: string } = {}) {
  await page.goto('/production-orders/new')
  await page.getByLabel('Product').selectOption({ label: opts.product ?? 'P-1004 — Drive shaft' })
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
