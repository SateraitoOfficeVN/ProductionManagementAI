import { expect, test } from '@playwright/test'
import { createOrder, expectNoAxeViolations, futureDate, openCreateForm, productField, signIn } from './helpers'

// Screen A (SCR-001) user journeys — DD-001 E-level test viewpoints.

test.beforeEach(async ({ page }) => {
  await signIn(page)
})

test('create → edit → In progress (locked) → Completed (terminal), accessible at each step', async ({ page }) => {
  await page.getByRole('link', { name: 'New production order' }).click()
  await expect(page).toHaveURL(/\/production-orders\/new$/)
  await expect(page.getByText('Assigned on save')).toBeVisible()
  await expectNoAxeViolations(page)

  const { orderNumber } = await createOrder(page, { notes: 'Priority run for line 2' })
  await expect(page).toHaveURL(/\/production-orders\/[0-9a-f-]{36}$/)
  await expect(page.getByRole('heading', { level: 1 })).toHaveText(`Production order ${orderNumber}`)
  await expect(page).toHaveTitle(`Production order ${orderNumber} — ProductionManagementAI`)

  // Draft → In progress: product/quantity become read-only but stay focusable.
  await page.getByLabel('Status').selectOption({ label: 'In progress' })
  await page.getByRole('button', { name: 'Save' }).click()
  await expect(page.getByRole('status')).toHaveText(`Production order ${orderNumber} saved.`)
  await expect(page.getByLabel('Quantity')).toHaveAttribute('readonly', '')
  await expect(productField(page)).toHaveValue('P-1004 — Drive shaft')
  await expect(page.getByText('Locked after the order leaves Draft.')).toBeVisible()
  await productField(page).focus()
  await expect(productField(page)).toBeFocused()
  await expect(page.getByLabel('Status').locator('option')).toHaveText(['In progress', 'Completed', 'Cancelled'])
  await expectNoAxeViolations(page)

  // In progress → Completed: terminal, the status select is disabled.
  await page.getByLabel('Status').selectOption({ label: 'Completed' })
  await page.getByRole('button', { name: 'Save' }).click()
  await expect(page.getByRole('status')).toHaveText(`Production order ${orderNumber} saved.`)
  await expect(page.getByLabel('Status')).toBeDisabled()

  // Reload keeps the saved state (persisted, not just client state).
  await page.reload()
  await expect(page.getByLabel('Status')).toHaveValue('Completed')
  await expect(page.getByLabel('Notes')).toHaveValue('Priority run for line 2')
})

test('empty Save shows inline errors, focuses Product, and is accessible', async ({ page }) => {
  await openCreateForm(page)
  await page.getByRole('button', { name: 'Save' }).click()

  await expect(page.getByText('Select a product.')).toBeVisible()
  await expect(page.getByText('Enter a whole number of 1 or more.')).toBeVisible()
  await expect(page.getByText('Enter a due date.')).toBeVisible()
  await expect(productField(page)).toBeFocused()
  await expectNoAxeViolations(page)

  await page.getByLabel('Due date').fill('2000-01-01')
  await page.getByLabel('Due date').blur()
  await expect(page.getByText("Due date can't be in the past.")).toBeVisible()
})

test('Cancel asks before discarding changes; Escape keeps editing; Discard leaves without saving', async ({ page }) => {
  await openCreateForm(page)
  await page.getByLabel('Quantity').fill('12')
  await page.getByRole('button', { name: 'Cancel' }).click()

  const dialog = page.getByRole('dialog', { name: 'Discard your changes?' })
  await expect(dialog).toBeVisible()
  await expect(dialog.getByRole('button', { name: 'Keep editing' })).toBeFocused()
  await expectNoAxeViolations(page)

  await page.keyboard.press('Escape')
  await expect(dialog).toBeHidden()
  await expect(page.getByLabel('Quantity')).toHaveValue('12')
  await expect(page.getByRole('button', { name: 'Cancel' })).toBeFocused()

  await page.getByRole('button', { name: 'Cancel' }).click()
  await dialog.getByRole('button', { name: 'Discard' }).click()
  await expect(page).toHaveURL(/\/production-orders$/)
})

test('Cancel with no changes leaves straight away', async ({ page }) => {
  await openCreateForm(page)
  await page.getByRole('button', { name: 'Cancel' }).click()
  await expect(page).toHaveURL(/\/production-orders$/)
  await expect(page.getByRole('dialog')).toHaveCount(0)
})

test('two users editing the same order: the second save is rejected and Reload shows the first', async ({ page, browser }) => {
  const { url } = await createOrder(page)

  const otherContext = await browser.newContext()
  const other = await otherContext.newPage()
  await signIn(other)
  await other.goto(url)
  await page.goto(url)

  await other.getByLabel('Notes').fill('saved by the other user')
  await other.getByRole('button', { name: 'Save' }).click()
  await expect(other.getByRole('status')).toContainText('saved.')

  await page.getByLabel('Notes').fill('my stale edit')
  await page.getByRole('button', { name: 'Save' }).click()
  const alert = page.getByRole('alert')
  await expect(alert).toContainText('This order was changed by someone else.')
  await expect(page.getByLabel('Notes')).toHaveValue('my stale edit')

  await alert.getByRole('button', { name: 'Reload' }).click()
  await expect(page.getByLabel('Notes')).toHaveValue('saved by the other user')
  await otherContext.close()
})

test('unknown order shows the not-found panel', async ({ page }) => {
  await page.goto('/production-orders/00000000-0000-0000-0000-000000000000')
  await expect(page.getByRole('alert')).toHaveText(/This production order doesn't exist\./)
  await expect(page.getByRole('button', { name: 'Save' })).toHaveCount(0)
})

test('the product list shows all 30 seeded products', async ({ page }) => {
  await openCreateForm(page)
  // 30 products + the "Select a product" placeholder.
  await expect(productField(page).locator('option')).toHaveCount(31)
  await expect(page.getByLabel('Due date')).toBeEditable()
  expect(futureDate()).toMatch(/^\d{4}-\d{2}-\d{2}$/)
})
