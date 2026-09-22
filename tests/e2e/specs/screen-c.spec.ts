import { expect, test, type Page } from '@playwright/test'
import { expectNoAxeViolations, futureDate, openCreateForm, signIn } from './helpers'

// Screen C (SCR-003) and the shared navbar — DD-003 E-level viewpoints (TC-201, TC-202, TC-216, TC-217, TC-222,
// TC-223, TC-227). The stack carries DB-004's demo seed; figures depend on the day the seed ran, so assertions are
// structural or relative (tiles sum to the total, workload sums to Draft + In progress), never absolute.

const nav = (page: Page) => page.getByRole('navigation', { name: 'Main' })

async function tileValue(page: Page, list: string, label: string): Promise<number> {
  const tile = page.getByRole('list', { name: list }).getByRole('listitem').filter({ hasText: label }).first()
  const text = await tile.locator('span.text-2xl').innerText()
  return Number(text.replace(/,/g, ''))
}

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

test('login lands on the dashboard with every widget, healthy status and no axe violations (TC-201, TC-217)', async ({ page }) => {
  await expect(page).toHaveURL(/\/$/)
  await expect(page).toHaveTitle('Dashboard — ProductionManagementAI')
  await expect(page.getByText(/^As of \d{4}-\d{2}-\d{2} \d{2}:\d{2} \(Asia\/Tokyo\)$/)).toBeVisible()
  await expect(nav(page).getByRole('link', { name: 'Dashboard' })).toHaveAttribute('aria-current', 'page')

  for (const heading of ['Needs attention', 'Top products by open quantity', 'Open workload by due week', 'Completed per week, last 12 weeks']) {
    await expect(page.getByRole('heading', { name: heading })).toBeVisible()
  }
  await expect(page.getByRole('status').filter({ hasText: 'Database: OK' })).toBeVisible()

  const total = await tileValue(page, 'Orders by status', 'Total')
  const parts = await Promise.all(
    ['Draft', 'In progress', 'Completed', 'Cancelled'].map((label) => tileValue(page, 'Orders by status', label)),
  )
  expect(total).toBeGreaterThan(0)
  expect(parts.reduce((a, b) => a + b, 0)).toBe(total)

  // The workload bars sum to the active orders (DEC-009, one snapshot): read them from the chart's own table.
  await page.getByRole('button', { name: 'View as table' }).first().click()
  const orders = await page.getByRole('table', { name: 'Open workload by due week' }).locator('tbody tr td:nth-child(3)').allInnerTexts()
  expect(orders).toHaveLength(10)
  expect(orders.map((n) => Number(n.replace(/,/g, ''))).reduce((a, b) => a + b, 0)).toBe(parts[0] + parts[1])

  await expectNoAxeViolations(page)
})

test('widgets are read-only and the navbar reaches every screen with the right current page (TC-202, TC-222)', async ({ page }) => {
  await expect(page.getByRole('main').getByRole('link')).toHaveCount(0)

  await nav(page).getByRole('link', { name: 'Production orders' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'Production orders' })).toBeVisible()
  await expect(nav(page).getByRole('link', { name: 'Production orders' })).toHaveAttribute('aria-current', 'page')

  await nav(page).getByRole('link', { name: 'New production order' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'New production order' })).toBeVisible()
  await expect(nav(page).getByRole('link', { name: 'New production order' })).toHaveAttribute('aria-current', 'page')

  await nav(page).getByRole('link', { name: 'Dashboard' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'Dashboard' })).toBeVisible()
  await expectNoAxeViolations(page)
})

test('a failed load shows Retry and no figures; Retry recovers (TC-216)', async ({ page }) => {
  let failures = 1
  await page.route('**/api/dashboard', async (route) => {
    if (failures-- > 0) {
      await route.fulfill({ status: 500, contentType: 'application/problem+json', body: '{"status":500,"code":"MSG-E013"}' })
    } else {
      await route.continue()
    }
  })
  await page.goto('/')

  const alert = page.getByRole('alert')
  await expect(alert).toContainText('Something went wrong. Try again.')
  await expect(page.getByRole('list', { name: 'Orders by status' })).toHaveCount(0)
  await expect(nav(page)).toBeVisible()

  await alert.getByRole('button', { name: 'Retry' }).click()
  await expect(page.getByRole('list', { name: 'Orders by status' })).toBeVisible()
})

test('a chart maximizes to a dialog, keeps focus inside, and restores focus on Escape (TC-227)', async ({ page }) => {
  const expand = page.getByRole('button', { name: 'Expand Completed per week, last 12 weeks' })
  let requests = 0
  page.on('request', (r) => { if (r.url().endsWith('/api/dashboard')) requests++ })

  await expand.click()
  const dialog = page.getByRole('dialog', { name: 'Completed per week, last 12 weeks' })
  await expect(dialog).toBeVisible()
  await expect(dialog.getByRole('button', { name: 'Restore' })).toBeFocused()
  await expect(dialog.getByRole('table')).toBeVisible()
  await expect(dialog.getByRole('img', { name: /^Orders completed per week, last 12 weeks: / })).toBeVisible()

  await page.keyboard.press('Tab')
  await page.keyboard.press('Tab')
  expect(await dialog.evaluate((d) => d.contains(document.activeElement))).toBe(true)
  await expectNoAxeViolations(page)

  await page.keyboard.press('Escape')
  await expect(dialog).toBeHidden()
  await expect(expand).toBeFocused()
  expect(requests).toBe(0)
})

test('an edited order asks before a navbar link leaves it; Discard follows the link (TC-223)', async ({ page }) => {
  await openCreateForm(page)
  await page.getByLabel('Quantity').fill('7')
  await page.getByLabel('Due date').fill(futureDate())

  await nav(page).getByRole('link', { name: 'Dashboard' }).click()
  const dialog = page.getByRole('dialog', { name: 'Discard your changes?' })
  await expect(dialog).toBeVisible()
  await dialog.getByRole('button', { name: 'Keep editing' }).click()
  await expect(page).toHaveURL(/\/production-orders\/new$/)
  await expect(page.getByLabel('Quantity')).toHaveValue('7')

  await nav(page).getByRole('link', { name: 'Dashboard' }).click()
  await page.getByRole('dialog', { name: 'Discard your changes?' }).getByRole('button', { name: 'Discard' }).click()
  await expect(page.getByRole('heading', { level: 1, name: 'Dashboard' })).toBeVisible()
})
